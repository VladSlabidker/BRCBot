FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

ARG PROJECT_NAME
RUN dotnet restore "Data.SQL/Data.SQL.csproj"

WORKDIR "/src/Data.SQL"
RUN dotnet build "Data.SQL.csproj" -c Release -o /app/build

FROM build AS publish
ARG PROJECT_NAME
RUN dotnet publish "Data.SQL.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ARG PROJECT_NAME
COPY --from=publish /app/publish .
COPY Data.SQL/appsettings*.json .
ENTRYPOINT ["dotnet", "Data.SQL.dll"]

