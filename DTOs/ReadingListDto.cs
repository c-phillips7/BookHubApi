using System.ComponentModel.DataAnnotations;
using BookHub.Models;

public class ReadingListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsPublic { get; set; }

    public string UserId { get; set; } = "";
    public string UserName { get; set; } = ""; //  just the user's display name

    public List<ReadingListItemDto> Items { get; set; } = new List<ReadingListItemDto>();
}
public class ReadingListInputDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(2000)]
    public string Description { get; set; } = "";
    
    public bool IsPublic { get; set; }
}
