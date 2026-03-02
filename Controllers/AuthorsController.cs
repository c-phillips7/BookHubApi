using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context, ILogger<AuthorsController> logger)
            : base(logger)
        {
            _context = context;
        }

        // GET: api/authors
        [HttpGet]
        public async Task<IActionResult> GetAuthors()
        {
            Logger.LogInformation("GetAuthors called");

            var authorsDto = await _context.Authors
                .Select(a => new AuthorDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Bio = a.Bio,
                    Books = a.Books.Select(b => new BookDto
                    {
                        Id = b.Id,
                        Title = b.Title
                    }).ToList()
                })
                .ToListAsync();

            Logger.LogInformation("GetAuthors returned {Count} authors", authorsDto.Count);
            return Ok(authorsDto);
        }

        // GET: api/authors/{authorId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthor(int id)
        {
            Logger.LogInformation("GetAuthor called for id {Id}", id);

            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                Logger.LogWarning("GetAuthor: author not found with id {Id}", id);
                return NotFound();
            }

            var authorDto = new AuthorDto
            {
                Id = author.Id,
                Name = author.Name,
                Bio = author.Bio,
                Books = author.Books.Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title
                }).ToList()
            };

            return Ok(authorDto);
        }

        // POST: api/authors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAuthor(AuthorInputDto input)
        {
            Logger.LogInformation("CreateAuthor called with name {Name}", input.Name);
            
            // Using new DTO for input to simplify access
                // as AuthorDto includes Books which we don't need for creation
            var author = new Author
            {
                Name = input.Name,
                Bio = input.Bio
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var authorDto = new AuthorDto
            {
                Id = author.Id,
                Name = author.Name,
                Bio = author.Bio,
                Books = new List<BookDto>()
            };

            Logger.LogInformation("Author created with id {Id}", author.Id);

            // Return 201
            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, authorDto);
        }

        // PUT: api/authors/{authorId}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorInputDto input)
        {

            Logger.LogInformation("UpdateAuthor called for id {Id}", id);

            var author = await _context.Authors
                .Include(a => a.Books) // <-- ensure Books are loaded for DTO
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                Logger.LogWarning("UpdateAuthor: author not found with id {Id}", id);
                return NotFound();
            }

            author.Name = input.Name;
            author.Bio = input.Bio;

            await _context.SaveChangesAsync();

            var authorDto = new AuthorDto
            {
                Id = author.Id,
                Name = author.Name,
                Bio = author.Bio,
                Books = author.Books.Select(b => new BookDto { Id = b.Id, Title = b.Title }).ToList()
            };

            Logger.LogInformation("Author with id {Id} updated", id);
            return Ok(authorDto);
        }

        // DELETE: api/authors/{authorId}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            Logger.LogInformation("DeleteAuthor called for id {Id}", id);

            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                Logger.LogWarning("DeleteAuthor: author not found with id {Id}", id);
                return NotFound();
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            Logger.LogInformation("Author with id {Id} deleted", id);
            return NoContent();
        }
    }
}