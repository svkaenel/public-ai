using System;

namespace Evanto.Mcp.Common.Settings;

public class EvQdrantSettings
{
    public String       QdrantEndpoint          { get; set; } = "localhost";
    public Int32        QdrantPort              { get; set; } = 6334;
    public String       VectorCollectionName    { get; set; } = "ev_support_documents";
    
    // The dimension of the vector used for embeddings, 768 for nomic_embed_text.
    public UInt64       VectorDimension         { get; set; } = 768;
    public Int32        SearchLimit             { get; set; } = 10;
    public Single       MinimumScore            { get; set; } = 0.5f;

}
