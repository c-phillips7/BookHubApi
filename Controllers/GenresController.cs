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

        //TODO GET: api/genres
        //TODO GET: api/genres/{genreId}
        //TODO POST: api/genres
        //TODO PUT: api/genres/{genreId}
        //TODO DELETE: api/genres/{genreId}
    }
}