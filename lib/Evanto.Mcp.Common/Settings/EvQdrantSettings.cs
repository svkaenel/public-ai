using System;

namespace Evanto.Mcp.Common.Settings;

public class EvQdrantSettings
{
    public String       QdrantEndpoint          { get; set; } = "localhost";
    public Int32        QdrantPort              { get; set; } = 6334;
    public String       VectorCollectionName    { get; set; } = "support_documents";
    public Int32        SearchLimit             { get; set; } = 10;
    public Single       MinimumScore            { get; set; } = 0.5f;

}
