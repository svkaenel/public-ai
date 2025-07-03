namespace Evanto.Mcp.Vectorize.Models;

public class EvProcessingResult
{
    public int ProcessedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
    
    public int TotalFiles => ProcessedCount + SkippedCount + ErrorCount;
    
    public bool HasErrors => ErrorCount > 0;
    
    public bool IsSuccessful => ErrorCount == 0 && (ProcessedCount > 0 || SkippedCount > 0);
    
    public void AddError(string fileName, string errorMessage)
    {
        Errors[fileName] = errorMessage;
        ErrorCount++;
    }
    
    public void AddProcessed()
    {
        ProcessedCount++;
    }
    
    public void AddSkipped()
    {
        SkippedCount++;
    }
    
    public override string ToString()
    {
        return $"Processing Result: {ProcessedCount} processed, {SkippedCount} skipped, {ErrorCount} errors";
    }
}
