using System.Diagnostics;

namespace TapsiDOC.Order.EndPoints.Api.V1.Middlewares;

public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString(); // Capture early

        context.Response.OnStarting(() =>
        {
            if (!string.IsNullOrEmpty(traceId))
            {
                context.Response.Headers["X-Trace-Id"] = traceId;
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

