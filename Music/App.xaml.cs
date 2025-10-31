using Music.Views;

namespace Music;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

      
        MainPage = new NavigationPage(new MusiciansPage())
        {
            BarBackgroundColor = Colors.Lime,
            BarTextColor = Colors.Black,
        };
    }
}
