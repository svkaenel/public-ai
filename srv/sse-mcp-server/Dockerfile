# Use the official .NET 9.0 runtime-deps image for Alpine Linux
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS base
WORKDIR /app
EXPOSE 5560

# Install SQLite for database access
RUN apk add --no-cache sqlite

# Use the .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution file and project files
COPY public-ai.sln .
COPY Directory.Packages.props .

# Copy all project files for proper dependency resolution
COPY lib/Evanto.Mcp.Apps/Evanto.Mcp.Apps.csproj lib/Evanto.Mcp.Apps/
COPY lib/Evanto.Mcp.Common/Evanto.Mcp.Common.csproj lib/Evanto.Mcp.Common/
COPY lib/Evanto.Mcp.Tools.SupportWizard/Evanto.Mcp.Tools.SupportWizard.csproj lib/Evanto.Mcp.Tools.SupportWizard/
COPY srv/sse-mcp-server/sse-mcp-server.csproj srv/sse-mcp-server/

# Restore dependencies
RUN dotnet restore srv/sse-mcp-server/sse-mcp-server.csproj

# Copy all source code
COPY . .

# Build and publish the application
WORKDIR /src/srv/sse-mcp-server
RUN dotnet publish sse-mcp-server.csproj -c Release -o /app/publish \
    --no-restore --self-contained true -r linux-x64

# Final stage - create runtime image
FROM base AS final
WORKDIR /app

# Create directories for database and configuration
RUN mkdir -p /app/db /app/config

# Copy published application
COPY --from=build /app/publish .

# Copy configuration files
COPY --from=build /src/srv/sse-mcp-server/appsettings.json ./appsettings.json

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5560
ENV ConnectionStrings__SupportWizardDB="Filename=/app/db/ev-supportwizard.db"

# fix for "Globalization Invariant Mode is not supported" startup error, see https://github.com/dotnet/SqlClient/issues/220, still necessary for .NET 8
# 1. Install ICU & other native deps
RUN apk add --no-cache \
      icu-libs \
      icu-data-full \
      tzdata 

# 2. Disable invariant globalization and set Docker indicator
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    LANG=en_US.UTF-8 \
    LC_ALL=en_US.UTF-8

# Create a non-root user for security
RUN adduser -D -s /bin/sh appuser && \
    chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5560/health || exit 1

ENTRYPOINT ["./sse-mcp-server"]