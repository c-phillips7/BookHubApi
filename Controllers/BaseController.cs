using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

// Class for helper methods to be inhereted by all controllers.
namespace BookHub.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected string GetCurrentUserId() => 
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        protected bool IsOwner(string ownerId) =>
            string.Equals(GetCurrentUserId(), ownerId, StringComparison.Ordinal) ||
            User.IsInRole("Admin");
    }
}