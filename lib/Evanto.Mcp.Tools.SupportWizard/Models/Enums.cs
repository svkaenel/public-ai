namespace Evanto.Mcp.Tools.SupportWizard.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Channel through which the support request was received. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public enum Channel
{
    Phone,
    Email,
    Chat,
    Web,
    Mobile
}

///-------------------------------------------------------------------------------------------------
/// <summary>   Priority level of the support request. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public enum Priority : byte
{
    Low    = 1,
    Medium = 2,
    High   = 3
}

///-------------------------------------------------------------------------------------------------
/// <summary>   Current status of the support request. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public enum Status
{
    New,
    InProgress,
    Resolved,
    Closed,
    Cancelled
}

///-------------------------------------------------------------------------------------------------
/// <summary>   Topic categories for support requests. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public enum Topic
{
    Billing,
    Technical,
    Feature,
    Account,
    General
}