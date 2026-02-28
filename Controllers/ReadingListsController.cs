using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingListsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReadingListsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/readinglists
        [HttpGet]
        [Authorize(Roles = "Admin")]
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
        [Authorize]
        public async Task<IActionResult> GetReadingList(int id)
        {
            var list = await _context.ReadingLists
                .Include(rl => rl.User)
                .Include(rl => rl.Items)
                    .ThenInclude(item => item.Book)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            if (list == null)
                return NotFound();
        
            // Check that the caller is the owner of the list or the list is public
            if (!IsOwner(list.UserId) && !list.IsPublic)
                return Forbid();

            return Ok(list);
        }

        // POST: api/readinglists
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReadingList(ReadingList readingList)
        {
            _context.ReadingLists.Add(readingList);
            await _context.SaveChangesAsync();

            // Check that the caller is the owner of the list
            if (!IsOwner(readingList.UserId))
                return Forbid();

            return CreatedAtAction(nameof(GetReadingList), new { id = readingList.Id }, readingList);
        }

        // PUT: api/readinglists/{listId}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReadingList(int id, ReadingList updatedList)
        {
            if (id != updatedList.Id)
                return BadRequest();

            var list = await _context.ReadingLists.FindAsync(id);

            if (list == null)
                return NotFound();

            // Check that the caller is the owner of the list
            if (!IsOwner(list.UserId))
                return Forbid();

            list.Name = updatedList.Name;
            list.Description = updatedList.Description;
            list.IsPublic = updatedList.IsPublic;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // TODO Maybe add seperate put method with auth to change userId of a list

        // DELETE: api/readinglists/{listId}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReadingList(int id)
        {
            var list = await _context.ReadingLists
                .Include(rl => rl.Items)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            if (list == null)
                return NotFound();
            
            // Check that the caller is the owner of the list
            if (!IsOwner(list.UserId))
                return Forbid();

            // Remove link table entries first
            _context.ReadingListItems.RemoveRange(list.Items);

            _context.ReadingLists.Remove(list);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
