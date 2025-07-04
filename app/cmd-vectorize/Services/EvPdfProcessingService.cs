using Microsoft.Extensions.Logging;
using Evanto.Mcp.Vectorize.Contracts;
using Evanto.Mcp.Embeddings.Contracts;
using Evanto.Mcp.Vectorize.Settings;
using Evanto.Mcp.Vectorize.Models;
using Evanto.Mcp.QdrantDB.Contracts;
using Evanto.Mcp.QdrantDB.Models;
using Microsoft.Extensions.Options;

namespace Evanto.Mcp.Vectorize.Services;

///-------------------------------------------------------------------------------------------------
/// <summary>   PDF processing service implementation. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvPdfProcessingService(
    IEvPdfTextExtractor textExtractor,
    IEvEmbeddingService embeddingService,
    IEvDocumentRepository documentRepository,
    IEvFileTrackingService fileTrackingService,
    IOptions<EvVectorizeAppSettings> settings,
    ILogger<EvPdfProcessingService> logger) : IEvPdfProcessingService
{
    private readonly IEvPdfTextExtractor mTextExtractor = textExtractor;
    private readonly IEvEmbeddingService mEmbeddingService = embeddingService;
    private readonly IEvDocumentRepository mDocumentRepository = documentRepository;
    private readonly IEvFileTrackingService mFileTrackingService = fileTrackingService;
    private readonly EvVectorizeAppSettings mSettings = settings.Value;
    private readonly ILogger<EvPdfProcessingService> mLogger = logger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Processes all new PDF files found in the configured directory. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <returns>   A EvProcessingResult summarizing the outcome of the processing. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvProcessingResult> ProcessNewPdfsAsync()
    {   // process all new PDF files found in directory
        var result = new EvProcessingResult();

        try
        {   // initialize document repository
            await mDocumentRepository.InitializeAsync();

            // ensure PDF directory exists
            if (!Directory.Exists(mSettings.FullPdfDirectory))
            {   // create directory if missing
                mLogger.LogWarning("PDF directory does not exist: {Directory}", mSettings.FullPdfDirectory);
                Directory.CreateDirectory(mSettings.FullPdfDirectory);
                return result; // return empty result if directory had to be created
            }

            // get all PDF files
            var pdfFiles = Directory.GetFiles(mSettings.FullPdfDirectory, "*.pdf", SearchOption.TopDirectoryOnly);

            if (!pdfFiles.Any())
            {   // no files found
                mLogger.LogInformation("No PDF files found in directory: {Directory}", mSettings.FullPdfDirectory);
                return result; // return empty result if no files found
            }

            mLogger.LogInformation("Found {Count} PDF files in directory", pdfFiles.Length);

            // process each file
            foreach (var filePath in pdfFiles)
            {
                try
                {   // process individual file
                    var fileResult = await ProcessSpecificPdfAsync(filePath);
                    result.ProcessedCount += fileResult.ProcessedCount;
                    result.SkippedCount += fileResult.SkippedCount;
                    result.ErrorCount += fileResult.ErrorCount;

                    foreach (var error in fileResult.Errors)
                    {   // add errors to result
                        result.Errors[error.Key] = error.Value;
                    }

                }

                catch (Exception ex)
                {   // handle individual file processing error
                    mLogger.LogError(ex, "Failed to process PDF: {FilePath}", filePath);
                    result.ErrorCount++;
                    result.Errors[Path.GetFileName(filePath)] = ex.Message;
                }
            }

            return result;
        }

        catch (Exception ex)
        {   // handle overall processing error
            mLogger.LogError(ex, "Failed to process PDFs");
            throw; // re-throw the exception to be handled by the caller
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Processes a specific PDF file. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="filePath">   The absolute path to the PDF file. </param>
    ///
    /// <returns>   A EvProcessingResult summarizing the outcome of processing the single file. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvProcessingResult> ProcessSpecificPdfAsync(String filePath)
    {   // process specific PDF file
        var result = new EvProcessingResult();
        var fileName = Path.GetFileName(filePath);

        try
        {   // start processing
            mLogger.LogInformation("Processing PDF: {FileName}", fileName);

            // calculate file hash
            var fileHash = mFileTrackingService.CalculateFileHash(filePath);

            // check if already processed
            if (await mFileTrackingService.IsFileProcessedAsync(filePath, fileHash))
            {   // already processed
                mLogger.LogInformation("PDF already processed: {FileName}", fileName);
                result.SkippedCount = 1;
                return result;
            }

            // extract text
            var text = await mTextExtractor.ExtractTextAsync(filePath);

            if (String.IsNullOrWhiteSpace(text))
            {   // no text content
                mLogger.LogWarning("No text extracted from PDF: {FileName}", fileName);
                result.ErrorCount = 1;
                result.Errors[fileName] = "No text content found";
                return result;
            }

            // chunk text
            var chunks = mTextExtractor.ChunkText(text, mSettings.Embeddings!.ChunkSize, mSettings.Embeddings!.ChunkOverlap);

            if (chunks == null || !chunks.Any()) // added null check for safety
            {   // no chunks created
                mLogger.LogWarning("No text chunks created for PDF: {FileName}", fileName);
                result.ErrorCount = 1;
                result.Errors[fileName] = "No text chunks created";
                return result;
            }

            mLogger.LogInformation("Created {ChunkCount} chunks for PDF: {FileName}", chunks.Count, fileName);

            // generate embeddings
            var embeddings = await mEmbeddingService.GenerateEmbeddingsAsync(chunks);

            // create document records using unified model
            var documents = new List<EvDocument>();
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);

            for (var i = 0; i < chunks.Count; i++)
            {   // create document record for each chunk
                var documentId = Guid.NewGuid().ToString(); // use pure GUID for Qdrant compatibility
                var chunkId = $"{baseFileName}_{i}";

                documents.Add(new EvDocument
                {
                    Id = documentId,
                    FileName = fileName,
                    Content = chunks[i],
                    Vector = embeddings[i],
                    ProcessedAt = DateTime.UtcNow,
                    FileHash = fileHash,
                    ChunkIndex = i,
                    TotalChunks = chunks.Count,
                    BaseFileName = baseFileName,
                    ChunkId = chunkId
                });
            }

            // store in document repository
            await mDocumentRepository.StoreDocumentsAsync(documents);

            // mark as processed
            await mFileTrackingService.AddProcessedFileAsync(filePath, fileHash);

            mLogger.LogInformation("Successfully processed PDF: {FileName} ({ChunkCount} chunks)", fileName, chunks.Count);
            result.ProcessedCount = 1;

            return result;
        }

        catch (Exception ex)
        {   // handle processing error
            mLogger.LogError(ex, "Failed to process PDF: {FileName}", fileName);

            result.ErrorCount = 1;
            result.Errors[fileName] = ex.Message;

            return result;
        }
    }
}