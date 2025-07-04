# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Run
```bash
# Build entire solution
dotnet build

# Run the main application (MCP client)
dotnet run --project app/cmd-mcp-host

# Run with specific parameters
dotnet run --project app/cmd-mcp-host -- --help        # Show help
dotnet run --project app/cmd-mcp-host -- --list        # List available providers
dotnet run --project app/cmd-mcp-host -- --test        # Run MCP server tests
dotnet run --project app/cmd-mcp-host -- --telemetry   # Enable OpenTelemetry monitoring

# Run PDF vectorization utility
dotnet run --project app/cmd-vectorize                 # Process PDFs to vector embeddings

# Run MCP servers independently
dotnet run --project srv/sse-mcp-server                # HTTP/SSE MCP server
dotnet run --project srv/stdio-mcp-server              # STDIO MCP server

# Docker deployment
docker-compose up -d                                   # Run both servers in containers
docker-compose logs -f                                 # View container logs
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

**Multi-Layer Architecture:**
1. **Console Applications** (app/) - Standalone command-line tools:
   - **cmd-mcp-host** - Interactive MCP client with AI chat integration
   - **cmd-vectorize** - PDF processing and vectorization utility
2. **Core Libraries** (lib/) - Business logic and infrastructure:
   - **Evanto.Mcp.Host** - Core MCP hosting logic, factories, and testing framework
   - **Evanto.Mcp.Common** - Shared configuration, settings, and common utilities
   - **Evanto.Mcp.Apps** - Application helper services and shared app functionality
   - **Evanto.Mcp.Embeddings** - Text embedding services and vector operations
   - **Evanto.Mcp.Qdrant** - Unified Qdrant vector database repository and document management
3. **MCP Tool Libraries** (lib/) - Specialized tool implementations:
   - **Evanto.Mcp.Tools.SupportWizard** - Support request tracking with SQLite database
   - **Evanto.Mcp.Tools.SupportDocs** - Support documentation management tools
4. **MCP Server Implementations** (srv/) - Standalone MCP servers:
   - **sse-mcp-server** - SSE transport-based MCP server
   - **stdio-mcp-server** - STDIO transport-based MCP server

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

### Database Integration
- **SQLite Support**: Local database storage for SupportWizard tools
- **Entity Framework Core**: Database abstraction with Code-First migrations
- **Guid Primary Keys**: All entities use System.Guid for better security and uniqueness
- **Database Location**: `db/ev-supportwizard.db` for SupportWizard data persistence

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

### Database Configuration
For SupportWizard and other database-enabled tools:
- `ConnectionStrings.SupportWizardDB`: SQLite connection string
- Automatic database creation and migration on first run
- Configurable through appsettings.json or environment variables

### SupportDocs Configuration
For document management and embedding functionality:
- `Embeddings`: Configuration for text embedding provider (Ollama, OpenAI, etc.)
  - `ProviderName`: Embedding service provider
  - `DefaultModel`: Model for text embeddings (e.g., "nomic-embed-text")
  - `Endpoint`: Service endpoint URL
  - `ChunkSize`: Text chunk size for processing
  - `ChunkOverlap`: Overlap between text chunks
- `Qdrant`: Vector database configuration
  - `Endpoint`: Qdrant server endpoint
  - `CollectionName`: Collection name for document vectors
  - `Port`: Qdrant server port
  - `SearchLimit`: Maximum search results
  - `MinimumScore`: Minimum similarity score threshold
  - `VectorDimension`: Embedding vector dimensions

### cmd-vectorize Configuration
For PDF processing and vectorization:
- `PdfDirectory`: Directory path containing PDF files to process
- `TrackingFilePath`: JSON file path for tracking processed files
- `Embeddings`: Same embedding configuration as SupportDocs
- `Qdrant`: Same vector database configuration for storing processed embeddings

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
- `ModelContextProtocol`: Official MCP client library for protocol implementation
- `ModelContextProtocol.AspNetCore`: MCP server-side tools and attributes
- `OpenTelemetry.*`: Observability and telemetry collection
- `Microsoft.EntityFrameworkCore.Sqlite`: SQLite database provider for local storage
- `SQLitePCLRaw.provider.sqlite3`: System SQLite provider for Alpine Linux compatibility
- `Spectre.Console`: Rich console output and formatting
- `BoxOfYellow.ConsoleMarkdownRenderer`: Markdown rendering in console

### Vector Database Dependencies
- `Qdrant.Client`: Official Qdrant vector database client for .NET
- `Evanto.Mcp.Qdrant`: Unified repository layer for Qdrant operations
- `Evanto.Mcp.Embeddings`: Text embedding services and vector processing

### SupportDocs Specific Dependencies
- **Embedding Providers**: Ollama, OpenAI, or other text embedding services
- **Vector Database**: Qdrant for storing and searching document embeddings via unified repository
- **Text Processing**: Configurable chunking and overlap for document processing

## Project Structure
```
public-ai/
├── app/
│   ├── cmd-mcp-host/                    # Interactive MCP client application
│   └── cmd-vectorize/                   # PDF processing and vectorization utility
├── lib/
│   ├── Evanto.Mcp.Common/               # Shared settings and utilities
│   ├── Evanto.Mcp.Host/                 # Core MCP hosting logic
│   ├── Evanto.Mcp.Apps/                 # Application helper services
│   ├── Evanto.Mcp.Embeddings/           # Text embedding services and vector operations
│   ├── Evanto.Mcp.Qdrant/               # Unified Qdrant vector database repository
│   ├── Evanto.Mcp.Tools.SupportWizard/ # Support ticket tracking system
│   └── Evanto.Mcp.Tools.SupportDocs/   # Documentation management tools
├── srv/
│   ├── sse-mcp-server/                  # SSE-based MCP server
│   └── stdio-mcp-server/                # STDIO-based MCP server
├── db/
│   └── ev-supportwizard.db              # SQLite database files
├── pdfs/                                # PDF documents for vectorization
│   └── processed_files.json            # File tracking for vectorization
└── docs/
    ├── CLAUDE.md                        # This documentation file
    ├── CodingRules.md                   # C# coding standards
    ├── README-Docker.md                 # Docker deployment guide
    └── Directory.Packages.props         # Central package management
