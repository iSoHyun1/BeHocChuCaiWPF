using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BeHocChuCaiWPF
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private bool isMusicOn = true;
        public StartWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Mở màn hình progress và đóng màn hình start
            var progressWindow = new ProgressWindow();
            progressWindow.Show();
            this.Close();
        }


        private void btnToggleMusic_Click(object sender, RoutedEventArgs e)
        {
            if (isMusicOn)
            {
                App.BGMPlayer.Pause();
                btnToggleMusic.Content = "Bật Nhạc";
                isMusicOn = false;
            }
            else
            {
                App.BGMPlayer.Play();
                btnToggleMusic.Content = "Tắt Nhạc";
                isMusicOn = true;
            }
        }

    }
}
