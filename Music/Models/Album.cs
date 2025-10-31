namespace Music.Models;

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int ReleaseYear { get; set; }
    public string Genre { get; set; } = "";
    public int MusicianId { get; set; }
    public string FilePath { get; set; } = "";
    public string Cover { get; set; } = "";
    public string MusicianName { get; set; } = "";
    public string MusicianCountry { get; set; } = "";
}
