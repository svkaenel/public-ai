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

### Library Organization Philosophy

The library structure follows a **clean architecture approach** with clear separation of concerns:

- **Core Libraries**: Pure business logic with minimal external dependencies
- **External Tool Integration Libraries**: Thin abstraction layers over external tools (iText7, Qdrant)
- **MCP Tool Libraries**: Domain-specific implementations using core and integration libraries
- **Applications**: Composition root that combines libraries to create executable programs

This approach provides:
- **Testability**: Easy to mock external dependencies via abstraction layers
- **Maintainability**: Clear boundaries between business logic and external tools
- **Flexibility**: Easy to swap implementations (e.g., different PDF processors)
- **Dependency Management**: External tools are isolated to specific libraries

### Core Components

**Multi-Layer Architecture:**
1. **Console Applications** (app/) - Standalone command-line tools:
   - **cmd-mcp-host** - Interactive MCP client with AI chat integration
   - **cmd-vectorize** - PDF processing and vectorization utility
2. **Core Libraries** (lib/) - Business logic and infrastructure:
   - **Evanto.Mcp.Host** - Core MCP hosting logic, factories, and testing framework
   - **Evanto.Mcp.Common** - Shared configuration, settings, and common utilities
   - **Evanto.Mcp.Apps** - Application helper services and shared app functionality
   - **Evanto.Mcp.Embeddings** - Multi-provider text embedding services using Microsoft.Extensions.AI
3. **External Tool Integration Libraries** (lib/) - Wrappers for external dependencies:
   - **Evanto.Mcp.Pdfs** - PDF text extraction services using iText7
   - **Evanto.Mcp.QdrantDB** - Unified Qdrant vector database repository and document management
4. **MCP Tool Libraries** (lib/) - Specialized tool implementations:
   - **Evanto.Mcp.Tools.SupportWizard** - Support request tracking with SQLite database
   - **Evanto.Mcp.Tools.SupportDocs** - Support documentation management tools
5. **MCP Server Implementations** (srv/) - Standalone MCP servers:
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
- `ApiKey`: Authentication credentials (can be overridden by environment variables)
- `DefaultModel`: Default model to use
- `AvailableModels`: List of supported models

### Environment Variable Overrides for API Keys
API keys in the ChatClients configuration can be overridden using environment variables for better security:

**Supported Environment Variables:**
- `OPENAI_API_KEY`: Overrides API key for OpenAI provider
- `IONOS_API_KEY`: Overrides API key for Ionos provider
- `AZURE_API_KEY`: Overrides API key for Azure provider
- `AZUREOAI_API_KEY`: Overrides API key for AzureOAI provider
- `LMSTUDIO_API_KEY`: Overrides API key for LMStudio provider
- `OLLAMA_API_KEY`: Overrides API key for Ollama provider

**Usage Options:**

1. **Environment Variables (System-wide):**
```bash
# Set environment variables in shell
export OPENAI_API_KEY="***REMOVED***your-openai-key"
export IONOS_API_KEY="your-ionos-token"

# Run the application
dotnet run --project app/cmd-mcp-host
```

2. **Inline Environment Variables:**
```bash
# Run with environment variables inline
OPENAI_API_KEY="***REMOVED***your-key" dotnet run --project app/cmd-mcp-host
```

3. **.env File (Recommended):**
```bash
# Copy example file and customize
cp .env.example .env

# Edit .env file with your API keys
nano .env

# Run application (automatically loads .env)
dotnet run --project app/cmd-mcp-host
```

4. **Docker Compose with .env:**
```bash
# Copy and customize .env file
cp .env.example .env

# Run with Docker Compose (automatically uses .env)
docker-compose up -d
```

**Security Benefits:**
- **No API keys in source code**: API keys can be removed from appsettings.json files
- **Git safety**: .env files are automatically ignored by git (.gitignore)
- **Environment separation**: Different keys for development, staging, and production
- **Industry standard**: Follows 12-factor app methodology for configuration
- **Docker integration**: Seamless integration with Docker Compose environments

**File Priority (highest to lowest):**
1. **Command line environment variables** (highest priority)
2. **System environment variables**
3. **.env file variables** 
4. **appsettings.json values** (lowest priority)

**Important Notes:**
- The .env file is automatically searched for in the project root directory
- .env files are automatically ignored by git (already in .gitignore)
- Use .env.example as a template for creating your .env file
- All applications and Docker containers automatically support .env file loading

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

### External Tool Dependencies
#### PDF Processing Dependencies
- `iText7.Core`: PDF text extraction and document processing
- `Evanto.Mcp.Pdfs`: Abstraction layer for PDF text extraction services

