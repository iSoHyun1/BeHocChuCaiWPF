using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BeHocChuCaiWPF
{
    public partial class ProgressWindow : Window
    {
        private const string ConnectionString = "Data Source=alphabet.db;Version=3;";
        private ObservableCollection<EggPair> eggPairs;
        private static Dictionary<int, bool> crackedStates = new Dictionary<int, bool>();

        public ProgressWindow()
        {
            InitializeComponent();
            LoadEggPairs();
        }

        private void btnBackToStart_Click(object sender, RoutedEventArgs e)
        {
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
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT Letter FROM Alphabet LIMIT 26", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<string> letters = new List<string>();
                        while (reader.Read())
                        {
                            letters.Add(reader["Letter"].ToString());
                        }

                        for (int i = 0; i < 26; i += 2)
                        {
                            var pair = new EggPair
                            {
                                Index = i,
                                Letter1 = letters[i],
                                Letter2 = i + 1 < letters.Count ? letters[i + 1] : null,
                                IsCracked = IsEggCracked(i),
                                IsLocked = !CanAccessEgg(i)
                            };
                            eggPairs.Add(pair);
                        }
                    }
                }

                eggPairsPanel.ItemsSource = eggPairs;
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Lỗi cơ sở dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không mong muốn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsEggCracked(int index) => crackedStates.ContainsKey(index) && crackedStates[index];

        private bool CanAccessEgg(int index)
        {
            if (index == 0) return true;
            return IsEggCracked(index - 2);
        }

        public void UpdateCrackedState(int index)
        {
            crackedStates[index] = true;
            LoadEggPairs();
        }

        private void EggPair_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is int index)
            {
                var pair = eggPairs.FirstOrDefault(p => p.Index == index);
                if (pair != null && CanAccessEgg(index))
                {
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                    fadeOut.Completed += (s, _) =>
                    {
                        var mainWindow = new MainWindow();
                        mainWindow.Owner = this;
                        mainWindow.SetStartIndex(index);
                        mainWindow.Closed += (s, args) =>
                        {
                            UpdateCrackedState(index);
                            this.Show();
                            // Animation khi quay lại
                            this.Opacity = 0;
                            this.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3)));
                        };
                        mainWindow.Show();
                        this.Hide();
                    };
                    this.BeginAnimation(OpacityProperty, fadeOut);
                }
            }
        }
    }
}