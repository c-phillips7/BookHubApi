using System.ComponentModel.DataAnnotations;

public class AuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Bio { get; set; }

    public List<BookDto> Books { get; set; } = new List<BookDto>();
};

public class AuthorInputDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Bio { get; set; }
};