```

## MCP Tool Systems Overview

### SupportWizard Tool System
The SupportWizard system provides comprehensive support request tracking:

#### Features
- **Create Support Requests**: Customer email, name, channel, subject, description, topic, priority
- **Assign to Users**: Route tickets to appropriate support staff based on expertise
- **Status Management**: Track progress from New → InProgress → Resolved → Closed
- **Search & Filter**: Find tickets by customer, status, topic, priority, assignee
- **User Management**: Manage support staff and their topic specializations

#### Database Schema
- **SupportRequests Table**: Main ticket tracking with Guid primary keys
- **Users Table**: Support staff with topic assignments
- **Automatic Timestamps**: CreatedAt, UpdatedAt, ResolvedAt tracking
- **Foreign Key Relations**: Proper referential integrity between tables

### SupportDocs Tool System
The SupportDocs system provides intelligent document management and search:

#### Features
- **Document Embedding**: Convert documents to vector embeddings for semantic search
- **Vector Storage**: Store embeddings in Qdrant vector database
- **Semantic Search**: Find relevant documents based on semantic similarity
- **Multi-Format Support**: Handle various document formats and content types
- **Embedding Providers**: Support multiple embedding services (Ollama, OpenAI, etc.)

#### Architecture
- **Embedding Service**: Text-to-vector conversion using configurable models
- **Vector Database**: Qdrant for high-performance vector similarity search
- **Repository Pattern**: Abstracted data access for embeddings and documents
- **Configurable Chunking**: Adjustable text chunk sizes and overlap settings

### Evanto.Mcp.Qdrant Library
A unified repository library for Qdrant vector database operations:

#### Features
- **Unified Document Model**: Single `EvDocument` model for all vector operations
- **Repository Pattern**: `IEvDocumentRepository` interface with consistent API
- **Multiple Search Methods**: Vector search, text-based search, and combined queries
- **Dependency Injection**: Easy registration with `AddQdrantDocumentRepository()`
- **Cross-Project Compatibility**: Replaces individual Qdrant implementations

#### Key Components
- **Models/EvDocument.cs**: Unified document model with metadata and vector storage
- **Contracts/IEvDocumentRepository.cs**: Repository interface for document operations
- **Repository/EvDocumentRepository.cs**: Core implementation with Qdrant integration
- **Extensions/EvQdrantExtensions.cs**: Dependency injection and service registration

#### Migration Benefits
- **Code Consolidation**: Eliminated duplicate Qdrant access code from multiple projects
- **Consistent API**: Single interface for all document and vector operations
- **Reduced Dependencies**: Projects no longer need direct Qdrant.Client references
- **Better Maintainability**: Centralized vector database logic in one library

## Standalone Applications

### cmd-vectorize Utility
A command-line application for batch processing PDF documents into vector embeddings:

#### Features
- **PDF Text Extraction**: Extract text content from PDF files using iText7
- **Text Chunking**: Split documents into configurable chunks with overlap
- **Vector Embeddings**: Convert text chunks to embeddings using Ollama or other providers
- **Vector Storage**: Store embeddings in Qdrant vector database
- **File Tracking**: JSON-based tracking to avoid reprocessing files
- **Batch Processing**: Process multiple PDFs in a single run

#### Use Cases
- **Initial Setup**: Populate vector database with existing documentation
- **Bulk Processing**: Process large document collections offline
- **Scheduled Updates**: Run periodically to process new documents
- **Development**: Test embedding and vectorization workflows

#### Dependencies
- **iText7**: PDF text extraction and processing
- **OllamaSharp**: Integration with Ollama embedding service
- **Qdrant.Client**: Vector database connectivity
- **Evanto.Mcp.Embeddings**: Shared embedding service abstractions