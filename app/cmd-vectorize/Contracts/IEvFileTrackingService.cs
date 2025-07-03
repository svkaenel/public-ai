namespace Evanto.Mcp.Vectorize.Contracts;

public interface IEvFileTrackingService
{
    Task<HashSet<String>>   GetProcessedFilesAsync();
    Task                    AddProcessedFileAsync(String filePath, String fileHash);
    Task<Boolean>           IsFileProcessedAsync(String filePath, String fileHash);
    String                  CalculateFileHash(String filePath);
}