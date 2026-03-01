using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public BooksController(ApplicationDbContext context, ILogger<BooksController> logger)
            : base(logger)
        {
            _context = context;
        }

        // GET: api/books
        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            Logger.LogInformation("GetBooks called");
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Author = new AuthorDto
                    {
                        Id = b.Author.Id,
                        Name = b.Author.Name
                    },
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList()
                })
                .ToListAsync();

            Logger.LogInformation("GetBooks returned {Count} results", books.Count);

            return Ok(books);
        }

        // GET: api/books/{bookId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            Logger.LogInformation("GetBook called for id {Id}", id);
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                Logger.LogWarning("GetBook: book not found with id {Id}", id);
                return NotFound();
            }

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Author = new AuthorDto
                {
                    Id = book.Author.Id,
                    Name = book.Author.Name
                },
                Genres = book.BookGenres.Select(bg => bg.Genre.Name).ToList()
            };

            return Ok(bookDto);
        }


        // POST: api/books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBook(BookInputDto input)
        {
            Logger.LogInformation("CreateBook called with title {Title}", input.Title);
            // Verify author exists
            var author = await _context.Authors.FindAsync(input.AuthorId);
            if (author == null) return BadRequest("Invalid AuthorId");

            // Verify genres exist
            var validGenreCount = await _context.Genres
                .CountAsync(g => input.GenreIds.Contains(g.Id));
                // If the count of valid genres doesn't match the input count, some IDs are invalid
            if (validGenreCount != input.GenreIds.Count)
                return BadRequest("One or more GenreIds are invalid.");

            // Create entity from input DTO
            var book = new Book
            {
                Title = input.Title,
                Description = input.Description,
                AuthorId = input.AuthorId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Add genres if provided
            if (input.GenreIds.Any())
            {
                var bookGenres = input.GenreIds.Select(gid => new BookGenre
                {
                    BookId = book.Id,
                    GenreId = gid
                });
                _context.BookGenres.AddRange(bookGenres);
                await _context.SaveChangesAsync();
            }

            // Return DTO for response
            var bookDto = await _context.Books
                .Where(b => b.Id == book.Id)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Author = new AuthorDto { Id = b.Author.Id, Name = b.Author.Name },
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList()
                })
                .FirstOrDefaultAsync();

            Logger.LogInformation("CreateBook created book with id {Id}", book.Id);
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
        }

        // PUT: api/books/{bookId}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, BookInputDto input)
        {
            Logger.LogInformation("UpdateBook called for id {Id}", id);

            // Verify book exists
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                Logger.LogWarning("UpdateBook: book not found with id {Id}", id);
                return NotFound();
            }

            // Verify author
            var author = await _context.Authors.FindAsync(input.AuthorId);
            if (author == null) 
            {
                Logger.LogWarning("UpdateBook: invalid AuthorId {AuthorId} for book id {Id}", input.AuthorId, id);
                return BadRequest("Invalid AuthorId");
            }

            // Update entity
            book.Title = input.Title;
            book.Description = input.Description;
            book.AuthorId = input.AuthorId;

            // Update genres - remove existing and add new
            var existingGenres = _context.BookGenres.Where(bg => bg.BookId == book.Id);
            _context.BookGenres.RemoveRange(existingGenres);

            if (input.GenreIds.Any())
            {
                var newGenres = input.GenreIds.Select(gid => new BookGenre
                {
                    BookId = book.Id,
                    GenreId = gid
                });
                _context.BookGenres.AddRange(newGenres);
            }

            await _context.SaveChangesAsync();

            // Return updated DTO
            var bookDto = await _context.Books
                .Where(b => b.Id == book.Id)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Author = new AuthorDto { Id = b.Author.Id, Name = b.Author.Name },
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(bookDto);
        }

        // DELETE: api/books/{bookId}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            Logger.LogInformation("DeleteBook called for id {Id}", id);

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                Logger.LogWarning("DeleteBook: book not found with id {Id}", id);
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            Logger.LogInformation("DeleteBook:  book with id {Id} deleted", id);
            return NoContent();
        }
    }
}