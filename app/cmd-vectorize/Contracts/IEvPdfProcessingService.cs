using Evanto.Mcp.Vectorize.Models;

namespace Evanto.Mcp.Vectorize.Contracts;

///-------------------------------------------------------------------------------------------------
/// <summary>   Interface for PDF processing service. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public interface IEvPdfProcessingService
{
    Task<EvProcessingResult>    ProcessNewPdfsAsync();
    Task<EvProcessingResult>    ProcessSpecificPdfAsync(String filePath);
}