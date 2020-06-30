using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProblemDetails.Api.Infrastructure;
using System;
using System.Threading.Tasks;


namespace ProblemDetails.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            // Simulate successful request
            return Ok("Success");
            
        }


        [HttpGet("{value}", Name = "Get")]
        public async Task<ActionResult<string>> Get(string value)
        {
            // Simulate request with invalid parameters
            var error = new MyProblemDetails(HttpContext)
            {
                Title = "Test Error",
                Detail = "Testing bad request error",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://www.my-error-portal.com/myproject/400",
                ErrorCode = "400"
            };

            return BadRequest(error);
            
        }


        [HttpPost]
        public async Task<ActionResult<bool>> Post()
        {
            // Simulate internal server error
            throw new NullReferenceException("Some error message");
        }
    }
}