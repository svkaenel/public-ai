# MCP Server Tester

## Übersicht

Der MCP Server Tester ist ein allgemeines, konfigurationsgesteuertes Testing-Framework für Model Context Protocol (MCP) Server und ihre Tools. Er ermöglicht automatisierte Tests mit konfigurierbaren Parametern, Timeouts und Ausgabeformaten.

## Architektur

### Kernkomponenten

#### 1. Konfigurationsklassen

**McpToolTestConfiguration.cs** - Definiert die Testkonfiguration für einzelne Tools:
```csharp
public class McpToolTestConfiguration
{
    public String ToolName { get; set; } = String.Empty;
    public Dictionary<String, Object?> TestParameters { get; set; } = new();
    public Boolean Enabled { get; set; } = true;
    public Int32 TimeoutSeconds { get; set; } = 30;
}
```

**McpServerSettings** - Erweitert um ToolTests:
```csharp
public class McpServerSettings
{
    // ... bestehende Eigenschaften ...
    public IList<McpToolTestConfiguration> ToolTests { get; set; } = new List<McpToolTestConfiguration>();
}
```

#### 2. Result-Klassen

**McpTestResult.cs** - Gesamtergebnis eines Server-Tests:
```csharp
public class McpTestResult
{
    public String ServerName { get; set; } = String.Empty;
    public Boolean Success { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public IList<McpToolTestResult> ToolResults { get; set; } = new List<McpToolTestResult>();
    public String? ErrorMessage { get; set; }
}
```

**McpToolTestResult.cs** - Ergebnis eines einzelnen Tool-Tests:
```csharp
public class McpToolTestResult
{
    public String ToolName { get; set; } = String.Empty;
    public Boolean Success { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<String, Object?> UsedParameters { get; set; } = new();
    public String? Response { get; set; }
    public String? ErrorMessage { get; set; }
}
```

#### 3. McpServerTester

Die Hauptklasse für die Durchführung der Tests:

```csharp
public class McpServerTester(ILogger<McpServerTester> logger)
{
    public async Task<McpTestResult> TestServerAsync(
        ConfigurableMcpClientFactory.McpClientInfo mcpClientInfo,
        IList<McpToolTestConfiguration> toolTests,
        Boolean quickTest = false);
    
    public async Task<McpToolTestResult> TestToolAsync(
        IMcpClient client,
        McpClientTool tool,
        McpToolTestConfiguration? testConfig,
        Int32 timeoutSeconds);
}
```

**Hauptfunktionen:**
- **Automatische Parametergenerierung**: Generiert Default-Parameter basierend auf Tool-Schema und Parameternamen
- **Timeout-Management**: Konfigurierbare Timeouts pro Tool und Server
- **Quick-Test-Modus**: Testet nur das erste konfigurierte Tool pro Server
- **Fehlerbehandlung**: Robuste Fehlerbehandlung, Tests laufen auch bei einzelnen Fehlern weiter

#### 4. Erweiterte ProgramExtensions

**Neue Methoden in ProgramExtensions.cs:**

```csharp
public static async Task TestAllMcpServersAsync(
    this ILogger logger,
    IConfiguration configuration,
    IList<ConfigurableMcpClientFactory.McpClientInfo> mcpClients,
    Boolean quickTest = false);

public static void PrintTestResults(this ILogger logger, McpTestResult results);
```

## Konfiguration

### appsettings.json Beispiel

```json
{
  "McpServers": [
    {
      "Name": "Time MCP Server",
      "Command": "docker",
      "Arguments": ["run", "-i", "--rm", "mcp/time"],
      "Enabled": true,
      "TimeoutSeconds": 30,
      "TransportType": "STDIO",
      "ToolTests": [
        {
          "ToolName": "get_current_time",
          "TestParameters": {
            "timezone": "Europe/Berlin"
          },
          "Enabled": true,
          "TimeoutSeconds": 15
        }
      ]
    },
    {
      "Name": "Brunner DotNet MCP Server (SSE)",
      "Url": "http://localhost:5555",
      "Enabled": true,
      "TimeoutSeconds": 30,
      "TransportType": "SSE",
      "ToolTests": [
        {
          "ToolName": "FindeKundeMitAdressenUndProdukten",
          "TestParameters": {
            "kundenName": "Selma Horn"
          },
          "Enabled": true,
          "TimeoutSeconds": 15
        },
        {
          "ToolName": "FindeHandwerkerFirma",
          "TestParameters": {
            "firmenNummer": 111122
          },
          "Enabled": true,
          "TimeoutSeconds": 10
        }
      ]
    }
  ]
}
```

