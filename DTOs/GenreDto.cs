using System.ComponentModel.DataAnnotations;

public class GenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<BookDto> Books { get; set; } = new List<BookDto>();
}

public class GenreInputDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
}