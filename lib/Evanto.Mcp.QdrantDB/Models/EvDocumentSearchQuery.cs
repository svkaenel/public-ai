using System;

namespace Evanto.Mcp.QdrantDB.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Query parameters for document search operations. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvDocumentSearchQuery
{
    /// <summary>   Text query to search for (will be converted to vector). </summary>
    public String                   QueryText           { get; set; } = String.Empty;

    /// <summary>   Pre-computed query vector for direct vector search. </summary>
    public ReadOnlyMemory<Single>   QueryVector         { get; set; }

    /// <summary>   Maximum number of results to return. </summary>
    public Int32                    Limit               { get; set; } = 10;

    /// <summary>   Minimum similarity score threshold. </summary>
    public Single                   MinimumScore        { get; set; } = 0.5f;

    /// <summary>   Filter results by specific file names. </summary>
    public String[]                 FileNameFilters     { get; set; } = Array.Empty<String>();

    /// <summary>   Include only documents processed after this date. </summary>
    public DateTime?                ProcessedAfter      { get; set; }

    /// <summary>   Include only documents processed before this date. </summary>
    public DateTime?                ProcessedBefore     { get; set; }
}