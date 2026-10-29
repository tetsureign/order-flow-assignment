FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/Inventory.Service/Inventory.Service.csproj", "services/Inventory.Service/"]
RUN dotnet restore "services/Inventory.Service/Inventory.Service.csproj"
COPY . .
WORKDIR "/src/services/Inventory.Service"
RUN dotnet build "./Inventory.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
RUN arch="$TARGETARCH" && \
    if [ "$arch" = "amd64" ]; then arch="x64"; fi && \
    dotnet publish "./Inventory.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish -a "$arch" /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Service.dll"]
