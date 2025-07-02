# Docker Deployment for MCP Servers

This document explains how to deploy the MCP servers using Docker and Docker Compose.

## Quick Start

1. **Copy environment configuration:**
   ```bash
   cp .env.example .env
   ```

2. **Start both MCP servers:**
   ```bash
   docker-compose up -d
   ```

3. **View logs:**
   ```bash
   # All services
   docker-compose logs -f
   
   # Specific service
   docker-compose logs -f sse-mcp-server
   docker-compose logs -f stdio-mcp-server
   ```

## Environment Variables

Configure the following variables in your `.env` file:

| Variable | Default | Description |
|----------|---------|-------------|
| `SSE_PORT` | `5560` | Port for SSE MCP server |
| `DB_PATH` | `./db` | Path to SQLite database directory |
| `SSE_CONFIG_PATH` | `./srv/sse-mcp-server/appsettings.json` | SSE server configuration |
| `STDIO_CONFIG_PATH` | `./srv/stdio-mcp-server/appsettings.json` | STDIO server configuration |

## Services

### SSE MCP Server
- **Port:** 5560 (configurable via `SSE_PORT`)
- **Transport:** HTTP/SSE
- **Health Check:** Available at `http://localhost:5560/health`
- **Access:** REST API endpoints for MCP tools

### STDIO MCP Server
- **Transport:** STDIO (Standard Input/Output)
- **Usage:** For command-line MCP clients
- **Interactive:** Supports stdin/stdout communication

## Database

Both servers share the same SQLite database located at `${DB_PATH}/ev-supportwizard.db`. The database is automatically created and migrated on first startup.

## Custom Configuration

You can override the default `appsettings.json` files by:

1. Creating custom configuration files
2. Setting the environment variables to point to your custom files
3. Restarting the containers

Example:
```bash
# Create custom config
cp srv/sse-mcp-server/appsettings.json my-sse-config.json

# Edit my-sse-config.json as needed

# Update .env file
echo "SSE_CONFIG_PATH=./my-sse-config.json" >> .env

# Restart
docker-compose restart sse-mcp-server
```

## Building Images

To build the Docker images manually:

```bash
# Build SSE MCP Server
docker build -f srv/sse-mcp-server/Dockerfile -t sse-mcp-server .

# Build STDIO MCP Server
docker build -f srv/stdio-mcp-server/Dockerfile -t stdio-mcp-server .
```

## Network

Both services are connected via a `mcp-network` bridge network, allowing them to communicate with each other if needed.

## Troubleshooting

### Check container status:
```bash
docker-compose ps
```

### Check logs for errors:
```bash
docker-compose logs sse-mcp-server
docker-compose logs stdio-mcp-server
```

### Restart services:
```bash
docker-compose restart
```

### Rebuild and restart:
```bash
docker-compose up --build -d
```

### Access container shell:
```bash
docker-compose exec sse-mcp-server sh
docker-compose exec stdio-mcp-server sh
```