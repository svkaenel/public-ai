///-------------------------------------------------------------------------------------------------
/// <summary>   Configuration settings for the WebUI application. </summary>
///
/// <remarks>   SvK, 04.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
///
namespace Evanto.Mcp.Common.Settings;

public class EvWebSettings
{
    public Int32    Port            { get; set; } = 5558;
    public Int32    HttpsPort       { get; set; } = 7555;

    public String   HttpUrl         => $"http://0.0.0.0:{Port}";
    public String   HttpsUrl        => $"https://0.0.0.0:{HttpsPort}";

    public String[] GetUrls()       => new[] { HttpUrl, HttpsUrl };
    public String[] GetHttpUrls()   => new[] { HttpUrl };
}
