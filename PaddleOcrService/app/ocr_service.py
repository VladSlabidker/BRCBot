import base64
import tempfile
import os
from typing import Optional

from paddleocr import PaddleOCR

class OCRService:
    def __init__(self, lang: str = 'en'):
        # Инициализация PaddleOCR — может занять время
        # Для русского/украинского можно указать 'ru' или 'en' + 'ch' в зависимости от нужд
        # Если у вас GPU — установите paddlepaddle-gpu и PaddleOCR под GPU
        self._ocr = PaddleOCR(use_angle_cls=True, lang=lang)

    def image_b64_to_text(self, image_b64: str) -> dict:
        """Принимает base64 (строка), возвращает dict {text, error} """
        try:
            header_sep = image_b64.find(',')
            if header_sep != -1 and image_b64[:header_sep].startswith('data:'):
                image_b64 = image_b64[header_sep+1:]

            img_bytes = base64.b64decode(image_b64)

            # Сохраним временный файл и дадим в PaddleOCR
            with tempfile.NamedTemporaryFile(delete=False, suffix='.png') as f:
                f.write(img_bytes)
                tmp_path = f.name

            # PaddleOCR вернёт список списков с результатами
            result = self._ocr.ocr(tmp_path, cls=True)

            # Соберём текст из результата
            lines = []
            for page in result:
                for line in page:
                    # line = [box, (text, confidence)]
                    try:
                        text = line[1][0]
                    except Exception:
                        text = ''
                    if text:
                        lines.append(text)

            text_out = '\n'.join(lines)
            return {"text": text_out, "error": None}

        except Exception as e:
            return {"text": "", "error": str(e)}
        finally:
            try:
                if 'tmp_path' in locals() and os.path.exists(tmp_path):
                    os.remove(tmp_path)
            except Exception:
                pass
