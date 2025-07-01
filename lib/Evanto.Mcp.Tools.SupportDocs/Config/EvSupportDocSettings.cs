using System;

namespace Evanto.Mcp.Tools.SupportDocs.Config;

/// <summary>
/// Configuration für die Produktdokumentation.
/// Created: 30.05.2025
/// </summary>
public class EvSupportDocSettings
{
    private static String GetDefaultOllamaEndpoint()
    {   // check if running in Docker container
        var inDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
                      File.Exists("/.dockerenv");
        
        return inDocker ? "http://host.docker.internal:11434" : "http://localhost:11434";
    }

    private static String GetDefaultQdrantEndpoint()
    {   // check if running in Docker container  
        var inDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
                      File.Exists("/.dockerenv");
        
        return inDocker ? "host.docker.internal" : "localhost";
    }

    public String                                           OllamaEndpoint          { get; set; } = GetDefaultOllamaEndpoint();
    public String                                           OllamaModel             { get; set; } = "nomic-embed-text";
    public String                                           QdrantEndpoint          { get; set; } = GetDefaultQdrantEndpoint();
    public String                                           VectorCollectionName    { get; set; } = "br_pdf_documents";
    public Int32                                            SearchLimit             { get; set; } = 10;
    public Single                                           MinimumScore            { get; set; } = 0.5f;
}
