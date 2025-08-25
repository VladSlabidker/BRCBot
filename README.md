# Telegram Bot for Receipt Verification

## 1. Project Goal

Create a Telegram bot that:

- Accepts a photo or screenshot of a payment receipt from the user.
- Extracts the receipt code using OCR.
- Verifies the authenticity and details of the receipt via relevant websites (PrivatBank, [check.gov.ua](https://check.gov.ua)).
- Ensures scalability and fault tolerance through a microservices architecture.

---

## 2. Project Architecture (Microservices)

| Service               | Functions                                | Interaction                                               | Technologies / Tools                          |
|---------------------- |-----------------------------------------|----------------------------------------------------------|-----------------------------------------------|
| **Common**            | Shared library                           | gRPC Interceptors, Enums, Models, proto files           | C# (.NET)                                     |
| **Telegram.Gateway**  | API gateway for Telegram (Webhook)       | HTTP with Storefront, Telegram API                       | ASP.NET Core, gRPC                             |
| **Storefront**        | Controllers for all endpoints            | gRPC with Telegram.Gateway, ValidationService, OcrService | OpenAPI/Swagger                               |
| **OcrService**        | Interacts with ML OCR model, message queue consumer | gRPC with ValidationService, RabbitMQ with PaddleOcrService | C# (.NET), RabbitMQ                           |
| **PaddleOcrService**  | ML OCR model                             | RabbitMQ with OcrService, Python microservice           | Python, EasyOCR, Fast API, uvicorn                               |
| **ValidationService** | Receipt verification via websites        | gRPC with OcrService, Redis, EntityFramework Core, PlaywrightSharp | C# (.NET), PlaywrightSharp, EF Core, Redis |
| **Data.SQL**          | Database models, migrations, configurations | EntityFramework Core                                    | Microsoft SQL Server                           |
| **Data.Cache**        | Cache models and configurations          | Redis Client                                             | Redis                                         |

---

## 3. Key Technologies and Tools

| Category              | Technologies / Libraries                                                          |
|---------------------- |----------------------------------------------------------------------------------|
| **Language & Platform** | C# (.NET 8), ASP.NET Core, gRPC, Python                                         |
| **API Type**           | REST API, gRPC, AMQP (Message Broker)                                           |
| **Database**           | Microsoft SQL Server Express (local), SQL Server (production), EntityFramework Core |
| **Caching**            | Redis (StackExchange.Redis)                                                     |
| **Message Queue**      | RabbitMQ                                                                        |
| **OCR**                | EasyOCR                                                                         |
| **Web Scraping**       | PlaywrightSharp                                                                 |
| **Containerization**   | Docker, docker-compose (local and production)                                   |
| **CI/CD & Secrets**    | Git, GitHub Actions (CI/CD pipelines, secrets)                                  |
| **Web Server (prod)**  | nginx + certbot for TLS and reverse proxy on VPS                                 |
| **Telegram API**       | Telegram.Bot NuGet                                                              |
| **Documentation**      | OpenAPI/Swagger                                                                 |

---

## 4. Deployment

The project is deployed using a VPS with Docker and Docker-Compose, integrated with deployment via `github/workflows/cd.yaml`, and nginx + certbot for TLS.

---

**Made by:** Vlad Slabidker
