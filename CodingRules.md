---
applyTo: '**/*.cs'
---

# C# .NET Codingrules

## Sourcecode formatting guidelines

### Member Variables and Constructors

1. Use a leading "m" to indicate a class member variable, e.g. "mPascalCaseVariable" - but not for local variables, never combine the 'm' prefix with 'var', this is wrong: var mTelemetryEnabled = .., this is right: var telemetryEnabled = ..
2. Format multiple variables vertically aligned (visibility, type, names) as shown in example
3. Use primary constructors direct in class declaration whereever possible
4. Use .NET types instead of C# types, e.g. "String" instead of "string", "Int32" instead of "int"

Example:

```c#
public class WebStatisticsProvider(ILogger<WebStatisticsProvider> logger) : IWebStatisticsProvider
{
    private static   IDictionary<String, WebUserStatistics>         mUsers              = new ConcurrentDictionary<String, WebUserStatistics>();
   	private static   IDictionary<DateTime, EmailHistory>            mEmails             = new ConcurrentDictionary<DateTime, EmailHistory>();
    private static   IDictionary<DateTime, CleanupHistory>          mCleanups           = new ConcurrentDictionary<DateTime, CleanupHistory>();
    public           IEnumerable<WebUserStatistics>                 AllUsers            { get { return mUsers.Values.AsEnumerable(); } }
    public           IEnumerable<WebUserStatistics>                 KnownUsers          { get { return AllUsers.Where(i => (i.LoginName != null) && !Guid.TryParse(i.LoginName, out _) && (i.LoginName != "-")); } }

    private readonly ILogger                                        mLogger             = logger; 
    private readonly DateTime                                       mStartTime          = DateTime.Now;
    private          DateTime                                       mLastCleanup        = DateTime.Now;
    public  const    Int32                                          MAX_PAGE_SIZE       = 20;
    public  Int32    UserCount                                      { get { return mUsers.Count; } }
    public  Int32    KnownUserCount                                 { get { return KnownUsers.Count(); } }
    public  Int32    EmailCount                                     { get { return mEmails.Count; } }
    public  Int32    CleanupCount                                   { get { return mCleanups.Count; } }
```

### Variables

1. Use "var" instead of explicit type whereever possible
2. Vertical align the "=" if there are multiple assignments in consecutive lines as shown in example
3. Use cameCase for variables
4. If a variable ends with with a two letter abbreviation like "..ID" or "..IP" do keep or set the abbreviation in uppercase, e.g. "userID" instead of "userId" and "installationIP" instead of "installationIp". 
5. Place block comments directly behind the opening brace "{" for a block with same indentation as block

Example:

```c#
[AllowAnonymous]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Login(LoginViewModel viewModel)
{	  // initial assignments
    var watch   = Stopwatch.StartNew();
    var result  = String.Empty;
    var status  = STATUS_ERROR;
    var role    = ProjectConstants.ANONYMOUS_ROLE;
    var param   = $"User: {viewModel.LoginName}";
```

5. When creating objects use the format and vertical alignments as shown in following example:

```c#
try
{   // create new history
    var history = new WebUserHistory()
    {
        Action          = action.ToLower(),
        InstallationIP  = installationIP,
        CommonName      = commonName,
        Parameter       = parameter,
        ExecutionTime   = executionTime,
        Status          = status,
        ErrorMessage    = errorMessage
    }; 
```

### Block statements

1. Please add an empty line after closing block statement "}" like in the following example.
2. Do not add the empty line if multiple closing block statements follow each other.

```c#
[AllowAnonymous] 
public async Task<ActionResult> LogOff()
{   
    var watch    = Stopwatch.StartNew();
    var result   = String.Empty;
    var status   = STATUS_ERROR;
    var name     = String.Empty;
    var role     = String.Empty;
    try
    {   // get the user data (valid, because we have checked the session)
        var userData = mUserDataProvider.GetData();
        name         = userData?.LoginName ?? name;
        role         = userData?.Role ?? ProjectConstants.ANONYMOUS_ROLE;

      	if (command.DoExecute() == CommandStatus.Executed)
        {   // success
            status = STATUS_OK;
            name   = command.LoginName;
        
            return View();
        }

        else
        {
            name = command.LoginName ?? name;
        }

        // sign out and clear user data
        await HttpContext
            .SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        mUserDataProvider.Clear();

        status = STATUS_OK;
    }

    catch (Exception ex)
    {   // *log*
        result = $"Interner Fehler beim Abmelden: {ex.Message} für Benutzer {name}!";
        mLogger.LogError(result);
    }

    finally
    {   // *measure execution time and save request*
        watch?.Stop();
        mStatisticsProvider?.AddUserHistory(name!, nameof(LogOff), null, null, String.Empty, watch?.Elapsed ?? new TimeSpan(0), status, result);
        mMetricsProvider?.RegisterLogout(name, status == STATUS_OK);
        TrackRequest(nameof(LogOff), "finish", status, watch?.ElapsedMilliseconds, role);
    }

    return RedirectToAction("Index", "Home");
}
```

### Method signature

Do format method signatures aligned like this when there are multiple parameters:

```c#
private IList<McpClientTool> DetermineToolsToTest(
    IList<McpClientTool>             availableTools,
    IList<McpToolTestConfiguration>  toolTests,
    Boolean                          quickTest)
{
}
```

### Comments for methods

1. Please add a comment block like in the example above each method
2. Create a meaningful summary with help of method name
3. Use the current date
4. Add and explain the method parameters as shown in example

```C#
///-------------------------------------------------------------------------------------------------
/// <summary>   Email confirmation. </summary>
///
/// <remarks>   SvK, 19.12.2012. </remarks>
///
/// <param name="userGuid">   Unique identifier for the authentication. </param>
///
/// <returns>   View. </returns>
///-------------------------------------------------------------------------------------------------

[AllowAnonymous]
public ViewResult EmailConfirmation(Guid userGuid)
{
      var watch   = Stopwatch.StartNew();

```

### Namespace for classes

1. Please use the short form:

```c#
namespace Evanto.Core.Sample;
``` 
instead of the long form with block:

```c#
namespace Evanto.Core.Sample
{
    ...
}
```
2. Use one namespace per assembly 
3. Do not add subdirectories to the namespace, e.g. "Evanto.Core.Sample.Web" instead of "Evanto.Core.Sample.Web.Controllers"
4. Prefer including using statements instead of adding namespaces to class names in code.

### View models / Data Transfer Objects

1. Data Transfer Objects (DTO) should be named as "<EntityName>ViewModel", e.g. if there is an Entity "User" the DTO must have the name "UserViewModel"
2. Do not add recursive relations to DTOs, e.g. if an entity "User" has a sub list of "Accounts" and the "Account" entity refers to "User" then do not add a "User" field to the "Account" DTO.
3. Add an InitFrom() method to the DTO which has the corresponding entity as parameter and sets the DTO fields with the corresponding field from entity. Return "this" from InitFrom() to enable fluent calls:

```
public UserViewModel InitFrom(User user)
{
	this.PreName 	= user.PreName;
	this.LastName	= user.LastName;

	return this;
}
```

to enable calls like this:

```
var viewModel = new UserViewModel().InitFrom(user);
```

4. If there are inherited entities, make the base view models (DTOs) generic, e.g. if an entity class Customer is inherited from entity User, then make the UserViewModel generic to allow overriding the InitFrom() method:

```
public class UserViewModel<TEntity> where TEntity : User 
{

  public virtual UserViewModel InitFrom<TEntity>(TEntity user)
  {
    this.PreName 	= user.PreName;
    this.LastName	= user.LastName;

    return this;

  }
} 
```

