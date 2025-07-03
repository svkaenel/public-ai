namespace Evanto.Mcp.Vectorize.Models;

public class EvDocumentRecord
{
    public required String                      Id              { get; set; }
    public required String                      FileName        { get; set; }
    public required String                      Content         { get; set; }
    public required ReadOnlyMemory<Single>      Vector          { get; set; }
    public          DateTime                    ProcessedAt     { get; set; }
    public          String                      FileHash        { get; set; } = String.Empty;
    public          Int32                       ChunkIndex      { get; set; }
    public          Int32                       TotalChunks     { get; set; }
    public          Single?                     Score           { get; set; }
}