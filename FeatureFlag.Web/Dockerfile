FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FeatureFlag.Web/FeatureFlag.Web.csproj FeatureFlag.Web/
RUN dotnet restore FeatureFlag.Web/FeatureFlag.Web.csproj

COPY FeatureFlag.Web/ FeatureFlag.Web/
WORKDIR /src/FeatureFlag.Web
RUN dotnet publish FeatureFlag.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FeatureFlag.Web.dll"]
