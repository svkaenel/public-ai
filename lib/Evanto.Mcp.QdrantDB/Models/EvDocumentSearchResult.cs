using System.Linq;

namespace Evanto.Mcp.QdrantDB.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Result of a document search operation. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvDocumentSearchResult
{
    /// <summary>   Documents found matching the search criteria. </summary>
    public IEnumerable<EvDocument> Documents { get; set; } = Enumerable.Empty<EvDocument>();

    /// <summary>   Total number of documents found. </summary>
    public Int32 TotalCount { get; set; }

    /// <summary>   Original query that produced this result. </summary>
    public String Query { get; set; } = String.Empty;

    /// <summary>   Time taken to execute the search. </summary>
    public TimeSpan SearchDuration { get; set; }

    /// <summary>   Indicates if the search was successful. </summary>
    public Boolean Success { get; set; } = true;

    /// <summary>   Error message if search failed. </summary>
    public String? ErrorMessage { get; set; }
}