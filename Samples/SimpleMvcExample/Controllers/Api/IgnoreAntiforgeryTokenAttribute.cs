using Microsoft.AspNetCore.Antiforgery;

namespace SimpleMvcExample.Controllers.Api;

public class IgnoreAntiforgeryMiddleware
{
    private readonly RequestDelegate _next;

    public IgnoreAntiforgeryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        if (IsApiRequest(context.Request))
        {
            // Generate a new antiforgery token and add it to the response headers
            var tokens = antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions
            {
                HttpOnly = false
            });
        }

        await _next(context);
    }

    private bool IsApiRequest(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/api") || request.Headers["Accept"].ToString().Contains("application/json");
    }
}