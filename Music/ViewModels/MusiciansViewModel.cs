using System.Collections.ObjectModel;
using Music.Models;
using Music.Services;

namespace Music.ViewModels;

public class MusiciansViewModel : BindableObject
{
    private readonly DB _db = new();

    public ObservableCollection<Musician> Musicians { get; } = new();

    private Musician? _selected;
    public Musician? Selected
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
        Musicians.Clear();
        var list = await _db.GetMusiciansAsync();
        foreach (var m in list)
            Musicians.Add(m);
    }

    public async Task AddAsync(Musician musician)
    {
        await _db.AddMusicianAsync(musician);
        await LoadDataAsync();
    }

    public async Task UpdateAsync(Musician musician)
    {
        await _db.UpdateMusicianAsync(musician);
        await LoadDataAsync();
    }

    public async Task DeleteAsync(Musician musician)
    {
        if (musician == null) return;

        try
        {
            await _db.DeleteMusicianAsync(musician.Id);
            await LoadDataAsync();
        }
        catch (InvalidOperationException ex)
        {
          
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
