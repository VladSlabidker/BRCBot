FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "ValidationService/ValidationService.csproj"

WORKDIR "/src/ValidationService"

RUN dotnet build "ValidationService.csproj" -c Release

RUN dotnet tool install --global Microsoft.Playwright.CLI && \
    export PATH="$PATH:/root/.dotnet/tools" && \
    playwright install --with-deps

RUN dotnet publish "ValidationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libglib2.0-0 \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libx11-6 \
    libxcb1 \
    libxcomposite1 \
    libxrandr2 \
    libgbm1 \
    libpango-1.0-0 \
    libcairo2 \
    libasound2 \
    fonts-liberation \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libdrm2 \
    libexpat1 \
    libgtk-3-0 \
    libxrender1 \
    libxtst6 \
    libpulse0 \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

ENTRYPOINT ["dotnet", "ValidationService.dll"]
