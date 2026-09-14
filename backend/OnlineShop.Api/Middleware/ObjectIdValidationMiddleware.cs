using MongoDB.Bson;

namespace OnlineShop.Api.Middleware;

public class ObjectIdValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ObjectIdValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var routeValues = context.Request.RouteValues;

        foreach (var kv in routeValues)
        {
            if (kv.Key.ToLower().Contains("id"))
            {
                var value = kv.Value?.ToString();

                if (string.IsNullOrWhiteSpace(value) || !ObjectId.TryParse(value, out _))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync($"Invalid ObjectId: {value}");
                    return;
                }
            }
        }

        await _next(context);
    }
}
