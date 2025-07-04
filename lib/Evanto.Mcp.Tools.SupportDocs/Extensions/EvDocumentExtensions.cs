using Evanto.Mcp.QdrantDB.Models;
using Evanto.Mcp.Tools.SupportDocs.ViewModels;

namespace Evanto.Mcp.Tools.SupportDocs.Extensions;

///-------------------------------------------------------------------------------------------------
/// <summary>   Extension methods for EvDocument conversion. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public static class EvDocumentExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Converts an EvDocument to EvSupportDocViewModel. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="document"> The document to convert. </param>
    ///
    /// <returns>   The converted EvSupportDocViewModel. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static EvSupportDocViewModel ToSupportDocViewModel(this EvDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new EvSupportDocViewModel
        {
            FileName     = document.FileName,
            Content      = document.Content,
            Score        = document.Score ?? 0.0f,
            ChunkIndex   = document.ChunkIndex,
            TotalChunks  = document.TotalChunks,
            ChunkId      = document.ChunkId,
            BaseFileName = document.BaseFileName
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Converts a collection of EvDocument to EvSupportDocViewModel. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="documents"> The documents to convert. </param>
    ///
    /// <returns>   The converted collection of EvSupportDocViewModel. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IEnumerable<EvSupportDocViewModel> ToSupportDocViewModels(this IEnumerable<EvDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        return documents.Select(ToSupportDocViewModel);
    }
}