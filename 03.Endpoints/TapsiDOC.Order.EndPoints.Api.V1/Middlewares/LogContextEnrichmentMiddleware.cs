using Serilog.Context;
using System.Diagnostics;
using System.Text.Json;

namespace TapsiDOC.Order.EndPoints.Api.V1.Middlewares;
public class LogContextEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public LogContextEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        var traceId = Activity.Current?.TraceId.ToString() ?? "no-trace";

        if (context.Request.Method == HttpMethod.Post.Method &&
            context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Request.EnableBuffering();

            string body = await new StreamReader(context.Request.Body, leaveOpen: true).ReadToEndAsync();
            context.Request.Body.Position = 0; 

            string? orderCode = null;
            string? vendorCode = null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.TryGetProperty("orderCode", out var orderCodeProp))
                    orderCode = orderCodeProp.GetString();

                if (root.TryGetProperty("vendorCode", out var vendorCodeProp))
                    vendorCode = vendorCodeProp.GetString();

                // Fallback to parsing from `event` JSON field if missing
                if ((string.IsNullOrEmpty(orderCode) || string.IsNullOrEmpty(vendorCode)) &&
                    root.TryGetProperty("event", out var eventProp) &&
                    eventProp.ValueKind == JsonValueKind.String)
                {
                    var eventJson = eventProp.GetString();
                    if (!string.IsNullOrWhiteSpace(eventJson))
                    {
                        using var eventDoc = JsonDocument.Parse(eventJson);

                        if (string.IsNullOrEmpty(orderCode) &&
                            eventDoc.RootElement.TryGetProperty("orderCode", out var innerOrderCode))
                            orderCode = innerOrderCode.GetString();

                        if (string.IsNullOrEmpty(vendorCode) &&
                            eventDoc.RootElement.TryGetProperty("vendorCode", out var innerVendorCode))
                            vendorCode = innerVendorCode.GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "Failed to extract order/vendor info from request body.");
            }

            var enrichers = new List<IDisposable>
            {
                LogContext.PushProperty("TraceId", traceId)
            };

            if (!string.IsNullOrEmpty(orderCode))
                enrichers.Add(LogContext.PushProperty("OrderCode", orderCode));

            if (!string.IsNullOrEmpty(vendorCode))
                enrichers.Add(LogContext.PushProperty("VendorCode", vendorCode));

            using (new DisposableEnricherScope(enrichers))
            {
                await _next(context);
            }
        }
        else
        {
            using (LogContext.PushProperty("TraceId", traceId))
            {
                await _next(context);
            }
        }
    }

    private class DisposableEnricherScope : IDisposable
    {
        private readonly List<IDisposable> _disposables;

        public DisposableEnricherScope(List<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var d in Enumerable.Reverse(_disposables))
            {
                d.Dispose();
            }
        }
    }
}
