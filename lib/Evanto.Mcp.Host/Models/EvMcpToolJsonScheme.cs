using System;
using System.Text.Json.Serialization;

namespace Evanto.Mcp.Host.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>
/// Represents the overall JSON Schema.
/// </summary>
///-------------------------------------------------------------------------------------------------
public class EvMcpToolJsonScheme
{
    [JsonPropertyName("type")]
    public String                                           Type            { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, EvJsonPropertyDefinition>?    Properties      { get; set; }

    [JsonPropertyName("required")]
    public List<String>?                                    Required        { get; set; }
}

///-------------------------------------------------------------------------------------------------
/// <summary>
/// Definition of a single property (type, description, ...).
/// </summary>
///-------------------------------------------------------------------------------------------------
public class EvJsonPropertyDefinition
{
    [JsonPropertyName("type")]
    public String                                           Type            { get; set; } = "string";

    [JsonPropertyName("description")]
    public String                                           Description     { get; set; } = string.Empty;
}
