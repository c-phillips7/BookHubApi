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
    }

    //TODO GET: api/books
    //TODO GET: api/books/{bookId}
    //TODO POST: api/books
    //TODO PUT: api/books/{bookId}
    //TODO DELETE: api/books/{bookId}

}