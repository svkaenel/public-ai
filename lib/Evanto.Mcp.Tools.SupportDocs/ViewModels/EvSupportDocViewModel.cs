using System;

namespace Evanto.Mcp.Tools.SupportDocs.ViewModels;

public class EvSupportDocViewModel
{
    public String       FileName                { get; set; } = String.Empty;
    public String       Content                 { get; set; } = String.Empty;
    public Single       Score                   { get; set; }
    public Int32        ChunkIndex              { get; set; }
    public Int32        TotalChunks             { get; set; }
    public String       ChunkId                 { get; set; } = String.Empty;
    public String       BaseFileName            { get; set; } = String.Empty;
}