## Verwendung

### Grundlegende Verwendung

```csharp
// 1. MCP Clients erstellen
var mcpClients = await ConfigurableMcpClientFactory.CreateMcpClientsFromConfigurationAsync(configuration, logger);

// 2. Tests ausführen
await logger.TestAllMcpServersAsync(configuration, mcpClients, quickTest: false);
```

### Quick-Test-Modus

```csharp
// Nur das erste konfigurierte Tool pro Server testen
await logger.TestAllMcpServersAsync(configuration, mcpClients, quickTest: true);
```

### Einzelne Server testen

```csharp
var tester = new McpServerTester(logger);
var serverConfig = GetServerConfiguration("Brunner DotNet MCP Server");
var result = await tester.TestServerAsync(mcpClientInfo, serverConfig.ToolTests, quickTest: false);
```

## Automatische Parametergenerierung

Wenn keine TestParameters konfiguriert sind, generiert der Tester automatisch Default-Parameter basierend auf:

### 1. JSON Schema Type
- **string**: Kontextabhängig (siehe unten)
- **integer**: 0
- **number**: 0.0
- **boolean**: false
- **array**: []
- **object**: null

### 2. Parametername-Heuristik
- **email/mail**: "test@example.com"
- **name/kunde**: "Test User"
- **nummer/id**: "12345"
- **url/link**: "https://example.com"
- **phone/telefon**: "+49 123 456789"
- **address/adresse**: "Teststraße 123"
- **city/stadt**: "Teststadt"
- **country/land**: "Deutschland"
- **date/datum**: "2025-06-23"
- **default**: "Test"

## Ausgabeformat

```
🧪 Testing Server: Brunner DotNet MCP Server (SSE)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 Tool: FindeKundeMitAdressenUndProdukten
   Parameters: { "kundenName": "Selma Horn" }
   ✅ Success | Duration: 234ms
   
📋 Tool: FindeHandwerkerFirma  
   Parameters: { "firmenNummer": 111122 }
   ❌ Failed | Duration: 45ms | Error: Connection timeout

Server Test Summary: 1/2 tools passed | Total Duration: 279ms

═══════════════════════════════════════════════════════════
🎯 Overall Test Summary: 2/3 servers passed | Total Duration: 1.2s
```

## Test-Modi

### 1. Quick-Test-Modus (quickTest: true)
- Testet nur das erste konfigurierte Tool pro Server
- Falls keine Tools konfiguriert: erstes verfügbares Tool
- Schnelle Smoke-Tests

### 2. Vollständiger Test-Modus (quickTest: false)
- Testet alle konfigurierten Tools
- Falls keine Tools konfiguriert: alle verfügbaren Tools
- Umfassende Validierung

## Fehlerbehandlung

- **Tool-Fehler**: Tests laufen weiter, Fehler werden protokolliert
- **Server-Fehler**: Andere Server werden weiterhin getestet
- **Timeout-Behandlung**: Konfigurierbare Timeouts pro Tool
- **Schema-Fehler**: Fallback auf String-Parameter bei Schema-Problemen

## Integration

### Dependency Injection
Der McpServerTester verwendet standardmäßig ILogger<McpServerTester> für typsichere Protokollierung.

### Erweiterbarkeit
- Custom Parameter-Generatoren durch Überschreiben von `GenerateDefaultValueFromPropertyDefinition`
- Custom Output-Formate durch Überschreiben von `PrintTestResults`
- Additional Test-Validatoren durch Erweiterung von `McpToolTestResult`

## Best Practices

1. **Konfiguration**: Definiere explizite TestParameters für wichtige Szenarien
2. **Timeouts**: Verwende realistische Timeouts basierend auf Tool-Komplexität
3. **Quick-Tests**: Nutze Quick-Test-Modus für CI/CD-Pipelines
4. **Monitoring**: Überwache Test-Ergebnisse für Regression-Detection
5. **Parameter-Validierung**: Teste mit verschiedenen Parameter-Kombinationen

## Erweiterungsmöglichkeiten

- **Test-Zyklen**: Wiederholte Tests für Stabilität
- **Load-Testing**: Parallele Tool-Aufrufe
- **Response-Validierung**: Schema-Validierung der Tool-Antworten
- **Performance-Metriken**: Detaillierte Performance-Analyse
- **Report-Export**: JSON/XML-Export der Test-Ergebnisse
