using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
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
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
                return NotFound();
            
            return Ok(genre);
        }

        // POST: api/genres
        [HttpPost]
        public async Task<IActionResult> CreateGenre(Genre genre)
        {
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
        }

        // PUT: api/genres/{genreId}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, Genre updatedGenre)
        {
            if (id != updatedGenre.Id)
                return BadRequest();

            var genre = await _context.Genres.FindAsync(id);

            if (genre == null)
                return NotFound();
            
            genre.Name = updatedGenre.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // DELETE: api/genres/{genreId}
        [HttpDelete("{id}")]
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