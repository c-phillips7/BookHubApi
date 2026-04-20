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

        public ReviewsController(ApplicationDbContext context, ILogger<ReviewsController> logger)
            : base(logger)
        {
            _context = context;
        }

        // GET: api/reviews
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReviews()
        {
            Logger.LogInformation("GetReviews called");

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

            Logger.LogInformation("GetReviews returned {Count} results", reviews.Count);
            return Ok(reviews);
        }

        // GET: api/reviews/book/{bookId}
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetReviewsForBook(int bookId)
        {
            Logger.LogInformation("GetReviewsForBook called for book id {BookId}", bookId);

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == bookId)
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

            Logger.LogInformation("GetReviewsForBook returned {Count} reviews for book {BookId}", reviews.Count, bookId);
            return Ok(reviews);
        }

        // GET: api/reviews/{reviewId}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReview(int id)
        {
            Logger.LogInformation("GetReview called for id {Id}", id);

            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                Logger.LogWarning("GetReview: review not found with id {Id}", id);
                return NotFound();
            }

            // Check that the caller is the owner of the review
            // Unsure if this is the desired behaviour as it means users cannot view other users reviews, even if they are public.
            // if (!IsOwner(review.UserId))
            //     return Forbid();

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

            Logger.LogInformation("GetReview: review with id {Id} retrieved successfully", id);
            return Ok(reviewDto);
        }

        // POST: api/reviews
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview(ReviewCreateDto input)
        {
            Logger.LogInformation("CreateReview called for book id {BookId}", input.BookId);

            // make sure the caller is the owner of the review
            var userId = GetCurrentUserId();

            //Check Rating is a valid value
            if (input.Rating < 1 || input.Rating > 5)
            {
                Logger.LogWarning("CreateReview: invalid rating {Rating} provided by user {UserId}", input.Rating, userId);
                return BadRequest("Rating must be between 1 and 5.");
            }

            // Check that the book exists
            var bookExists = await _context.Books.AnyAsync(b => b.Id == input.BookId);
            if (!bookExists)
            {
                Logger.LogWarning("CreateReview: book not found with id {BookId} for user {UserId}", input.BookId, userId);
                return BadRequest("Invalid BookId");
            }

            var review = new Review
            {
                Content = input.Content,
                Rating = input.Rating,
                BookId = input.BookId,
                UserId = userId
            };

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

            Logger.LogInformation("CreateReview: review with id {Id} created successfully for user {UserId}", review.Id, userId);
            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, reviewDto);
        }


        // PUT: api/reviews/{reviewId}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, ReviewUpdateDto input)
        {
            Logger.LogInformation("UpdateReview called for id {Id}", id);

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            // Make sure the caller is the owner of the review
            if (!IsOwner(review.UserId))
            {
                Logger.LogWarning("UpdateReview: user {UserId} attempted to update review with id {Id} without permission", GetCurrentUserId(), id);
                return Forbid();
            }

            // Check if value of review is valid
            if (input.Rating < 1 || input.Rating > 5)
            {
                Logger.LogWarning("UpdateReview: invalid rating {Rating} provided by user {UserId} for review id {Id}", input.Rating, GetCurrentUserId(), id);
                return BadRequest("Rating must be between 1 and 5.");
            }

            review.Content = input.Content;
            review.Rating = input.Rating;

            await _context.SaveChangesAsync();

            Logger.LogInformation("UpdateReview: review with id {Id} updated successfully by user {UserId}", id, GetCurrentUserId());
            return NoContent();
        }

        // DELETE: api/reviews/{reviewId}
        [HttpDelete("{id}")]
        [Authorize]

        public async Task<IActionResult> DeleteReview(int id)
        {
            Logger.LogInformation("DeleteReview called for id {Id}", id);

            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                Logger.LogWarning("DeleteReview: review not found with id {Id} for user {UserId}", id, GetCurrentUserId());
                return NotFound();
            }


            // Check that the caller is the owner of the review
            if (!IsOwner(review.UserId))
            {
                Logger.LogWarning("DeleteReview: user {UserId} attempted to delete review with id {Id} without permission", GetCurrentUserId(), id);
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            Logger.LogInformation("DeleteReview: review with id {Id} deleted successfully", id);
            return NoContent();
        }
    }
}