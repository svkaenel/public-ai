using System.ComponentModel.DataAnnotations;

namespace Evanto.Mcp.Tools.SupportWizard.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Support team user entity. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class User
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the unique identifier (Primary Key). </summary>
    ///
    /// <value> The unique identifier. </value>
    ///-------------------------------------------------------------------------------------------------
    public Guid Uid { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the name of the user. </summary>
    ///
    /// <value> The name. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    [MaxLength(100)]
    public String Name { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the email address. </summary>
    ///
    /// <value> The email. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public String Email { get; set; } = String.Empty;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the topic this user is responsible for. </summary>
    ///
    /// <value> The topic. </value>
    ///-------------------------------------------------------------------------------------------------
    [Required]
    public Topic Topic { get; set; }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Navigation property for assigned support requests. </summary>
    ///
    /// <value> The assigned support requests. </value>
    ///-------------------------------------------------------------------------------------------------
    public virtual ICollection<SupportRequest> AssignedSupportRequests { get; set; } = new List<SupportRequest>();
}