#### Vector Database Dependencies
- `Qdrant.Client`: Official Qdrant vector database client for .NET
- `Evanto.Mcp.QdrantDB`: Unified repository layer for Qdrant operations
- `Evanto.Mcp.Embeddings`: Multi-provider text embedding services using Microsoft.Extensions.AI

### SupportDocs Specific Dependencies
- **Evanto.Mcp.Embeddings**: Multi-provider text embedding services using Microsoft.Extensions.AI
- **Evanto.Mcp.QdrantDB**: Vector database repository for document storage and search
- **Embedding Providers**: Multi-provider support (OpenAI, Azure, Ollama, LMStudio, Ionos) via Microsoft.Extensions.AI
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
│   ├── Evanto.Mcp.Pdfs/                 # PDF text extraction services (iText7 wrapper)
│   ├── Evanto.Mcp.QdrantDB/             # Unified Qdrant vector database repository
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
- **Multi-Provider Support**: Unified interface for OpenAI, Azure, Ollama, LMStudio, and Ionos embedding services

#### Architecture
- **Embedding Service**: Text-to-vector conversion using configurable models (via Evanto.Mcp.Embeddings)
- **Vector Database**: Qdrant for high-performance vector similarity search (via Evanto.Mcp.QdrantDB)
- **Repository Pattern**: Abstracted data access for embeddings and documents
- **Configurable Chunking**: Adjustable text chunk sizes and overlap settings

#### External Dependencies
- **Qdrant Database**: High-performance vector similarity search engine
- **Multiple Embedding Providers**: OpenAI, Azure, Ollama, LMStudio, and Ionos via Microsoft.Extensions.AI

### Evanto.Mcp.Pdfs Library
A PDF processing abstraction library that wraps iText7 for text extraction:

#### Features
- **PDF Text Extraction**: Extract raw text content from PDF documents
- **Service Abstraction**: Clean interface that hides iText7 complexity
- **Error Handling**: Robust error handling for corrupted or protected PDFs
- **Dependency Injection**: Easy registration with `AddPdfTextExtractor()`
- **Performance Optimized**: Efficient text extraction for large documents

#### Key Components
- **Contracts/IEvPdfExtractorService.cs**: Service interface for PDF text extraction
- **Services/EvPdfTextExtractorService.cs**: iText7-based implementation
- **Extensions/EvPdfExtractorExtensions.cs**: Dependency injection registration

#### External Dependencies
- **iText7.Core**: Commercial-grade PDF processing library
- Provides enterprise-level PDF text extraction capabilities
- Handles complex PDF structures, fonts, and encodings

### Evanto.Mcp.Embeddings Library (V-0.2.0 Refactored)
A comprehensive multi-provider text embedding library using Microsoft.Extensions.AI abstractions:

#### Features
- **Multi-Provider Support**: Unified interface for OpenAI, Azure, Ollama, LMStudio, and Ionos embedding services
- **Microsoft.Extensions.AI Integration**: Built on standardized AI abstractions for consistency and testability
- **Automatic Performance Enhancements**: Built-in caching, rate limiting, and OpenTelemetry integration
- **Configuration-Driven**: Provider selection and settings managed through configuration files
- **Connection Testing**: Built-in validation of provider connectivity and model availability
- **Memory Efficient**: Uses `ReadOnlyMemory<Single>` for optimal performance
- **Batch Processing**: Efficient handling of multiple text inputs with rate limiting

#### Architecture
- **Factory Pattern**: `EvEmbeddingGeneratorFactory` creates provider-specific clients
- **Service Abstraction**: `IEvEmbeddingService` provides consistent API regardless of provider
- **Dependency Injection**: Full DI support with automatic registration extensions
- **Settings Integration**: Deep integration with `EvEmbeddingSettings` configuration

#### Key Components
- **Contracts/IEvEmbeddingService.cs**: Main service interface for embedding operations
- **Services/EvEmbeddingService.cs**: Core service implementation with error handling
- **Factories/EvEmbeddingGeneratorFactory.cs**: Multi-provider factory with auto-enhancement
- **Extensions/EvEmbeddingExtensions.cs**: Dependency injection registration methods

#### Supported Providers
- **OpenAI**: Standard OpenAI embedding API
- **Azure OpenAI**: Azure-hosted OpenAI services
- **Azure AI Inference**: Azure AI inference endpoints
- **Ollama**: Local AI model hosting (both Microsoft.Extensions.AI and OllamaSharp)
- **LMStudio**: Local OpenAI-compatible server
- **Ionos**: OpenAI-compatible cloud service

