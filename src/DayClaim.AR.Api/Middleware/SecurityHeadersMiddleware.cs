namespace DayClaim.AR.Api.Middleware;

/// <summary>
/// Defense-in-depth headers at the service itself, in addition to
/// WAF/ALB/CloudFront in the target AWS topology (docs/ARCHITECTURE.md) —
/// this way the API is still safe to call directly in dev/test where the
/// edge stack isn't present.
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        context.Response.Headers.Append("Cache-Control", "no-store");
        await next(context);
    }
}
