using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ProjectExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>Serialize an object without recursion.</summary>
    /// 
    /// <param name="obj"> The object. </param>
    /// 
    /// <returns> The serialized object. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static String Serialize(this Object? obj)
    {
        if (obj == null)
        {   // protect against null objects
            return String.Empty;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler        = ReferenceHandler.IgnoreCycles,
            WriteIndented           = true,
            DefaultIgnoreCondition  = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(obj, jsonOptions);
    }
}