#### External Dependencies
- **Microsoft.Extensions.AI**: Core AI abstractions and provider integrations
- **Azure.AI.OpenAI**: Azure OpenAI client library
- **Azure.AI.Inference**: Azure AI inference client
- **OllamaSharp**: Direct Ollama integration for legacy compatibility
- **Microsoft.Extensions.Caching.Memory**: Performance caching support

#### Configuration Structure
```json
{
  "Embeddings": {
    "ProviderName": "OpenAI",
    "DefaultModel": "text-embedding-ada-002",
    "Endpoint": "https://api.openai.com/v1",
    "ApiKey": "your-api-key",
    "ChunkSize": 1000,
    "ChunkOverlap": 200,
    "EmbeddingDimensions": 1536
  }
}
```

#### Migration Benefits
- **Provider Flexibility**: Easy switching between embedding providers without code changes
- **Unified Interface**: Single API for all embedding operations regardless of provider
- **Performance Optimization**: Built-in caching and telemetry reduce costs and improve monitoring
- **Reduced Dependencies**: Centralized embedding logic eliminates duplicate provider integrations
- **Better Testing**: Standardized interfaces improve unit testing and mocking capabilities

### Evanto.Mcp.QdrantDB Library
A unified repository library for Qdrant vector database operations:

#### Features
- **Unified Document Model**: Single `EvDocument` model for all vector operations
- **Repository Pattern**: `IEvDocumentRepository` interface with consistent API
- **Multiple Search Methods**: Vector search, text-based search, and combined queries
- **Advanced Filtering**: Support for date ranges, file names, and custom filters
- **Dependency Injection**: Easy registration with `AddQdrantDocumentRepository()`
- **Cross-Project Compatibility**: Replaces individual Qdrant implementations

#### Key Components
- **Models/EvDocument.cs**: Unified document model with metadata and vector storage
- **Models/EvDocumentSearchQuery.cs**: Flexible search query model with filtering
- **Models/EvDocumentSearchResult.cs**: Search result model with metadata
- **Contracts/IEvDocumentRepository.cs**: Repository interface for document operations
- **Repository/EvDocumentRepository.cs**: Core implementation with Qdrant integration
- **Extensions/EvQdrantExtensions.cs**: Dependency injection and service registration

#### External Dependencies
- **Qdrant.Client**: Official .NET client for Qdrant vector database
- Provides high-performance vector similarity search
- Supports advanced filtering, payloads, and metadata storage

#### Migration Benefits
- **Code Consolidation**: Eliminated duplicate Qdrant access code from multiple projects
- **Consistent API**: Single interface for all document and vector operations
- **Reduced Dependencies**: Projects no longer need direct Qdrant.Client references
- **Better Maintainability**: Centralized vector database logic in one library
- **Enhanced Features**: Advanced search queries and filtering capabilities

## Standalone Applications

### cmd-vectorize Utility
A command-line application for batch processing PDF documents into vector embeddings:

#### Features
- **PDF Text Extraction**: Extract text content from PDF files using iText7
- **Text Chunking**: Split documents into configurable chunks with overlap
- **Vector Embeddings**: Convert text chunks to embeddings using multi-provider support (OpenAI, Azure, Ollama, etc.)
- **Vector Storage**: Store embeddings in Qdrant vector database
- **File Tracking**: JSON-based tracking to avoid reprocessing files
- **Batch Processing**: Process multiple PDFs in a single run

#### Use Cases
- **Initial Setup**: Populate vector database with existing documentation
- **Bulk Processing**: Process large document collections offline
- **Scheduled Updates**: Run periodically to process new documents
- **Development**: Test embedding and vectorization workflows

#### Dependencies
- **Evanto.Mcp.Pdfs**: PDF text extraction services (wraps iText7)
- **Evanto.Mcp.Embeddings**: Multi-provider text embedding services using Microsoft.Extensions.AI  
- **Evanto.Mcp.QdrantDB**: Vector database repository and document management
- **Multiple Embedding Providers**: OpenAI, Azure, Ollama, LMStudio, and Ionos via Microsoft.Extensions.AI

#### External Tool Integration
- **iText7 (via Evanto.Mcp.Pdfs)**: Enterprise PDF text extraction
- **Qdrant (via Evanto.Mcp.QdrantDB)**: High-performance vector database
- **Multiple Embedding Providers**: OpenAI, Azure, Ollama, LMStudio, and Ionos for text embeddings