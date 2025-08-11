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

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)


def _on_message_factory(ocr_service: OCRService, broker: RabbitMQBroker):
    def _on_message(body: bytes, properties):
        try:
            payload = json.loads(body.decode('utf-8'))
        except Exception as e:
            logger.exception('Invalid JSON payload: %s', e)
            return

        image_b64 = payload.get('image_b64')
        if not image_b64:
            logger.warning('Empty payload or missing image_b64')
            return

        # Выполняем OCR (блокирующий вызов)
        logger.info('Starting OCR for message, correlation_id=%s', getattr(properties, 'correlation_id', None))
        result = ocr_service.image_b64_to_text(image_b64)

        # Подготовим ответ
        response_body = json.dumps(result).encode('utf-8')

        # Отправляем в очередь reply_to
        reply_to = getattr(properties, 'reply_to', None)
        corr_id = getattr(properties, 'correlation_id', None)
        if reply_to:
            props = pika.BasicProperties(correlation_id=corr_id)  # type: ignore[name-defined]
            broker.publish(reply_to, response_body, properties=props)
            logger.info('Published OCR response to %s, correlation_id=%s', reply_to, corr_id)
        else:
            logger.warning('No reply_to in message properties; result discarded')

    return _on_message


def _run():
    settings = get_settings()
    broker = RabbitMQBroker(settings)
    ocr_service = OCRService(lang='en')

    # start RabbitMQ consumer
    on_message = _on_message_factory(ocr_service, broker)
    broker.start_consume(on_message)

    # run uvicorn web server in same process (dev convenience)
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