using System;
using System.Text.Json.Serialization;

namespace Evanto.Mcp.Common.Models;

public class EvMcpToolResponse<T>
{
    [JsonPropertyName("status")]
    public String Status { get; set; } = "success";

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public String? Message { get; set; }

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public String? Details { get; set; }

    public static EvMcpToolResponse<T> Success(T data) =>
        new() { Status = "success", Data = data };

    public static EvMcpToolResponse<T> NotFound(string message) =>
        new() { Status = "not_found", Message = message };

    public static EvMcpToolResponse<T> Error(string message, String? details = null) =>
        new() { Status = "error", Message = message, Details = details };
}
