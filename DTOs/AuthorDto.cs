public class AuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Bio { get; set; }

    public List<BookDto> Books { get; set; } = new List<BookDto>();
}