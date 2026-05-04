FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FactuxGatewayApi.sln", "./"]
COPY ["src/FactuxGateway.API/FactuxGateway.API.csproj", "src/FactuxGateway.API/"]

RUN dotnet restore "FactuxGatewayApi.sln"

COPY . .

RUN dotnet publish "src/FactuxGateway.API/FactuxGateway.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} dotnet FactuxGateway.API.dll"]
