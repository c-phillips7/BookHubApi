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

        private BooksController(ApplicationDbContext context)
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
                .ToListAsync();
            
            return Ok(books);
        }

    // GET: api/books/{bookId}
    [HttpGet]
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
            return Ok(book);
        }

    // PUT: api/books/{bookId}
    [HttpPut]
    public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
        {
            var book =  await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.Title = updatedBook.Title;
            book.Description = updatedBook.Description;
            book.AuthorId = updatedBook.AuthorId;
            // TODO add updating genre

            await _context.SaveChangesAsync();
            return Ok(book);
        }

    //TODO DELETE: api/books/{bookId}
    [HttpDelete]
    public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }

}