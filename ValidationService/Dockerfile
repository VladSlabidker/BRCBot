# 1. Базовый runtime слой
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# 2. SDK слой для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "ValidationService/ValidationService.csproj"

WORKDIR "/src/ValidationService"
RUN dotnet build "ValidationService.csproj" -c Release -o /app/build

# 3. Устанавливаем Playwright CLI и скачиваем браузеры
RUN dotnet tool install --global Microsoft.Playwright.CLI \
    && /root/.dotnet/tools/playwright install

# 4. Публикация проекта
FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "ValidationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 5. Финальный runtime слой
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Копируем установленный Playwright CLI из build слоя
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ValidationService.dll"]
