using Microsoft.AspNetCore.Http;


namespace ProblemDetails.Api.Infrastructure
{
    /// <summary>
    /// Extends the default problem details class with additional custom properties
    /// </summary>
    public class MyProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public string ErrorCode { get; set; }

        public string CorrelationId { get; set; }


        public MyProblemDetails() { }


        public MyProblemDetails(HttpContext context)
        {
            Instance = context.Request.Path;
            CorrelationId = GetCorrelationId(context);
        }


        /// <summary>
        /// Retrieves the correlation id
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetCorrelationId(HttpContext context)
        {
            if (context.Items.TryGetValue("CorrelationId", out var value))
            {
                return value as string;
            }

            return null;
        }
    }
}