FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrderService/OrderService.csproj", "OrderService/OrderService.csproj"]
COPY ["Shared/Shared.csproj", "Shared/Shared.csproj"]
COPY OrderService/. ./OrderService/
COPY Shared/. ./Shared/
RUN dotnet restore "OrderService/OrderService.csproj"
WORKDIR "/src/OrderService"
RUN dotnet build "OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "OrderService.dll"]
