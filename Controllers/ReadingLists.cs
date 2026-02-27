using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingListsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReadingListsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //TODO GET: api/readinglists
        //TODO GET: api/readinglists/{listId}
        //TODO POST: api/readinglists
        //TODO PUT: api/readinglists/{listId}
        //TODO DELETE: api/readinglists/{listId}
    }
}
