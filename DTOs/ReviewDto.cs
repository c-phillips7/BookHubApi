using System.ComponentModel.DataAnnotations;

public class ReviewDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int Rating { get; set; }
    public BookDto Book { get; set; }
    public UserDto User { get; set; }
}

public class ReviewCreateDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = "";

    [Range(1, 5)]
    public int Rating { get; set; }
}

public class ReviewUpdateDto
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = "";
    
    [Range(1, 5)]
    public int Rating { get; set; }
}