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

            return Ok(authorsDto);
        }

        // GET: api/authors/{authorId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return NotFound();

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

            // Return 201
            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, authorDto);
        }

        // PUT: api/authors/{authorId}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorInputDto input)
        {
            
            // Check if Id of object passed matches request
                // Made redundant by using DTO which does not have the Id property
            // if (id != updatedAuthor.Id)
            //     return BadRequest();

            var author = await _context.Authors
                .Include(a => a.Books) // <-- ensure Books are loaded for DTO
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return NotFound();

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

            return Ok(authorDto);
        }

        // DELETE: api/authors/{authorId}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
                return NotFound();

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}