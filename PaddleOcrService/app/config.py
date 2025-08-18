from pydantic_settings  import BaseSettings

class Settings(BaseSettings):
    rabbitmq_host: str = "localhost"
    rabbitmq_port: int = 5672
    rabbitmq_user: str = "guest"
    rabbitmq_pass: str = "guest"
    request_queue: str = "ocr_requests"
    response_queue: str = "ocr_response"

    class Config:
        env_file = ".env"

# factory function for DI
def get_settings() -> Settings:
    return Settings()