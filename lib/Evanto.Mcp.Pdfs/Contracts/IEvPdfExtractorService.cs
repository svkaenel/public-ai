namespace Evanto.Mcp.Pdfs.Contracts;

///-------------------------------------------------------------------------------------------------
/// <summary>   Interface for PDF text extraction service. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public interface IEvPdfTextExtractorService
{
    Task<String>        ExtractTextAsync(String filePath);
    List<String>        ChunkText(String text, Int32 chunkSize, Int32 overlap);
}