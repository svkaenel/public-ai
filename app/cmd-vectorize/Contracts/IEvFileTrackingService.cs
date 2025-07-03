namespace Evanto.Mcp.Vectorize.Contracts;

///-------------------------------------------------------------------------------------------------
/// <summary>   Interface for file tracking service. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public interface IEvFileTrackingService
{
    Task<HashSet<String>>   GetProcessedFilesAsync();
    Task                    AddProcessedFileAsync(String filePath, String fileHash);
    Task<Boolean>           IsFileProcessedAsync(String filePath, String fileHash);
    String                  CalculateFileHash(String filePath);
}