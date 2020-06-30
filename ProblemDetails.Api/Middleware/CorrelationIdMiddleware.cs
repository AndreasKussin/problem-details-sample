using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;


namespace ProblemDetails.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string correlationIdHeader = "X-Request-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = "";

            // Try to get correlation id from header
            if (context.Request.Headers.TryGetValue(correlationIdHeader, out var value))
            {
                correlationId = value;
            }

            // Create new correlation ID if no header was found
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = context.TraceIdentifier;

                // Apply the correlation ID to the response header for client side tracking
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Add(correlationIdHeader, correlationId);
                    return Task.CompletedTask;
                });
            }

            // Save correlation ID
            context.Items["CorrelationId"] = correlationId;

            await next(context);
        }
    }
}