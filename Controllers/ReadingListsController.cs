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

            // Map to DTOs to control what data is sent back and to flatten the user info
            var listsDto = lists.Select(list => new ReadingListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                UserId = list.UserId,
                Items = list.Items.Select(i => new ReadingListItemDto
                {
                    Id = i.Id,
                    BookId = i.BookId,
                    Book = new BookDto
                    {
                        Id = i.Book.Id,
                        Title = i.Book.Title,
                    },
                    Status = i.Status.ToString(),  // Convert enum to string
                    DateAdded = i.DateAdded
                }).ToList()
            }).ToList();

            return Ok(listsDto);
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
        public async Task<IActionResult> CreateReadingList(ReadingListInputDto input)
        {
            // Check that the caller is the owner of the list
            if (!IsOwner(input.UserId))
                return Forbid();

            var readingList = new ReadingList
            {
                Name = input.Name,
                Description = input.Description,
                IsPublic = input.IsPublic,
                UserId = input.UserId
            };

            _context.ReadingLists.Add(readingList);
            await _context.SaveChangesAsync();

            var output = new ReadingListDto

            // Map to DTO for response to control what data is sent back and to flatten the user info
            {
                Id = readingList.Id,
                Name = readingList.Name,
                Description = readingList.Description,
                IsPublic = readingList.IsPublic,
                UserId = readingList.UserId,
                UserName = (await _context.Users
            .Where(u => u.Id == readingList.UserId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync()) ?? "",
                Items = new List<ReadingListItemDto>()
            };

            return CreatedAtAction(nameof(GetReadingList), new { id = readingList.Id }, readingList);
        }

        // PUT: api/readinglists/{listId}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReadingList(int id, ReadingList updatedList)
        {
            //TODO update with DTO 
            //TODO look into ReadingListItem updates
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
