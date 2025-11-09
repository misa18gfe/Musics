using Music.Models;

namespace Music.Services;

public class DB
{
    private readonly MusicRepository<Musician> _musicianRepo = new("musicians.json");
    private readonly MusicRepository<Album> _albumRepo = new("albums.json");

  
    public async Task<List<Musician>> GetMusiciansAsync() => await _musicianRepo.GetAllAsync();

    public async Task<Musician?> GetMusicianByIdAsync(int id)
    {
        var list = await _musicianRepo.GetAllAsync();
        return list.FirstOrDefault(m => m.Id == id);//бвд
    }

    public async Task AddMusicianAsync(Musician musician) => await _musicianRepo.AddAsync(musician);

    public async Task UpdateMusicianAsync(Musician musician) => await _musicianRepo.UpdateAsync(musician);

    public async Task DeleteMusicianAsync(int id)
    {
        var albums = await _albumRepo.GetAllAsync();
        if (albums.Any(a => a.MusicianId == id))
            throw new InvalidOperationException("Нельзя удалить музыканта — у него есть альбомы.");

        await _musicianRepo.DeleteAsync(id);
    }

  
    public async Task<List<Album>> GetAlbumsAsync() => await _albumRepo.GetAllAsync();

    public async Task<Album?> GetAlbumByIdAsync(int id)
    {
        var list = await _albumRepo.GetAllAsync();
        return list.FirstOrDefault(a => a.Id == id);
    }

    public async Task AddAlbumAsync(Album album) => await _albumRepo.AddAsync(album);

    public async Task UpdateAlbumAsync(Album album) => await _albumRepo.UpdateAsync(album);

    public async Task DeleteAlbumAsync(int id) => await _albumRepo.DeleteAsync(id);
}
