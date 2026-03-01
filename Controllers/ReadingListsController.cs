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

        public ReadingListsController(ApplicationDbContext context, ILogger<ReadingListsController> logger)
            : base(logger)
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
                UserName = list.User?.DisplayName ?? "",
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

            // Map to DTOs to control what data is sent back and to flatten the user info
            var listDto = new ReadingListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                UserId = list.UserId,
                UserName = list.User?.DisplayName ?? "",
                Items = list.Items.Select(i => new ReadingListItemDto
                {
                    Id = i.Id,
                    BookId = i.BookId,
                    Book = new BookDto
                    {
                        Id = i.Book.Id,
                        Title = i.Book.Title
                    },
                    Status = i.Status.ToString(),
                    DateAdded = i.DateAdded
                }).ToList()
            };
            return Ok(listDto);
        }

        // POST: api/readinglists
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReadingList(ReadingListInputDto input)
        {
            // Get user ID from JWT token, only allows creating lists for self, not other users
            var userId = GetCurrentUserId();

            var readingList = new ReadingList
            {
                Name = input.Name,
                Description = input.Description,
                IsPublic = input.IsPublic,
                UserId = userId
            };

            _context.ReadingLists.Add(readingList);
            await _context.SaveChangesAsync();

            // Map to DTO for response to control what data is sent back and to flatten the user info
            var output = new ReadingListDto
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

            return CreatedAtAction(nameof(GetReadingList), new { id = readingList.Id }, output);
        }

        // POST api/readinglists/{listId}/items
        [HttpPost("{id}/items")]
        [Authorize]
        public async Task<IActionResult> AddItemToReadingList(int id, ReadingListItemCreateDto input)
        {
            var list = await _context.ReadingLists.FindAsync(id);
            if (list == null) return NotFound();

            if (!IsOwner(list.UserId)) return Forbid();

            var bookExists = await _context.Books.AnyAsync(b => b.Id == input.BookId);
            if (!bookExists) return BadRequest("Invalid BookId");

            var newItem = new ReadingListItem
            {
                ReadingListId = id,
                BookId = input.BookId,
                Status = input.Status
            };

            _context.ReadingListItems.Add(newItem);
            await _context.SaveChangesAsync();

            var itemDto = new ReadingListItemDto
            {
                Id = newItem.Id,
                BookId = newItem.BookId,
                Status = newItem.Status.ToString(),
                DateAdded = newItem.DateAdded
            };

            return CreatedAtAction(nameof(GetReadingList), new { id }, itemDto);
        }

        // PUT: api/readinglists/{listId}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReadingList(int id, ReadingListInputDto input)
        {
            var list = await _context.ReadingLists.FindAsync(id);

            if (list == null)
                return NotFound();

            // Check that the caller is the owner of the list
            if (!IsOwner(list.UserId))
                return Forbid();

            list.Name = input.Name;
            list.Description = input.Description;
            list.IsPublic = input.IsPublic;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/readinglists/{listId}/items/{itemId}
        [HttpPut("{listId}/items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReadingListItem(int listId, int itemId, ReadingListItemUpdateDto input)
        {
            var item = await _context.ReadingListItems
                .Include(i => i.ReadingList)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ReadingListId == listId);

            if (item == null) return NotFound();

            if (!IsOwner(item.ReadingList.UserId)) return Forbid();

            item.Status = input.Status;

            await _context.SaveChangesAsync();
            var itemDto = new ReadingListItemDto

            {
                Id = item.Id,
                BookId = item.BookId,
                Status = item.Status.ToString(),
                DateAdded = item.DateAdded
            };

            return Ok(itemDto);
        }

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

        // DELETE: api/readinglists/{listId}/items/{itemId}
        [HttpDelete("{listId}/items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReadingListItem(int listId, int itemId)
        {
            var item = await _context.ReadingListItems
                .Include(i => i.ReadingList)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ReadingListId == listId);

            if (item == null) return NotFound();

            if (!IsOwner(item.ReadingList.UserId)) return Forbid();

            _context.ReadingListItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
