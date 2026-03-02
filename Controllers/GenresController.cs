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

        public GenresController(ApplicationDbContext context, ILogger<GenresController> logger)
            : base(logger)
        {
            _context = context;
        }

        // GET: api/genres
        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            Logger.LogInformation("GetGenres called");

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

            Logger.LogInformation("GetGenres returned {Count} results", genre.Count);
            return Ok(genre);
        }

        // GET: api/genres/{genreId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenre(int id)
        {
            Logger.LogInformation("GetGenre called for id {Id}", id);

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
            {
                Logger.LogWarning("GetGenre: genre not found with id {Id}", id);
                return NotFound();
            }

            return Ok(genre);
        }

        // POST: api/genres
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGenre(GenreInputDto input)
        {
            Logger.LogInformation("CreateGenre called with name {Name}", input.Name);

            var genre = new Genre { Name = input.Name };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            var genreDto = new GenreDto
            {
                Id = genre.Id,
                Name = genre.Name,
                Books = new List<BookDto>() // No books at creation
            };

            Logger.LogInformation("Genre created with id {Id}", genre.Id);
            return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genreDto);
        }

        // PUT: api/genres/{genreId}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGenre(int id, GenreInputDto input)
        {
            Logger.LogInformation("UpdateGenre called for id {Id}", id);

            var genre = await _context.Genres
                .Include(g => g.BookGenres)
                    .ThenInclude(bg => bg.Book)
                .FirstOrDefaultAsync(g => g.Id == id);

            
            if (genre == null)
            {
                Logger.LogWarning("UpdateGenre: genre not found with id {Id}", id);
                return NotFound();
            }

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

            Logger.LogInformation("Genre with id {Id} updated", id);
            return Ok(genreDto);
        }


        // DELETE: api/genres/{genreId}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            Logger.LogInformation("DeleteGenre called for id {Id}", id);

            var genre = await _context.Genres
                .Include(g => g.BookGenres)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
            {
                Logger.LogWarning("DeleteGenre: genre not found with id {Id}", id);
                return NotFound();
            }

            // Remove link table entires first
            _context.BookGenres.RemoveRange(genre.BookGenres);

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            Logger.LogInformation("Genre with id {Id} deleted", id);
            return NoContent();
        }
    }
}