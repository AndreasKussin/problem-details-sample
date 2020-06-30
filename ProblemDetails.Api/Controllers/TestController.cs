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
        /// <summary>
        /// Simulates a successful request and returns Ok.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> Get()
        {
            return Ok("Success");
        }

        /// <summary>
        /// Simulates a request with invalid or missing parameters and returns a Bad Request.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpGet("{value}", Name = "Get")]
        [ProducesResponseType(typeof(MyProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> Get(string value)
        {
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


        /// <summary>
        /// Simulates an internal server error.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MyProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Post()
        {
            throw new NullReferenceException("Some error message");
        }
    }
}