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
            Logger.LogInformation("GetReadingLists called");

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

            Logger.LogInformation("GetReadingLists returned {Count} results", listsDto.Count);
            return Ok(listsDto);
        }

        // GET: api/readinglists/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyReadingLists()
        {
            var userId = GetCurrentUserId();
            Logger.LogInformation("GetMyReadingLists called for user {UserId}", userId);

            var lists = await _context.ReadingLists
                .Include(rl => rl.User)
                .Include(rl => rl.Items)
                    .ThenInclude(item => item.Book)
                .Where(rl => rl.UserId == userId)
                .ToListAsync();

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
                    Status = i.Status.ToString(),
                    DateAdded = i.DateAdded
                }).ToList()
            }).ToList();

            Logger.LogInformation("GetMyReadingLists returned {Count} lists for user {UserId}", listsDto.Count, userId);
            return Ok(listsDto);
        }

        // GET: api/readinglists/{listId}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReadingList(int id)
        {
            Logger.LogInformation("GetReadingList called for id {Id}", id);

            var list = await _context.ReadingLists
                .Include(rl => rl.User)
                .Include(rl => rl.Items)
                    .ThenInclude(item => item.Book)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            if (list == null)
            {
                Logger.LogWarning("GetReadingList: list not found with id {Id}", id);
                return NotFound();
            }

            // Check that the caller is the owner of the list or the list is public
            if (!IsOwner(list.UserId) && !list.IsPublic)
            {
                Logger.LogWarning("GetReadingList: user {UserId} attempted to access private list owned by {Id}", GetCurrentUserId(), id);
                return Forbid();
            }

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
            
            Logger.LogInformation("GetReadingList returned list with id {Id}", listDto.Id);
            return Ok(listDto);
        }

        // POST: api/readinglists
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReadingList(ReadingListInputDto input)
        {
            // Get user ID from JWT token, only allows creating lists for self, not other users
            var userId = GetCurrentUserId();
            Logger.LogInformation("CreateReadingList called by user {UserId}", userId);

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

            Logger.LogInformation("ReadingList created with id {Id} for user {UserId}", readingList.Id, userId);
            return CreatedAtAction(nameof(GetReadingList), new { id = readingList.Id }, output);
        }

        // POST api/readinglists/{listId}/items
        [HttpPost("{id}/items")]
        [Authorize]
        public async Task<IActionResult> AddItemToReadingList(int id, ReadingListItemCreateDto input)
        {
            Logger.LogInformation("AddItemToReadingList called for list id {Id}, book {BookId}", id, input.BookId);

            var list = await _context.ReadingLists.FindAsync(id);
            if (list == null) 
            {
                Logger.LogWarning("AddItemToReadingList: list with id {Id} not found", id);
                return NotFound();
            }

            if (!IsOwner(list.UserId)) 
            {
                Logger.LogWarning("AddItemToReadingList: user {UserId} attempted to add item to list owned by {OwnerId}", GetCurrentUserId(), list.UserId);
                return Forbid();
            }

            var bookExists = await _context.Books.AnyAsync(b => b.Id == input.BookId);
            if (!bookExists)
            {
                Logger.LogWarning("AddItemToReadingList: book with id {BookId} not found", input.BookId);
                return BadRequest("Invalid BookId");
            }

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

            Logger.LogInformation("Item added to ReadingList {ListId} with item id {ItemId}", id, itemDto.Id);
            return CreatedAtAction(nameof(GetReadingList), new { id }, itemDto);
        }

        // PUT: api/readinglists/{listId}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReadingList(int id, ReadingListInputDto input)
        {
            Logger.LogInformation("UpdateReadingList called for id {Id}", id);

            var list = await _context.ReadingLists.FindAsync(id);

            if (list == null)
            {
                Logger.LogWarning("UpdateReadingList: list with id {Id} not found", id);
                return NotFound();
            }

            // Check that the caller is the owner of the list
            if (!IsOwner(list.UserId))
            {
                Logger.LogWarning("UpdateReadingList: user {UserId} attempted to update list with id {Id}", GetCurrentUserId(), id);
                return Forbid();
            }

            list.Name = input.Name;
            list.Description = input.Description;
            list.IsPublic = input.IsPublic;

            await _context.SaveChangesAsync();

            Logger.LogInformation("ReadingList with id {Id} updated successfully", id);
            return NoContent();
        }

        // PUT: api/readinglists/{listId}/items/{itemId}
        [HttpPut("{listId}/items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReadingListItem(int listId, int itemId, ReadingListItemUpdateDto input)
        {
            Logger.LogInformation("UpdateReadingListItem called for list id {ListId}, item id {ItemId}", listId, itemId);

            var item = await _context.ReadingListItems
                .Include(i => i.ReadingList)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ReadingListId == listId);

            if (item == null) 
            {
                Logger.LogWarning("UpdateReadingListItem: item with id {ItemId} not found in list {ListId}", itemId, listId);
                return NotFound();
            }

            if (!IsOwner(item.ReadingList.UserId))
            {
                Logger.LogWarning("UpdateReadingListItem: user {UserId} attempted to update item {ItemId} in list with id {ListId}", GetCurrentUserId(), itemId, listId);
                return Forbid();    
            }

            item.Status = input.Status;

            await _context.SaveChangesAsync();
            
            var itemDto = new ReadingListItemDto
            {
                Id = item.Id,
                BookId = item.BookId,
                Status = item.Status.ToString(),
                DateAdded = item.DateAdded
            };

            Logger.LogInformation("Item with id {ItemId} in ReadingList {ListId} updated successfully", itemId, listId);
            return Ok(itemDto);
        }

        // DELETE: api/readinglists/{listId}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReadingList(int id)
        {
            Logger.LogInformation("DeleteReadingList called for id {Id}", id);

            var list = await _context.ReadingLists
                .Include(rl => rl.Items)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            if (list == null)
            {
                Logger.LogWarning("DeleteReadingList: list with id {Id} not found", id);
                return NotFound();   
            }

            // Check that the caller is the owner of the list
            if (!IsOwner(list.UserId))
            {
                Logger.LogWarning("DeleteReadingList: user {UserId} attempted to delete list with id {Id}", GetCurrentUserId(), id);
                return Forbid();
            }

            // Remove link table entries first
            _context.ReadingListItems.RemoveRange(list.Items);

            _context.ReadingLists.Remove(list);
            await _context.SaveChangesAsync();

            Logger.LogInformation("DeleteReadingList: list with id {Id} deleted successfully", id);
            return NoContent();
        }

        // DELETE: api/readinglists/{listId}/items/{itemId}
        [HttpDelete("{listId}/items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReadingListItem(int listId, int itemId)
        {
            Logger.LogInformation("DeleteReadingListItem called for list id {ListId}, item id {ItemId}", listId, itemId);

            var item = await _context.ReadingListItems
                .Include(i => i.ReadingList)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ReadingListId == listId);

            if (item == null)
            {
                Logger.LogWarning("DeleteReadingListItem: item with id {ItemId} not found in list {ListId}", itemId, listId);
                return NotFound();   
            }

            if (!IsOwner(item.ReadingList.UserId))
            {
                Logger.LogWarning("DeleteReadingListItem: user {UserId} attempted to delete item {ItemId} in list with id {ListId}", GetCurrentUserId(), itemId, listId);
                return Forbid();
            }

            _context.ReadingListItems.Remove(item);
            await _context.SaveChangesAsync();

            Logger.LogInformation("DeleteReadingListItem: item with id {ItemId} in list {ListId} deleted successfully", itemId, listId);
            return NoContent();
        }
    }
}
