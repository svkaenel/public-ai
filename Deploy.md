  # Docker
  
  ## SSE MCP Server
  docker build -f srv/sse-mcp-server/Dockerfile -t sse-mcp-server .

  ## STDIO MCP Server  
  docker build -f srv/stdio-mcp-server/Dockerfile -t stdio-mcp-server .

  # Docker Compose

  ## Setup
  cp .env.example .env

  ## Start services
  docker-compose up -d

  ## Monitor logs
  docker-compose logs -f