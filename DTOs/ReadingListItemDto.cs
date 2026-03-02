using System.ComponentModel.DataAnnotations;
using BookHub.Models;


public class ReadingListItemDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public BookDto? Book { get; set; }
    public DateTime DateAdded { get; set; }
    public string Status { get; set; } = "";
}

public class ReadingListItemCreateDto
{
    [Required]
    public int BookId { get; set; }
    public ReadingStatus Status { get; set; } = ReadingStatus.WantToRead;
}

public class ReadingListItemUpdateDto
{
    [Required]
    public ReadingStatus Status { get; set; }
}