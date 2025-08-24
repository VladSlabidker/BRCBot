FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "Data.Cache/Data.Cache.csproj"

WORKDIR "/src/Data.Cache"

RUN dotnet build "Data.Cache.csproj" -c Release -o /app/build

FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "Data.Cache.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ARG PROJECT_NAME
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Data.Cache.dll"]

