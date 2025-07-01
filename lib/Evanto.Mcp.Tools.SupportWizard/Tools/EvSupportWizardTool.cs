using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            nameof(customerEmail),
            "Email must not be empty.");

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mSupportWizardRepository.GetSupportRequestsForCustomerByEmailAsync(customerEmail),
            results => results == null || !results.Any(),
            results => results.Select(sr => new SupportRequestViewModel().InitFrom(sr)),
            $"No support requests found for email '{customerEmail}'.");
        /*
        try
        {
            if (String.IsNullOrWhiteSpace(customerEmail))
            {
                return new { status = "error", message = "Email parameter is required and cannot be empty" }.ToJson();
            }

            var supportRequests = await mSupportWizardRepository.GetSupportRequestsForCustomerByEmailAsync(customerEmail);

            if (!supportRequests.Any())
            {
                return new { status = "not_found", message = $"No support requests found for email: {customerEmail}" }.ToJson();
            }

            var viewModels = supportRequests.Select(sr => new SupportRequestViewModel().InitFrom(sr));

            return new { status = "success", data = viewModels }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetSupportRequestsForCustomerByEmail() for '{customerEmail}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }*/
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
        try
        {
            if (String.IsNullOrWhiteSpace(customerName))
            {
                return new { status = "error", message = "Customer name parameter is required and cannot be empty" }.ToJson();
            }

            var supportRequests = await mSupportWizardRepository.GetSupportRequestsForCustomerByNameAsync(customerName);

            if (!supportRequests.Any())
            {
                return new { status = "not_found", message = $"No support requests found for customer name: {customerName}" }.ToJson();
            }

            var viewModels = supportRequests.Select(sr => new SupportRequestViewModel().InitFrom(sr));

            return new { status = "success", data = viewModels }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetSupportRequestsForCustomerByName() for '{customerName}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
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

            var entity          = viewModel.CreateEntity();
            var createdRequest  = await mSupportWizardRepository.CreateSupportRequestAsync(entity);
            var resultViewModel = new SupportRequestViewModel().InitFrom(createdRequest);

            return new { status = "success", data = resultViewModel }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in CreateSupportRequest(): {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
            var existingRequest = await mSupportWizardRepository.GetSupportRequestByUidAsync(uid);
            if (existingRequest == null)
            {
                return new { status = "not_found", message = $"Support request with UID {uid} not found" }.ToJson();
            }

            var viewModel = new SupportRequestViewModel
            {
                Uid             = uid,
                CustomerEmail   = customerEmail,
                CustomerName    = customerName,
                Channel         = channel,
                Subject         = subject,
                Description     = description,
                Topic           = topic,
                Priority        = priority,
                Status          = status,
                ResolutionNotes = resolutionNotes,
                ReceivedAt      = existingRequest.ReceivedAt,
                CreatedAt       = existingRequest.CreatedAt
            };

            var updatedEntity   = viewModel.UpdateEntity(existingRequest);
            var updatedRequest  = await mSupportWizardRepository.UpdateSupportRequestAsync(updatedEntity);
            var resultViewModel = new SupportRequestViewModel().InitFrom(updatedRequest);

            return new { status = "success", data = resultViewModel }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in UpdateSupportRequest() for UID {uid}: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
            var updatedRequest = await mSupportWizardRepository.AssignSupportRequestToUserAsync(supportRequestUid, userUid);
            var viewModel      = new SupportRequestViewModel().InitFrom(updatedRequest);

            return new { status = "success", data = viewModel }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in AssignSupportRequestToUser() for request {supportRequestUid} to user {userUid}: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets users by topic. </summary>
    /// 
    /// <param name="topic"> The topic (Billing, Technical, Feature, Account, General). </param>
    /// 
    /// <returns> The users responsible for the topic. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Findet Benutzer, die für ein bestimmtes Thema verantwortlich sind")]
    public async Task<String> GetUserByTopic(String topic)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(topic))
            {
                return new { status = "error", message = "Topic parameter is required and cannot be empty" }.ToJson();
            }

            if (!Enum.TryParse<Topic>(topic, out var topicEnum))
            {
                return new { status = "error", message = $"Invalid topic value: {topic}. Valid values are: {String.Join(", ", Enum.GetNames<Topic>())}" }.ToJson();
            }

            var users = await mSupportWizardRepository.GetUserByTopicAsync(topicEnum);

            if (!users.Any())
            {
                return new { status = "not_found", message = $"No users found for topic: {topic}" }.ToJson();
            }

            var viewModels = users.Select(u => new UserViewModel().InitFrom(u));

            return new { status = "success", data = viewModels }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetUserByTopic() for topic '{topic}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
            if (String.IsNullOrWhiteSpace(status))
            {
                return new { status = "error", message = "Status parameter is required and cannot be empty" }.ToJson();
            }

            if (!Enum.TryParse<Models.Status>(status, out var statusEnum))
            {
                return new { status = "error", message = $"Invalid status value: {status}. Valid values are: {String.Join(", ", Enum.GetNames<Models.Status>())}" }.ToJson();
            }

            var updatedRequest = await mSupportWizardRepository.UpdateStatusForSupportRequestAsync(supportRequestUid, statusEnum, resolutionNotes);
            var viewModel      = new SupportRequestViewModel().InitFrom(updatedRequest);

            return new { status = "success", data = viewModel }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in UpdateStatusForSupportRequest() for UID {supportRequestUid}: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
    public async Task<String> GetAllSupportRequests(int skip = 0, int take = 100)
    {
        try
        {
            if (take > 1000)
            {
                return new { status = "error", message = "Take parameter cannot exceed 1000 records" }.ToJson();
            }

            var supportRequests = await mSupportWizardRepository.GetAllSupportRequestsAsync(skip, take);
            var viewModels      = supportRequests.Select(sr => new SupportRequestViewModel().InitFrom(sr));

            return new { status = "success", data = viewModels, pagination = new { skip, take, count = viewModels.Count() } }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetAllSupportRequests(): {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
            if (String.IsNullOrWhiteSpace(status))
            {
                return new { status = "error", message = "Status parameter is required and cannot be empty" }.ToJson();
            }

            if (!Enum.TryParse<Models.Status>(status, out var statusEnum))
            {
                return new { status = "error", message = $"Invalid status value: {status}. Valid values are: {String.Join(", ", Enum.GetNames<Models.Status>())}" }.ToJson();
            }

            var supportRequests = await mSupportWizardRepository.GetSupportRequestsByStatusAsync(statusEnum);
            var viewModels      = supportRequests.Select(sr => new SupportRequestViewModel().InitFrom(sr));

            return new { status = "success", data = viewModels }.ToJson();
        }
        
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetSupportRequestsByStatus() for status '{status}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
            if (String.IsNullOrWhiteSpace(topic))
            {
                return new { status = "error", message = "Topic parameter is required and cannot be empty" }.ToJson();
            }

            if (!Enum.TryParse<Topic>(topic, out var topicEnum))
            {
                return new { status = "error", message = $"Invalid topic value: {topic}. Valid values are: {String.Join(", ", Enum.GetNames<Topic>())}" }.ToJson();
            }

            var supportRequests = await mSupportWizardRepository.GetSupportRequestsByTopicAsync(topicEnum);
            var viewModels      = supportRequests.Select(sr => new SupportRequestViewModel().InitFrom(sr));

            return new { status = "success", data = viewModels }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetSupportRequestsByTopic() for topic '{topic}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by priority. </summary>
    /// 
    /// <param name="priority"> The priority to filter by (1=Low, 2=Medium, 3=High). </param>
    /// 
    /// <returns> Support requests with the specified priority. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft Supportanfragen nach Priorität ab")]
    public async Task<String> GetSupportRequestsByPriority(byte priority)
    {
        try
        {
            if (!Enum.IsDefined(typeof(Priority), priority))
            {
                return new { status = "error", message = $"Invalid priority value: {priority}. Valid values are: 1 (Low), 2 (Medium), 3 (High)" }.ToJson();
            }

            var priorityEnum    = (Priority)priority;
            var supportRequests = await mSupportWizardRepository.GetSupportRequestsByPriorityAsync(priorityEnum);
            var viewModels      = supportRequests.Select(sr => new SupportRequestViewModel().InitFrom(sr));

            return new { status = "success", data = viewModels }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetSupportRequestsByPriority() for priority {priority}: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
            var supportRequests = await mSupportWizardRepository.GetSupportRequestsByAssigneeAsync(userUid);
            var viewModels      = supportRequests.Select(sr => new SupportRequestViewModel().InitFrom(sr));

            return new { status = "success", data = viewModels }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetSupportRequestsByAssignee() for user {userUid}: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all users. </summary>
    /// 
    /// <returns> All users. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Ruft alle Benutzer ab")]
    public async Task<String> GetAllUsers()
    {
        try
        {
            var users      = await mSupportWizardRepository.GetAllUsersAsync();
            var viewModels = users.Select(u => new UserViewModel().InitFrom(u));

            return new { status = "success", data = viewModels }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in GetAllUsers(): {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
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
        try
        {
            var viewModel = new UserViewModel
            {
                Name  = name,
                Email = email,
                Topic = topic
            };

            var entity          = viewModel.CreateEntity();
            var createdUser     = await mSupportWizardRepository.CreateUserAsync(entity);
            var resultViewModel = new UserViewModel().InitFrom(createdUser);

            return new { status = "success", data = resultViewModel }.ToJson();
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in CreateUser(): {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }
}
