using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult GetSecret()
        {
            return Ok("You are authorized!");
        }
    }
}