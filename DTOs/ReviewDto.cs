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
    public int BookId { get; set; }
    public string Content { get; set; } = "";
    public int Rating { get; set; }
}

public class ReviewUpdateDto
{
    public string Content { get; set; } = "";
    public int Rating { get; set; }
}