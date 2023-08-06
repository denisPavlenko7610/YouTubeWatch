using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace YouTubeRealtimeStats
{
    public partial class YouTubeWatch : Window
    {
        private YouTubeService youtubeService;
        private string channelId = "UCP_0FkzSCl-7V0Jv6ZaDszQ";

        public YouTubeWatch()
        {
            InitializeComponent();
            Left = 1828;
            Top = -20;

            UserCredential credential;
            using (var stream = new System.IO.FileStream("secret.json",
                System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeReadonly },
                    "user",
                    System.Threading.CancellationToken.None).Result;
            }

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YouTube Realtime Stats",
            });
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateStats();

            var timer = new System.Timers.Timer();
            timer.Elapsed += async (s, args) => await UpdateStats();
            timer.Interval = 300000;  // 5 min
            timer.Start();
        }

        private async Task UpdateStats()
        {
            try
            {
                var channelsListRequest = youtubeService.Channels.List("statistics");
                channelsListRequest.Id = channelId;
                var channelsListResponse = await channelsListRequest.ExecuteAsync();

                Dispatcher.Invoke(() =>
                {
                    lblSubscriberCount.Content = $"Subscribers: { channelsListResponse.Items[0].Statistics.SubscriberCount:N0}";
                    lblViewCount.Content = $"Views: {channelsListResponse.Items[0].Statistics.ViewCount:N0}";
                    lblVideosCount.Content = $"Videos: {channelsListResponse.Items[0].Statistics.VideoCount:N0}";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
