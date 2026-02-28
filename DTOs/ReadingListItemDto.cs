using BookHub.Models;


public class ReadingListItemDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public BookDto? Book { get; set; }
    public DateTime DateAdded { get; set; }
    public string Status { get; set; } = "";
}

public class ReadingListItemUpdateDto
{
    public ReadingStatus Status { get; set; }
    public DateTime? DateAdded { get; set; } // optional
}