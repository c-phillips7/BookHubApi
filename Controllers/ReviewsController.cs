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

        // GET: api/reviews
        [HttpGet]
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
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            //
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
        public async Task<IActionResult> CreateReview(Review review)
        {

            //TODO add check so user can only create posts for themselves
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

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

            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, reviewDto);
        }


        // PUT: api/reviews/{reviewId}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, Review updatedReview)
        {
            // Check if id of request and object match
            if (id != updatedReview.Id)
                return BadRequest();

            // Check if value of review is valid
            if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            if (string.IsNullOrWhiteSpace(updatedReview.Content))
            {
                return BadRequest("Review text cannot be empty.");
            }

            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return NotFound();

            //TODO add check so user can only change own posts

            review.Content = updatedReview.Content;
            review.Rating = updatedReview.Rating;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/reviews/{reviewId}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return NotFound();


            //TODO add check so user can only change own posts

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                Content = review.Content,
                Rating = review.Rating,
                Book = new BookDto { Id = review.Book.Id, Title = review.Book.Title },
                User = new UserDto { Id = review.User.Id, DisplayName = review.User.DisplayName, ProfilePictureUrl = review.User.ProfilePictureUrl }
            };

            return Ok(reviewDto);


        }

    }
}