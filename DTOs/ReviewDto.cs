public class ReviewDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int Rating { get; set; }
    public BookDto Book { get; set; }
    public UserDto User { get; set; }
}