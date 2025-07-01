# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Run
```bash
# Build entire solution
dotnet build

# Run the main application
dotnet run --project app/cmd-mcp-host

# Run with specific parameters
dotnet run --project app/cmd-mcp-host -- --help        # Show help
dotnet run --project app/cmd-mcp-host -- --list        # List available providers
dotnet run --project app/cmd-mcp-host -- --test        # Run MCP server tests
dotnet run --project app/cmd-mcp-host -- --telemetry   # Enable OpenTelemetry monitoring
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## High-Level Architecture

This is an **MCP (Model Context Protocol) Host System** that integrates multiple MCP servers with AI chat providers. The architecture follows a Factory Pattern with Dependency Injection and Configuration-Driven design.

### Core Components

**Three-Layer Architecture:**
1. **cmd-mcp-host** (app/) - Console application entry point
2. **Evanto.Mcp.Host** (lib/) - Core MCP hosting logic and factories
3. **Evanto.Mcp.Common** (lib/) - Shared configuration and settings

### Key System Workflows

**MCP Integration Flow:**
- Configuration → MCP Client Factory → Transport Layer (STDIO/SSE/HTTP) → Tool Aggregation → Chat Integration

**Supported Transport Types:**
- **STDIO**: Docker containerized MCP servers
- **SSE**: Web-based MCP servers with real-time communication  
- **HTTP**: RESTful MCP servers

### Factory Classes (lib/Evanto.Mcp.Host/Factories/)
- `EvMcpClientFactory`: Creates and manages MCP client connections across transport types
- `EvChatClientFactory`: Creates AI chat clients for multiple providers (OpenAI, Azure, Ollama, etc.)

### Configuration System
- **appsettings.json**: Main configuration file with providers, MCP servers, tool tests, and telemetry
- **Hierarchical structure**: Host → Provider → Server → Tool level configurations
- **Multiple AI providers**: OpenAI, Azure OpenAI, Ollama, LMStudio, Ionos supported
- **OpenTelemetry integration**: Configurable telemetry with OTLP and console exporters

### Testing Framework (lib/Evanto.Mcp.Host/Tests/)
- `EvMcpServerTester`: Comprehensive testing of MCP servers and tools
- Automatic parameter generation based on JSON schemas
- Configurable test scenarios with timeouts
- Detailed result reporting with success/failure metrics

## Important Configuration

### appsettings.json Structure
The configuration uses a multi-provider approach where each provider has:
- `ProviderName`: Identifier for the AI service
- `Endpoint`: API endpoint URL
- `ApiKey`: Authentication credentials
- `DefaultModel`: Default model to use
- `AvailableModels`: List of supported models

### OpenTelemetry Configuration
The `Telemetry` section in appsettings.json configures observability:
- `Enabled`: Enable/disable telemetry collection
- `ServiceName`: Service identifier for traces
- `OtlpEndpoint`: OTLP exporter endpoint (default: http://localhost:4317)
- `EnableConsoleExporter`: Output telemetry to console
- `LogSensitiveData`: Control sensitive data logging
- `ActivitySources`: Configure which activity sources to monitor

### MCP Server Configuration
Each MCP server requires:
- `Name`: Display name for the server
- `Command`/`Url`: Connection method (STDIO command or HTTP/SSE URL)
- `TransportType`: STDIO, SSE, or HTTP
- `TimeoutSeconds`: Connection timeout
- `ToolTests`: Optional testing configuration per tool

## Development Notes

- Important: Always apply the coding rules defined in CodingRules.md when creating or modifying source code 
- Uses .NET 9.0 with nullable reference types enabled
- Central package management via Directory.Packages.props
- Microsoft.Extensions.AI abstractions for unified AI provider interface
- Official ModelContextProtocol NuGet package for MCP implementation
- Spectre.Console for rich terminal UI output
- Comprehensive logging with configurable levels

## Documentation

- If there are tools or frameworks necessary for implementation please use the Context7 MCP server to find the necessary documentation
- If you don't find the documentation there, please perform a web search via Brave Search MCP server

## Key Dependencies
- `Microsoft.Extensions.AI.*`: AI provider abstractions with built-in OpenTelemetry support
- `ModelContextProtocol`: Official MCP client library
- `OpenTelemetry.*`: Observability and telemetry collection
- `Spectre.Console`: Rich console output
- `BoxOfYellow.ConsoleMarkdownRenderer`: Markdown rendering in console