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

        // GET: api/readinglists
        [HttpGet]
        public async Task<IActionResult> GetReadingLists()
        {
            var lists = await _context.ReadingLists
                .Include(rl => rl.User)
                .Include(rl => rl.Items)
                    .ThenInclude(item => item.Book)
                .ToListAsync();

            return Ok(lists);
        }

        // GET: api/readinglists/{listId}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReadingList(int id)
        {
            var list = await _context.ReadingLists
                .Include(rl => rl.User)
                .Include(rl => rl.Items)
                    .ThenInclude(item => item.Book)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            if (list == null)
                return NotFound();

            return Ok(list);
        }

        // POST: api/readinglists
        [HttpPost]
        public async Task<IActionResult> CreateReadingList(ReadingList readingList)
        {
            _context.ReadingLists.Add(readingList);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReadingList), new { id = readingList.Id }, readingList);
        }

        // PUT: api/readinglists/{listId}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReadingList(int id, ReadingList updatedList)
        {
            if (id != updatedList.Id)
                return BadRequest();

            var list = await _context.ReadingLists.FindAsync(id);

            if (list == null)
                return NotFound();

            // TODO add Auth check to see if user owns list
            
            list.Name = updatedList.Name;
            list.Description = updatedList.Description;
            list.IsPublic = updatedList.IsPublic;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // TODO Maybe add seperate put method with auth to change userId of a list

        // DELETE: api/readinglists/{listId}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReadingList(int id)
        {
            var list = await _context.ReadingLists
                .Include(rl => rl.Items)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            if (list == null)
                return NotFound();
            
            // Remove link table entries first
            _context.ReadingListItems.RemoveRange(list.Items);

            _context.ReadingLists.Remove(list);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
