using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace BeHocChuCaiWPF
{
    public partial class MainWindow : Window
    {
        private int currentIndex = 0;
        private string currentLetter1, currentLetter2, letterImage1, letterImage2;
        private string word1, word2, wordImage1, wordImage2;
        private bool egg1Cracked = false, egg2Cracked = false;
        private string imageFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        private readonly VietnameseSpeechHelper speechHelper;

        public MainWindow()
        {
            InitializeComponent();
            speechHelper = new VietnameseSpeechHelper();
            LoadEggs();
        }

        public void SetStartIndex(int index)
        {
            currentIndex = index;
            LoadEggs();
            UpdateProgress(); // Cập nhật tiến độ khi thiết lập chỉ số bắt đầu
        }
        private void UpdateProgress()
        {
            int totalPairs = 12;
            int currentPair = (currentIndex / 2) + 1;
            progressText.Text = $"Cặp {currentPair}/{totalPairs}";
        }
        private void LoadEggs()
        {
            try
            {
                using (var conn = new SQLiteConnection("Data Source=alphabet.db;Version=3;"))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("SELECT Letter, LetterImage FROM Alphabet LIMIT 2 OFFSET @index", conn);
                    cmd.Parameters.AddWithValue("@index", currentIndex);

                    var reader = cmd.ExecuteReader();

                    // Trứng 1
                    if (reader.Read())
                    {
                        currentLetter1 = reader["Letter"].ToString().Trim();
                        letterImage1 = Path.Combine(imageFolderPath, reader["LetterImage"].ToString());

                        picEgg1Whole.Source = new BitmapImage(new Uri(Path.Combine(imageFolderPath, "egg_whole.png")));
                        picEgg1Cracked.Source = new BitmapImage(new Uri(Path.Combine(imageFolderPath, "egg_cracked.png")));

                        picEgg1Whole.Visibility = Visibility.Visible;
                        picEgg1Cracked.Visibility = Visibility.Hidden;
                        picLetter1.Visibility = Visibility.Hidden;
                        word1Container.Visibility = Visibility.Hidden;
                        egg1Cracked = false;
                    }

                    // Trứng 2
                    if (reader.Read())
                    {
                        currentLetter2 = reader["Letter"].ToString().Trim();
                        letterImage2 = Path.Combine(imageFolderPath, reader["LetterImage"].ToString());

                        picEgg2Whole.Source = new BitmapImage(new Uri(Path.Combine(imageFolderPath, "egg_whole.png")));
                        picEgg2Cracked.Source = new BitmapImage(new Uri(Path.Combine(imageFolderPath, "egg_cracked.png")));

                        picEgg2Whole.Visibility = Visibility.Visible;
                        picEgg2Cracked.Visibility = Visibility.Hidden;
                        picLetter2.Visibility = Visibility.Hidden;
                        word2Container.Visibility = Visibility.Hidden;
                        egg2Cracked = false;
                    }
                    else
                    {
                        picEgg2Whole.Visibility = Visibility.Hidden;
                    }

                    btnNextPair.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}");
            }

            UpdateProgress(); // Cập nhật tiến độ khi tải trứng
        }

        private async void picEgg1_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Ẩn trứng nguyên
                picEgg1Whole.Visibility = Visibility.Hidden;
                picEgg1Cracked.Visibility = Visibility.Visible;

                // Animation cho trứng vỡ
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
                var scaleUp = new DoubleAnimation(1, 1.2, TimeSpan.FromSeconds(0.3));

                var crackedTransform = picEgg1Cracked.RenderTransform as ScaleTransform;
                picEgg1Cracked.BeginAnimation(OpacityProperty, fadeOut);
                crackedTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
                crackedTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

                await Task.Delay(1000);

                picEgg1Cracked.Visibility = Visibility.Hidden;

                // Animation cho chữ cái xuất hiện
                picLetter1.Opacity = 0;
                picLetter1.Visibility = Visibility.Visible;
                picLetter1.Source = new BitmapImage(new Uri(letterImage1));

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                var scaleIn = new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.5));

                var letterTransform = picLetter1.RenderTransform as ScaleTransform;
                picLetter1.BeginAnimation(OpacityProperty, fadeIn);
                letterTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
                letterTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);

                egg1Cracked = true;

                if (egg2Cracked)
                {
                    btnNextPair.Visibility = Visibility.Visible;
                    btnNextPair.Opacity = 0;
                    btnNextPair.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)));
                }

                speechHelper.SpeakVietnamese(currentLetter1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xử lý animation: {ex.Message}");
            }
            
        }

        private async void picEgg2_Click(object sender, MouseButtonEventArgs e)
        {
            // Check if first egg is not cracked yet
            if (!egg1Cracked)
            {
                return; // Exit the method without doing anything
            }

            try
            {
                // Ẩn trứng nguyên
                picEgg2Whole.Visibility = Visibility.Hidden;
                picEgg2Cracked.Visibility = Visibility.Visible;

                // Rest of the existing code remains the same...
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
                var scaleUp = new DoubleAnimation(1, 1.2, TimeSpan.FromSeconds(0.3));

                var crackedTransform = picEgg2Cracked.RenderTransform as ScaleTransform;
                picEgg2Cracked.BeginAnimation(OpacityProperty, fadeOut);
                crackedTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
                crackedTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

                await Task.Delay(1000);

                picEgg2Cracked.Visibility = Visibility.Hidden;

                // Animation cho chữ cái xuất hiện
                picLetter2.Opacity = 0;
                picLetter2.Visibility = Visibility.Visible;
                picLetter2.Source = new BitmapImage(new Uri(letterImage2));

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                var scaleIn = new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.5));

                var letterTransform = picLetter2.RenderTransform as ScaleTransform;
                picLetter2.BeginAnimation(OpacityProperty, fadeIn);
                letterTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
                letterTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);

                egg2Cracked = true;

                if (egg1Cracked)
                {
                    btnNextPair.Visibility = Visibility.Visible;
                    btnNextPair.Opacity = 0;
                    btnNextPair.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)));
                }
                speechHelper.SpeakVietnamese(currentLetter2);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xử lý animation: {ex.Message}");
            }
           
        }

        private async void picLetter1_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string wordToSpeak = "";
                using (var conn = new SQLiteConnection("Data Source=alphabet.db;Version=3;"))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("SELECT Word, WordImage FROM Vocabulary WHERE Alphabet_ID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", currentIndex + 1);

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        word1 = reader["Word"].ToString().Trim();
                        wordToSpeak = word1; // Lưu từ vựng trước khi đóng connection
                        wordImage1 = Path.Combine(imageFolderPath, reader["WordImage"].ToString());

                        // Ẩn chữ cái với animation
                        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                        picLetter1.BeginAnimation(OpacityProperty, fadeOut);

                        // Hiện container từ vựng
                        word1Container.Visibility = Visibility.Visible;
                        word1Container.Opacity = 0;
                        lblWord1.Text = word1;
                        picWord1.Source = new BitmapImage(new Uri(wordImage1));

                        // Animation hiện từ vựng
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                        word1Container.BeginAnimation(OpacityProperty, fadeIn);

                        picLetter1.Visibility = Visibility.Hidden;
                    }
                }

                // Đọc từ vựng sau khi đã load xong
                if (!string.IsNullOrEmpty(wordToSpeak))
                {
                    await speechHelper.SpeakVietnamese(wordToSpeak); // Sửa ở đây, dùng wordToSpeak thay vì currentLetter1
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải từ vựng: {ex.Message}");
            }
        }

        private async void picLetter2_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string wordToSpeak = "";
                using (var conn = new SQLiteConnection("Data Source=alphabet.db;Version=3;"))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("SELECT Word, WordImage FROM Vocabulary WHERE Alphabet_ID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", currentIndex + 2);

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        word2 = reader["Word"].ToString().Trim();
                        wordToSpeak = word2; // Lưu từ vựng trước khi đóng connection
                        wordImage2 = Path.Combine(imageFolderPath, reader["WordImage"].ToString());

                        // Ẩn chữ cái với animation
                        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                        picLetter2.BeginAnimation(OpacityProperty, fadeOut);

                        // Hiện container từ vựng
                        word2Container.Visibility = Visibility.Visible;
                        word2Container.Opacity = 0;
                        lblWord2.Text = word2;
                        picWord2.Source = new BitmapImage(new Uri(wordImage2));

                        // Animation hiện từ vựng
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                        word2Container.BeginAnimation(OpacityProperty, fadeIn);

                        picLetter2.Visibility = Visibility.Hidden;
                    }
                }

                // Đọc từ vựng sau khi đã load xong
                if (!string.IsNullOrEmpty(wordToSpeak))
                {
                    await speechHelper.SpeakVietnamese(wordToSpeak); // Sửa ở đây, dùng wordToSpeak thay vì word2
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải từ vựng: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            speechHelper?.Dispose();
            base.OnClosed(e);
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            fadeOut.Completed += (s, _) => this.Close();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }
        private void btnNextPair_Click(object sender, RoutedEventArgs e)
        {
            // Đánh dấu cặp hiện tại đã hoàn thành
            if (egg1Cracked && egg2Cracked)
            {
                var progressWindow = Owner as ProgressWindow;
                if (progressWindow != null)
                {
                    progressWindow.UpdateCrackedState(currentIndex);
                }
            }

            if (currentIndex >= 24)
            {
                MessageBox.Show("Hoàn thành bảng chữ cái!", "Chúc mừng",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            currentIndex += 2;
            LoadEggs();
            UpdateProgress(); // Cập nhật tiến độ khi chuyển cặp mới
        }
    }
}