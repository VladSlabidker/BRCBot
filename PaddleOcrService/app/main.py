import json
import logging
from fastapi import FastAPI, Depends
from fastapi.responses import JSONResponse

from .config import get_settings, Settings
from .ocr_service import OCRService
from .broker import RabbitMQBroker, BrokerInterface

logger = logging.getLogger("ocr_service")
app = FastAPI()

# DI factories
def get_ocr_service() -> OCRService:
    # Здесь можно переключить параметры OCR (язык, предобученные модели и т.д.)
    return OCRService()

def get_broker(settings: Settings = Depends(get_settings)) -> BrokerInterface:
    return RabbitMQBroker(settings)

@app.get('/health')
async def health():
    return JSONResponse({"status": "ok"})

# NOTE: we don't expose a public HTTP OCR endpoint by default because interaction is via RabbitMQ.
# but for convenience we expose a small POST for quick testing if needed (optional):
from fastapi import Body

@app.post('/ocr_http')
async def ocr_http(payload: dict = Body(...), ocr: OCRService = Depends(get_ocr_service)):
    image_b64 = payload.get('image_b64')
    if not image_b64:
        return JSONResponse({"text": "", "error": "image_b64 missing"}, status_code=400)
    result = await app.loop.run_in_executor(None, ocr.image_b64_to_text, image_b64)
    return JSONResponse(result)

# wrapper to attach loop to app
@app.on_event('startup')
async def attach_loop():
    import asyncio
    app.loop = asyncio.get_event_loop()
