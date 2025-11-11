using Music.ViewModels;
using Music.Models;

namespace Music.Views;

public partial class MusiciansPage : ContentPage
{
    private MusiciansViewModel ViewModel => BindingContext as MusiciansViewModel;

    public MusiciansPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel.Musicians == null || ViewModel.Musicians.Count == 0)
        {
            await ViewModel.LoadDataAsync();
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null)
            return;

        var selected = e.CurrentSelection.FirstOrDefault() as Musician;
        ViewModel.Selected = selected;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var page = new EditMusicianPage(new Musician());
        page.MusicianSaved += async (s, musician) =>
        {
            if (musician.Id == 0)
                musician.Id = ViewModel.Musicians.Any() ? ViewModel.Musicians.Max(m => m.Id) + 1 : 1;

            await ViewModel.AddAsync(musician);
        };
        await Navigation.PushAsync(page);
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (ViewModel.Selected == null)
        {
            await DisplayAlert("Ошибка", "Выберите музыканта для редактирования.", "OK");
            return;
        }

        var copy = new Musician
        {
            Id = ViewModel.Selected.Id,
            Name = ViewModel.Selected.Name,
            Genre = ViewModel.Selected.Genre,
            Country = ViewModel.Selected.Country,
            DebutYear = ViewModel.Selected.DebutYear,
            IsActive = ViewModel.Selected.IsActive
        };

        var page = new EditMusicianPage(copy);
        page.MusicianSaved += async (s, musician) => await ViewModel.UpdateAsync(musician);
        await Navigation.PushAsync(page);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (ViewModel.Selected == null)
        {
            await DisplayAlert("Ошибка", "Выберите музыканта для удаления.", "OK");
            return;
        }

        bool ok = await DisplayAlert("Подтверждение",
                                     $"Удалить музыканта {ViewModel.Selected.Name}?",
                                     "Да", "Нет");
        if (!ok)
            return;

        await ViewModel.DeleteAsync(ViewModel.Selected);
        ViewModel.Selected = null;
    }

    private async void OnOpenAlbumsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AlbumsPage());
    }
}
