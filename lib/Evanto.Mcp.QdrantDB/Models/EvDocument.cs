namespace Evanto.Mcp.QdrantDB.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Unified document model for vector storage and search operations. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvDocument
{
    /// <summary>   Unique identifier for the document chunk. </summary>
    public required String Id { get; set; }

    /// <summary>   Original file name. </summary>
    public required String FileName { get; set; }

    /// <summary>   Text content of the document chunk. </summary>
    public required String Content { get; set; }

    /// <summary>   Vector representation of the content. </summary>
    public ReadOnlyMemory<Single> Vector { get; set; }

    /// <summary>   Timestamp when document was processed. </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>   Hash of the original file for change detection. </summary>
    public String FileHash { get; set; } = String.Empty;

    /// <summary>   Index of this chunk within the document. </summary>
    public Int32 ChunkIndex { get; set; }

    /// <summary>   Total number of chunks in the document. </summary>
    public Int32 TotalChunks { get; set; }

    /// <summary>   Base file name without extension. </summary>
    public String BaseFileName { get; set; } = String.Empty;

    /// <summary>   Unique chunk identifier in format {BaseFileName}_{ChunkIndex}. </summary>
    public String ChunkId { get; set; } = String.Empty;

    /// <summary>   Similarity score (only populated in search results). </summary>
    public Single? Score { get; set; }
}