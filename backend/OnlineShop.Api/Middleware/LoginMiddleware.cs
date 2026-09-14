using MongoDB.Bson;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Middleware;

public class LoginMiddleware
{
    private readonly RequestDelegate _next;

    public LoginMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IInvalidTokenRepository invalidTokens)
    {
        // Extract token from Authorization header
        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "").Trim();

        // Reject invalidated tokens
        if (!string.IsNullOrWhiteSpace(token))
        {
            var isInvalid = await invalidTokens.ExistsAsync(token);
            if (isInvalid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token has been invalidated.");
                return;
            }
        }

        // Extract user ID from JWT claims
        var userId = context.User?.FindFirst("id")?.Value?.Trim()
                     ?? context.User?.FindFirst("sub")?.Value?.Trim();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            context.Items["UserId"] = userId;
        }

        await _next(context);
    }
}
