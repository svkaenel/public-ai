using Evanto.Mcp.Apps.Extensions;
using Evanto.Mcp.Common.Settings; 

namespace Evanto.Mcp.Vectorize.Settings;

///-------------------------------------------------------------------------------------------------
/// <summary>   Configuration settings for vectorization application. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvVectorizeAppSettings : EvBaseAppSettings
{
    public           String                     PdfDirectory            { get; set; } = "./pdfs";
    public           String                     FullPdfDirectory        { get => PdfDirectory.ResolveRelative(); }
    public           String                     TrackingFilePath        { get; set; } = "./processed_files.json";
    public           EvEmbeddingSettings?       Embeddings              { get; set; } = null;
    public           EvQdrantSettings?          Qdrant                  { get; set; } = null;
}