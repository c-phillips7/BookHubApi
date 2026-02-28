public class GenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<BookDto> Books { get; set; } = new List<BookDto>();
}

public class GenreInputDto
{
    public string Name { get; set; } = "";
}