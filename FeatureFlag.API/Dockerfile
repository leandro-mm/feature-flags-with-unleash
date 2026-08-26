FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FeatureFlag.API/FeatureFlag.API.csproj FeatureFlag.API/
RUN dotnet restore FeatureFlag.API/FeatureFlag.API.csproj

COPY FeatureFlag.API/ FeatureFlag.API/
WORKDIR /src/FeatureFlag.API
RUN dotnet publish FeatureFlag.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FeatureFlag.API.dll"]
