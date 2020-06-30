using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProblemDetails.Api.Infrastructure;


namespace ProblemDetails.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> logger;


        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }


        /// <summary>
        /// Handles all unhandled errors.
        /// </summary>
        /// <returns></returns>
        [Route("/error")]
        public IActionResult Error()
        {
            // Retrieve error information in case of internal errors
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var error = new MyProblemDetails(HttpContext)
            {
                Title = "Undefined error",
                Detail = "Some detailed error message",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://www.my-error-portal.com/myproject/500",
                Instance = feature.Path,
                ErrorCode = "500"
            };

            logger.LogError(feature.Error, $"An unhandled exception happend: {feature.Error.Message}");

            return new ObjectResult(error)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}