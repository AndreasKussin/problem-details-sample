using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;


namespace ProblemDetails.Api.Infrastructure
{
    public class MyTelemetryInitializer : TelemetryInitializerBase
    {
        public MyTelemetryInitializer(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }


        /// <summary>
        /// Adds additional information (e.g. correlation id) to application insights for request logging.
        /// </summary>
        /// <param name="platformContext"></param>
        /// <param name="requestTelemetry"></param>
        /// <param name="telemetry"></param>
        protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
        {
            // Set correlation id
            if (platformContext.Items.TryGetValue("CorrelationId", out var value))
            {
                var correlationId = value as string;
                requestTelemetry.Context.Operation.Id = correlationId;
                requestTelemetry.Context.Operation.ParentId = correlationId;
                telemetry.Context.Operation.Id = correlationId;
                telemetry.Context.Operation.ParentId = correlationId;
            }

            //  Add more custom properties here
        }
    }
}