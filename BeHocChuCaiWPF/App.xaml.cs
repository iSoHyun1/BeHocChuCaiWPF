using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace BeHocChuCaiWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static MediaPlayer BGMPlayer { get; } = new MediaPlayer();
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var startWindow = new StartWindow();
        startWindow.Show();
        BGMPlayer.Open(new Uri("Assets/background_music.mp3", UriKind.Relative));
        BGMPlayer.Volume = 0.3;
        // Khi nhạc chạy hết, tua về đầu và phát lại
        BGMPlayer.MediaEnded += (s, _) =>
        {
            BGMPlayer.Position = TimeSpan.Zero;
            BGMPlayer.Play();
        };

        // Bắt đầu phát nhạc
        BGMPlayer.Play();

    }
}

