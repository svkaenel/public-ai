# Use the official .NET 9.0 runtime image for Alpine Linux
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app

# Install SQLite for database access including development libraries
RUN apk add --no-cache sqlite sqlite-dev sqlite-libs

# Use the .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution file and project files
RUN echo "Copying solution and project files V-0.1.12 ..."

COPY public-ai.sln .
COPY Directory.Packages.props .

# Copy all project files for proper dependency resolution
COPY lib/Evanto.Mcp.Apps/Evanto.Mcp.Apps.csproj lib/Evanto.Mcp.Apps/
COPY lib/Evanto.Mcp.Common/Evanto.Mcp.Common.csproj lib/Evanto.Mcp.Common/
COPY lib/Evanto.Mcp.Embeddings/Evanto.Mcp.Embeddings.csproj lib/Evanto.Mcp.Embeddings/
COPY lib/Evanto.Mcp.Pdfs/Evanto.Mcp.Pdfs.csproj lib/Evanto.Mcp.Pdfs/
COPY lib/Evanto.Mcp.QdrantDB/Evanto.Mcp.QdrantDB.csproj lib/Evanto.Mcp.QdrantDB/
COPY lib/Evanto.Mcp.Tools.SupportDocs/Evanto.Mcp.Tools.SupportDocs.csproj lib/Evanto.Mcp.Tools.SupportDocs/
COPY lib/Evanto.Mcp.Tools.SupportWizard/Evanto.Mcp.Tools.SupportWizard.csproj lib/Evanto.Mcp.Tools.SupportWizard/
COPY srv/stdio-mcp-server/stdio-mcp-server.csproj srv/stdio-mcp-server/

# Clear NuGet cache and restore dependencies with verbose logging
RUN dotnet nuget locals all --clear && \
    dotnet restore srv/stdio-mcp-server/stdio-mcp-server.csproj --verbosity normal

# Copy all source code
COPY . .

# Build and publish the application with fresh restore
WORKDIR /src/srv/stdio-mcp-server

# Build for ARM64 (Apple Silicon) by default, override with --build-arg if needed
ARG RUNTIME_IDENTIFIER=linux-arm64
RUN echo "Building for Runtime ID: ${RUNTIME_IDENTIFIER}" && \
    dotnet publish stdio-mcp-server.csproj -c Release -o /app/publish \
    --self-contained false -r ${RUNTIME_IDENTIFIER}

# Final stage - create runtime image
FROM base AS final
WORKDIR /app

# Create directories for database and configuration
RUN mkdir -p /app/db /app/config

# Copy published application
COPY --from=build /app/publish .

# Debug: List published files
RUN ls -la /app && echo "--- Checking for executables ---" && find /app -name "*mcp*" -type f

# Set execute permissions for the main executable
RUN chmod +x ./stdio-mcp-server || chmod +x ./Evanto.Mcp.Server.Stdio || true

# Copy configuration files
COPY --from=build /src/srv/stdio-mcp-server/appsettings.json ./appsettings.json

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production
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

# Install SQLite libraries for runtime and create symlinks for compatibility
RUN apk add --no-cache sqlite sqlite-dev sqlite-libs && \
    ln -sf /usr/lib/libsqlite3.so.0 /usr/lib/libsqlite3.so && \
    ln -sf /usr/lib/libsqlite3.so.0 /app/libe_sqlite3.so || true

# Create a non-root user for security with specific UID/GID
RUN addgroup -g 1001 appgroup && \
    adduser -D -s /bin/sh -u 1001 -G appgroup appuser && \
    chown -R appuser:appgroup /app && \
    chmod -R 755 /app/db

USER appuser

# Use dotnet to run the application
ENTRYPOINT ["dotnet", "stdio-mcp-server.dll"]