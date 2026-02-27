using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //TODO GET: api/reviews
        //TODO GET: api/reviews/{reviewId}
        //TODO POST: api/reviews
        //TODO PUT: api/reviews/{reviewId}
        //TODO DELETE: api/reviews/{reviewId}
    }
}