using Music.Models;

namespace Music.Services
{
    public class DB
    {
        private readonly MusicRepository<Musician> _musicianRepo = new("musicians.json");
        private readonly MusicRepository<Album> _albumRepo = new("albums.json");

        private List<Musician> _musicians = new();
        private List<Album> _albums = new();

        private bool _loaded = false;

        private async Task EnsureLoadedAsync()
        {
            if (_loaded) return;

            _musicians = await _musicianRepo.GetAllAsync();
            _albums = await _albumRepo.GetAllAsync();

            _loaded = true;
        }

    
        public async Task<List<Musician>> GetMusiciansAsync()
        {
            await EnsureLoadedAsync();
            await Task.Delay(500); 
            return _musicians.ToList();
        }

        public async Task<Musician?> GetMusicianByIdAsync(int id)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);
            return _musicians.FirstOrDefault(m => m.Id == id);
        }

        public async Task AddMusicianAsync(Musician musician)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);

            musician.Id = _musicians.Any() ? _musicians.Max(m => m.Id) + 1 : 1;
            _musicians.Add(musician);
            await _musicianRepo.SaveAllAsync(_musicians);
        }

        public async Task UpdateMusicianAsync(Musician musician)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);

            int idx = _musicians.FindIndex(m => m.Id == musician.Id);
            if (idx != -1)
            {
                _musicians[idx] = musician;
                await _musicianRepo.SaveAllAsync(_musicians);
            }
        }

        public async Task DeleteMusicianAsync(int id)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);

            if (_albums.Any(a => a.MusicianId == id))
                throw new InvalidOperationException("Нельзя удалить музыканта — у него есть альбомы.");

            _musicians.RemoveAll(m => m.Id == id);
            await _musicianRepo.SaveAllAsync(_musicians);
        }

        
        public async Task<List<Album>> GetAlbumsAsync()
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);

            foreach (var a in _albums)
            {
                var m = _musicians.FirstOrDefault(x => x.Id == a.MusicianId);
                a.MusicianName = m?.Name ?? "Неизвестен";
                a.MusicianCountry = m?.Country ?? "—";
            }

            return _albums.ToList();
        }

        public async Task<Album?> GetAlbumByIdAsync(int id)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);
            return _albums.FirstOrDefault(a => a.Id == id);
        }

        public async Task AddAlbumAsync(Album album)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);

            var musician = _musicians.FirstOrDefault(m => m.Id == album.MusicianId);
            if (musician == null)
                throw new InvalidOperationException("Музыкант не найден.");

            album.Id = _albums.Any() ? _albums.Max(a => a.Id) + 1 : 1;
            _albums.Add(album);

            await _albumRepo.SaveAllAsync(_albums);
        }

        public async Task UpdateAlbumAsync(Album album)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);

            int idx = _albums.FindIndex(a => a.Id == album.Id);
            if (idx != -1)
            {
                _albums[idx] = album;
                await _albumRepo.SaveAllAsync(_albums);
            }
        }

        public async Task DeleteAlbumAsync(int id)
        {
            await EnsureLoadedAsync();
            await Task.Delay(500);

            _albums.RemoveAll(a => a.Id == id);
            await _albumRepo.SaveAllAsync(_albums);
        }
    }
}