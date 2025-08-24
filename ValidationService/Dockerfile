FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "ValidationService/ValidationService.csproj"

WORKDIR "/src/ValidationService"

RUN dotnet add package Microsoft.Playwright

RUN dotnet build "ValidationService.csproj" -c Release

RUN dotnet playwright install --with-deps

FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "ValidationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

ENTRYPOINT ["dotnet", "ValidationService.dll"]
