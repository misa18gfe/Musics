using Music.Models;
using Music.Services;

namespace Music.Views;

public partial class EditAlbumPage : ContentPage
{
    private Album _album;
    private readonly DB _db = new();
    public event EventHandler<Album>? AlbumSaved;

    public EditAlbumPage(Album? album = null)
    {
        InitializeComponent();
        _album = album ?? new Album();

        GenrePicker.ItemsSource = new List<string>
        {
            "Rock", "Pop", "Jazz", "Electronic", "Hip-Hop", "Other"
        };

        LoadMusicians();
        FillFields();
    }

    private async void LoadMusicians()
    {
        try
        {
            var musicians = await _db.GetMusiciansAsync();
            MusicianPicker.ItemsSource = musicians;
            MusicianPicker.ItemDisplayBinding = new Binding(nameof(Musician.Name));

            if (_album.MusicianId != 0)
            {
                var selected = musicians.FirstOrDefault(m => m.Id == _album.MusicianId);
                if (selected != null)
                    MusicianPicker.SelectedItem = selected;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить музыкантов: {ex.Message}", "OK");
        }
    }

    private void FillFields()
    {
        TitleEntry.Text = _album.Title;
        if (!string.IsNullOrEmpty(_album.Genre))
            GenrePicker.SelectedItem = _album.Genre;

        var year = _album.ReleaseYear == 0 ? DateTime.Now.Year : _album.ReleaseYear;
        ReleaseDatePicker.Date = new DateTime(year, 1, 1);
        FileEntry.Text = _album.FilePath;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("Ошибка", "Название музыки обязательно.", "OK");
            return;
        }

        if (MusicianPicker.SelectedItem is not Musician selectedMusician)
        {
            await DisplayAlert("Ошибка", "Выберите музыканта.", "OK");
            return;
        }

        _album.Title = TitleEntry.Text.Trim();
        _album.Genre = GenrePicker.SelectedItem?.ToString() ?? "Other";
        _album.ReleaseYear = ReleaseDatePicker.Date.Year;
        _album.FilePath = FileEntry.Text?.Trim() ?? string.Empty;
        _album.MusicianId = selectedMusician.Id;

        AlbumSaved?.Invoke(this, _album);
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
