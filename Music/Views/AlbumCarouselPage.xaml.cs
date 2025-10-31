using System.Timers;
using Music.Models;
using Music.ViewModels;
using Plugin.Maui.Audio;

namespace Music.Views;

public partial class AlbumCarouselPage : ContentPage
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _player;
    private Stream? _currentStream;
    private System.Timers.Timer? _timer;
    private bool _isDragging = false;
    private bool _isDisposing = false;

    private Slider? _activeSlider;
    private Label? _currentLabel;
    private Label? _totalLabel;
    private Button? _activeButton;

    private AlbumsViewModel ViewModel => BindingContext as AlbumsViewModel;

    public AlbumCarouselPage()
    {
        InitializeComponent();
        _audioManager = AudioManager.Current;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadDataAsync();
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not Album album)
            return;

        try
        {
            var parentLayout = btn.Parent as Layout;
            _activeSlider = parentLayout?.FindByName<Slider>("ProgressSlider");
            _currentLabel = parentLayout?.FindByName<Label>("CurrentTimeLabel");
            _totalLabel = parentLayout?.FindByName<Label>("TotalTimeLabel");
            _activeButton = btn;

          
            if (_player != null && _player.IsPlaying)
            {
                StopTimer();
                await SafeDisposeAsync();
                ResetUI(btn);
                return;
            }

          
            StopTimer();
            await SafeDisposeAsync();

            string path;
            if (File.Exists(album.FilePath))
            {
                path = album.FilePath;
            }
            else
            {
                path = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(album.FilePath));
                if (!File.Exists(path))
                {
                    using var input = await FileSystem.OpenAppPackageFileAsync(album.FilePath);
                    using var output = File.Create(path);
                    await input.CopyToAsync(output);
                }
            }

            _currentStream = File.OpenRead(path);
            _player = _audioManager.CreatePlayer(_currentStream);
            _player.Play();
            btn.Text = "Стоп";

            _totalLabel!.Text = TimeSpan.FromSeconds(_player.Duration).ToString(@"m\:ss");
            StartTimer();

            _player.PlaybackEnded += async (s, _) =>
            {
                StopTimer();
                await MainThread.InvokeOnMainThreadAsync(() => ResetUI(btn));
                await SafeDisposeAsync();
            };
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось воспроизвести трек: {ex.Message}", "OK");
        }
    }

   
    private void StartTimer()
    {
        _timer = new System.Timers.Timer(500);
        _timer.Elapsed += (s, e) =>
        {
            if (_player == null || _activeSlider == null || _isDragging)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_player.Duration > 0)
                {
                    _activeSlider.Value = _player.CurrentPosition / _player.Duration;
                    _currentLabel!.Text = TimeSpan.FromSeconds(_player.CurrentPosition).ToString(@"m\:ss");
                }
            });
        };
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

   
    private async Task SafeDisposeAsync()
    {
        if (_isDisposing)
            return;

        _isDisposing = true;

        try
        {
            var oldPlayer = _player;
            var oldStream = _currentStream;
            _player = null;
            _currentStream = null;

            await Task.Run(() =>
            {
                try
                {
                    oldPlayer?.Stop();
                    Thread.Sleep(300);
                    oldPlayer?.Dispose();
                    oldStream?.Dispose();
                }
                catch { }
            });
        }
        finally
        {
            _isDisposing = false;
        }
    }

    private void ResetUI(Button btn)
    {
        btn.Text = "▶️ Играть";
        if (_activeSlider != null)
            _activeSlider.Value = 0;
        if (_currentLabel != null)
            _currentLabel.Text = "0:00";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            StopTimer();
            await SafeDisposeAsync();
            await Task.Delay(200);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка при выходе: {ex.Message}", "OK");
        }
    }

    
    private void OnSliderDragStarted(object sender, EventArgs e) => _isDragging = true;

    private void OnSliderDragCompleted(object sender, EventArgs e)
    {
        _isDragging = false;
        if (_player != null && sender is Slider slider)
        {
            double newPosition = slider.Value * _player.Duration;
            _player.Seek(newPosition);
        }
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isDragging && _player != null && sender is Slider slider)
        {
            double preview = slider.Value * _player.Duration;
            _currentLabel!.Text = TimeSpan.FromSeconds(preview).ToString(@"m\:ss");
        }
    }
}
