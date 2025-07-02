#!/bin/bash

# Setup script for Docker volume permissions
echo "Setting up Docker volume permissions..."

# Create directories if they don't exist
mkdir -p ./run/sse/db
mkdir -p ./run/stdio/db

# Copy configuration files if they don't exist
if [ ! -f "./run/sse/appsettings.json" ]; then
    cp ./srv/sse-mcp-server/appsettings.json ./run/sse/appsettings.json
    echo "Copied SSE configuration to ./run/sse/appsettings.json"
fi

if [ ! -f "./run/stdio/appsettings.json" ]; then
    cp ./srv/stdio-mcp-server/appsettings.json ./run/stdio/appsettings.json
    echo "Copied STDIO configuration to ./run/stdio/appsettings.json"
fi

# Set proper permissions for the directories
# Use UID/GID 1001 to match the container user
if command -v sudo &> /dev/null; then
    echo "Setting ownership to 1001:1001 for database directories..."
    sudo chown -R 1001:1001 ./run/*/db
    sudo chmod -R 775 ./run/*/db
    echo "Permissions set: owner can read/write/execute, group can read/write/execute"
else
    echo "No sudo available, setting permissions with current user..."
    chmod -R 775 ./run/*/db
    echo "Warning: Using current user permissions - container may not be able to write"
fi

# Verify permissions
echo ""
echo "Current permissions:"
ls -la ./run/sse/db/
ls -la ./run/stdio/db/

echo "Setup complete!"
echo ""
echo "You can now run: docker-compose up --build -d"