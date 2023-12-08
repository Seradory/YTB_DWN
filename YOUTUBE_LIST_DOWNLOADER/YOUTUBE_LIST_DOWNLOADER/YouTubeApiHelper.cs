using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Google.Apis.YouTube.v3.Data;


public class YouTubeApiHelper
{
    private YouTubeService youtubeService;

    public YouTubeApiHelper(string apiKey)
    {
        youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = this.GetType().ToString()
        });
    }

    public async Task<List<Playlist>> GetPlaylistsForChannel(string channelId)
    {
        var playlists = new List<Playlist>();
        var nextPageToken = "";

        while (nextPageToken != null)
        {
            var playlistRequest = youtubeService.Playlists.List("snippet,contentDetails");
            playlistRequest.ChannelId = channelId;
            playlistRequest.MaxResults = 50;
            playlistRequest.PageToken = nextPageToken;

            var response = await playlistRequest.ExecuteAsync();

            playlists.AddRange(response.Items);
            nextPageToken = response.NextPageToken;
        }

        return playlists;
    }
}

