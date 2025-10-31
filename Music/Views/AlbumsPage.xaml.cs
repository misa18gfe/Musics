using Plugin.Maui.Audio;
using Music.Models;
using Music.ViewModels;

namespace Music.Views;

public partial class AlbumsPage : ContentPage
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _player;
    private Stream? _currentStream;

    private AlbumsViewModel ViewModel => BindingContext as AlbumsViewModel;

    public AlbumsPage()
    {
        InitializeComponent();
        _audioManager = AudioManager.Current;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadDataAsync();
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var page = new EditAlbumPage();
        page.AlbumSaved += async (s, album) =>
        {
            if (album.Id == 0)
                album.Id = ViewModel.Albums.Any() ? ViewModel.Albums.Max(a => a.Id) + 1 : 1;

            await ViewModel.AddAsync(album);
        };
        await Navigation.PushAsync(page);
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (ViewModel.Selected == null)
        {
            await DisplayAlert("Ошибка", "Выберите музыку для редактирования.", "OK");
            return;
        }

        var copy = new Album
        {
            Id = ViewModel.Selected.Id,
            Title = ViewModel.Selected.Title,
            Genre = ViewModel.Selected.Genre,
            ReleaseYear = ViewModel.Selected.ReleaseYear,
            MusicianId = ViewModel.Selected.MusicianId,
            FilePath = ViewModel.Selected.FilePath,
            Cover = ViewModel.Selected.Cover
        };

        var page = new EditAlbumPage(copy);
        page.AlbumSaved += async (s, album) => await ViewModel.UpdateAsync(album);
        await Navigation.PushAsync(page);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (ViewModel.Selected == null)
        {
            await DisplayAlert("Ошибка", "Выберите музыку для удаления.", "OK");
            return;
        }

        bool ok = await DisplayAlert("Подтверждение",
                                     $"Удалить музыку {ViewModel.Selected.Title}?",
                                     "Да", "Нет");
        if (!ok)
            return;

        await ViewModel.DeleteAsync(ViewModel.Selected);
        ViewModel.Selected = null;
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton btn || btn.BindingContext is not Album album)
            return;

        try
        {
            if (_player != null && _player.IsPlaying)
            {
                _player.Dispose();
                _player = null;
                _currentStream?.Dispose();
                _currentStream = null;
                btn.Source = "play_button.png";
                return;
            }

            string tempPath = album.FilePath;
            if (!File.Exists(tempPath))
            {
                tempPath = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(album.FilePath));
                if (!File.Exists(tempPath))
                {
                    using var input = await FileSystem.OpenAppPackageFileAsync(album.FilePath);
                    using var output = File.Create(tempPath);
                    await input.CopyToAsync(output);
                }
            }

            _currentStream = File.OpenRead(tempPath);
            _player = _audioManager.CreatePlayer(_currentStream);
            _player.Play();
            btn.Source = "pause_butoon.png";

            _player.PlaybackEnded += (s, _) =>
            {
                MainThread.BeginInvokeOnMainThread(() => btn.Source = "play_button.png");
                _player.Dispose();
                _player = null;
                _currentStream?.Dispose();
                _currentStream = null;
            };
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось воспроизвести трек: {ex.Message}", "OK");
        }
    }

    private async void OnOpenCarouselClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AlbumCarouselPage());
    }
}
