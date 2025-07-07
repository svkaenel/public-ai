using Evanto.Mcp.Tools.SupportWizard.Models;

namespace Evanto.Mcp.Tools.SupportWizard.Contracts;

///-------------------------------------------------------------------------------------------------
/// <summary>   Interface for the SupportWizard repository. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public interface ISupportWizardRepository
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new support request asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequest"> The support request to create. </param>
    ///
    /// <returns>   The created support request with generated ID. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<SupportRequest> CreateSupportRequestAsync(SupportRequest supportRequest);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates an existing support request asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequest"> The support request to update. </param>
    ///
    /// <returns>   The updated support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<SupportRequest> UpdateSupportRequestAsync(SupportRequest supportRequest);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests for a customer by email address asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="customerEmail"> The customer email address. </param>
    ///
    /// <returns>   List of support requests for the customer. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetSupportRequestsForCustomerByEmailAsync(String customerEmail);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests for a customer by name asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="customerName"> The customer name. </param>
    ///
    /// <returns>   List of support requests for the customer. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetSupportRequestsForCustomerByNameAsync(String customerName);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Assigns a support request to a user asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequestUid"> The support request UID. </param>
    /// <param name="userUid">           The user UID to assign to. </param>
    ///
    /// <returns>   The updated support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<SupportRequest> AssignSupportRequestToUserAsync(Guid supportRequestUid, Guid userUid);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets users by topic asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="topic"> The topic to search for. </param>
    ///
    /// <returns>   List of users responsible for the topic. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<User>> GetUsersByTopicAsync(Topic topic);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates the status of a support request asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequestUid"> The support request UID. </param>
    /// <param name="status">            The new status. </param>
    /// <param name="resolutionNotes">   Optional resolution notes. </param>
    ///
    /// <returns>   The updated support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<SupportRequest> UpdateStatusForSupportRequestAsync(Guid supportRequestUid, Status status, String? resolutionNotes = null);

    // Additional repository methods for enhanced functionality

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all support requests asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="skip"> Number of records to skip for paging. </param>
    /// <param name="take"> Number of records to take for paging. </param>
    ///
    /// <returns>   List of all support requests. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetAllSupportRequestsAsync(Int32 skip = 0, Int32 take = 100);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by status asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="status"> The status to filter by. </param>
    ///
    /// <returns>   List of support requests with the specified status. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetSupportRequestsByStatusAsync(Status status);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by topic asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="topic"> The topic to filter by. </param>
    ///
    /// <returns>   List of support requests with the specified topic. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetSupportRequestsByTopicAsync(Topic topic);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by priority asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="priority"> The priority to filter by. </param>
    ///
    /// <returns>   List of support requests with the specified priority. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetSupportRequestsByPriorityAsync(Priority priority);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests assigned to a specific user asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="userUid"> The user UID. </param>
    ///
    /// <returns>   List of support requests assigned to the user. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetSupportRequestsByAssigneeAsync(Guid userUid);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets overdue support requests based on SLA times asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="slaHours"> SLA hours for different priorities. </param>
    ///
    /// <returns>   List of overdue support requests. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<SupportRequest>> GetOverdueRequestsAsync(Dictionary<Priority, Int32> slaHours);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets a support request by UID asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="uid"> The support request UID. </param>
    ///
    /// <returns>   The support request or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<SupportRequest?> GetSupportRequestByUidAsync(Guid uid);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all users asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <returns>   List of all users. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<IEnumerable<User>> GetAllUsersAsync();

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new user asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="user"> The user to create. </param>
    ///
    /// <returns>   The created user with generated ID. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<User> CreateUserAsync(User user);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates an existing user asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="user"> The user to update. </param>
    ///
    /// <returns>   The updated user. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<User> UpdateUserAsync(User user);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets a user by UID asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="uid"> The user UID. </param>
    ///
    /// <returns>   The user or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<User?> GetUserByUidAsync(Guid uid);
}