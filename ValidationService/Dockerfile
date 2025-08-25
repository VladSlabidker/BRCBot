# Финальный образ на базе Playwright + .NET
FROM mcr.microsoft.com/playwright/dotnet:v1.54.0-noble AS base
WORKDIR /app

# Стадия сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "ValidationService/ValidationService.csproj"

WORKDIR "/src/ValidationService"
RUN dotnet build "ValidationService.csproj" -c Release

# Установка Playwright CLI и браузера Chromium
RUN dotnet tool install --global Microsoft.Playwright.CLI && \
    export PATH="$PATH:/root/.dotnet/tools" && \
    playwright install --with-deps chromium

# Публикуем проект
RUN dotnet publish "ValidationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM mcr.microsoft.com/playwright/dotnet:v1.54.0-noble AS final
WORKDIR /app

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Копируем Playwright браузеры и кэш
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

# Переменная окружения для Playwright (указывает путь к браузерам)
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright

# Запуск микросервиса
ENTRYPOINT ["dotnet", "ValidationService.dll"]
