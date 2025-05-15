# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FinanceTracker/FinanceTracker.csproj", "FinanceTracker/"]
RUN dotnet restore "./FinanceTracker/FinanceTracker.csproj"
COPY . .
WORKDIR "/src/FinanceTracker"
RUN dotnet build "./FinanceTracker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FinanceTracker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure appsettings files are copied
COPY ["FinanceTracker/appsettings.json", "/app/"]
COPY ["FinanceTracker/appsettings.Production.json", "/app/"]
# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "FinanceTracker.dll"] 