namespace Music.Models;

public class Musician
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int DebutYear { get; set; }
    public bool IsActive { get; set; } = true;
}
