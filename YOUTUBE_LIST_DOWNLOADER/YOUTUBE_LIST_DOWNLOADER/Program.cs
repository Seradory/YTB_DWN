using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Services;
using System.IO;
using YoutubeExplode;
using MediaToolkit;
using MediaToolkit.Model;
using System.Linq;


namespace YOUTUBE_LIST_DOWNLOADER
{
    internal class Program
    {
        private static YouTubeService youtubeService;

        static async Task Main(string[] args)
        {
            InitializeYouTubeService();
            string playlistId = "PLHHq0UkPfqud2eVvGy0aDNkEH_aKWggYH";
            var videoUrls = await GetVideoUrlsFromPlaylist(playlistId);
            int i = 0;
            foreach (var url in videoUrls)
            {
                i++;
                await DownloadVideoAndConvertToMp3(url, @"C:\Users\alise\Desktop\video\"+i.ToString()+".mp3");
            }

        }

        private static void InitializeYouTubeService()
        {
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyD3OKXBgbISUTaGYpEa-TPzXj5BBKwxJd4", // API Anahtarınızı buraya girin
                ApplicationName = "YOUTUBE_LIST_DOWNLOADER"
            });
        }

        public static async Task<List<string>> GetVideoUrlsFromPlaylist(string playlistId)
        {
            var videoUrls = new List<string>();
            var nextPageToken = "";

            while (nextPageToken != null)
            {
                var playlistItemsRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");
                playlistItemsRequest.PlaylistId = playlistId;
                playlistItemsRequest.MaxResults = 50;
                playlistItemsRequest.PageToken = nextPageToken;

                var response = await playlistItemsRequest.ExecuteAsync();

                foreach (var item in response.Items)
                {
                    string videoId = item.Snippet.ResourceId.VideoId;
                    videoUrls.Add($"https://www.youtube.com/watch?v={videoId}");
                }

                nextPageToken = response.NextPageToken;
            }

            return videoUrls;
        }


        public static async Task DownloadVideoAndConvertToMp3(string videoUrl, string outputPath)
        {
            var youtube = new YoutubeClient();
            var videoId = YoutubeExplode.Videos.VideoId.Parse(videoUrl);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            // En yüksek kalitedeki muxed stream'i seç
            var streamInfo = streamManifest.GetMuxedStreams()
                                           .OrderByDescending(s => s.VideoQuality)
                                           .FirstOrDefault();


            if (streamInfo != null)
            {
                // Videoyu indir
                Console.WriteLine("Video indiriliyor....");
                string videoPath = Path.GetTempFileName();
                await youtube.Videos.Streams.DownloadAsync(streamInfo, videoPath);


                // Video dosyasından sesi çıkar ve MP3'e dönüştür
                Console.WriteLine("Video dosyası indirildi mp3e çevriliyor.");
                var inputFile = new MediaFile { Filename = videoPath };
                var outputFile = new MediaFile { Filename = outputPath };

                using (var engine = new Engine())
                {
                    engine.Convert(inputFile, outputFile);
                }

                // Geçici video dosyasını sil
                File.Delete(videoPath);
            }
            else
            {
                Console.WriteLine("Uygun video akışı bulunamadı.");
            }
        }
    }
}
