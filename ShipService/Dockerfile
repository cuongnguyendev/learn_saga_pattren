FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShipService/ShipService.csproj", "ShipService/ShipService.csproj"]
COPY ["Shared/Shared.csproj", "Shared/Shared.csproj"]
COPY ShipService/. ./ShipService/
COPY Shared/. ./Shared/
RUN dotnet restore "ShipService/ShipService.csproj"
WORKDIR "/src/ShipService"
RUN dotnet build "ShipService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "ShipService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "ShipService.dll"]
