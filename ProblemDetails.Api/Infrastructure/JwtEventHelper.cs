using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace ProblemDetails.Api.Infrastructure
{
    public class JwtEventHelper
    {
        /// <summary>
        /// Returns an error message if authentication failed.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task AuthenticationFailed(AuthenticationFailedContext arg)
        {
            // Check first if response was already handled
            if (!arg.Response.HasStarted)
            {
                // Track error in app insights
                var logger = arg.HttpContext.RequestServices.GetRequiredService<ILogger<JwtEventHelper>>();
                logger.LogError(arg.Exception, "Authentication error.");

                var error = new MyProblemDetails(arg.HttpContext)
                {
                    Title = "Authentication Error",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://www.my-error-portal.com/myproject/401",
                    ErrorCode = "401"
                };

#if DEBUG
                error.Detail = arg.Exception.Message.Replace("\n", " ");
#else
                error.Detail = "Authentication failed.";
#endif

                // Add error message to response
                await WriteResponse(error, arg.Response, StatusCodes.Status401Unauthorized);
            }
        }


        /// <summary>
        /// Returns an error message if the jwt token is missing or the token validation failed.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task ChallengeFailedResponse(JwtBearerChallengeContext arg)
        {
            // Important: Skip default error handling!
            arg.HandleResponse();

            // Check first if response was already handled in AuthenticationFailed()
            if (!arg.Response.HasStarted)
            {
                var error = new MyProblemDetails(arg.HttpContext)
                {
                    Title = "Authentication Error",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://www.my-error-portal.com/myproject/401",
                    ErrorCode = "401"
                };

                if (string.IsNullOrWhiteSpace(arg.Error))
                {
                    error.Detail = "Authorization header is missing.";
                }
                else
                {
                    var logger = arg.HttpContext.RequestServices.GetRequiredService<ILogger<JwtEventHelper>>();
                    logger.LogError($"Authentication failed with error: {arg.Error}.");
                    error.Detail = $"Authentication failed with error: {arg.Error}.";
                }

                // Add error message to response
                await WriteResponse(error, arg.Response, StatusCodes.Status401Unauthorized);
            }
        }


        /// <summary>
        /// Returns an error message if authorization failed.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task AuthorizationFailed(ForbiddenContext arg)
        {
            // Check first if response was already handled
            if (!arg.Response.HasStarted)
            {
                var logger = arg.HttpContext.RequestServices.GetRequiredService<ILogger<JwtEventHelper>>();
                logger.LogInformation($"Authorization failed for user {arg.Principal}");

                var error = new MyProblemDetails(arg.HttpContext)
                {
                    Title = "Authorization Error",
                    Detail = "Missing access rights",
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://www.my-error-portal.com/myproject/403",
                    ErrorCode = "403"
                };

                // Add error message to response
                await WriteResponse(error, arg.Response, StatusCodes.Status403Forbidden);
            }
        }


        private static async Task WriteResponse(MyProblemDetails error, HttpResponse response, int code)
        {
            var message = JsonSerializer.Serialize(error);
            response.ContentLength = message.Length;
            response.StatusCode = code;
            response.ContentType = "application/problem+json";
            await response.Body.WriteAsync(Encoding.UTF8.GetBytes(message), 0, message.Length);
        }
    }
}