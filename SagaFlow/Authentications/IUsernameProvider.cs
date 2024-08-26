using System.Security.Claims;

namespace SagaFlow.Authentications;

public interface IUsernameProvider
{
    string? CurrentUsername { get; }
}

/// <summary>
/// A stub fallback implementation of IUsernameProvider if an implementation is not provided. Will simply
/// report the current Claims Principal (if set), if not then simply returns "System".
/// </summary>
internal class StubUsernameProvider : IUsernameProvider
{
    public string? CurrentUsername => ClaimsPrincipal.Current?.Identity?.Name ?? "System";
}