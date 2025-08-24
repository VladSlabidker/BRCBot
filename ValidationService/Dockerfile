FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "ValidationService/ValidationService.csproj"

WORKDIR "/src/ValidationService"
RUN dotnet build "ValidationService.csproj" -c Release -o /app/build

FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "ValidationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN dotnet tool install --global Microsoft.Playwright.CLI \
    && playwright install

ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ValidationService.dll"]
