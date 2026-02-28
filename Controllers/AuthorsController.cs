using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/authors
        [HttpGet]
        public async Task<IActionResult> GetAuthors()
        {
            var authors = await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();

            return Ok(authors);
        }

        // GET: api/authors/{authorId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if(author ==null)
                return NotFound();

            return Ok(author);
        }

        // POST: api/authors
        [HttpPost]
        public async Task<IActionResult> CreateAuthor(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            // Return 201
            return CreatedAtAction(nameof(GetAuthor), new {id = author.Id}, author);
        }

        // PUT: api/authors/{authorId}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, Author updatedAuthor)
        {
            // Check if Id of object passed matches request
            if (id != updatedAuthor.Id)
                return BadRequest();

            var author = await _context.Authors.FindAsync(id);

            if (author == null)
                return NotFound();

            author.Name = updatedAuthor.Name;
            author.Bio = updatedAuthor.Bio;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/authors/{authorId}
        [HttpDelete("{id}")]
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