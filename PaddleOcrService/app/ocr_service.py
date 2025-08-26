import base64
from io import BytesIO
from typing import Union, List, Dict
import numpy as np
from PIL import Image, ImageEnhance
import easyocr


def _strip_data_prefix(s: str) -> str:
    return s.split(",", 1)[1] if s.startswith("data:") and "," in s else s


class OCRService:
    LANG_MAP = {  
        "ru": "ru",
        "en": "en",
    }

    def __init__(self, langs_priority: List[str] = None):
        self.langs_priority = langs_priority or ["ru", "en"]
        self._easy_cache: Dict[str, easyocr.Reader] = {}
        self._warmup_models()

    def _map_langs(self, langs: List[str]) -> List[str]:
        """Преобразуем коды языков в формат EasyOCR"""
        return [self.LANG_MAP.get(l, l) for l in langs]

    def _warmup_models(self):
        """Загружаем модели EasyOCR при старте"""
        try:
            self._get_easy(self.langs_priority)
        except Exception:
            pass

    def _get_easy(self, langs: List[str]) -> easyocr.Reader:
        mapped_langs = self._map_langs(langs)
        key = "-".join(mapped_langs)
        if key not in self._easy_cache:
            self._easy_cache[key] = easyocr.Reader(mapped_langs)
        return self._easy_cache[key]

    def _to_image_bytes(self, data: Union[str, bytes]) -> bytes:
        if isinstance(data, str):
            try:
                b64 = _strip_data_prefix(data)
                return base64.b64decode(b64)
            except Exception:
                raise ValueError("Provided string is not valid Base64 image data.")
        elif isinstance(data, (bytes, bytearray)):
            try:
                return base64.b64decode(bytes(data))
            except Exception:
                return bytes(data)
        else:
            raise TypeError("image data must be str (base64) or bytes.")

    def _preprocess(self, img: Image.Image) -> Image.Image:
        if img.mode != "RGB":
            img = img.convert("RGB")

        w, h = img.size
        short_side = min(w, h)
        if short_side < 900:
            scale = 900 / float(short_side)
            img = img.resize((int(w * scale), int(h * scale)), Image.LANCZOS)

        img = img.convert("L")
        img = img.resize((img.width * 2, img.height * 2))
        try:
            enhancer = ImageEnhance.Contrast(img)
            img = enhancer.enhance(1.15)
        except Exception:
            pass

        return img

    def image_b64_to_text(self, image_input: Union[str, bytes]) -> dict:
        try:
            img_bytes = self._to_image_bytes(image_input)

            try:
                pil_img = Image.open(BytesIO(img_bytes))
            except Exception as e:
                return {"text": "", "error": f"Image load error: {e}"}

            pil_img = self._preprocess(pil_img)
            img_np = np.array(pil_img)

            try:
                reader = self._get_easy(self.langs_priority)
                result = reader.readtext(img_np)
                if result:
                    text = "\n".join([item[1] for item in result])
                    return {"text": text, "error": None}
            except Exception as e:
                return {"text": "", "error": f"EasyOCR error: {e}"}

            return {"text": "", "error": "No text detected by EasyOCR"}

        except Exception as e:
            return {"text": "", "error": str(e)}
