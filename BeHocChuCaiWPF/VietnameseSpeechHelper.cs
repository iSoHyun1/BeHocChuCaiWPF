using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace BeHocChuCaiWPF
{
    public class VietnameseSpeechHelper : IDisposable
    {
        private readonly MediaPlayer mediaPlayer;
        private readonly HttpClient httpClient;
        private bool isDisposed;
        private TaskCompletionSource<bool>? playbackCompletion;

        private const string API_KEY = "api";
        private const string API_URL = "https://api.fpt.ai/hmi/tts/v5";

        public VietnameseSpeechHelper()
        {
            mediaPlayer = new MediaPlayer();
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("api-key", API_KEY);
            httpClient.DefaultRequestHeaders.Add("voice", "banmai");
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            playbackCompletion?.TrySetResult(true);
        }

        public async Task SpeakVietnamese(string text)
        {
            if (isDisposed) return;

            try
            {
                if (playbackCompletion != null)
                {
                    await playbackCompletion.Task;
                }

                playbackCompletion = new TaskCompletionSource<bool>();

                // Tạo request JSON
                var requestData = new { text = text.Trim() };
                var jsonString = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(API_URL, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API call failed: {response.StatusCode}\nResponse: {jsonResponse}");
                }

                var jsonObject = JObject.Parse(jsonResponse);
                var audioUrl = jsonObject["async"]?.ToString();

                if (string.IsNullOrEmpty(audioUrl))
                {
                    throw new Exception("Không nhận được URL audio");
                }

                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(2000);

                    var audioResponse = await httpClient.GetAsync(audioUrl);
                    if (audioResponse.IsSuccessStatusCode)
                    {
                        var tempFile = Path.Combine(Path.GetTempPath(), $"speech_{Guid.NewGuid()}.mp3");
                        await File.WriteAllBytesAsync(tempFile, await audioResponse.Content.ReadAsByteArrayAsync());

                        mediaPlayer.MediaEnded -= OnMediaEnded;
                        mediaPlayer.MediaEnded += OnMediaEnded;
                        mediaPlayer.Open(new Uri(tempFile));
                        mediaPlayer.Play();

                        void OnMediaEnded(object? s, EventArgs e)
                        {
                            try
                            {
                                File.Delete(tempFile);
                                mediaPlayer.MediaEnded -= OnMediaEnded;
                            }
                            catch { }
                        }

                        return;
                    }
                }

                throw new Exception("Không thể tải audio sau nhiều lần thử");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi phát âm: {ex.Message}");
                playbackCompletion?.TrySetResult(true);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                mediaPlayer?.Close();
                httpClient?.Dispose();
                isDisposed = true;
            }
        }
    }
}