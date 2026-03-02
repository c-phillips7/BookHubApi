using System.ComponentModel.DataAnnotations;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public AuthorDto? Author { get; set; }
    public List<string> Genres { get; set; } = new List<string>();
}

// Used input as Update and Create have same fields
public class BookInputDto
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = "";

    [MaxLength(2000)]
    public string Description { get; set; } = "";

    [Required]
    public int AuthorId { get; set; }
    public List<int> GenreIds { get; set; } = new List<int>();
}

