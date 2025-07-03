using Evanto.Mcp.Vectorize.Models;

namespace Evanto.Mcp.Vectorize.Contracts;

public interface IEvPdfProcessingService
{
    Task<EvProcessingResult> ProcessNewPdfsAsync();
    Task<EvProcessingResult> ProcessSpecificPdfAsync(String filePath);
}