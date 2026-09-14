using MongoDB.Bson;

namespace OnlineShop.Api.Middleware;

public class LoginMiddleware
{
    private readonly RequestDelegate _next;

    public LoginMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst("sub")?.Value?.Trim();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            if (!context.Items.ContainsKey("UserId"))
                context.Items["UserId"] = userId;
        }

        await _next(context);
    }
}
