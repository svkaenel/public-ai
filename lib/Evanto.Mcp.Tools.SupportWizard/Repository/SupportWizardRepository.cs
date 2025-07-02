using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Evanto.Mcp.Tools.SupportWizard.Context;
using Evanto.Mcp.Tools.SupportWizard.Contracts;
using Evanto.Mcp.Tools.SupportWizard.Models;

namespace Evanto.Mcp.Tools.SupportWizard.Repository;

///-------------------------------------------------------------------------------------------------
/// <summary>   Repository implementation for SupportWizard operations. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class SupportWizardRepository : ISupportWizardRepository
{
    private readonly SupportWizardDbContext               mContext;
    private readonly ILogger<SupportWizardRepository>   mLogger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Constructor. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="context"> The database context. </param>
    /// <param name="logger">  The logger. </param>
    ///-------------------------------------------------------------------------------------------------
    public SupportWizardRepository(SupportWizardDbContext context, ILogger<SupportWizardRepository> logger)
    {
        mContext = context ?? throw new ArgumentNullException(nameof(context));
        mLogger  = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new support request asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequest"> The support request to create. </param>
    ///
    /// <returns>   The created support request with generated ID. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<SupportRequest> CreateSupportRequestAsync(SupportRequest supportRequest)
    {
        ArgumentNullException.ThrowIfNull(supportRequest);

        try
        {
            mContext.SupportRequests.Add(supportRequest);

            await mContext.SaveChangesAsync();

            mLogger.LogInformation("Created support request {Uid} for customer {CustomerEmail}", 
                supportRequest.Uid, supportRequest.CustomerEmail);

            return supportRequest;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error creating support request for customer {CustomerEmail}", supportRequest.CustomerEmail);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates an existing support request asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="supportRequest"> The support request to update. </param>
    ///
    /// <returns>   The updated support request. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<SupportRequest> UpdateSupportRequestAsync(SupportRequest supportRequest)
    {
        ArgumentNullException.ThrowIfNull(supportRequest);

        try
        {
            mContext.SupportRequests.Update(supportRequest);
            await mContext.SaveChangesAsync();

            mLogger.LogInformation("Updated support request {Uid}", supportRequest.Uid);

            return supportRequest;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error updating support request {Uid}", supportRequest.Uid);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests for a customer by email address asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="customerEmail"> The customer email address. </param>
    ///
    /// <returns>   List of support requests for the customer. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<SupportRequest>> GetSupportRequestsForCustomerByEmailAsync(String customerEmail)
    {
        ArgumentException.ThrowIfNullOrEmpty(customerEmail, nameof(customerEmail));

        try
        {
            var requests = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.CustomerEmail.ToLower() == customerEmail.ToLower())
                .OrderByDescending(sr => sr.ReceivedAt)
                .ToListAsync();

            mLogger.LogInformation("Found {Count} support requests for customer email {CustomerEmail}", 
                requests.Count, customerEmail);

            return requests;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving support requests for customer email {CustomerEmail}", 
                customerEmail);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests for a customer by name asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="customerName"> The customer name. </param>
    ///
    /// <returns>   List of support requests for the customer. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<SupportRequest>> GetSupportRequestsForCustomerByNameAsync(String customerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(customerName, nameof(customerName));

        try
        {
            var requests = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.CustomerName.ToLower().Contains(customerName.ToLower()))
                .OrderByDescending(sr => sr.ReceivedAt)
                .ToListAsync();

            mLogger.LogInformation("Found {Count} support requests for customer name containing '{CustomerName}'", 
                requests.Count, customerName);

            return requests;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving support requests for customer name {CustomerName}", 
                customerName);
            throw;
        }
    }

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
    public async Task<SupportRequest> AssignSupportRequestToUserAsync(Guid supportRequestUid, Guid userUid)
    {
        try
        {
            var supportRequest = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .FirstOrDefaultAsync(sr => sr.Uid == supportRequestUid);

            if (supportRequest == null)
                throw new InvalidOperationException($"Support request with UID {supportRequestUid} not found");

            var user = await mContext.Users.FirstOrDefaultAsync(u => u.Uid == userUid);
            if (user == null)
                throw new InvalidOperationException($"User with UID {userUid} not found");

            supportRequest.AssignedToUserUid = userUid;
            if (supportRequest.Status == Status.New)
            {
                supportRequest.Status = Status.InProgress;
            }

            await mContext.SaveChangesAsync();

            mLogger.LogInformation("Assigned support request {SupportRequestUid} to user {UserUid} ({UserName})", 
                supportRequestUid, userUid, user.Name);

            return supportRequest;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error assigning support request {SupportRequestUid} to user {UserUid}", 
                supportRequestUid, userUid);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets users by topic asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="topic"> The topic to search for. </param>
    ///
    /// <returns>   List of users responsible for the topic. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<User>> GetUserByTopicAsync(Topic topic)
    {
        try
        {
            var users = await mContext.Users
                .Where(u => u.Topic == topic)
                .ToListAsync();

            mLogger.LogInformation("Found {Count} users for topic {Topic}", users.Count, topic);

            return users;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving users for topic {Topic}", topic);
            throw;
        }
    }

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
    public async Task<SupportRequest> UpdateStatusForSupportRequestAsync(Guid supportRequestUid, Status status, String? resolutionNotes = null)
    {
        try
        {
            var supportRequest = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .FirstOrDefaultAsync(sr => sr.Uid == supportRequestUid);

            if (supportRequest == null)
                throw new InvalidOperationException($"Support request with UID {supportRequestUid} not found");

            var oldStatus = supportRequest.Status;
            supportRequest.Status = status;

            if (!String.IsNullOrWhiteSpace(resolutionNotes))
            {
                supportRequest.ResolutionNotes = resolutionNotes;
            }

            if (status == Status.Resolved && supportRequest.ResolvedAt == null)
            {
                supportRequest.ResolvedAt = DateTimeOffset.UtcNow;
            }

            await mContext.SaveChangesAsync();

            mLogger.LogInformation("Updated support request {SupportRequestUid} status from {OldStatus} to {NewStatus}", 
                supportRequestUid, oldStatus, status);

            return supportRequest;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error updating status for support request {SupportRequestUid}", supportRequestUid);
            throw;
        }
    }

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
    public async Task<IEnumerable<SupportRequest>> GetAllSupportRequestsAsync(int skip = 0, int take = 100)
    {
        try
        {
            var requests = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .OrderByDescending(sr => sr.ReceivedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            mLogger.LogInformation("Retrieved {Count} support requests (skip: {Skip}, take: {Take})", 
                requests.Count, skip, take);

            return requests;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving all support requests");
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by status asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="status"> The status to filter by. </param>
    ///
    /// <returns>   List of support requests with the specified status. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<SupportRequest>> GetSupportRequestsByStatusAsync(Status status)
    {
        try
        {
            var requests = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.Status == status)
                .OrderByDescending(sr => sr.ReceivedAt)
                .ToListAsync();

            mLogger.LogInformation("Found {Count} support requests with status {Status}", requests.Count, status);

            return requests;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving support requests by status {Status}", status);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by topic asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="topic"> The topic to filter by. </param>
    ///
    /// <returns>   List of support requests with the specified topic. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<SupportRequest>> GetSupportRequestsByTopicAsync(Topic topic)
    {
        try
        {
            var requests = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.Topic == topic)
                .OrderByDescending(sr => sr.ReceivedAt)
                .ToListAsync();

            mLogger.LogInformation("Found {Count} support requests with topic {Topic}", requests.Count, topic);

            return requests;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving support requests by topic {Topic}", topic);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests by priority asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="priority"> The priority to filter by. </param>
    ///
    /// <returns>   List of support requests with the specified priority. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<SupportRequest>> GetSupportRequestsByPriorityAsync(Priority priority)
    {
        try
        {
            var requests = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.Priority == priority)
                .OrderByDescending(sr => sr.ReceivedAt)
                .ToListAsync();

            mLogger.LogInformation("Found {Count} support requests with priority {Priority}", requests.Count, priority);

            return requests;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving support requests by priority {Priority}", priority);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets support requests assigned to a specific user asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="userUid"> The user UID. </param>
    ///
    /// <returns>   List of support requests assigned to the user. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<SupportRequest>> GetSupportRequestsByAssigneeAsync(Guid userUid)
    {
        try
        {
            var requests = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.AssignedToUserUid == userUid)
                .OrderByDescending(sr => sr.ReceivedAt)
                .ToListAsync();

            mLogger.LogInformation("Found {Count} support requests assigned to user {UserUid}", requests.Count, userUid);

            return requests;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving support requests for assignee {UserUid}", userUid);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets overdue support requests based on SLA times asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="slaHours"> SLA hours for different priorities. </param>
    ///
    /// <returns>   List of overdue support requests. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<SupportRequest>> GetOverdueRequestsAsync(Dictionary<Priority, int> slaHours)
    {
        ArgumentNullException.ThrowIfNull(slaHours);

        try
        {
            var now = DateTimeOffset.UtcNow;
            var overdueRequests = new List<SupportRequest>();

            foreach (var sla in slaHours)
            {
                var cutoffTime = now.AddHours(-sla.Value);
                var requests = await mContext.SupportRequests
                    .Include(sr => sr.AssignedToUser)
                    .Where(sr => sr.Priority == sla.Key 
                              && sr.Status != Status.Resolved 
                              && sr.Status != Status.Closed 
                              && sr.Status != Status.Cancelled
                              && sr.ReceivedAt <= cutoffTime)
                    .ToListAsync();

                overdueRequests.AddRange(requests);
            }

            mLogger.LogInformation("Found {Count} overdue support requests", overdueRequests.Count);

            return overdueRequests.OrderByDescending(sr => sr.ReceivedAt);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving overdue support requests");
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets a support request by UID asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="uid"> The support request UID. </param>
    ///
    /// <returns>   The support request or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<SupportRequest?> GetSupportRequestByUidAsync(Guid uid)
    {
        try
        {
            var request = await mContext.SupportRequests
                .Include(sr => sr.AssignedToUser)
                .FirstOrDefaultAsync(sr => sr.Uid == uid);

            if (request != null)
            {
                mLogger.LogInformation("Found support request {Uid}", uid);
            }
            else
            {
                mLogger.LogWarning("Support request {Uid} not found", uid);
            }

            return request;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving support request {Uid}", uid);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all users asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <returns>   List of all users. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            var users = await mContext.Users.ToListAsync();

            mLogger.LogInformation("Retrieved {Count} users", users.Count);

            return users;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a new user asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="user"> The user to create. </param>
    ///
    /// <returns>   The created user with generated ID. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<User> CreateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            mContext.Users.Add(user);
            await mContext.SaveChangesAsync();

            mLogger.LogInformation("Created user {Uid} with email {Email}", user.Uid, user.Email);

            return user;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error creating user with email {Email}", user.Email);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates an existing user asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="user"> The user to update. </param>
    ///
    /// <returns>   The updated user. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<User> UpdateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            mContext.Users.Update(user);
            await mContext.SaveChangesAsync();

            mLogger.LogInformation("Updated user {Uid}", user.Uid);

            return user;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error updating user {Uid}", user.Uid);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets a user by UID asynchronously. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="uid"> The user UID. </param>
    ///
    /// <returns>   The user or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<User?> GetUserByUidAsync(Guid uid)
    {
        try
        {
            var user = await mContext.Users.FirstOrDefaultAsync(u => u.Uid == uid);

            if (user != null)
            {
                mLogger.LogInformation("Found user {Uid}", uid);
            }
            else
            {
                mLogger.LogWarning("User {Uid} not found", uid);
            }

            return user;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error retrieving user {Uid}", uid);
            throw;
        }
    }
}