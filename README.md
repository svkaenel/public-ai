# Evanto MCP Host System

A comprehensive **Model Context Protocol (MCP)** host system built with .NET 9.0 that integrates multiple AI providers with support ticket management (as sample solution), document vectorization, and semantic search capabilities.

## Table of Contents

1. [Project Overview](#project-overview)
2. [System Architecture](#system-architecture)
3. [Project Structure](#project-structure)
4. [Core Libraries](#core-libraries)
5. [Applications](#applications)
6. [Prerequisites & Setup](#prerequisites--setup)
7. [Configuration Guide](#configuration-guide)
8. [Docker Setup & Deployment](#docker-setup--deployment)
9. [Getting Started Workflow](#getting-started-workflow)
10. [PDF Vectorization Guide](#pdf-vectorization-guide)
11. [Using the MCP Host Client](#using-the-mcp-host-client)
12. [MCP Tools Overview](#mcp-tools-overview)
13. [Development Guide](#development-guide)
14. [Troubleshooting](#troubleshooting)
15. [Advanced Topics](#advanced-topics)
16. [API Reference](#api-reference)
17. [Contributing](#contributing)
18. [License & Legal](#license--legal)

## Project Overview

### What is this project?

The Evanto MCP Host System is a demo .NET application that implements the **Model Context Protocol (MCP)** to create a unified interface for AI chat providers and specialized tools. It enables seamless integration between multiple AI providers (OpenAI, Azure, Ollama, etc.) and custom business tools for support ticket management and document processing.

### Key Capabilities

- **Multi-Provider AI Integration**: Support for OpenAI, Azure OpenAI, Ollama, LMStudio, and Ionos
- **Support Ticket Management**: Complete CRUD operations for support requests with SQLite storage
- **Document Vectorization**: PDF processing and semantic search using Qdrant vector database
- **Interactive Chat Client**: Command-line interface for AI conversations with tool integration
- **Containerized Deployment**: Docker Compose setup for easy deployment and scaling
- **Comprehensive Testing**: Built-in MCP server testing framework with automated parameter

### Target Audience

This system is designed for **C# developers** who want to:
- Integrate AI capabilities into their applications
- Build MCP-compliant tools and servers
- Implement semantic search and document processing
- Create multi-provider AI chat systems
- Deploy containerized AI infrastructure

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Evanto MCP Host System                       │
├─────────────────────────────────────────────────────────────────┤
│                     Applications Layer                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ cmd-mcp-host    │  │ cmd-vectorize   │  │ MCP Servers     │  │
│  │ (Interactive    │  │ (PDF            │  │ (SSE/STDIO)     │  │
│  │  Client)        │  │  Processing)    │  │                 │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                    Core Libraries Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ Evanto.Mcp.Host │  │ Evanto.Mcp.Apps │  │ Evanto.Mcp.     │  │
│  │ (Factories &    │  │ (App Helpers)   │  │ Common          │  │
│  │  Testing)       │  │                 │  │ (Settings)      │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                External Integration Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ Evanto.Mcp.     │  │ Evanto.Mcp.     │  │ Evanto.Mcp.     │  │
│  │ Embeddings      │  │ Pdfs            │  │ QdrantDB        │  │
│  │ (Multi-Provider)│  │ (iText7 Wrapper)│  │ (Vector DB)     │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                     MCP Tools Layer                             │
│  ┌─────────────────┐  ┌─────────────────┐                       │
│  │ SupportWizard   │  │ SupportDocs     │                       │
│  │ (Ticket System) │  │ (Doc Search)    │                       │
│  └─────────────────┘  └─────────────────┘                       │
└─────────────────────────────────────────────────────────────────┘
```

### Core Components

1. **Applications**: Standalone executable programs
2. **Core Libraries**: Business logic and infrastructure
3. **External Integration**: Wrappers for external dependencies
4. **MCP Tools**: Domain-specific tool implementations

### Technology Stack

- **.NET 9.0**: Commonly used C# features with nullable reference types
- **Microsoft.Extensions.AI**: Unified AI provider abstractions
- **Model Context Protocol**: Official MCP client/server implementation
- **Entity Framework Core**: Database abstraction with SQLite
- **OpenTelemetry**: Observability and telemetry
- **Docker & Docker Compose**: Containerization and orchestration
- **iText7**: PDF processing and text extraction
- **Qdrant**: Vector database for semantic search

### Design Patterns

- **Factory Pattern**: AI client and MCP client creation
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Clean Architecture**: Separation of concerns across layers
- **Configuration-Driven**: Extensive use of appsettings.json

## Project Structure

```
public-ai/
├── app/                                     # Standalone Applications
│   ├── cmd-mcp-host/                        # Interactive MCP client
│   │   ├── Program.cs                       # Main entry point
│   │   ├── appsettings.json                 # Client configuration
│   │   └── system-prompt.txt                # AI system prompt
│   └── cmd-vectorize/                       # PDF vectorization utility
│       ├── Program.cs                       # Main entry point
│       ├── appsettings.json                 # Vectorization settings
│       └── Services/                        # Processing services
├── lib/                                     # Core Libraries
│   ├── Evanto.Mcp.Common/                   # Shared utilities
│   │   ├── Settings/                        # Configuration models
│   │   ├── Mcp/                             # MCP base classes
│   │   └── Extensions/                      # Extension methods
│   ├── Evanto.Mcp.Host/                     # MCP hosting infrastructure
│   │   ├── Factories/                       # Factory implementations
│   │   ├── Tests/                           # Testing framework
│   │   └── Models/                          # Core models
│   ├── Evanto.Mcp.Apps/                     # Application helpers
│   │   ├── EvBaseAppHelper.cs               # Base application logic
│   │   ├── EvCmdAppHelper.cs                # Command app helpers
│   │   └── EvSrvAppHelper.cs                # Server app helpers
│   ├── Evanto.Mcp.Embeddings/               # Text embedding services
│   │   ├── Factories/                       # Multi-provider factory
│   │   ├── Services/                        # Embedding implementations
│   │   └── Extensions/                      # DI extensions
│   ├── Evanto.Mcp.Pdfs/                     # PDF processing
│   │   ├── Services/                        # iText7 wrapper
│   │   └── Extensions/                      # DI extensions
│   ├── Evanto.Mcp.QdrantDB/                 # Vector database
│   │   ├── Repository/                      # Qdrant repository
│   │   ├── Models/                          # Document models
│   │   └── Extensions/                      # DI extensions
│   ├── Evanto.Mcp.Tools.SupportWizard/      # Support ticket system
│   │   ├── Tools/                           # MCP tool implementations
│   │   ├── Repository/                      # Database repository
│   │   ├── Models/                          # Entity models
│   │   └── Context/                         # EF Core context
│   └── Evanto.Mcp.Tools.SupportDocs/        # Document search tools
│       ├── Tools/                           # MCP tool implementations
│       └── Extensions/                      # DI extensions
├── srv/                                     # MCP Servers
│   ├── sse-mcp-server/                      # SSE-based MCP server
│   │   ├── Program.cs                       # ASP.NET Core app
│   │   ├── appsettings.json                 # Server configuration
│   │   └── Dockerfile                       # Container definition
│   └── stdio-mcp-server/                    # STDIO-based MCP server
│       ├── Program.cs                       # Console host app
│       ├── appsettings.json                 # Server configuration
│       └── Dockerfile                       # Container definition
├── db/                                      # Database files
│   └── ev-supportwizard.db                  # SQLite database
├── pdfs/                                    # PDF documents
│   └── processed_files.json                 # File tracking
├── run/                                     # Runtime configurations
│   ├── sse/appsettings.json                 # SSE server config
│   └── stdio/appsettings.json               # STDIO server config
├── Directory.Packages.props                 # Central package management
├── docker-compose.yaml                      # Container orchestration
├── .env.example                             # Environment variables template
└── CLAUDE.md                                # AI assistant instructions
```

## Core Libraries

### Evanto.Mcp.Host
**Purpose**: Core MCP hosting infrastructure with factories and testing framework

**Key Components**:
- `EvMcpClientFactory`: Creates MCP clients for different transport types (STDIO, SSE, HTTP)
- `EvChatClientFactory`: Creates AI chat clients for multiple providers
- `EvMcpServerTester`: Comprehensive testing framework for MCP servers and tools

**Usage Example**:
```csharp
// Create MCP client
var mcpClient = await mcpClientFactory.CreateAsync(serverSettings);

// Create chat client
var chatClient = chatClientFactory.Create("OpenAI");

// Test MCP server
var testResult = await mcpTester.TestServerAsync(serverSettings);
```

### Evanto.Mcp.Common
**Purpose**: Shared configuration models, settings, and utilities

**Key Components**:
- `EvHostAppSettings`: Main application configuration
- `EvChatClientSettings`: AI provider configurations
- `EvMcpServerSettings`: MCP server configurations
- `EvMcpToolBase`: Base class for MCP tool implementations

### Evanto.Mcp.Apps
**Purpose**: Application helper services and shared functionality

**Key Components**:
- `EvBaseAppHelper`: Common application initialization
- `EvCmdAppHelper`: Command-line application helpers
- `EvSrvAppHelper`: Server application helpers

### Evanto.Mcp.Embeddings
**Purpose**: Multi-provider text embedding services using Microsoft.Extensions.AI

**Key Features**:
- **Multi-Provider Support**: OpenAI, Azure, Ollama, LMStudio, Ionos
- **Unified Interface**: Single API regardless of provider
- **Performance Optimization**: Built-in caching and rate limiting
- **Configuration-Driven**: Provider selection via settings

**Usage Example**:
```csharp
// Register embedding service
services.AddEmbeddingService(settings);

// Use embedding service
var embeddings = await embeddingService.GenerateEmbeddingsAsync(texts);
```

### Evanto.Mcp.Pdfs
**Purpose**: PDF text extraction services using iText7

**Key Features**:
- **Enterprise PDF Processing**: Handles complex PDF structures
- **Service Abstraction**: Clean interface hiding iText7 complexity
- **Error Handling**: Robust handling of corrupted PDFs
- **Performance Optimized**: Efficient text extraction

**Usage Example**:
```csharp
// Register PDF service
services.AddPdfTextExtractor();

// Extract text from PDF
var text = await pdfExtractor.ExtractTextAsync(pdfPath);
```

### Evanto.Mcp.QdrantDB
**Purpose**: Unified repository for Qdrant vector database operations

**Key Features**:
- **Unified Document Model**: Single `EvDocument` for all operations
- **Advanced Search**: Vector, text, and combined search queries
- **Metadata Support**: Rich document metadata and filtering
- **Repository Pattern**: Clean data access abstraction

**Usage Example**:
```csharp
// Register Qdrant repository
services.AddQdrantDocumentRepository(settings);

// Store document
await repository.StoreDocumentAsync(document);

// Search documents
var results = await repository.SearchDocumentsAsync(query);
```

### Evanto.Mcp.Tools.SupportWizard
**Purpose**: Support ticket management system with SQLite database

**Key Features**:
- **Complete CRUD Operations**: Create, read, update, delete support requests
- **User Management**: Support staff with topic assignments
- **Status Tracking**: Ticket lifecycle management
- **Entity Framework Core**: Code-first database approach

**Database Schema**:
```sql
-- Support Requests
CREATE TABLE SupportRequests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CustomerEmail TEXT NOT NULL,
    CustomerName TEXT NOT NULL,
    Subject TEXT NOT NULL,
    Description TEXT NOT NULL,
    Status INTEGER NOT NULL,
    Priority INTEGER NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);

-- Users
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    Topic TEXT NOT NULL,
    IsActive BOOLEAN NOT NULL
);
```

### Evanto.Mcp.Tools.SupportDocs
**Purpose**: Document search and management with semantic similarity

**Key Features**:
- **Semantic Search**: Find documents by meaning, not just keywords
- **Document Management**: Store and organize documentation
- **Vector Integration**: Uses Qdrant for high-performance search
- **Multi-Provider Embeddings**: Flexible embedding provider support

## Applications

### cmd-mcp-host
**Purpose**: Interactive MCP client with AI chat integration

**Key Features**:
- **Multi-Provider Chat**: Switch between OpenAI, Azure, Ollama, etc.
- **MCP Tool Integration**: Access SupportWizard and SupportDocs tools
- **Interactive Interface**: Rich console experience with Spectre.Console
- **Configuration Management**: Supports multiple AI providers simultaneously

**Usage**:
```bash
# Run interactive client
dotnet run --project app/cmd-mcp-host

# Show help
dotnet run --project app/cmd-mcp-host -- --help

# List available providers
dotnet run --project app/cmd-mcp-host -- --list

# Run server tests
dotnet run --project app/cmd-mcp-host -- --test
```

### cmd-vectorize
**Purpose**: PDF processing and vectorization utility

**Key Features**:
- **Batch PDF Processing**: Process multiple PDFs in one run
- **Text Chunking**: Configurable chunk sizes and overlap
- **Vector Storage**: Store embeddings in Qdrant database
- **File Tracking**: Avoid reprocessing with JSON tracking file

**Usage**:
```bash
# Process PDFs in configured directory
dotnet run --project app/cmd-vectorize

# Configuration in appsettings.json:
{
    "PdfDirectory": "../../../../../pdfs",
    "TrackingFilePath": "../../../../../pdfs/processed_files.json"
}
```

### MCP Servers

#### sse-mcp-server
**Purpose**: HTTP/SSE-based MCP server for web integration

**Key Features**:
- **ASP.NET Core**: Modern web server infrastructure
- **Server-Sent Events**: Real-time communication
- **Health Checks**: Built-in endpoint monitoring
- **Docker Support**: Containerized deployment

#### stdio-mcp-server
**Purpose**: STDIO-based MCP server for command-line integration

**Key Features**:
- **Standard I/O**: Works with any MCP client
- **Console Host**: Lightweight deployment
- **Docker Support**: Container-based execution
- **Interactive Mode**: TTY support for debugging

## Prerequisites & Setup

### System Requirements

- **.NET 9.0 SDK** or later
- **Docker** 20.10 or later
- **Docker Compose** 2.0 or later
- **Git** for source control

### Required External Services

1. **Qdrant Vector Database**: Provided via Docker Compose
2. **AI Provider API Keys**: At least one of:
   - OpenAI API key
   - Azure OpenAI credentials
   - Ollama (local installation)
   - LMStudio (local installation)
   - Ionos AI API key

### Development Tools (Recommended)

- **Visual Studio 2024** or **VS Code** with C# extension
- **Docker Desktop** for container management
- **Postman** or similar for API testing
- **DB Browser for SQLite** for database inspection

### Installation Steps

1. **Clone the repository**:
```bash
git clone <repository-url>
cd public-ai
```

2. **Verify .NET installation**:
```bash
dotnet --version
# Should show 9.0.x or later
```

3. **Restore packages**:
```bash
dotnet restore
```

4. **Build the solution**:
```bash
dotnet build
```

5. **Set up environment variables** (see Configuration Guide)

## Configuration Guide

### Environment Variables Setup

The system uses environment variables for API keys and sensitive configuration. This approach keeps secrets out of source code and supports different environments.

#### Step 1: Create .env file

```bash
# Copy the example file
cp .env.example .env

# Edit with your values
nano .env  # or your preferred editor
```

#### Step 2: Configure API Keys

Edit `.env` file with your API keys:

```bash
# OpenAI API Key
OPENAI_API_KEY=***REMOVED***your-openai-api-key-here

# Ionos AI API Key (JWT Token)
IONOS_API_KEY=your-ionos-jwt-token-here

# Azure AI API Key
AZURE_API_KEY=your-azure-ai-api-key-here

# Azure OpenAI API Key
AZUREOAI_API_KEY=your-azure-openai-api-key-here

# LMStudio API Key (usually empty for local)
LMSTUDIO_API_KEY=

# Ollama API Key (usually empty for local)
OLLAMA_API_KEY=

# Docker Compose Configuration
SSE_PORT=5561
SSE_CONFIG_PATH=./run/sse/appsettings.json
STDIO_CONFIG_PATH=./run/stdio/appsettings.json
```

#### Step 3: Verify Configuration

The system will automatically load environment variables in this priority order:
1. Command line environment variables (highest priority)
2. System environment variables
3. .env file variables
4. appsettings.json values (lowest priority)

### AI Provider Configuration

The `ChatClients` section in `appsettings.json` configures AI providers:

```json
{
  "DefaultChatClient": "OpenAI",
  "ChatClients": [
    {
      "ProviderName": "OpenAI",
      "Endpoint": "https://api.openai.com/v1",
      "DefaultModel": "o4-mini",
      "AvailableModels": [
        "o4-mini",
        "gpt-4.1-mini",
        "gpt-4.1",
        "o1"
      ]
    },
    {
      "ProviderName": "Azure",
      "Endpoint": "https://your-resource.services.ai.azure.com/models",
      "DefaultModel": "DeepSeek-R1",
      "AvailableModels": [
        "DeepSeek-R1"
      ]
    },
    {
      "ProviderName": "Ollama",
      "Endpoint": "http://localhost:11434",
      "DefaultModel": "qwen3:14b",
      "AvailableModels": [
        "qwen3:4b",
        "qwen3:14b",
        "gemma3:12b"
      ]
    }
  ]
}
```

### Database Configuration

SQLite databases are configured via connection strings:

```json
{
  "ConnectionStrings": {
    "SupportWizardDB": "Filename=db/ev-supportwizard.db"
  }
}
```

### OpenTelemetry Configuration

Configure observability and telemetry:

```json
{
  "Telemetry": {
    "Enabled": true,
    "ServiceName": "cmd-mcp-host",
    "OtlpEndpoint": "http://localhost:4317",
    "EnableConsoleExporter": false,
    "EnableOtlpExporter": true,
    "LogSensitiveData": false,
    "ActivitySources": [
      "Microsoft.Extensions.AI"
    ]
  }
}
```

## Docker Setup & Deployment

### Docker Compose Overview

The system uses Docker Compose to orchestrate multiple services:

- **qdrantdb**: Vector database for document embeddings
- **aspire-dashboard**: .NET Aspire dashboard for telemetry
- **sse-mcp-server**: HTTP/SSE MCP server
- **stdio-mcp-server**: STDIO MCP server

### Environment Preparation

1. **Set up environment variables**:
```bash
cp .env.example .env
# Edit .env with your API keys
```

2. **Create runtime configuration directories**:
```bash
mkdir -p run/sse run/stdio
```

3. **Copy configuration files**:
```bash
# Copy example configurations
cp app/cmd-mcp-host/appsettings.json run/sse/
cp app/cmd-mcp-host/appsettings.json run/stdio/
```

### Step-by-Step Deployment

#### Step 1: Build and Start Services

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Check service status
docker-compose ps
```

Build MCP servers separately (from public-ai directory):

```bash
# SSE MCP Server
docker build -f srv/sse-mcp-server/Dockerfile -t sse-mcp-server .

# STDIO MCP Server  
docker build -f srv/stdio-mcp-server/Dockerfile -t stdio-mcp-server .
```

#### Step 2: Verify Services

```bash
# Check Qdrant is running
curl http://localhost:6335/

# Check SSE MCP server
curl http://localhost:5561/

# Check Aspire dashboard
# Open browser: http://localhost:4316
```

#### Step 3: Initialize Database

The MCP servers will automatically create and migrate the SQLite database on first run.

### Service Configuration

#### Qdrant Vector Database

- **REST API**: `http://localhost:6335`
- **gRPC**: `http://localhost:6336`
- **Data Storage**: Docker volume `qdrant_data`

#### SSE MCP Server

- **Endpoint**: `http://localhost:5561`
- **Transport**: HTTP with Server-Sent Events
- **Configuration**: `./run/sse/appsettings.json`
- **Database**: Docker volume `sse_db_data`

#### STDIO MCP Server

- **Transport**: Standard Input/Output
- **Configuration**: `./run/stdio/appsettings.json`
- **Database**: Docker volume `stdio_db_data`

#### Aspire Dashboard

- **Endpoint**: `http://localhost:4316`
- **Purpose**: OpenTelemetry visualization
- **Authentication**: Token-based (see docker-compose.yaml)

### Docker Management Commands

```bash
# Stop all services
docker-compose down

# Rebuild and restart
docker-compose up -d --build

# View specific service logs
docker-compose logs -f sse-mcp-server

# Remove all data (WARNING: destroys databases)
docker-compose down -v

# Scale services
docker-compose up -d --scale sse-mcp-server=2
```

## Getting Started Workflow

Follow these steps to get the system running from scratch:

### Step 1: Clone and Build

```bash
# Clone repository
git clone <repository-url>
cd public-ai

# Build solution
dotnet build

# Run tests
dotnet test
```

### Step 2: Configure Environment

```bash
# Create environment file
cp .env.example .env

# Edit with your API keys
nano .env

# Set at least one AI provider:
# OPENAI_API_KEY=***REMOVED***your-key
# or
# OLLAMA_API_KEY=  # for local Ollama
```

### Step 3: Start Docker Services

```bash
# Start infrastructure services
docker-compose up -d

# Verify services are running
docker-compose ps

# Check logs if needed
docker-compose logs -f
```

### Step 4: Populate Vector Database

```bash
# Place PDF files in pdfs/ directory
cp /path/to/your/docs/*.pdf pdfs/

# Run vectorization
dotnet run --project app/cmd-vectorize

# Check processing results
cat pdfs/processed_files.json
```

### Step 5: Test MCP Servers

```bash
# Test all MCP servers and tools
dotnet run --project app/cmd-mcp-host -- --test

# List available providers
dotnet run --project app/cmd-mcp-host -- --list
```

### Step 6: Run Interactive Client

```bash
# Start interactive chat client
dotnet run --project app/cmd-mcp-host

# Example conversation:
# > Hello, can you help me search for documents about embeddings?
# > Create a new support ticket for customer john@example.com
# > Show me all support tickets with high priority
```

### Verification Checklist

- [ ] All Docker services running (`docker-compose ps`)
- [ ] Qdrant accessible at `http://localhost:6335`
- [ ] SSE MCP server accessible at `http://localhost:5561`
- [ ] At least one AI provider configured
- [ ] PDFs processed and stored in vector database
- [ ] MCP server tests passing
- [ ] Interactive client starts successfully

### Helpful Docker commands

```bash
# Monitor logs
docker-compose logs -f

# List current environment variables for project
docker compose config --environment

# Clean up (be careful, other Docker ressources can be affected!)
docker system prune -a -f
docker builder prune -f
```
## PDF Vectorization Guide

### Purpose

The `cmd-vectorize` utility processes PDF documents and converts them into vector embeddings for semantic search. This enables the SupportDocs tool to find relevant documents based on meaning rather than just keywords.

### Configuration

Edit `app/cmd-vectorize/appsettings.json`:

```json
{
  "PdfDirectory": "../../../../../pdfs",
  "TrackingFilePath": "../../../../../pdfs/processed_files.json",
  "DefaultEmbeddingProvider": "OpenAI",
  "DefaultEmbeddingProviderAlt": "OllamaSharp",
  "EmbeddingProviders": [
    {
      "ProviderName": "OpenAI",
      "Endpoint": "https://api.openai.com/v1",
      "DefaultModel": "text-embedding-3-small",
      "EmbeddingDimensions": 1536
    },
    {
      "ProviderName": "Ollama",
      "Endpoint": "http://localhost:11434",
      "DefaultModel": "nomic-embed-text",
      "EmbeddingDimensions": 768
    }
  ],
  "Qdrant": {
    "QdrantEndpoint": "localhost",
    "CollectionName": "ev_support_documents",
    "QdrantPort": 6336,
    "VectorDimension": 1536
  }
}
```

### Usage Workflow

#### Step 1: Prepare PDF Files

```bash
# Create pdfs directory (if not exists)
mkdir -p pdfs

# Copy your PDF documents
cp /path/to/your/docs/*.pdf pdfs/

# Example structure:
# pdfs/
# ├── user-manual.pdf
# ├── api-documentation.pdf
# ├── troubleshooting-guide.pdf
# └── processed_files.json (auto-generated)
```

#### Step 2: Run Vectorization

```bash
# Process all PDFs in the directory
dotnet run --project app/cmd-vectorize

# Expected output:
# [2024-01-01 10:00:00] Starting PDF vectorization...
# [2024-01-01 10:00:01] Processing: user-manual.pdf
# [2024-01-01 10:00:05] Extracted 1,234 words, created 5 chunks
# [2024-01-01 10:00:10] Generated embeddings and stored in Qdrant
# [2024-01-01 10:00:10] Processing: api-documentation.pdf
# [2024-01-01 10:00:15] Skipping (already processed): troubleshooting-guide.pdf
# [2024-01-01 10:00:15] Vectorization complete!
```

#### Step 3: Verify Results

```bash
# Check tracking file
cat pdfs/processed_files.json

# Example content:
{
  "processed_files": [
    {
      "filename": "user-manual.pdf",
      "processed_at": "2024-01-01T10:00:10Z",
      "chunk_count": 5,
      "word_count": 1234
    }
  ]
}
```

### Processing Details

#### Text Extraction

The system uses **iText7** for robust PDF text extraction:

- **Enterprise-grade**: Handles complex PDF structures
- **Font support**: Processes various font types and encodings
- **Error handling**: Gracefully handles corrupted PDFs
- **Performance optimized**: Efficient memory usage

#### Text Chunking

Documents are split into manageable chunks:

- **Configurable size**: Default 1000 characters
- **Overlap**: Default 200 characters between chunks
- **Semantic boundaries**: Attempts to break at sentence boundaries
- **Metadata preservation**: Maintains document source information

#### Vector Generation

Text chunks are converted to embeddings:

- **Multi-provider support**: OpenAI, Ollama, Azure, etc.
- **Consistent dimensions**: Configured per provider
- **Batch processing**: Efficient API usage
- **Error resilience**: Retries on failure

#### Vector Storage

Embeddings are stored in Qdrant:

- **High performance**: Optimized for similarity search
- **Metadata**: Document name, chunk index, timestamps
- **Scalable**: Handles large document collections
- **Persistent**: Data survives container restarts

### File Tracking System

The `processed_files.json` file prevents reprocessing:

```json
{
  "processed_files": [
    {
      "filename": "document.pdf",
      "processed_at": "2024-01-01T10:00:00Z",
      "chunk_count": 10,
      "word_count": 2500,
      "file_size": 1048576,
      "file_hash": "sha256:abc123..."
    }
  ]
}
```

**Benefits**:
- **Incremental processing**: Only new files are processed
- **Resume capability**: Can restart after interruption
- **Audit trail**: Track processing history
- **Performance**: Avoids expensive reprocessing

### Troubleshooting

#### Common Issues

1. **PDF extraction fails**:
   - Check PDF is not password-protected
   - Verify PDF is not corrupted
   - Ensure sufficient disk space

2. **Embedding generation fails**:
   - Verify API key is correct
   - Check internet connection
   - Confirm provider endpoint is accessible

3. **Qdrant connection fails**:
   - Ensure Qdrant is running (`docker-compose ps`)
   - Check port configuration (6336 for gRPC)
   - Verify network connectivity

#### Debug Commands

```bash
# Check Qdrant status
curl http://localhost:6335/

# List Qdrant collections
curl http://localhost:6335/collections

# Check collection info
curl http://localhost:6335/collections/ev_support_documents

# Test embedding provider
dotnet run --project app/cmd-vectorize -- --test-embedding

# Force reprocessing (delete tracking file)
rm pdfs/processed_files.json
```

## Using the MCP Host Client

### Interactive Chat Interface

The `cmd-mcp-host` application provides a rich interactive experience for AI conversations with integrated MCP tools.

#### Starting the Client

```bash
# Run with default configuration
dotnet run --project app/cmd-mcp-host

# Show available options
dotnet run --project app/cmd-mcp-host -- --help

# List configured providers
dotnet run --project app/cmd-mcp-host -- --list

# Test MCP servers before starting
dotnet run --project app/cmd-mcp-host -- --test
```

#### Basic Usage

```
Welcome to Evanto MCP Host Client
Current Provider: OpenAI (o4-mini)
Available Tools: SupportWizard, SupportDocs
Type 'help' for commands, 'exit' to quit

> Hello, can you help me with support tickets?
```

### Available Commands

#### System Commands

```bash
# Get help
> help

# List all available providers
> /providers

# Switch AI provider
> /provider OpenAI
> /provider Azure
> /provider Ollama

# List available models for current provider
> /models

# Switch model
> /model o4-mini
> /model gpt-4.1

# Show current configuration
> /status

# Clear conversation history
> /clear

# Exit application
> exit
```

#### Tool Integration Commands

The client automatically integrates with MCP tools. You can use natural language to interact with them:

```bash
# SupportWizard tool examples
> "Create a new support ticket for customer john@example.com with subject 'Login issues'"
> "Show me all support tickets with high priority"
> "List all users who can handle technical issues"
> "Update support ticket ID 123 to resolved status"

# SupportDocs tool examples
> "Search for documentation about embeddings"
> "Find information about API authentication"
> "Look up troubleshooting guides for database connections"
```

### Provider Configuration

#### OpenAI

```json
{
  "ProviderName": "OpenAI",
  "Endpoint": "https://api.openai.com/v1",
  "DefaultModel": "o4-mini",
  "AvailableModels": [
    "o4-mini",
    "gpt-4.1-mini",
    "gpt-4.1",
    "o1"
  ]
}
```

**Environment Variable**: `OPENAI_API_KEY`

#### Azure OpenAI

```json
{
  "ProviderName": "AzureOAI",
  "Endpoint": "https://your-resource.cognitiveservices.azure.com/",
  "DefaultModel": "o4-mini",
  "AvailableModels": [
    "o4-mini",
    "o1"
  ]
}
```

**Environment Variable**: `AZUREOAI_API_KEY`

#### Ollama (Local)

```json
{
  "ProviderName": "Ollama",
  "Endpoint": "http://localhost:11434",
  "DefaultModel": "qwen3:14b",
  "AvailableModels": [
    "qwen3:4b",
    "qwen3:14b",
    "gemma3:12b"
  ]
}
```

**Setup**: Install and run Ollama locally, then pull models:
```bash
# Install Ollama
curl -fsSL https://ollama.ai/install.sh | sh

# Pull models
ollama pull qwen3:14b
ollama pull gemma3:12b
```

### Advanced Features

#### Conversation History

The client maintains conversation context:

```bash
> Tell me about our support system
AI: Our support system uses the SupportWizard tool to manage tickets...

> How many tickets do we have?
AI: Let me check the current tickets... [calls SupportWizard tool]
```

#### Multi-Step Operations

The AI can perform complex operations across multiple tools:

```bash
> Find documentation about embeddings and create a support ticket if there are issues

AI: I'll search for embedding documentation first...
    [searches SupportDocs]
    Found 3 relevant documents about embeddings.
    Would you like me to create a support ticket for any specific issues?
```

#### Rich Output

The client uses Spectre.Console for rich formatting:

- **Syntax highlighting** for code blocks
- **Tables** for structured data
- **Progress indicators** for long operations
- **Color coding** for different message types

### Configuration Options

#### System Prompt

Customize the AI behavior by editing `app/cmd-mcp-host/system-prompt.txt`:

```text
You are an AI assistant specialized in support ticket management and documentation search.
You have access to the following tools:
- SupportWizard: For managing support tickets and users
- SupportDocs: For searching documentation and knowledge base

Always be helpful, accurate, and provide actionable responses.
When creating support tickets, always ask for required information if not provided.
```

#### Application Settings

Configure client behavior in `app/cmd-mcp-host/appsettings.json`:

```json
{
  "DefaultChatClient": "OpenAI",
  "UseConsoleLogging": false,
  "Telemetry": {
    "Enabled": true,
    "ServiceName": "cmd-mcp-host"
  },
  "McpServers": [
    {
      "Name": "Evanto Support MCP Server (SSE)",
      "Url": "http://localhost:5561",
      "Enabled": true,
      "TimeoutSeconds": 30,
      "TransportType": "SSE"
    }
  ]
}
```

### Example Conversations

#### Support Ticket Management

```
> Create a support ticket for customer jane@example.com about login problems

AI: I'll create a support ticket for Jane. Let me gather the required information:

    Creating support ticket with:
    - Customer: jane@example.com
    - Subject: Login problems
    - Priority: Medium (default)
    - Status: New

    Would you like me to add more details or assign it to a specific user?

> Assign it to someone who handles authentication issues

AI: Let me find users who can handle authentication issues...
    
    Found 2 users who handle Technical topics:
    - John Smith (john@company.com)
    - Sarah Johnson (sarah@company.com)
    
    I'll assign it to John Smith. The ticket has been created with ID: 12345
```

#### Documentation Search

```
> How do I configure embeddings for different providers?

AI: Let me search our documentation for embedding configuration...

    Found 3 relevant documents:
    
    1. "Embedding Configuration Guide" (Score: 0.92)
       - Covers multi-provider setup
       - Includes OpenAI, Azure, and Ollama examples
    
    2. "API Provider Setup" (Score: 0.85)
       - Environment variable configuration
       - Authentication setup
    
    3. "Troubleshooting Embeddings" (Score: 0.78)
       - Common issues and solutions
       - Performance optimization
    
    Based on the documentation, here's how to configure embeddings...
```

## MCP Tools Overview

### SupportWizard Tool System

The SupportWizard system provides comprehensive support ticket management with SQLite database persistence.

#### Core Features

1. **Support Request Management**
   - Create, read, update, delete support tickets
   - Status tracking (New → InProgress → Resolved → Closed)
   - Priority management (Low, Medium, High, Critical)
   - Customer and contact information

2. **User Management**
   - Support staff database
   - Topic-based assignment
   - Skill-based routing

3. **Search and Filtering**
   - Find tickets by customer email/name
   - Filter by status, priority, topic
   - Date range queries

#### Database Schema

```sql
-- Support Requests Table
CREATE TABLE SupportRequests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CustomerEmail TEXT NOT NULL,
    CustomerName TEXT NOT NULL,
    Channel TEXT NOT NULL,          -- Email, Phone, Web, etc.
    Subject TEXT NOT NULL,
    Description TEXT NOT NULL,
    Topic TEXT NOT NULL,            -- Technical, Billing, General, etc.
    Priority INTEGER NOT NULL,      -- 0=Low, 1=Medium, 2=High, 3=Critical
    Status INTEGER NOT NULL,        -- 0=New, 1=InProgress, 2=Resolved, 3=Closed
    AssignedToUserId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    ResolvedAt DATETIME NULL,
    
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id)
);

-- Users Table
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    Topic TEXT NOT NULL,            -- Primary expertise area
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);
```

#### Available MCP Tools

1. **get_support_requests_for_customer_by_email**
   - Find all tickets for a specific customer
   - Input: customer email
   - Output: List of support requests

2. **get_support_requests_for_customer_by_name**
   - Find tickets by customer name
   - Input: customer name
   - Output: List of support requests

3. **get_support_requests_by_status**
   - Filter tickets by status
   - Input: status (New, InProgress, Resolved, Closed)
   - Output: List of support requests

4. **get_support_requests_by_priority**
   - Filter tickets by priority level
   - Input: priority (Low, Medium, High, Critical)
   - Output: List of support requests

5. **create_support_request**
   - Create new support ticket
   - Input: customer details, subject, description, topic, priority
   - Output: Created ticket with ID

6. **update_support_request_status**
   - Update ticket status
   - Input: ticket ID, new status
   - Output: Updated ticket information

7. **assign_support_request_to_user**
   - Assign ticket to support staff
   - Input: ticket ID, user ID
   - Output: Updated assignment

8. **get_users_by_topic**
   - Find users by expertise area
   - Input: topic
   - Output: List of qualified users

9. **create_user**
   - Add new support staff member
   - Input: name, email, topic, active status
   - Output: Created user with ID

#### Usage Examples

```csharp
// Create support ticket via MCP
var ticket = await mcpClient.CallToolAsync("create_support_request", new {
    customerEmail = "john@example.com",
    customerName = "John Doe",
    channel = "Email",
    subject = "Cannot access dashboard",
    description = "User reports 500 error when accessing main dashboard",
    topic = "Technical",
    priority = "High"
});

// Find tickets by status
var activeTickets = await mcpClient.CallToolAsync("get_support_requests_by_status", new {
    status = "InProgress"
});

// Assign ticket to user
var assignment = await mcpClient.CallToolAsync("assign_support_request_to_user", new {
    supportRequestId = "12345-67890-abcde",
    userId = "user-12345"
});
```

### SupportDocs Tool System

The SupportDocs system provides intelligent document search and management using vector embeddings and semantic similarity.

#### Core Features

1. **Semantic Search**
   - Find documents by meaning, not just keywords
   - Vector similarity search using embeddings
   - Contextual understanding of queries

2. **Document Management**
   - Store documents with metadata
   - Support for various content types
   - Version tracking and updates

3. **Multi-Provider Embeddings**
   - OpenAI, Azure, Ollama, LMStudio support
   - Configurable embedding dimensions
   - Fallback provider support

#### Architecture Components

1. **Embedding Service** (`Evanto.Mcp.Embeddings`)
   - Text-to-vector conversion
   - Multi-provider abstraction
   - Caching and optimization

2. **Vector Database** (`Evanto.Mcp.QdrantDB`)
   - High-performance similarity search
   - Metadata filtering
   - Scalable storage

3. **Document Repository**
   - Unified document model
   - Advanced search queries
   - Metadata management

#### Available MCP Tools

1. **get_infos_from_documentation**
   - Semantic search in document database
   - Input: search query
   - Output: Ranked list of relevant documents

2. **store_document**
   - Add new document to search index
   - Input: document content, metadata
   - Output: Stored document ID

3. **update_document**
   - Update existing document
   - Input: document ID, new content
   - Output: Updated document information

4. **delete_document**
   - Remove document from index
   - Input: document ID
   - Output: Deletion confirmation

#### Configuration

```json
{
  "Embeddings": {
    "ProviderName": "OpenAI",
    "DefaultModel": "text-embedding-3-small",
    "Endpoint": "https://api.openai.com/v1",
    "ChunkSize": 1000,
    "ChunkOverlap": 200,
    "EmbeddingDimensions": 1536
  },
  "Qdrant": {
    "QdrantEndpoint": "localhost",
    "CollectionName": "ev_support_documents",
    "QdrantPort": 6336,
    "SearchLimit": 10,
    "MinimumScore": 0.5,
    "VectorDimension": 1536
  }
}
```

#### Usage Examples

```csharp
// Search documentation
var results = await mcpClient.CallToolAsync("get_infos_from_documentation", new {
    query = "How to configure embeddings for multiple providers"
});

// Store new document
var stored = await mcpClient.CallToolAsync("store_document", new {
    title = "API Authentication Guide",
    content = "This guide explains how to authenticate with our API...",
    metadata = new {
        category = "API",
        version = "1.0",
        author = "Tech Team"
    }
});
```

#### Search Query Features

1. **Vector Search**
   - Semantic similarity based on embeddings
   - Finds conceptually related content
   - Handles synonyms and related terms

2. **Metadata Filtering**
   - Filter by document properties
   - Date ranges, categories, authors
   - Combined with vector search

3. **Hybrid Search**
   - Combine vector and text search
   - Boost specific terms
   - Balanced relevance scoring

#### Performance Optimization

1. **Caching**
   - Embedding cache for repeated queries
   - Document cache for frequent access
   - Configurable cache sizes

2. **Batch Processing**
   - Process multiple documents together
   - Efficient API usage
   - Parallel processing support

3. **Indexing**
   - Optimized vector indices
   - Metadata indexing
   - Incremental updates

## Development Guide

### Building the Solution

#### Prerequisites Check

```bash
# Verify .NET version
dotnet --version

# Check Docker
docker --version
docker-compose --version

# Verify Git
git --version
```

#### Build Commands

```bash
# Clean solution
dotnet clean

# Restore NuGet packages
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build app/cmd-mcp-host

# Build for release
dotnet build --configuration Release
```

#### Package Management

The solution uses **Central Package Management** via `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.AI" Version="9.6.0" />
    <PackageVersion Include="ModelContextProtocol" Version="0.3.0-preview.2" />
    <!-- ... other packages -->
  </ItemGroup>
</Project>
```

**Benefits**:
- Consistent versions across all projects
- Easier dependency management
- Reduced package conflicts

### Running Tests

#### Unit Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests for specific project
dotnet test lib/Evanto.Mcp.Host.Tests

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests matching pattern
dotnet test --filter "TestCategory=Integration"
```

#### Integration Tests

```bash
# Test MCP servers
dotnet run --project app/cmd-mcp-host -- --test

# Test specific server
dotnet run --project app/cmd-mcp-host -- --test --server "SSE"

# Test with timeout
dotnet run --project app/cmd-mcp-host -- --test --timeout 30
```

#### Docker Tests

```bash
# Test Docker services
docker-compose up -d
docker-compose ps

# Test service endpoints
curl http://localhost:6335/  # Qdrant
curl http://localhost:5561/  # SSE MCP server

# Run container tests
docker-compose exec sse-mcp-server dotnet test
```

### Development Workflow

#### Daily Development

1. **Start development environment**:
```bash
# Start Docker services
docker-compose up -d qdrantdb aspire-dashboard

# Build and run local services
dotnet build
dotnet run --project app/cmd-mcp-host
```

2. **Make changes**:
   - Edit code in your preferred IDE
   - Follow coding standards in `CodingRules.md`
   - Write tests for new functionality

3. **Test changes**:
```bash
# Run unit tests
dotnet test

# Test MCP integration
dotnet run --project app/cmd-mcp-host -- --test

# Manual testing
dotnet run --project app/cmd-mcp-host
```

4. **Commit changes**:
```bash
git add .
git commit -m "feat: add new MCP tool for document search"
git push origin feature/new-tool
```

#### Adding New MCP Tools

1. **Create tool library**:
```bash
mkdir lib/Evanto.Mcp.Tools.NewTool
cd lib/Evanto.Mcp.Tools.NewTool
dotnet new classlib
```

2. **Add project reference**:
```xml
<ProjectReference Include="../Evanto.Mcp.Common/Evanto.Mcp.Common.csproj" />
```

3. **Implement tool class**:
```csharp
[McpServerToolType]
public class NewMcpTool : EvMcpToolBase
{
    [McpServerTool, Description("Description of the tool")]
    public async Task<String> DoSomething(String parameter)
    {
        // Implementation
        return await ExecuteAsync(
            () => repository.DoSomethingAsync(parameter),
            result => result == null,
            result => result,
            "No results found");
    }
}
```

4. **Add dependency injection**:
```csharp
public static class NewToolExtensions
{
    public static IServiceCollection AddNewTool(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<NewMcpTool>();
        // Add other dependencies
        return services;
    }
}
```

5. **Register in MCP server**:
```csharp
// In Program.cs
builder.Services
    .AddNewTool(configuration)
    .AddMcpServer()
    .WithNewToolMcpTools();
```

### Debugging

#### Application Debugging

1. **Visual Studio**:
   - Set breakpoints in code
   - Use F5 to start debugging
   - Step through code execution

2. **VS Code**:
   - Install C# extension
   - Use launch.json configuration
   - Set breakpoints and debug

3. **Command Line**:
```bash
# Debug with verbose logging
dotnet run --project app/cmd-mcp-host --verbosity diagnostic

# Debug specific configuration
dotnet run --project app/cmd-mcp-host --configuration Debug
```

#### Docker Debugging

1. **Container logs**:
```bash
docker-compose logs -f sse-mcp-server
docker-compose logs -f qdrantdb
```

2. **Container shell access**:
```bash
docker-compose exec sse-mcp-server /bin/bash
docker-compose exec qdrantdb /bin/bash
```

3. **Port debugging**:
```bash
# Check port usage
netstat -tlnp | grep 5561
lsof -i :5561

# Test connectivity
telnet localhost 5561
curl -v http://localhost:5561/
```

#### Database Debugging

1. **SQLite inspection**:
```bash
# Open database
sqlite3 db/ev-supportwizard.db

# List tables
.tables

# Show schema
.schema SupportRequests

# Query data
SELECT * FROM SupportRequests LIMIT 10;
```

2. **Qdrant inspection**:
```bash
# Check collections
curl http://localhost:6335/collections

# Collection info
curl http://localhost:6335/collections/ev_support_documents

# Search vectors
curl -X POST http://localhost:6335/collections/ev_support_documents/points/search \
  -H "Content-Type: application/json" \
  -d '{"vector": [0.1, 0.2, 0.3], "limit": 5}'
```

### Performance Optimization

#### Profiling

1. **dotTrace** (JetBrains):
   - Memory profiling
   - Performance profiling
   - CPU usage analysis

2. **PerfView** (Microsoft):
   - ETW event tracing
   - Memory allocation tracking
   - GC analysis

3. **BenchmarkDotNet**:
```csharp
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class EmbeddingBenchmark
{
    [Benchmark]
    public async Task GenerateEmbeddings()
    {
        await embeddingService.GenerateEmbeddingsAsync(texts);
    }
}
```

#### Optimization Strategies

1. **Caching**:
   - Memory cache for embeddings
   - Response caching for searches
   - Database query caching

2. **Async/Await**:
   - Use async patterns consistently
   - Avoid blocking calls
   - Configure synchronization context

3. **Resource Management**:
   - Dispose resources properly
   - Use using statements
   - Monitor memory usage

### Code Quality

#### Code Style

Follow guidelines in `CodingRules.md`:
- Use C# naming conventions
- Write XML documentation
- Follow async/await patterns
- Use nullable reference types

#### Static Analysis

```bash
# Run code analysis
dotnet build --verbosity normal

# Fix formatting
dotnet format

# Security analysis
dotnet list package --vulnerable
```

#### Code Reviews

1. **Pull Request Process**:
   - Create feature branch
   - Implement changes with tests
   - Submit pull request
   - Address review feedback

2. **Review Checklist**:
   - [ ] Code follows style guidelines
   - [ ] Tests are included
   - [ ] Documentation is updated
   - [ ] Performance impact considered
   - [ ] Security implications reviewed

## Troubleshooting

### Common Issues

#### Docker Issues

**Problem**: `docker-compose up` fails with port conflicts
```bash
Error: bind: address already in use
```

**Solution**:
```bash
# Find process using port
lsof -i :5561
netstat -tlnp | grep 5561

# Kill process or change port in docker-compose.yaml
# Option 1: Kill process
kill -9 <PID>

# Option 2: Change port
export SSE_PORT=5562
docker-compose up -d
```

**Problem**: Container fails to start with permission errors
```bash
Error: Permission denied
```

**Solution**:
```bash
# Fix permissions
sudo chown -R $USER:$USER db/
sudo chown -R $USER:$USER pdfs/

# Or use setup script
chmod +x setup-permissions.sh
./setup-permissions.sh
```

#### Database Issues

**Problem**: SQLite database locked
```bash
Error: database is locked
```

**Solution**:
```bash
# Check for running processes
ps aux | grep dotnet

# Kill processes accessing database
pkill -f cmd-mcp-host
pkill -f sse-mcp-server

# Remove lock file if exists
rm -f db/ev-supportwizard.db-wal
rm -f db/ev-supportwizard.db-shm
```

**Problem**: Entity Framework migration fails
```bash
Error: A network-related or instance-specific error occurred
```

**Solution**:
```bash
# Check connection string
cat app/cmd-mcp-host/appsettings.json | grep ConnectionStrings

# Create database directory
mkdir -p db

# Run migration manually
dotnet ef database update --project lib/Evanto.Mcp.Tools.SupportWizard

# Or enable auto-migration
# Set "AutoMigrateDatabase": true in appsettings.json
```

#### API Provider Issues

**Problem**: OpenAI API authentication fails
```bash
Error: 401 Unauthorized
```

**Solution**:
```bash
# Check API key
echo $OPENAI_API_KEY

# Test API key
curl -H "Authorization: Bearer $OPENAI_API_KEY" \
  https://api.openai.com/v1/models

# Update .env file
nano .env
# Add: OPENAI_API_KEY=***REMOVED***your-key-here
```

**Problem**: Ollama connection fails
```bash
Error: Connection refused to localhost:11434
```

**Solution**:
```bash
# Check if Ollama is running
curl http://localhost:11434/api/tags

# Start Ollama
ollama serve

# In another terminal, pull model
ollama pull qwen3:14b

# Test model
ollama run qwen3:14b "Hello"
```

#### Qdrant Issues

**Problem**: Qdrant collection not found
```bash
Error: Collection 'ev_support_documents' not found
```

**Solution**:
```bash
# Check Qdrant status
curl http://localhost:6335/

# List collections
curl http://localhost:6335/collections

# Create collection manually
curl -X PUT http://localhost:6335/collections/ev_support_documents \
  -H "Content-Type: application/json" \
  -d '{
    "vectors": {
      "size": 1536,
      "distance": "Cosine"
    }
  }'

# Or run vectorization to auto-create
dotnet run --project app/cmd-vectorize
```

#### Embedding Issues

**Problem**: Embedding dimensions mismatch
```bash
Error: Vector dimension mismatch. Expected 1536, got 768
```

**Solution**:
```bash
# Check embedding configuration
cat app/cmd-vectorize/appsettings.json | grep EmbeddingDimensions

# Update Qdrant configuration to match
# Edit appsettings.json:
# "VectorDimension": 768  # Match embedding provider

# Or recreate collection with correct dimensions
curl -X DELETE http://localhost:6335/collections/ev_support_documents
# Then run vectorization to recreate
```

### Debugging Commands

#### System Health Check

```bash
# Check all services
docker-compose ps

# Test endpoints
curl http://localhost:6335/       # Qdrant
curl http://localhost:5561/       # SSE MCP server
curl http://localhost:4316/       # Aspire dashboard

# Check logs
docker-compose logs --tail=50 sse-mcp-server
docker-compose logs --tail=50 qdrantdb
```

#### Application Testing

```bash
# Test MCP servers
dotnet run --project app/cmd-mcp-host -- --test

# Test specific provider
dotnet run --project app/cmd-mcp-host -- --provider OpenAI --test

# Test with verbose output
dotnet run --project app/cmd-mcp-host -- --test --verbosity Debug
```

#### Performance Monitoring

```bash
# Monitor Docker resource usage
docker stats

# Monitor disk usage
df -h
du -sh db/ pdfs/

# Monitor memory usage
free -h
ps aux --sort=-%mem | head -10
```

### Log Analysis

#### Application Logs

```bash
# View application logs
dotnet run --project app/cmd-mcp-host 2>&1 | tee app.log

# Filter for errors
grep -i error app.log

# Filter for specific component
grep -i "embedding" app.log
grep -i "qdrant" app.log
```

#### Docker Logs

```bash
# Follow logs in real-time
docker-compose logs -f

# Save logs to file
docker-compose logs > docker-logs.txt

# Filter logs by service
docker-compose logs sse-mcp-server | grep -i error
docker-compose logs qdrantdb | grep -i warning
```

#### OpenTelemetry Traces

1. **Access Aspire Dashboard**:
   - Open http://localhost:4316
   - Use token from docker-compose.yaml

2. **View Traces**:
   - Navigate to "Traces" section
   - Filter by service name
   - Analyze performance bottlenecks

3. **Custom Traces**:
```csharp
// Add custom activity
using var activity = ActivitySource.StartActivity("CustomOperation");
activity?.SetTag("operation", "embedding");
activity?.SetTag("provider", "OpenAI");
```

### Recovery Procedures

#### Complete System Reset

```bash
# Stop all services
docker-compose down -v

# Remove all data (WARNING: destroys databases)
rm -rf db/*
rm -rf pdfs/processed_files.json

# Rebuild containers
docker-compose build --no-cache

# Start fresh
docker-compose up -d

# Reprocess PDFs
dotnet run --project app/cmd-vectorize
```

#### Partial Recovery

```bash
# Reset only vector database
curl -X DELETE http://localhost:6335/collections/ev_support_documents
dotnet run --project app/cmd-vectorize

# Reset only support database
rm -f db/ev-supportwizard.db
dotnet run --project app/cmd-mcp-host -- --migrate

# Reset only processed files
rm -f pdfs/processed_files.json
dotnet run --project app/cmd-vectorize
```

## Advanced Topics

### Adding New AI Providers

The system uses **Microsoft.Extensions.AI** abstractions for unified provider support. Adding new providers involves configuration and factory registration.

#### Step 1: Add Provider Configuration

Edit `appsettings.json`:

```json
{
  "ChatClients": [
    {
      "ProviderName": "NewProvider",
      "Endpoint": "https://api.newprovider.com/v1",
      "DefaultModel": "new-model-v1",
      "AvailableModels": [
        "new-model-v1",
        "new-model-v2"
      ]
    }
  ]
}
```

#### Step 2: Add Environment Variable Support

Edit `.env` file:

```bash
# New provider API key
NEWPROVIDER_API_KEY=your-api-key-here
```

#### Step 3: Update Chat Client Factory

Edit `lib/Evanto.Mcp.Host/Factories/EvChatClientFactory.cs`:

```csharp
public IChatClient Create(String providerName)
{
    var settings = GetProviderSettings(providerName);
    
    return providerName switch
    {
        "OpenAI" => CreateOpenAIClient(settings),
        "Azure" => CreateAzureClient(settings),
        "Ollama" => CreateOllamaClient(settings),
        "NewProvider" => CreateNewProviderClient(settings),
        _ => throw new ArgumentException($"Unknown provider: {providerName}")
    };
}

private IChatClient CreateNewProviderClient(EvChatClientSettings settings)
{
    var apiKey = Environment.GetEnvironmentVariable("NEWPROVIDER_API_KEY") 
                 ?? settings.ApiKey;
    
    var client = new NewProviderClient(
        new Uri(settings.Endpoint),
        new ApiKeyCredential(apiKey));
    
    return client.AsChatClient(settings.DefaultModel);
}
```

#### Step 4: Add NuGet Package

Add provider package to `Directory.Packages.props`:

```xml
<PackageVersion Include="NewProvider.AI.Client" Version="1.0.0" />
```

### Extending MCP Tools

#### Creating Custom Tool

1. **Create tool class**:
```csharp
[McpServerToolType]
public class CustomMcpTool : EvMcpToolBase
{
    private readonly ICustomRepository _repository;
    
    public CustomMcpTool(ICustomRepository repository)
    {
        _repository = repository;
    }
    
    [McpServerTool, Description("Custom tool description")]
    public async Task<String> CustomOperation(String parameter)
    {
        return await ExecuteAsync(
            () => _repository.CustomOperationAsync(parameter),
            result => result == null,
            result => result.ToString(),
            "No results found");
    }
}
```

2. **Create repository interface**:
```csharp
public interface ICustomRepository
{
    Task<CustomResult> CustomOperationAsync(String parameter);
}
```

3. **Implement repository**:
```csharp
public class CustomRepository : ICustomRepository
{
    public async Task<CustomResult> CustomOperationAsync(String parameter)
    {
        // Implementation
        return new CustomResult { Value = parameter };
    }
}
```

4. **Register dependencies**:
```csharp
public static class CustomToolExtensions
{
    public static IServiceCollection AddCustomTool(
        this IServiceCollection services)
    {
        services.AddSingleton<ICustomRepository, CustomRepository>();
        services.AddSingleton<CustomMcpTool>();
        return services;
    }
    
    public static IMcpServerBuilder WithCustomMcpTools(
        this IMcpServerBuilder builder)
    {
        builder.AddTool<CustomMcpTool>();
        return builder;
    }
}
```

#### Tool Testing

Add tool tests to `appsettings.json`:

```json
{
  "McpServers": [
    {
      "Name": "Custom MCP Server",
      "ToolTests": [
        {
          "ToolName": "custom_operation",
          "TestParameters": {
            "parameter": "test_value"
          },
          "Enabled": true,
          "TimeoutSeconds": 10
        }
      ]
    }
  ]
}
```

### Custom Embedding Providers

#### Provider Interface

```csharp
public interface ICustomEmbeddingProvider
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text, 
        CancellationToken cancellationToken = default);
    
    Task<ReadOnlyMemory<float>[]> GenerateEmbeddingsAsync(
        IEnumerable<string> texts, 
        CancellationToken cancellationToken = default);
}
```

#### Implementation

```csharp
public class CustomEmbeddingProvider : ICustomEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly CustomEmbeddingSettings _settings;
    
    public CustomEmbeddingProvider(
        HttpClient httpClient, 
        CustomEmbeddingSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }
    
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text, 
        CancellationToken cancellationToken = default)
    {
        var request = new CustomEmbeddingRequest
        {
            Text = text,
            Model = _settings.Model
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            "/embeddings", 
            request, 
            cancellationToken);
        
        var result = await response.Content
            .ReadFromJsonAsync<CustomEmbeddingResponse>(cancellationToken);
        
        return result.Embedding.AsMemory();
    }
}
```

#### Factory Integration

```csharp
public class EvEmbeddingGeneratorFactory
{
    public IEmbeddingGenerator<string, Embedding<float>> Create(
        EvEmbeddingSettings settings)
    {
        return settings.ProviderName switch
        {
            "OpenAI" => CreateOpenAIGenerator(settings),
            "Azure" => CreateAzureGenerator(settings),
            "Custom" => CreateCustomGenerator(settings),
            _ => throw new ArgumentException($"Unknown provider: {settings.ProviderName}")
        };
    }
    
    private IEmbeddingGenerator<string, Embedding<float>> CreateCustomGenerator(
        EvEmbeddingSettings settings)
    {
        var httpClient = new HttpClient();
        var provider = new CustomEmbeddingProvider(httpClient, settings);
        
        return new CustomEmbeddingGenerator(provider);
    }
}
```

### OpenTelemetry Integration

#### Custom Metrics

```csharp
public class CustomMetrics
{
    private readonly Counter<int> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    
    public CustomMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Evanto.Mcp.Custom");
        
        _requestCounter = meter.CreateCounter<int>(
            "custom_requests_total",
            "Total number of custom requests");
            
        _requestDuration = meter.CreateHistogram<double>(
            "custom_request_duration_seconds",
            "Duration of custom requests in seconds");
    }
    
    public void RecordRequest(double duration)
    {
        _requestCounter.Add(1);
        _requestDuration.Record(duration);
    }
}
```

#### Custom Activities

```csharp
public class CustomService
{
    private static readonly ActivitySource ActivitySource = 
        new("Evanto.Mcp.Custom");
    
    public async Task<string> ProcessAsync(string input)
    {
        using var activity = ActivitySource.StartActivity("CustomService.Process");
        activity?.SetTag("input.length", input.Length);
        
        try
        {
            var result = await DoProcessingAsync(input);
            activity?.SetTag("result.length", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

#### Registration

```csharp
public static class TelemetryExtensions
{
    public static IServiceCollection AddCustomTelemetry(
        this IServiceCollection services)
    {
        services.AddSingleton<CustomMetrics>();
        
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder.AddSource("Evanto.Mcp.Custom");
            })
            .WithMetrics(builder =>
            {
                builder.AddMeter("Evanto.Mcp.Custom");
            });
        
        return services;
    }
}
```

### Performance Optimization

#### Caching Strategies

1. **Memory Caching**:
```csharp
public class CachedEmbeddingService : IEvEmbeddingService
{
    private readonly IEvEmbeddingService _innerService;
    private readonly IMemoryCache _cache;
    
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
    {
        var cacheKey = $"embedding:{text.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out ReadOnlyMemory<float> cached))
        {
            return cached;
        }
        
        var embedding = await _innerService.GenerateEmbeddingAsync(text);
        
        _cache.Set(cacheKey, embedding, TimeSpan.FromHours(1));
        
        return embedding;
    }
}
```

2. **Distributed Caching**:
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

services.Decorate<IEvEmbeddingService, DistributedCachedEmbeddingService>();
```

#### Batch Processing

```csharp
public class BatchEmbeddingProcessor
{
    private readonly IEvEmbeddingService _embeddingService;
    private readonly Channel<string> _channel;
    
    public BatchEmbeddingProcessor(IEvEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
        _channel = Channel.CreateUnbounded<string>();
        
        _ = Task.Run(ProcessBatchesAsync);
    }
    
    private async Task ProcessBatchesAsync()
    {
        const int batchSize = 10;
        var batch = new List<string>(batchSize);
        
        await foreach (var item in _channel.Reader.ReadAllAsync())
        {
            batch.Add(item);
            
            if (batch.Count >= batchSize)
            {
                await ProcessBatchAsync(batch);
                batch.Clear();
            }
        }
        
        if (batch.Count > 0)
        {
            await ProcessBatchAsync(batch);
        }
    }
    
    private async Task ProcessBatchAsync(IList<string> texts)
    {
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(texts);
        // Process embeddings
    }
}
```

#### Connection Pooling

```csharp
services.AddHttpClient<ICustomService, CustomService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    MaxConnectionsPerServer = 50
});
```

### Security Considerations

#### API Key Management

1. **Azure Key Vault**:
```csharp
services.AddAzureKeyVault(options =>
{
    options.VaultUri = new Uri("https://your-keyvault.vault.azure.net/");
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
});
```

2. **Environment Variables**:
```csharp
public class SecureConfigurationProvider
{
    public string GetApiKey(string providerName)
    {
        var envKey = $"{providerName.ToUpper()}_API_KEY";
        return Environment.GetEnvironmentVariable(envKey) 
               ?? throw new InvalidOperationException($"API key not found: {envKey}");
    }
}
```

#### Input Validation

```csharp
public class ValidatedMcpTool : EvMcpToolBase
{
    [McpServerTool, Description("Validated operation")]
    public async Task<String> ValidatedOperation(
        [Required] String parameter,
        [Range(1, 100)] int count = 10)
    {
        var validationError = ValidateNotEmpty(parameter, "Parameter is required");
        if (validationError != null)
            return validationError;
        
        // Additional validation
        if (parameter.Length > 1000)
            return CreateErrorResponse("Parameter too long");
        
        return await ExecuteAsync(
            () => repository.OperationAsync(parameter, count),
            result => result == null,
            result => result,
            "No results found");
    }
}
```

#### Rate Limiting

```csharp
public class RateLimitedEmbeddingService : IEvEmbeddingService
{
    private readonly IEvEmbeddingService _innerService;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _timer;
    
    public RateLimitedEmbeddingService(IEvEmbeddingService innerService)
    {
        _innerService = innerService;
        _semaphore = new SemaphoreSlim(5, 5); // 5 concurrent requests
        _timer = new Timer(ReleaseSemaphore, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }
    
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            return await _innerService.GenerateEmbeddingAsync(text);
        }
        finally
        {
            // Don't release immediately - let timer handle it
        }
    }
    
    private void ReleaseSemaphore(object state)
    {
        if (_semaphore.CurrentCount < 5)
        {
            _semaphore.Release();
        }
    }
}
```

## API Reference

### Key Interfaces

#### IEvEmbeddingService
```csharp
public interface IEvEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text, 
        CancellationToken cancellationToken = default);
    
    Task<ReadOnlyMemory<float>[]> GenerateEmbeddingsAsync(
        IEnumerable<string> texts, 
        CancellationToken cancellationToken = default);
    
    Task<bool> TestConnectionAsync(
        CancellationToken cancellationToken = default);
}
```

#### IEvDocumentRepository
```csharp
public interface IEvDocumentRepository
{
    Task<string> StoreDocumentAsync(
        EvDocument document, 
        CancellationToken cancellationToken = default);
    
    Task<EvDocumentSearchResult> SearchDocumentsAsync(
        EvDocumentSearchQuery query, 
        CancellationToken cancellationToken = default);
    
    Task<EvDocument> GetDocumentAsync(
        string id, 
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteDocumentAsync(
        string id, 
        CancellationToken cancellationToken = default);
}
```

#### IEvPdfExtractorService
```csharp
public interface IEvPdfExtractorService
{
    Task<string> ExtractTextAsync(
        string pdfPath, 
        CancellationToken cancellationToken = default);
    
    Task<string> ExtractTextAsync(
        Stream pdfStream, 
        CancellationToken cancellationToken = default);
}
```

### Configuration Models

#### EvHostAppSettings
```csharp
public class EvHostAppSettings : EvBaseAppSettings
{
    public string DefaultChatClient { get; set; } = "OpenAI";
    public EvChatClientSettings[] ChatClients { get; set; } = Array.Empty<EvChatClientSettings>();
    public EvMcpServerSettings[] McpServers { get; set; } = Array.Empty<EvMcpServerSettings>();
    public EvTelemetrySettings Telemetry { get; set; } = new();
}
```

#### EvChatClientSettings
```csharp
public class EvChatClientSettings
{
    public string ProviderName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
    public string[] AvailableModels { get; set; } = Array.Empty<string>();
}
```

#### EvEmbeddingSettings
```csharp
public class EvEmbeddingSettings
{
    public string ProviderName { get; set; } = "OpenAI";
    public string DefaultModel { get; set; } = "text-embedding-3-small";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int ChunkSize { get; set; } = 1000;
    public int ChunkOverlap { get; set; } = 200;
    public int EmbeddingDimensions { get; set; } = 1536;
}
```

### Extension Methods

#### Service Registration
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmbeddingService(
        this IServiceCollection services,
        EvEmbeddingSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IEvEmbeddingService, EvEmbeddingService>();
        return services;
    }
    
    public static IServiceCollection AddQdrantDocumentRepository(
        this IServiceCollection services,
        EvQdrantSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IEvDocumentRepository, EvDocumentRepository>();
        return services;
    }
    
    public static IServiceCollection AddPdfTextExtractor(
        this IServiceCollection services)
    {
        services.AddSingleton<IEvPdfExtractorService, EvPdfTextExtractorService>();
        return services;
    }
}
```

#### MCP Server Builder
```csharp
public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithSupportWizardMcpTools(
        this IMcpServerBuilder builder)
    {
        builder.AddTool<EvSupportWizardTool>();
        return builder;
    }
    
    public static IMcpServerBuilder WithSupportDocMcpTools(
        this IMcpServerBuilder builder)
    {
        builder.AddTool<EvSupportDocsTool>();
        return builder;
    }
}
```

### Factory Patterns

#### EvChatClientFactory
```csharp
public class EvChatClientFactory
{
    public IChatClient Create(string providerName)
    {
        var settings = GetProviderSettings(providerName);
        
        return providerName switch
        {
            "OpenAI" => CreateOpenAIClient(settings),
            "Azure" => CreateAzureClient(settings),
            "Ollama" => CreateOllamaClient(settings),
            _ => throw new ArgumentException($"Unknown provider: {providerName}")
        };
    }
    
    public string[] GetAvailableProviders()
    {
        return _settings.ChatClients.Select(c => c.ProviderName).ToArray();
    }
    
    public string[] GetAvailableModels(string providerName)
    {
        var provider = _settings.ChatClients
            .FirstOrDefault(c => c.ProviderName == providerName);
        
        return provider?.AvailableModels ?? Array.Empty<string>();
    }
}
```

#### EvMcpClientFactory
```csharp
public class EvMcpClientFactory
{
    public async Task<IMcpClient> CreateAsync(
        EvMcpServerSettings settings,
        CancellationToken cancellationToken = default)
    {
        return settings.TransportType switch
        {
            EvMcpTransportType.STDIO => await CreateStdioClientAsync(settings, cancellationToken),
            EvMcpTransportType.SSE => await CreateSseClientAsync(settings, cancellationToken),
            EvMcpTransportType.HTTP => await CreateHttpClientAsync(settings, cancellationToken),
            _ => throw new ArgumentException($"Unknown transport type: {settings.TransportType}")
        };
    }
}
```

### Error Handling

#### EvMcpToolBase
```csharp
public abstract class EvMcpToolBase
{
    protected string? ValidateNotEmpty(string? value, string errorMessage)
    {
        return string.IsNullOrWhiteSpace(value) 
            ? CreateErrorResponse(errorMessage)
            : null;
    }
    
    protected async Task<string> ExecuteAsync<T>(
        Func<Task<T>> operation,
        Func<T, bool> isNotFound,
        Func<T, object> resultSelector,
        string notFoundMessage)
    {
        try
        {
            var result = await operation();
            
            if (isNotFound(result))
            {
                return CreateNotFoundResponse(notFoundMessage);
            }
            
            return CreateSuccessResponse(resultSelector(result));
        }
        catch (Exception ex)
        {
            return CreateErrorResponse($"Operation failed: {ex.Message}");
        }
    }
    
    protected string CreateErrorResponse(string message)
    {
        return JsonSerializer.Serialize(new { error = message });
    }
    
    protected string CreateNotFoundResponse(string message)
    {
        return JsonSerializer.Serialize(new { message, found = false });
    }
    
    protected string CreateSuccessResponse(object data)
    {
        return JsonSerializer.Serialize(new { data, success = true });
    }
}
```

## Contributing

### Code Style Guidelines

This project follows the coding standards defined in `CodingRules.md`. Key points:

#### Naming Conventions
- **Classes**: PascalCase (e.g., `EvMcpClientFactory`)
- **Methods**: PascalCase (e.g., `CreateAsync`)
- **Properties**: PascalCase (e.g., `ProviderName`)
- **Fields**: camelCase with underscore prefix (e.g., `_repository`)
- **Constants**: PascalCase (e.g., `DefaultTimeout`)

#### Code Structure
```csharp
namespace Evanto.Mcp.Tools.Example;

/// <summary>
/// Example MCP tool implementation.
/// </summary>
[McpServerToolType]
public class ExampleMcpTool : EvMcpToolBase
{
    private readonly IExampleRepository _repository;
    
    public ExampleMcpTool(IExampleRepository repository)
    {
        _repository = repository;
    }
    
    /// <summary>
    /// Example tool operation.
    /// </summary>
    /// <param name="parameter">The parameter description.</param>
    /// <returns>The operation result.</returns>
    [McpServerTool, Description("Example operation description")]
    public async Task<String> ExampleOperation(String parameter)
    {
        var validationError = ValidateNotEmpty(parameter, "Parameter is required");
        if (validationError != null)
            return validationError;
        
        return await ExecuteAsync(
            () => _repository.ExampleOperationAsync(parameter),
            result => result == null,
            result => result,
            "No results found");
    }
}
```

### Testing Guidelines

#### Unit Tests
```csharp
[TestClass]
public class EvMcpClientFactoryTests
{
    [TestMethod]
    public async Task CreateAsync_WithValidSettings_ReturnsClient()
    {
        // Arrange
        var settings = new EvMcpServerSettings
        {
            TransportType = EvMcpTransportType.STDIO,
            Command = "test-command"
        };
        
        var factory = new EvMcpClientFactory();
        
        // Act
        var client = await factory.CreateAsync(settings);
        
        // Assert
        Assert.IsNotNull(client);
        Assert.IsInstanceOfType(client, typeof(IMcpClient));
    }
}
```

#### Integration Tests
```csharp
[TestClass]
public class SupportWizardIntegrationTests
{
    private TestServer _server;
    private HttpClient _client;
    
    [TestInitialize]
    public async Task Initialize()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddSupportWizard();
        
        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }
    
    [TestMethod]
    public async Task CreateSupportRequest_WithValidData_CreatesRequest()
    {
        // Arrange
        var request = new CreateSupportRequestRequest
        {
            CustomerEmail = "test@example.com",
            Subject = "Test Issue"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/support", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SupportRequest>();
        Assert.IsNotNull(result);
        Assert.AreEqual(request.CustomerEmail, result.CustomerEmail);
    }
}
```

### Pull Request Process

1. **Fork the repository**
2. **Create feature branch**:
   ```bash
   git checkout -b feature/new-mcp-tool
   ```

3. **Implement changes**:
   - Write code following style guidelines
   - Add unit tests for new functionality
   - Update documentation as needed

4. **Run tests**:
   ```bash
   dotnet test
   dotnet run --project app/cmd-mcp-host -- --test
   ```

5. **Submit pull request**:
   - Provide clear description of changes
   - Reference related issues
   - Include test results

### Code Review Checklist

- [ ] **Code Style**: Follows CodingRules.md guidelines
- [ ] **Documentation**: XML comments for public APIs
- [ ] **Tests**: Unit tests for new functionality
- [ ] **Performance**: No obvious performance issues
- [ ] **Security**: No security vulnerabilities
- [ ] **Compatibility**: Works with existing code
- [ ] **Configuration**: Updates to appsettings.json if needed

### Development Environment Setup

1. **Install prerequisites**:
   ```bash
   # .NET 9.0 SDK
   winget install Microsoft.DotNet.SDK.9
   
   # Docker Desktop
   winget install Docker.DockerDesktop
   
   # Visual Studio or VS Code
   winget install Microsoft.VisualStudio.2024.Community
   ```

2. **Configure IDE**:
   - Install C# extension (VS Code)
   - Enable EditorConfig support
   - Configure code formatting

3. **Set up project**:
   ```bash
   git clone <repository-url>
   cd public-ai
   dotnet restore
   dotnet build
   ```

4. **Run tests**:
   ```bash
   dotnet test
   ```

### Documentation Updates

When making changes that affect the public API or user experience:

1. **Update this README.md** for major changes
2. **Update CLAUDE.md** for AI assistant instructions
3. **Update code comments** and XML documentation
4. **Update configuration examples** if applicable

### Issue Reporting

When reporting issues:

1. **Use issue templates** if available
2. **Provide system information**:
   - OS version
   - .NET version
   - Docker version
   - Error messages and stack traces

3. **Include reproduction steps**:
   - Minimal code example
   - Configuration files
   - Expected vs actual behavior

4. **Add relevant labels**:
   - `bug` for defects
   - `enhancement` for new features
   - `documentation` for docs issues

## License & Legal

### Open Source License

This project is licensed under the **MIT License**. See the `LICENSE` file for full details.

```
MIT License

Copyright (c) 2024 Evanto

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### Third-Party Dependencies

This project includes several third-party libraries with their own licenses:

#### Microsoft Libraries (MIT License)
- **Microsoft.Extensions.AI**: AI provider abstractions
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Logging**: Logging abstractions
- **Microsoft.Extensions.Hosting**: Application hosting
- **Microsoft.EntityFrameworkCore**: Object-relational mapping

#### Commercial Dependencies

##### iText7 (AGPL/Commercial License)
- **Package**: iText7.Core
- **License**: AGPL 3.0 (open source) or Commercial License
- **Purpose**: PDF text extraction and processing
- **Usage**: PDF processing in `Evanto.Mcp.Pdfs` library

**Important**: iText7 is dual-licensed. If you plan to use this software in a commercial application without releasing your source code under AGPL, you must purchase a commercial license from iText.

For more information: https://itextpdf.com/pricing

##### AI Provider APIs
- **OpenAI API**: Usage subject to OpenAI Terms of Service
- **Azure OpenAI**: Usage subject to Microsoft Azure Terms
- **Ollama**: Apache 2.0 License (open source)

#### External Services

##### Qdrant Vector Database
- **License**: Apache 2.0 License
- **Purpose**: Vector similarity search
- **Usage**: Document embeddings storage and retrieval

##### Docker
- **License**: Apache 2.0 License
- **Purpose**: Containerization and deployment
- **Usage**: Container orchestration via Docker Compose

### Compliance Notes

#### GDPR Compliance
This software processes personal data in support tickets. When deploying in production:

1. **Data Protection**: Implement appropriate data protection measures
2. **User Rights**: Provide mechanisms for data access, correction, and deletion
3. **Data Retention**: Implement data retention policies
4. **Consent**: Ensure proper consent mechanisms for data processing

#### Security Considerations
- **API Keys**: Store API keys securely using environment variables or key vaults
- **Database Security**: Secure SQLite database files with appropriate file permissions
- **Network Security**: Use HTTPS in production deployments
- **Input Validation**: All user inputs are validated to prevent injection attacks

### Contributing License Agreement

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

### Disclaimer

This software is provided "as is" without warranty of any kind. The authors are not responsible for any damages or issues arising from the use of this software.

### Contact Information

For questions about licensing or legal issues, please contact:
- **Email**: [Insert contact email]
- **Issues**: Create an issue on the GitHub repository

---

**Thank you for using the Evanto MCP Host System!**

This comprehensive documentation should help you understand, deploy, and extend the system effectively. For additional support or questions, please refer to the GitHub repository or contact the development team.