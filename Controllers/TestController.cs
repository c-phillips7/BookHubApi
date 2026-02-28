using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookHub.Services;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly EmailService _emailService;

        public TestController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("send")]
        public IActionResult SendTestEmail()
        {
            try
            {
                _emailService.SendEmail(
                    "yourdev@gmail.com", // recipient
                    "Test Email from BookHub",
                    "This is a test email to verify EmailService is working."
                );
                return Ok("Test email sent successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            return Ok(new
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = User.Identity?.Name,
                Email = User.FindFirstValue(ClaimTypes.Email),
                Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value)
            });
        }
    }
}