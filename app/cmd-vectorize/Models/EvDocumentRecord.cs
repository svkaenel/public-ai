namespace Evanto.Mcp.Vectorize.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Document record for vector storage. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
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