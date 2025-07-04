using System;

namespace Evanto.Mcp.Common.Settings;

public class EvMcpSrvAppSettings : EvBaseAppSettings
{
    public String               SystemName              { get; set; } = String.Empty;
    public String               SSEListenUrls           { get; set; } = "http://0.0.0.0:5555";
    public Boolean              AutoMigrateDatabase     { get; set; } = true;
    public EvEmbeddingSettings? Embeddings              { get; set; } = null;
    public EvQdrantSettings?    Qdrant                  { get; set; } = null;
}
