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

        //TODO GET: api/authors
        //TODO GET: api/authors/{authorId}
        //TODO POST: api/authors
        //TODO PUT: api/authors/{authorId}
        //TODO DELETE: api/authors/{authorId}
    }
}