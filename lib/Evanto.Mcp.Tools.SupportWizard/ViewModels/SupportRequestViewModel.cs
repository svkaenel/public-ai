using System.Text.Json;
using Evanto.Mcp.Tools.SupportWizard.Models;

namespace Evanto.Mcp.Tools.SupportWizard.ViewModels;

///-------------------------------------------------------------------------------------------------
/// <summary>   Support Request Data Transfer Object. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class SupportRequestViewModel
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the unique identifier. </summary>
    ///
    /// <value> The unique identifier. </value>
    ///-------------------------------------------------------------------------------------------------
    public Guid Uid { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the customer email. </summary>
    ///
    /// <value> The customer email. </value>
    ///-------------------------------------------------------------------------------------------------
    public String CustomerEmail { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the name of the customer. </summary>
    ///
    /// <value> The name of the customer. </value>
    ///-------------------------------------------------------------------------------------------------
    public String CustomerName { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the channel. </summary>
    ///
    /// <value> The channel. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Channel { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the received at. </summary>
    ///
    /// <value> The received at. </value>
    ///-------------------------------------------------------------------------------------------------
    public DateTimeOffset ReceivedAt { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the subject. </summary>
    ///
    /// <value> The subject. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Subject { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the description. </summary>
    ///
    /// <value> The description. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Description { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the topic. </summary>
    ///
    /// <value> The topic. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Topic { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the priority. </summary>
    ///
    /// <value> The priority. </value>
    ///-------------------------------------------------------------------------------------------------
    public byte Priority { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the status. </summary>
    ///
    /// <value> The status. </value>
    ///-------------------------------------------------------------------------------------------------
    public String Status { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the assigned to user uid. </summary>
    ///
    /// <value> The assigned to user uid. </value>
    ///-------------------------------------------------------------------------------------------------
    public Guid? AssignedToUserUid { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the assigned to user name. </summary>
    ///
    /// <value> The name of the assigned to user. </value>
    ///-------------------------------------------------------------------------------------------------
    public String? AssignedToUserName { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the resolved at. </summary>
    ///
    /// <value> The resolved at. </value>
    ///-------------------------------------------------------------------------------------------------
    public DateTimeOffset? ResolvedAt { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the resolution notes. </summary>
    ///
    /// <value> The resolution notes. </value>
    ///-------------------------------------------------------------------------------------------------
    public String? ResolutionNotes { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the created at. </summary>
    ///
    /// <value> The created at. </value>
    ///-------------------------------------------------------------------------------------------------
    public DateTimeOffset CreatedAt { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the updated at. </summary>
    ///
    /// <value> The updated at. </value>
    ///-------------------------------------------------------------------------------------------------
    public DateTimeOffset UpdatedAt { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Initializes this DTO from a SupportRequest entity. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequest"> The support request entity. </param>
    ///
    /// <returns>   This SupportRequestViewModel instance for method chaining. </returns>
    ///-------------------------------------------------------------------------------------------------
    public SupportRequestViewModel InitFrom(SupportRequest supportRequest)
    {
        ArgumentNullException.ThrowIfNull(supportRequest);

        Uid = supportRequest.Uid;
        CustomerEmail = supportRequest.CustomerEmail;
        CustomerName = supportRequest.CustomerName;
        Channel = supportRequest.Channel.ToString();
        ReceivedAt = supportRequest.ReceivedAt;
        Subject = supportRequest.Subject;
        Description = supportRequest.Description;
        Topic = supportRequest.Topic.ToString();
        Priority = (byte)supportRequest.Priority;
        Status = supportRequest.Status.ToString();
        AssignedToUserUid = supportRequest.AssignedToUserUid;
        AssignedToUserName = supportRequest.AssignedToUser?.Name;
        ResolvedAt = supportRequest.ResolvedAt;
        ResolutionNotes = supportRequest.ResolutionNotes;
        CreatedAt = supportRequest.CreatedAt;
        UpdatedAt = supportRequest.UpdatedAt;

        return this;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new SupportRequest entity from this DTO. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <returns>   A new SupportRequest entity. </returns>
    ///-------------------------------------------------------------------------------------------------
    public SupportRequest CreateEntity()
    {
        if (!Enum.TryParse<Channel>(Channel, out var channelEnum))
        {
            throw new ArgumentException($"Invalid channel value: {Channel}");
        }

        if (!Enum.TryParse<Topic>(Topic, out var topicEnum))
        {
            throw new ArgumentException($"Invalid topic value: {Topic}");
        }

        if (!Enum.TryParse<Models.Status>(Status, out var statusEnum))
        {
            throw new ArgumentException($"Invalid status value: {Status}");
        }

        if (!Enum.IsDefined(typeof(Models.Priority), Priority))
        {
            throw new ArgumentException($"Invalid priority value: {Priority}");
        }

        var priorityEnum = (Models.Priority)Priority;

        return new SupportRequest
        {
            CustomerEmail = CustomerEmail,
            CustomerName = CustomerName,
            Channel = channelEnum,
            ReceivedAt = ReceivedAt == default ? DateTimeOffset.UtcNow : ReceivedAt,
            Subject = Subject,
            Description = Description,
            Topic = topicEnum,
            Priority = priorityEnum,
            Status = statusEnum,
            AssignedToUserUid = AssignedToUserUid,
            ResolvedAt = ResolvedAt,
            ResolutionNotes = ResolutionNotes
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates an existing SupportRequest entity with values from this DTO. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequest"> The support request entity to update. </param>
    ///
    /// <returns>   The updated SupportRequest entity. </returns>
    ///-------------------------------------------------------------------------------------------------
    public SupportRequest UpdateEntity(SupportRequest supportRequest)
    {
        ArgumentNullException.ThrowIfNull(supportRequest);

        if (!Enum.TryParse<Channel>(Channel, out var channelEnum))
        {
            throw new ArgumentException($"Invalid channel value: {Channel}");
        }

        if (!Enum.TryParse<Topic>(Topic, out var topicEnum))
        {
            throw new ArgumentException($"Invalid topic value: {Topic}");
        }

        if (!Enum.TryParse<Models.Status>(Status, out var statusEnum))
        {
            throw new ArgumentException($"Invalid status value: {Status}");
        }

        if (!Enum.IsDefined(typeof(Models.Priority), Priority))
        {
            throw new ArgumentException($"Invalid priority value: {Priority}");
        }

        var priorityEnum = (Models.Priority)Priority;

        supportRequest.CustomerEmail = CustomerEmail;
        supportRequest.CustomerName = CustomerName;
        supportRequest.Channel = channelEnum;
        supportRequest.ReceivedAt = ReceivedAt;
        supportRequest.Subject = Subject;
        supportRequest.Description = Description;
        supportRequest.Topic = topicEnum;
        supportRequest.Priority = priorityEnum;
        supportRequest.Status = statusEnum;
        supportRequest.AssignedToUserUid = AssignedToUserUid;
        supportRequest.ResolvedAt = ResolvedAt;
        supportRequest.ResolutionNotes = ResolutionNotes;

        return supportRequest;
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