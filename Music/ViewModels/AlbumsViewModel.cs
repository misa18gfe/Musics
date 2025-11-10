using System.Collections.ObjectModel;
using Music.Models;
using Music.Services;

namespace Music.ViewModels
{
    public class AlbumsViewModel : BindableObject
    {
        private readonly DB _db = new();

        public ObservableCollection<Album> Albums { get; } = new();

        private Album? _selected;
        public Album? Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task LoadDataAsync()
        {
            Albums.Clear(); 
            var albums = await _db.GetAlbumsAsync();
            foreach (var album in albums)
                Albums.Add(album);
        }

        public async Task AddAsync(Album album)
        {
           
            await _db.AddAlbumAsync(album);

        
            var albums = await _db.GetAlbumsAsync();

            Albums.Clear(); 
            foreach (var a in albums)
                Albums.Add(a);
        }

        public async Task UpdateAsync(Album album)
        {
            await _db.UpdateAlbumAsync(album);
            await LoadDataAsync();
        }

        public async Task DeleteAsync(Album album)
        {
            if (album == null) return;
            await _db.DeleteAlbumAsync(album.Id);
            await LoadDataAsync();
        }
    }
}