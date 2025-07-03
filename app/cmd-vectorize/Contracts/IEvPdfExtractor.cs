namespace Evanto.Mcp.Vectorize.Contracts;

public interface IEvPdfTextExtractor
{
    Task<String> ExtractTextAsync(String filePath);
    List<String> ChunkText(String text, Int32 chunkSize, Int32 overlap);
}