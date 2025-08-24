FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "ValidationService/ValidationService.csproj"

WORKDIR "/src/ValidationService"
# Сначала добавляем пакет Playwright
RUN dotnet add package Microsoft.Playwright

# Затем билдим проект, чтобы Playwright мог найти зависимости
RUN dotnet build "ValidationService.csproj" -c Release -o /app/build

# Теперь устанавливаем CLI и скачиваем браузеры
RUN dotnet tool install --global Microsoft.Playwright.CLI \
    && export PATH="$PATH:/root/.dotnet/tools" \
    && dotnet build "ValidationService.csproj" \
    && playwright install

FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "ValidationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Копируем CLI и выставляем PATH
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ValidationService.dll"]
