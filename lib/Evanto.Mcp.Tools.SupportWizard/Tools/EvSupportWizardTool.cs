using System.ComponentModel;
using Evanto.Mcp.Common.Mcp;
using Evanto.Mcp.Tools.SupportWizard.Contracts;
using Evanto.Mcp.Tools.SupportWizard.Models;
using Evanto.Mcp.Tools.SupportWizard.ViewModels;
using ModelContextProtocol.Server;

namespace Evanto.Mcp.Tools.SupportWizard.Tools;

///-------------------------------------------------------------------------------------------------
/// <summary>   MCP Tool implementation for SupportWizard operations. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
[McpServerToolType]
public class EvSupportWizardTool(ISupportWizardRepository supportWizardRepository) : EvMcpToolBase
{
    private readonly ISupportWizardRepository mSupportWizardRepository = supportWizardRepository;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets the support requests for a customer by email. </summary>
    /// 
    /// <param name="customerEmail"> The email. </param>
    /// 
    /// <returns> The support requests. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Supportanfragen für einen Kunden nach Email")]
    public async Task<String> GetSupportRequestsForCustomerByEmail(String customerEmail)
    {   // check requirements
        var validationError = ValidateNotEmpty(
            customerEmail,
            "Email must not be empty.");

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetSupportRequestsForCustomerByEmailAsync(customerEmail),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found for email '{customerEmail}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets the support requests for a customer by name. </summary>
    /// 
    /// <param name="customerName"> The customer name. </param>
    /// 
    /// <returns> The support requests. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Supportanfragen für einen Kunden nach Name")]
    public async Task<String> GetSupportRequestsForCustomerByName(String customerName)
    {
        var validationError = ValidateNotEmpty(
            customerName,
            "Customer name must not be empty.");

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetSupportRequestsForCustomerByNameAsync(customerName),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found for customer '{customerName}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new support request. </summary>
    /// 
    /// <param name="customerEmail">   The customer email. </param>
    /// <param name="customerName">    The customer name. </param>
    /// <param name="channel">         The channel (Phone, Email, Chat, Web, Mobile). </param>
    /// <param name="subject">         The subject. </param>
    /// <param name="description">     The description. </param>
    /// <param name="topic">           The topic (Billing, Technical, Feature, Account, General). </param>
    /// <param name="priority">        The priority (1=Low, 2=Medium, 3=High). </param>
    /// 
    /// <returns> The created support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Erstellt eine neue Supportanfrage")]
    public async Task<String> CreateSupportRequest(String customerEmail, String customerName, String channel, String subject, String description, String topic, Byte priority)
    {
        var validationError = ValidateNotEmpty(
            customerName,
            "Customer name must not be empty.");

        validationError = validationError ?? ValidateNotEmpty(
            customerEmail,
            "Customer email must not be empty.");

        if (validationError != null)
            return validationError;

        var viewModel = new SupportRequestViewModel
        {
            CustomerEmail = customerEmail,
            CustomerName  = customerName,
            Channel       = channel,
            Subject       = subject,
            Description   = description,
            Topic         = topic,
            Priority      = priority,
            Status        = "New",
            ReceivedAt    = DateTimeOffset.UtcNow
        };

        var entity = viewModel.CreateEntity();

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.CreateSupportRequestAsync(entity),
            result => result == null,
            result => new SupportRequestViewModel().InitFrom(result),
            $"Support request could not be created for customer '{customerName}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates an existing support request. </summary>
    /// 
    /// <param name="uid">             The support request UID. </param>
    /// <param name="customerEmail">   The customer email. </param>
    /// <param name="customerName">    The customer name. </param>
    /// <param name="channel">         The channel. </param>
    /// <param name="subject">         The subject. </param>
    /// <param name="description">     The description. </param>
    /// <param name="topic">           The topic. </param>
    /// <param name="priority">        The priority. </param>
    /// <param name="status">          The status. </param>
    /// <param name="resolutionNotes"> The resolution notes. </param>
    /// 
    /// <returns> The updated support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Aktualisiert eine bestehende Supportanfrage")]
    public async Task<String> UpdateSupportRequest(Guid uid, String customerEmail, String customerName, String channel, String subject, String description, String topic, byte priority, String status, String? resolutionNotes = null)
    {
        var validationError = ValidateNotEmpty(
            customerName,
            "Customer name must not be empty.");

        validationError = validationError ?? ValidateNotEmpty(
            customerEmail,
            "Customer email must not be empty.");

        validationError = validationError ?? ValidateUid(
            uid,
            "Customer key must not be empty.");

        if (validationError != null)
            return validationError;

        var existingRequest = await mSupportWizardRepository.GetSupportRequestByUidAsync(uid);
        if (existingRequest == null)
        {
            return new { status = "not_found", message = $"Support request with UID {uid} not found" }.ToJson();
        }

        var viewModel = new SupportRequestViewModel
        {
            CustomerEmail = customerEmail,
            CustomerName  = customerName,
            Channel       = channel,
            Subject       = subject,
            Description   = description,
            Topic         = topic,
            Priority      = priority,
            Status        = "New",
            ReceivedAt    = DateTimeOffset.UtcNow
        };

        var updatedEntity   = viewModel.UpdateEntity(existingRequest);

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.UpdateSupportRequestAsync(updatedEntity),
            result => result == null,
            result => new SupportRequestViewModel().InitFrom(result),
            $"Support request could not be updated for customer '{customerName}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Assigns a support request to a user. </summary>
    /// 
    /// <param name="supportRequestUid"> The support request UID. </param>
    /// <param name="userUid">           The user UID. </param>
    /// 
    /// <returns> The updated support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Weist eine Supportanfrage einem Benutzer zu")]
    public async Task<String> AssignSupportRequestToUser(Guid supportRequestUid, Guid userUid)
    {
        var validationError = ValidateUid(
            supportRequestUid,
            "Support request key must not be empty.");

        validationError = validationError ?? ValidateUid(
            userUid,
            "User key must not be empty.");

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.AssignSupportRequestToUserAsync(supportRequestUid, userUid),
            result => result == null,
            result => new SupportRequestViewModel().InitFrom(result),
            $"Support request could not be assigned to user key '{userUid}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets users by topic. </summary>
    /// 
    /// <param name="topic"> The topic (Billing, Technical, Feature, Account, General). </param>
    /// 
    /// <returns> The users responsible for the topic. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Findet Benutzer, die für ein bestimmtes Thema verantwortlich sind")]
    public async Task<String> GetUsersByTopic(String topic)
    {
        var validationError = ValidateNotEmpty(
            topic,
            "Topic must not be empty.");
        
        validationError = validationError ?? ValidateEnum<Topic>(
            topic,
            "Invalid topic value. Valid values are: " + String.Join(", ", Enum.GetNames<Topic>()));

        if (validationError != null)
            return validationError;

        var topicEnum = Enum.Parse<Topic>(topic);

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetUsersByTopicAsync(topicEnum),
            results => results == null || !results.Any(),
            results => results.Select(u => new UserViewModel().InitFrom(u)),
            $"No users found for topic '{topic}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates the status of a support request. </summary>
    /// 
    /// <param name="supportRequestUid"> The support request UID. </param>
    /// <param name="status">            The new status (New, InProgress, Resolved, Closed, Cancelled). </param>
    /// <param name="resolutionNotes">   Optional resolution notes. </param>
    /// 
    /// <returns> The updated support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Aktualisiert den Status einer Supportanfrage")]
    public async Task<String> UpdateStatusForSupportRequest(Guid supportRequestUid, String status, String? resolutionNotes = null)
    {
        var validationError = ValidateUid(
            supportRequestUid,
            "Supprt request key must not be empty.");
        
        validationError = validationError ?? ValidateEnum<Models.Status>(
            status,
            "Invalid status value. Valid values are: " + String.Join(", ", Enum.GetNames<Models.Status>()));

        if (validationError != null)
            return validationError;

        var statusEnum = Enum.Parse<Models.Status>(status);

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.UpdateStatusForSupportRequestAsync(supportRequestUid, statusEnum, resolutionNotes),
            results => results == null,
            results => new SupportRequestViewModel().InitFrom(results),
            $"Status cannot be updated to '{status}' for support request key '{supportRequestUid}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all support requests with paging. </summary>
    /// 
    /// <param name="skip"> Number of records to skip. </param>
    /// <param name="take"> Number of records to take. </param>
    /// 
    /// <returns> All support requests. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft alle Supportanfragen mit Paging ab")]
    public async Task<String> GetAllSupportRequests(Int32 skip = 0, Int32 take = 100)
    {
        var validationError = ValidateNumber(
            skip,
            "Skip parameter must be greater zero.",
            value => skip >= 0);

        validationError = validationError ?? ValidateNumber(
            take,
            "Take parameter must be in range 1..1000.",
            value => take > 0 && take <= 1000);

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetAllSupportRequestsAsync(skip, take),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found with parameters '{skip}' and '{take}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by status. </summary>
    /// 
    /// <param name="status"> The status to filter by. </param>
    /// 
    /// <returns> Support requests with the specified status. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft Supportanfragen nach Status ab")]
    public async Task<String> GetSupportRequestsByStatus(String status)
    {
        var validationError = ValidateEnum<Models.Status>(
            status,
            "Invalid status value. Valid values are: " + String.Join(", ", Enum.GetNames<Models.Status>()));

        if (validationError != null)
            return validationError;

        var statusEnum = Enum.Parse<Models.Status>(status);

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetSupportRequestsByStatusAsync(statusEnum),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found for status '{status}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by topic. </summary>
    /// 
    /// <param name="topic"> The topic to filter by. </param>
    /// 
    /// <returns> Support requests with the specified topic. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft Supportanfragen nach Thema ab")]
    public async Task<String> GetSupportRequestsByTopic(String topic)
    {
        var validationError = ValidateEnum<Topic>(
            topic,
            "Invalid topic value. Valid values are: " + String.Join(", ", Enum.GetNames<Topic>()));

        if (validationError != null)
            return validationError;

        var topicEnum = Enum.Parse<Topic>(topic);

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetSupportRequestsByTopicAsync(topicEnum),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found for topic '{topic}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by priority. </summary>
    /// 
    /// <param name="priority"> The priority to filter by (1=Low, 2=Medium, 3=High). </param>
    /// 
    /// <returns> Support requests with the specified priority. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft Supportanfragen nach Priorität ab")]
    public async Task<String> GetSupportRequestsByPriority(String priority)
    {
        var validationError = ValidateEnum<Priority>(
            priority,
            "Invalid priority value. Valid values are: " + String.Join(", ", Enum.GetNames<Priority>()));

        if (validationError != null)
            return validationError;

        var priorityEnum = Enum.Parse<Priority>(priority);

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetSupportRequestsByPriorityAsync(priorityEnum),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found for priority '{priority}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests assigned to a specific user. </summary>
    /// 
    /// <param name="userUid"> The user UID. </param>
    /// 
    /// <returns> Support requests assigned to the user. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft Supportanfragen ab, die einem bestimmten Benutzer zugewiesen sind")]
    public async Task<String> GetSupportRequestsByAssignee(Guid userUid)
    {
        var validationError = ValidateUid(
            userUid,
            "Invalid user key value.");

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetSupportRequestsByAssigneeAsync(userUid),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found for user with key '{userUid}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all users. </summary>
    /// 
    /// <returns> All users. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft alle Benutzer ab")]
    public async Task<String> GetAllUsers()
    {   // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetAllUsersAsync(),
            results => results == null || !results.Any(),
            results => results.Select(u => new UserViewModel().InitFrom(u)),
            $"No users found.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new user. </summary>
    /// 
    /// <param name="name">  The user name. </param>
    /// <param name="email"> The user email. </param>
    /// <param name="topic"> The user's responsible topic. </param>
    /// 
    /// <returns> The created user. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Erstellt einen neuen Benutzer")]
    public async Task<String> CreateUser(String name, String email, String topic)
    {
        var validationError = ValidateNotEmpty(
            name,
            "User name must not be empty.");

        validationError = validationError ?? ValidateNotEmpty(
            email,
            "User email must not be empty.");

        if (validationError != null)
            return validationError;

        var viewModel = new UserViewModel
        {
            Name  = name,
            Email = email,
            Topic = topic
        };

        var entity = viewModel.CreateEntity();

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.CreateUserAsync(entity),
            result => result == null,
            result => new UserViewModel().InitFrom(result),
            $"Support request could not be created for user '{name}'.");
    }
}
