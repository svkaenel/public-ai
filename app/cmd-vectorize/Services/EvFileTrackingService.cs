using System.Security.Cryptography; // Required for SHA256
using System.Text.Json; // Required for JsonSerializer, JsonSerializerOptions
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Evanto.Mcp.Vectorize.Settings;
using Evanto.Mcp.Vectorize.Contracts;

namespace Evanto.Mcp.Vectorize.Services;

///-------------------------------------------------------------------------------------------------
/// <summary>   Service for tracking processed files to avoid redundant processing. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvFileTrackingService(
    IOptions<EvVectorizeAppSettings>     config,
    ILogger<EvFileTrackingService>       logger) : IEvFileTrackingService
{
    private readonly EvVectorizeAppSettings               mConfig             = config.Value;
    private readonly ILogger<EvFileTrackingService>       mLogger             = logger;
    private readonly Dictionary<String, String>          mProcessedFiles      = []; // use collection expression

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets a set of all processed file names. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <returns>   A HashSet of processed file names. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<HashSet<String>> GetProcessedFilesAsync()
    {   // load tracking data and return processed file names
        await LoadTrackingDataAsync();
        return new HashSet<String>(mProcessedFiles.Keys); // ensure new HashSet is created
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds a file to the list of processed files. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="filePath">   The path to the processed file. </param>
    /// <param name="fileHash">   The hash of the processed file. </param>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task AddProcessedFileAsync(String filePath, String fileHash)
    {   // add file to processed files list
        await LoadTrackingDataAsync();
        
        var fileName                = Path.GetFileName(filePath);
        mProcessedFiles[fileName]   = fileHash;
        
        await SaveTrackingDataAsync();

        mLogger.LogDebug("Added processed file: {FileName} with hash: {Hash}", fileName, fileHash);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Checks if a file has already been processed with the same hash. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="filePath">   The path to the file. </param>
    /// <param name="fileHash">   The hash of the file. </param>
    ///
    /// <returns>   True if file is processed and hash matches; otherwise, false. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Boolean> IsFileProcessedAsync(String filePath, String fileHash)
    {   // check if file was already processed with same hash
        await LoadTrackingDataAsync();
        
        var fileName = Path.GetFileName(filePath);

        return mProcessedFiles.TryGetValue(fileName, out var existingHash) && existingHash == fileHash;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Calculates the SHA256 hash of a file. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="filePath">   The path to the file. </param>
    ///
    /// <returns>   The SHA256 hash of the file as a hexadecimal string. </returns>
    ///-------------------------------------------------------------------------------------------------
    public String CalculateFileHash(String filePath)
    {   // calculate file hash using SHA256
        using var sha256    = SHA256.Create();
        using var stream    = File.OpenRead(filePath);
        
        var hashBytes       = sha256.ComputeHash(stream);

        return Convert.ToHexString(hashBytes);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Loads tracking data from the configured file path. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    private async Task LoadTrackingDataAsync()
    {   // load tracking data from file
        try
        {   // check if tracking file exists
            if (File.Exists(mConfig.TrackingFilePath))
            {   // read and deserialize tracking data
                var json = await File.ReadAllTextAsync(mConfig.TrackingFilePath);
                var data = JsonSerializer.Deserialize<Dictionary<String, String>>(json);
                
                if (data != null)
                {   // clear and reload processed files
                    mProcessedFiles.Clear();
                    foreach (var kvp in data)
                    {
                        mProcessedFiles[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        catch (Exception ex)
        {   // log warning on load failure
            mLogger.LogWarning(ex, "Failed to load tracking data from {Path}", mConfig.TrackingFilePath);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Saves the current tracking data to the configured file path. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    private async Task SaveTrackingDataAsync()
    {   // save tracking data to file
        try
        {   // ensure directory exists
            var directory = Path.GetDirectoryName(mConfig.TrackingFilePath);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {   // create directory if needed
                Directory.CreateDirectory(directory);
            }

            // serialize and save tracking data
            var json = JsonSerializer.Serialize(mProcessedFiles, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(mConfig.TrackingFilePath, json);
        }

        catch (Exception ex)
        {   // log error on save failure
            mLogger.LogError(ex, "Failed to save tracking data to {Path}", mConfig.TrackingFilePath);
            // consider re-throwing or handling more gracefully depending on requirements
        }
    }
}
