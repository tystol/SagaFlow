using Microsoft.AspNetCore.Http;
using SagaFlow.Authentications;

namespace SagaFlow.AspNetCore.Authentications;

public class HttpContextUsernameProvider : IUsernameProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// When there is Http context but there is no identity names claim, this will be the username used.
    /// </summary>
    public string AnonymousUserName { get; internal set; } = "Anonymous";

    /// <summary>
    /// The username assigned to system initiated commands, such as Cron based jobs.
    /// </summary>
    public string SystemUsername { get; internal set; } = "System";

    public HttpContextUsernameProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CurrentUsername => _httpContextAccessor switch
    {
        { HttpContext: null } => this.SystemUsername,
        _ => _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? AnonymousUserName
    };
}