namespace Evanto.Mcp.Vectorize.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Result of PDF processing operation. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvProcessingResult
{
    public           Int32                              ProcessedCount     { get; set; }
    public           Int32                              SkippedCount       { get; set; }
    public           Int32                              ErrorCount         { get; set; }
    public           Dictionary<String, String>         Errors             { get; set; } = new();
    
    public           Int32                              TotalFiles         => ProcessedCount + SkippedCount + ErrorCount;
    public           Boolean                            HasErrors          => ErrorCount > 0;
    public           Boolean                            IsSuccessful       => ErrorCount == 0 && (ProcessedCount > 0 || SkippedCount > 0);
    
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds an error to the processing result. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="fileName">       Name of the file. </param>
    /// <param name="errorMessage">   Message describing the error. </param>
    ///-------------------------------------------------------------------------------------------------
    public void AddError(String fileName, String errorMessage)
    {   // add error to collection and increment count
        Errors[fileName] = errorMessage;
        ErrorCount++;
    }
    
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Increments the processed count. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public void AddProcessed()
    {   // increment processed count
        ProcessedCount++;
    }
    
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Increments the skipped count. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public void AddSkipped()
    {   // increment skipped count
        SkippedCount++;
    }
    
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Returns a string representation of this object. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <returns>   A String that represents this object. </returns>
    ///-------------------------------------------------------------------------------------------------
    public override String ToString()
    {   // return summary of processing result
        return $"Processing Result: {ProcessedCount} processed, {SkippedCount} skipped, {ErrorCount} errors";
    }
}
