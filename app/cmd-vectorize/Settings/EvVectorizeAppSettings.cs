using Evanto.Mcp.Common.Settings; 

namespace Evanto.Mcp.Vectorize.Settings;

public class EvVectorizeAppSettings
{
    public String               PdfDirectory            { get; set; } = "./pdfs";
    public String               TrackingFilePath        { get; set; } = "./processed_files.json";
    public EvEmbeddingSettings? EmbeddingSettings       { get; set; } = null;
    public EvQdrantSettings?    QdrantSettings          { get; set; } = null;
}