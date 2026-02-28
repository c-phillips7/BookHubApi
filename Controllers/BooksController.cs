using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
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

            return Ok(book);
        }


        // POST: api/books
        [HttpPost]
        public async Task<IActionResult> CreateBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // return Ok(book);
            // Changed to CreatedAtAction to return 201
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        // PUT: api/books/{bookId}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
        {
            // validation check
            if (id != updatedBook.Id)
                return BadRequest();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.Title = updatedBook.Title;
            book.Description = updatedBook.Description;
            book.AuthorId = updatedBook.AuthorId;
            // TODO add updating genre

            await _context.SaveChangesAsync();
            return Ok(book);
        }

        // DELETE: api/books/{bookId}
        [HttpDelete("{id}")]
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