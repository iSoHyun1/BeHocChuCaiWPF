using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BeHocChuCaiWPF
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private ObservableCollection<EggPair> eggPairs;
        private static Dictionary<int, bool> crackedStates = new Dictionary<int, bool>();

        public ProgressWindow()
        {
            InitializeComponent();
            LoadEggPairs();
        }

        private void btnBackToStart_Click(object sender, RoutedEventArgs e)
        {
            // Animation fade out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            fadeOut.Completed += (s, _) =>
            {
                var startWindow = new StartWindow();
                startWindow.Show();
                this.Close();
            };
            this.BeginAnimation(OpacityProperty, fadeOut);
        }
        private void LoadEggPairs()
        {
            eggPairs = new ObservableCollection<EggPair>();

            try
            {
                using (var conn = new SQLiteConnection("Data Source=alphabet.db;Version=3;"))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("SELECT Letter FROM Alphabet LIMIT 24", conn); // Giới hạn 24 chữ cái
                    var reader = cmd.ExecuteReader();

                    List<string> letters = new List<string>();
                    while (reader.Read())
                    {
                        letters.Add(reader["Letter"].ToString());
                    }

                    // Tạo 12 cặp trứng
                    for (int i = 0; i < 24; i += 2)
                    {
                        var pair = new EggPair
                        {
                            Index = i,
                            Letter1 = letters[i],
                            Letter2 = i + 1 < letters.Count ? letters[i + 1] : null,
                            IsCracked = crackedStates.ContainsKey(i) && crackedStates[i],
                            IsLocked = i > 0 && (!crackedStates.ContainsKey(i - 2) || !crackedStates[i - 2])
                        };
                        eggPairs.Add(pair);
                    }
                }

                eggPairsPanel.ItemsSource = eggPairs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }
        public void UpdateCrackedState(int index)
        {
            crackedStates[index] = true;
            LoadEggPairs(); // Cập nhật giao diện
        }

        private void EggPair_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is int index)
            {
                var pair = eggPairs.FirstOrDefault(p => p.Index == index);
                if (pair != null && !pair.IsLocked)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Owner = this; // Thiết lập Owner
                    mainWindow.SetStartIndex(index);
                    mainWindow.Closed += (s, args) =>
                    {
                        crackedStates[index] = true;
                        LoadEggPairs();
                        this.Show();
                    };
                    mainWindow.Show();
                    this.Hide();
                }
            }
        }
    }
}
