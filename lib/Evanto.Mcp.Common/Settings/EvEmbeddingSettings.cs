using System;

namespace Evanto.Mcp.Common.Settings;

public class EvEmbeddingSettings : EvChatClientSettings
{
    public Int32  ChunkSize             { get; set; } = 1000;
    public Int32  ChunkOverlap          { get; set; } = 200;
}
