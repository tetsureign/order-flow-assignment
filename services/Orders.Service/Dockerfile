FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/Orders.Service/Orders.Service.csproj", "services/Orders.Service/"]
RUN dotnet restore "services/Orders.Service/Orders.Service.csproj"
COPY . .
WORKDIR "/src/services/Orders.Service"
RUN dotnet build "./Orders.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
RUN arch="$TARGETARCH" && \
    if [ "$arch" = "amd64" ]; then arch="x64"; fi && \
    dotnet publish "./Orders.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish -a "$arch" /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.Service.dll"]
