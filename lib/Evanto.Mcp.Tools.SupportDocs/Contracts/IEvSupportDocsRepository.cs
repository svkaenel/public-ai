using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Evanto.Mcp.Tools.SupportDocs.ViewModels;

namespace Evanto.Mcp.Tools.SupportDocs.Contracts;

public interface IEvSupportDocsRepository
{
    Task<IEnumerable<EvSupportDocViewModel>>    GetSupportDocsAsync(String query, Int32 limit = 10);
    Task<IEnumerable<String>>                   GetFileNames(String query, Int32 limit = 10);
}
