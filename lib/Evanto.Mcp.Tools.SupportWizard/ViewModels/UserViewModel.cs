using System.Text.Json;
using Evanto.Mcp.Tools.SupportWizard.Models;

namespace Evanto.Mcp.Tools.SupportWizard.ViewModels;

///-------------------------------------------------------------------------------------------------
/// <summary>   User Data Transfer Object. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class UserViewModel
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the unique identifier. </summary>
    ///
    /// <value> The unique identifier. </value>
    ///-------------------------------------------------------------------------------------------------
    public Guid Uid { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the name. </summary>
    ///
    /// <value> The name. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Name { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the email. </summary>
    ///
    /// <value> The email. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Email { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the topic. </summary>
    ///
    /// <value> The topic. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Topic { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Initializes this DTO from a User entity. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="user"> The user entity. </param>
    ///
    /// <returns>   This UserViewModel instance for method chaining. </returns>
    ///-------------------------------------------------------------------------------------------------
    public UserViewModel InitFrom(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Uid = user.Uid;
        Name = user.Name;
        Email = user.Email;
        Topic = user.Topic.ToString();

        return this;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new User entity from this DTO. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <returns>   A new User entity. </returns>
    ///-------------------------------------------------------------------------------------------------
    public User CreateEntity()
    {
        if (!Enum.TryParse<Topic>(Topic, out var topicEnum))
        {
            throw new ArgumentException($"Invalid topic value: {Topic}");
        }

        return new User
        {
            Name = Name,
            Email = Email,
            Topic = topicEnum
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates an existing User entity with values from this DTO. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="user"> The user entity to update. </param>
    ///
    /// <returns>   The updated User entity. </returns>
    ///-------------------------------------------------------------------------------------------------
    public User UpdateEntity(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!Enum.TryParse<Topic>(Topic, out var topicEnum))
        {
            throw new ArgumentException($"Invalid topic value: {Topic}");
        }

        user.Name = Name;
        user.Email = Email;
        user.Topic = topicEnum;

        return user;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Serializes this instance to JSON. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <returns>   JSON representation of this instance. </returns>
    ///-------------------------------------------------------------------------------------------------
    public String Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }
}