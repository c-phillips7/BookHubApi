using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Content = r.Content,
                    Rating = r.Rating,
                    Book = new BookDto { Id = r.Book.Id, Title = r.Book.Title },
                    User = new UserDto
                    {
                        Id = r.User.Id,
                        DisplayName = r.User.DisplayName,
                        ProfilePictureUrl = r.User.ProfilePictureUrl
                    }
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/reviews/{reviewId}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            // Check that the caller is the owner of the review
            if (!IsOwner(review.UserId))
                return Forbid();

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                Content = review.Content,
                Rating = review.Rating,
                Book = new BookDto
                {
                    Id = review.Book.Id,
                    Title = review.Book.Title
                },
                User = new UserDto
                {
                    Id = review.User.Id,
                    DisplayName = review.User.DisplayName,
                    ProfilePictureUrl = review.User.ProfilePictureUrl
                }
            };

            return Ok(reviewDto);
        }

        // POST: api/reviews
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview(Review review)
        {
            // make sure the caller is the owner of the review
            if (!IsOwner(review.UserId))
                return Forbid();

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Include Book data for added Dto used
            var reviewDto = await _context.Reviews
                .Where(r => r.Id == review.Id)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Content = r.Content,
                    Rating = r.Rating,
                    Book = new BookDto
                    {
                        Id = r.Book.Id,
                        Title = r.Book.Title
                    },
                    User = new UserDto
                    {
                        Id = r.User.Id,
                        DisplayName = r.User.DisplayName,
                        ProfilePictureUrl = r.User.ProfilePictureUrl
                    }
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, reviewDto);
        }


        // PUT: api/reviews/{reviewId}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, Review updatedReview)
        {
            // Check if id of request and object match
            if (id != updatedReview.Id)
                return BadRequest();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            // Make sure the caller is the owner of the review
            if (!IsOwner(review.UserId))
                return Forbid();

            // Check if value of review is valid
            if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
                return BadRequest("Rating must be between 1 and 5.");
        

            if (string.IsNullOrWhiteSpace(updatedReview.Content))
                return BadRequest("Review text cannot be empty.");
        

            review.Content = updatedReview.Content;
            review.Rating = updatedReview.Rating;

            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // DELETE: api/reviews/{reviewId}
        [HttpDelete("{id}")]
        [Authorize]

        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return NotFound();


            // Check that the caller is the owner of the review
            if (!IsOwner(review.UserId))
                return Forbid();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}