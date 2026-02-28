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

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/books
        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
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

            return Ok(books);
        }

        // GET: api/books/{bookId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

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
        public async Task<IActionResult> CreateBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var authorExists = await _context.Authors.AnyAsync(a => a.Id == book.AuthorId);
            if (!authorExists)
                return BadRequest("Invalid AuthorId");

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Author = await _context.Authors
                    .Where(a => a.Id == book.AuthorId)
                    .Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
                    .FirstOrDefaultAsync(),
                Genres = await _context.BookGenres
                    .Where(bg => bg.BookId == book.Id)
                    .Select(bg => bg.Genre.Name)
                    .ToListAsync()
            };

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
        }

        // PUT: api/books/{bookId}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
        {
            // validation check
            if (id != updatedBook.Id)
                return BadRequest();

            var authorExists = await _context.Authors.AnyAsync(a => a.Id == updatedBook.AuthorId);
            if (!authorExists)
                return BadRequest("Invalid AuthorId");

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.Title = updatedBook.Title;
            book.Description = updatedBook.Description;
            book.AuthorId = updatedBook.AuthorId;
            // TODO add updating genre

            await _context.SaveChangesAsync();

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Author = await _context.Authors
                    .Where(a => a.Id == book.AuthorId)
                    .Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
                    .FirstOrDefaultAsync(),
                Genres = await _context.BookGenres
                    .Where(bg => bg.BookId == book.Id)
                    .Select(bg => bg.Genre.Name)
                    .ToListAsync()
            };

            return Ok(bookDto);
        }

        // DELETE: api/books/{bookId}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}