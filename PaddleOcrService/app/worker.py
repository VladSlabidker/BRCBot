import json
import signal
import logging
from typing import Any
import pika
import uvicorn
from fastapi import FastAPI
from .config import get_settings
from .broker import RabbitMQBroker
from .ocr_service import OCRService
from urllib.parse import urlparse

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)


def _on_message_factory(channel, default_response_queue, ocr_service: OCRService):
    def _on_message(body, properties):
        try:
            print(f"Получено сообщение: \nHeaders: {properties}")
            print(type(body))
            try:
                payload = json.loads(body.decode("utf-8"))
                print(f"Payload: {payload}")
                image_b64 = payload.get("imageBase64", {})
                            
                correlationId = getattr(properties, "correlationId", None)
                
                if not correlationId:
                    correlationId = getattr(properties, "correlation_id", None)
                    
                if not correlationId:
                    print("NO CORRELATION ID FOUND")
                
                if not image_b64:
                    logger.warning("No imageBase64 in message")
                    result = {"text": "", "error": "No file found"}
            
                print("Sent Data to OCR")
                result = ocr_service.image_b64_to_text(image_b64)
                print(f"Recieved from OCR: {result}")
                
            except json.JSONDecodeError as e:
                print(f"Ошибка парсинга JSON: {e}")

            result_data = {
                "Text": result.get("text", ""),
                "Error": result.get("error", ""),
                "CorrelationId": correlationId
            }
            
            reply_to = getattr(properties, "reply_to", None)
            
            if not reply_to:
                reply_to = getattr(properties, "replyTo", None)
                
            if not reply_to:
                reply_to = payload.get("responseAddress", {})
            
            if not reply_to:
                reply_to = payload.get("replyTo", {})
            
            if not reply_to:
                print("NO REPLY_TO FOUND")
            
            if reply_to:
                incoming_envelope = json.loads(body.decode("utf-8"))

                incoming_envelope["message"] = result_data

                headers = {
                    "MT-MessageType": ["urn:message:OcrService.Models:OcrResponse"]
                }
                
                print(f"CorrelationId: {correlationId}")
                
                channel.publish(
                    routing_key=reply_to,
                    body=json.dumps(result_data).encode("utf-8"),
                    properties=pika.BasicProperties(
                        correlation_id=correlationId,
                        content_type="application/json"
                    )
                )
                print(f"Ответ отправлен в очередь {reply_to}")
            else:
                print("⚠ Нет очереди для ответа, ответ не отправлен")          
         

        except Exception as e:
            print(f"Ошибка при обработке сообщения: {e}")
    
    return _on_message

def _run():
    settings = get_settings()
    broker = RabbitMQBroker(settings)
    ocr_service = OCRService()

    on_message = _on_message_factory(broker, settings.response_queue, ocr_service)
    broker.start_consume(on_message)

    config = uvicorn.Config("app.main:app", host="0.0.0.0", port=8000, log_level="info")
    server = uvicorn.Server(config)

    def _signal_handler(sig, frame):
        logger.info('Shutting down..')
        broker.close()
        server.should_exit = True

    signal.signal(signal.SIGINT, _signal_handler)
    signal.signal(signal.SIGTERM, _signal_handler)

    logger.info('Starting service; connecting to RabbitMQ at %s:%s', settings.rabbitmq_host, settings.rabbitmq_port)
    server.run()


if __name__ == '__main__':
    _run()