FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "ValidationService/ValidationService.csproj"

WORKDIR "/src/ValidationService"

# Добавляем Playwright пакет
RUN dotnet add package Microsoft.Playwright

# Сначала собираем проект (чтобы создался playwright.ps1)
RUN dotnet build "ValidationService.csproj" -c Release -o /app/build

# Теперь устанавливаем Playwright CLI и браузеры
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global Microsoft.Playwright.CLI \
    && playwright install --with-deps

FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "ValidationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Копируем установленные браузеры Playwright
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

ENTRYPOINT ["dotnet", "ValidationService.dll"]
