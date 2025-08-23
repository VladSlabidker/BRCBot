FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Common/*.csproj Common/
COPY Data.Cache/*.csproj Data.Cache/
COPY Data.SQL/*.csproj Data.SQL/
COPY OcrService/*.csproj OcrService/
COPY SubscriptionService/*.csproj SubscriptionService/
COPY Storefront/*.csproj Storefront/
COPY Telegram.Gateway/*.csproj Telegram.Gateway/
COPY ValidationService/*.csproj ValidationService/

ARG PROJECT_NAME
RUN dotnet restore "$PROJECT_NAME/$PROJECT_NAME.csproj"

COPY . .

WORKDIR "/src/${PROJECT_NAME}"
RUN dotnet build "${PROJECT_NAME}.csproj" -c Release -o /app/build

FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "${PROJECT_NAME}.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ARG PROJECT_NAME
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "${PROJECT_NAME}.dll"]

