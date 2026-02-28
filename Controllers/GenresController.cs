using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public GenresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/genres
        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            var genre = await _context.Genres
                .Include(g => g.BookGenres)
                    .ThenInclude(bg => bg.Book)
                // Map to DTO to limit data sent to client
                .Select(g => new GenreDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Books = g.BookGenres.Select(bg => new BookDto
                    {
                        Id = bg.Book.Id,
                        Title = bg.Book.Title
                    }).ToList()
                })
                .ToListAsync();

            return Ok(genre);
        }

        // GET: api/genres/{genreId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenre(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.BookGenres)
                    .ThenInclude(bg => bg.Book)
                .Where(g => g.Id == id)
                // Map to DTO to limit data sent to client
                .Select(g => new GenreDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Books = g.BookGenres.Select(bg => new BookDto
                    {
                        Id = bg.Book.Id,
                        Title = bg.Book.Title
                    }).ToList()
                })
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
                return NotFound();

            return Ok(genre);
        }

        // POST: api/genres
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGenre(GenreInputDto input)
        {
            var genre = new Genre { Name = input.Name };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            var genreDto = new GenreDto
            {
                Id = genre.Id,
                Name = genre.Name,
                Books = new List<BookDto>() // No books at creation
            };

            return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
        }

        // PUT: api/genres/{genreId}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGenre(int id, GenreInputDto input)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            genre.Name = input.Name;
            await _context.SaveChangesAsync();

            var genreDto = new GenreDto
            {
                Id = genre.Id,
                Name = genre.Name,
                Books = genre.BookGenres?.Select(bg => new BookDto
                {
                    Id = bg.Book.Id,
                    Title = bg.Book.Title
                }).ToList() ?? new List<BookDto>()
            };

            return Ok(genreDto);
        }


        // DELETE: api/genres/{genreId}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.BookGenres)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
                return NotFound();

            // Remove link table entires first
            _context.BookGenres.RemoveRange(genre.BookGenres);

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}