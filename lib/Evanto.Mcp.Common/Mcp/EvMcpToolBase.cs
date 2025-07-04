using System;
using Evanto.Mcp.Common.Models;

namespace Evanto.Mcp.Common.Mcp;

public abstract class EvMcpToolBase
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Validates that a string is not empty or whitespace. </summary>
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="value">        The string value to validate. </param>
    /// <param name="errorMessage"> The error message to return if validation fails. </param>
    /// 
    /// <returns>   A JSON string representing an error response if validation fails, otherwise null. </returns
    ///-------------------------------------------------------------------------------------------------
    protected String? ValidateNotEmpty(String value, String errorMessage)
    {
        if (String.IsNullOrWhiteSpace(value))
            return EvMcpToolResponse<Object>.Error(errorMessage).ToJson();

        return null;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Validates that a number is within a specified range. </summary>
    /// 
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="value">        The string representation of the number to validate. </param>
    /// <param name="errorMessage"> The error message to return if validation fails. </param>
    /// <param name="isValid">      A function that checks if the number is valid according to custom logic. </param>
    /// 
    /// <returns>   A JSON string representing an error response if validation fails, otherwise null. </returns>
    ///-------------------------------------------------------------------------------------------------
    protected String? ValidateNumber(
        Int32                   value,
        String                  errorMessage,
        Func<Int32, Boolean>    isValid)
    {
        if (!isValid(value))
            return EvMcpToolResponse<Object>.Error(errorMessage).ToJson();

        return null;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Validates that a Guid is not empty. </summary>
    /// 
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="value">        The Guid value to validate. </param>
    /// <param name="errorMessage"> The error message to return if validation fails. </param>
    /// 
    /// <returns>   A JSON string representing an error response if validation fails, otherwise null</returns>
    ///-------------------------------------------------------------------------------------------------
    protected String? ValidateUid(Guid value, String errorMessage)
    {
        if (value == Guid.Empty)
            return EvMcpToolResponse<Object>.Error(errorMessage).ToJson();

        return null;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Validates that an enum value is defined. </summary>
    /// 
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <typeparam name="TEnum">    The type of the enum to validate. </typeparam>
    /// <param name="value">        The enum value as string to validate. </param>
    /// <param name="errorMessage"> The error message to return if validation fails. </param>
    /// 
    /// <returns>   A JSON string representing an error response if validation fails, otherwise null. </returns>
    ///-------------------------------------------------------------------------------------------------
    protected String? ValidateEnum<TEnum>(
        String  value,
        String  errorMessage) where TEnum : struct, Enum
    {
        if (String.IsNullOrWhiteSpace(value) ||
            !Enum.TryParse<TEnum>(value, out var valueEnum) ||
            !Enum.IsDefined(typeof(TEnum), valueEnum))
        {
            return EvMcpToolResponse<Object>.Error(errorMessage).ToJson();
        }

        return null;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Executes an asynchronous action and returns a JSON response. </summary>
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <typeparam name="T">          The type of the result. </typeparam>
    /// <typeparam name="D">          The type of the DTO to convert to, if applicable. </typeparam>
    /// 
    /// <param name="action">        The asynchronous action to execute. </param>
    /// <param name="isEmpty">       A function to determine if the result is empty. </param>
    /// <param name="convertToDto">  An optional function to convert the result to a DTO. </param>
    /// <param name="notFoundMessage">The message to return if the result is not found. </param>
    /// 
    /// <returns>   A JSON string representing the result of the action. </returns>
    ///-------------------------------------------------------------------------------------------------
    protected async Task<String> ExecuteAsync<T, D>(
        Func<Task<T>> action,
        Func<T, Boolean> isEmpty,
        Func<T, D>? convertToDto,
        String notFoundMessage)
    {
        try
        {   // execute the action and check if the result is empty
            var result = await action();
            if (isEmpty(result))
                return EvMcpToolResponse<T>.NotFound(notFoundMessage).ToJson();

            if (convertToDto != null)
            {   // convert the result to DTO if needed
                var dto = convertToDto(result);

                return EvMcpToolResponse<D>.Success(dto).ToJson();
            }

            return EvMcpToolResponse<T>.Success(result).ToJson();
        }

        catch (Exception ex)
        {   // don't use logger because of stdio transport
            Console.Error.WriteLine($"[{DateTime.UtcNow:o}] {ex}");
            return EvMcpToolResponse<T>.Error(ex.Message, ex.GetType().Name).ToJson();
        }
    }
}