using Music.Models;

namespace Music.Views;

public partial class EditMusicianPage : ContentPage
{
    private Musician _musician;

    public event EventHandler<Musician>? MusicianSaved;

    public EditMusicianPage(Musician musician)
    {
        InitializeComponent();
        _musician = musician;
        BindMusician();
    }

    private void BindMusician()
    {
        NameEntry.Text = _musician.Name;
        CountryEntry.Text = _musician.Country;
        DebutStepper.Value = _musician.DebutYear == 0 ? DateTime.Now.Year : _musician.DebutYear;
        DebutLabel.Text = DebutStepper.Value.ToString();
        IsActiveSwitch.IsToggled = _musician.IsActive;

        if (!string.IsNullOrEmpty(_musician.Genre))
            GenrePicker.SelectedItem = _musician.Genre;

        DebutStepper.ValueChanged += (s, e) => DebutLabel.Text = ((int)e.NewValue).ToString();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Ошибка", "Имя обязательно.", "OK");
            return;
        }

        _musician.Name = NameEntry.Text.Trim();
        _musician.Country = CountryEntry.Text?.Trim() ?? string.Empty;
        _musician.Genre = GenrePicker.SelectedItem?.ToString() ?? "Other";
        _musician.DebutYear = (int)DebutStepper.Value;
        _musician.IsActive = IsActiveSwitch.IsToggled;

        MusicianSaved?.Invoke(this, _musician);
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
