import json
import logging
import threading
import time
from typing import Callable, Optional

import pika

from .config import Settings, get_settings

logger = logging.getLogger(__name__)


class BrokerInterface:
    def start_consume(self, on_message: Callable[[bytes, pika.spec.BasicProperties], None]):
        raise NotImplementedError

    def publish(self, routing_key: str, body: bytes, properties: Optional[pika.BasicProperties] = None):
        raise NotImplementedError

    def close(self):
        raise NotImplementedError


class RabbitMQBroker(BrokerInterface):
    def __init__(self, settings: Settings):
        self.settings = settings
        credentials = pika.PlainCredentials(settings.rabbitmq_user, settings.rabbitmq_pass)
        params = pika.ConnectionParameters(host=settings.rabbitmq_host,
                                           port=settings.rabbitmq_port,
                                           credentials=credentials,
                                           heartbeat=600,
                                           blocked_connection_timeout=300)
        self._params = params
        self._connection = None
        self._channel = None
        self._consumer_thread = None
        self._stopping = False

    def _ensure_connection(self):
        if self._connection and self._connection.is_open:
            return
        self._connection = pika.BlockingConnection(self._params)
        self._channel = self._connection.channel()
        # Declare the request queue to ensure it exists
        self._channel.queue_declare(queue=self.settings.request_queue, durable=True)

    def start_consume(self, on_message: Callable[[bytes, pika.spec.BasicProperties], None]):
        """
        Запускает блокирующий consumer в отдельном потоке. on_message принимает (body, properties)
        properties — pika.BasicProperties (включая reply_to и correlation_id).
        """
        def _run():
            while not self._stopping:
                try:
                    self._ensure_connection()

                    for method, properties, body in self._channel.consume(self.settings.request_queue, inactivity_timeout=1):
                        if self._stopping:
                            break
                        if method is None:
                            continue

                        try:
                            on_message(body, properties)
                            self._channel.basic_ack(method.delivery_tag)
                        except Exception as e:
                            logger.exception('Ошибка при обработке сообщения: %s', e)
                            # Накатать nack с requeue в false — избегаем бесконечного цикла
                            self._channel.basic_nack(method.delivery_tag, requeue=False)

                except pika.exceptions.AMQPConnectionError:
                    logger.exception('RabbitMQ connection error — reconnecting in 5s')
                    time.sleep(5)
                except Exception:
                    logger.exception('Unexpected error in consumer — sleeping 5s')
                    time.sleep(5)

        self._consumer_thread = threading.Thread(target=_run, daemon=True)
        self._consumer_thread.start()

    def publish(self, routing_key: str, body: bytes, properties: Optional[pika.BasicProperties] = None):
        self._ensure_connection()
        self._channel.basic_publish(exchange='', routing_key=routing_key, body=body, properties=properties)

    def close(self):
        self._stopping = True
        try:
            if self._channel and self._channel.is_open:
                try:
                    self._channel.close()
                except Exception:
                    pass
            if self._connection and self._connection.is_open:
                try:
                    self._connection.close()
                except Exception:
                    pass
        except Exception:
            pass