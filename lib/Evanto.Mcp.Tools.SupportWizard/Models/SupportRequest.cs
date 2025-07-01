using System.ComponentModel.DataAnnotations;

namespace Evanto.Mcp.Tools.SupportWizard.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Support request entity. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class SupportRequest
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the unique identifier (Primary Key). </summary>
    ///
    /// <value> The unique identifier. </value>
    ///-------------------------------------------------------------------------------------------------
    public Guid Uid { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the customer email address. </summary>
    ///
    /// <value> The customer email. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public String CustomerEmail { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the name of the customer. </summary>
    ///
    /// <value> The name of the customer. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    [MaxLength(100)]
    public String CustomerName { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the channel through which the request was received. </summary>
    ///
    /// <value> The channel. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public Channel Channel { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the date and time when the request was received. </summary>
    ///
    /// <value> The received at. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public DateTimeOffset ReceivedAt { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the subject/title of the request. </summary>
    ///
    /// <value> The subject. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    [MaxLength(200)]
    public String Subject { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the detailed description. </summary>
    ///
    /// <value> The description. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public String Description { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the topic/category. </summary>
    ///
    /// <value> The topic. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public Topic Topic { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the priority level. </summary>
    ///
    /// <value> The priority. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public Priority Priority { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the current status. </summary>
    ///
    /// <value> The status. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public Status Status { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the assigned user identifier (Foreign Key). </summary>
    ///
    /// <value> The assigned to user uid. </value>
    ///-------------------------------------------------------------------------------------------------
    public Guid? AssignedToUserUid { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the date and time when resolved. </summary>
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
    /// <summary>   Gets or sets the date and time when created. </summary>
    ///
    /// <value> The created at. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the date and time when last updated. </summary>
    ///
    /// <value> The updated at. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public DateTimeOffset UpdatedAt { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Navigation property for the assigned user. </summary>
    ///
    /// <value> The assigned to user. </value>
    ///-------------------------------------------------------------------------------------------------
    public virtual User? AssignedToUser { get; set; }
}