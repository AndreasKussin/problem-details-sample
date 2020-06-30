using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProblemDetails.Api.Infrastructure;
using System.Collections.Generic;


namespace ProblemDetails.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestAuthenticationController : ControllerBase
    {
        /// <summary>
        /// User have to be authenticated otherwise an Unauthorized will be returned.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MyProblemDetails), StatusCodes.Status401Unauthorized)]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}