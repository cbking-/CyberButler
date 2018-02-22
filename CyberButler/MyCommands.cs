using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Configuration;
using System.Collections.Generic;

namespace CyberButler
{

    public class MyCommands
    {
        [Command("restaurant")]
        public async Task Restaurant(CommandContext ctx)
        {
            await ctx.RespondAsync($"Tony Paco's");
        }

        [Command("random")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            var random = new Random();
            await ctx.RespondAsync($" Your random number is: {random.Next(min, max)}");
        }
    }

    [Group("spotify", CanInvokeWithoutSubcommand = false)]
    public class SpotifyGroup
    {
        static AutorizationCodeAuth auth = null;
        private static SpotifyWebAPI _spotify = null;
        private static Token token = null;
        private static string SpotifyClientSecret = ConfigurationManager.AppSettings["SpotifyClientSecret"].ToString();
        private static string playlistID = ConfigurationManager.AppSettings["SpotifyPlaylistID"].ToString();
        private static string userID = "";

        public SpotifyGroup()
        {
            //Create the auth object
            auth = new AutorizationCodeAuth()
            {
                ClientId = ConfigurationManager.AppSettings["SpotifyClientID"].ToString(),
                RedirectUri = "http://localhost:8000",
                Scope = Scope.UserReadPlaybackState | Scope.PlaylistModifyPublic | Scope.UserModifyPlaybackState
            };

            //This will be called, if the user cancled/accept the auth-request
            auth.OnResponseReceivedEvent += Auth_OnResponseReceivedEvent;
            //a local HTTP Server will be started (Needed for the response)
            auth.StartHttpServer(8000);
            //This will open the spotify auth-page. The user can decline/accept the request
            auth.DoAuth();
        }

        private static void Auth_OnResponseReceivedEvent(AutorizationCodeAuthResponse response)
        {
            token = auth.ExchangeAuthCode(response.Code, SpotifyClientSecret);

            _spotify = new SpotifyWebAPI()
            {
                TokenType = token.TokenType,
                AccessToken = token.AccessToken
            };

            //Stop the HTTP Server, done.
            auth.StopHttpServer();

            var profile = _spotify.GetPrivateProfile();
            userID = profile.Id;

            //Refresh token as needed
            TokenRefresh();

            //Start playlist cleaner
            CleanPlaylist();
        }

        private static async Task TokenRefresh()
        {
            //While this async method isn't awaited, it still yields control due to the Task.Delay() call.
            //Method is based on https://blogs.msdn.microsoft.com/benwilli/2016/06/30/asynchronous-infinite-loops-instead-of-timers/

            while (true)
            {
                Token newToken = auth.RefreshToken(token.RefreshToken, SpotifyClientSecret);
                _spotify.AccessToken = newToken.AccessToken;
                await Task.Delay(newToken.ExpiresIn * 1000);
            }
        }

        private static async Task CleanPlaylist()
        {
            //While this async method isn't awaited, it still yields control due to the Task.Delay() call.
            //Method is based on https://blogs.msdn.microsoft.com/benwilli/2016/06/30/asynchronous-infinite-loops-instead-of-timers/

            if (_spotify == null)
                Console.WriteLine($"Spotify is not loaded.");
            else
            {
                PlaybackContext context = _spotify.GetPlayingTrack();
                var prevTrack = context.Item.Id;

                while (true)
                {
                    context = _spotify.GetPlayingTrack();

                    //Determine if Spotify has changed tracks in the playlist
                    if ((context.Item != null) && (prevTrack != context.Item.Id))
                    {
                        //remove previous track
                        ErrorResponse response = _spotify.RemovePlaylistTrack(userID, playlistID, new DeleteTrackUri($"spotify:track:{prevTrack}"));
                        if (!response.HasError())
                        {
                            prevTrack = context.Item.Id;
                        }
                        else
                        {
                            Console.WriteLine($"Could not remove track. {response.Error.Message}");
                        }
                    }

                    //Do this every 30 seconds
                    await Task.Delay(30000);
                }
            }
        }

        [Command("add")]
        public async Task Add(CommandContext ctx, string URI)
        {
            string message = "";

            if (_spotify == null)
            {
                message += $"Spotify is not loaded.";
            }
            else
            {
                //Search Spotify if the user did not provide a URI
                if (!URI.Contains("spotify:track"))
                {
                    //Encode spaces as +
                    SearchItem item = _spotify.SearchItems(URI.Replace(' ', '+'), SearchType.Track);

                    if (item.Tracks == null)
                    {
                        await ctx.RespondAsync($"Could not find: {URI}");
                        return;
                    }
                    else
                    {
                        //Use the first track found
                        URI = item.Tracks.Items[0].Uri;
                    }
                }

                //Get the playlist's tracks
                Paging<PlaylistTrack> playlist = _spotify.GetPlaylistTracks(userID, playlistID);

                //Check if the track being requested is already in the playlist. Spotify does not
                //  prevent duplicate tracks from being added to a playlist through their API.
                if ((playlist.Items == null) || (!playlist.Items.Any(track => $"spotify:track:{track.Track.Id}" == URI)))
                {
                    //Attempt to add the track to the playlist
                    ErrorResponse response = _spotify.AddPlaylistTrack(userID, playlistID, URI);

                    if (!response.HasError())
                    {
                        FullTrack trackInfo = _spotify.GetTrack(URI.Split(':')[2]);
                        message += $"\"{trackInfo.Name} - {trackInfo.Artists[0].Name}\" added. ";

                        //Determine if the current playing context is the playlist
                        PlaybackContext context = _spotify.GetPlayingTrack();

                        //If there is no track playing or the currently played track is not in the playlist, then the playing context needs to be switched.
                        if ((context.Item == null) || (!playlist.Items.Any(track => track.Track.Id == context.Item.Id)))
                        {
                            List<Device> devices = _spotify.GetDevices().Devices;

                            //In CyberButler's case, there should only be one Spotify device so choose the first device.
                            if (devices.Count > 0)
                            {
                                string deviceId = devices[0].Id;

                                string contextUri = $"spotify:user:{userID}:playlist:{playlistID}";

                                //Attempt to change the playback context
                                response = _spotify.ResumePlayback(deviceId: deviceId, contextUri: contextUri);

                                if (response.HasError())
                                {
                                    message += $"Could not start playback. {response.Error.Message}";
                                }
                            }
                            else
                            {
                                message += $"Could not start playback. No devices found.";
                            }
                        }
                    }
                    else
                    {
                        message += $"Could not add track. {response.Error.Message}";
                    }
                }
                else
                {
                    message += $"Duplicate track.";
                }
            }

            await ctx.RespondAsync(message);
        }

        [Command("next")]
        public async Task Next(CommandContext ctx)
        {
            if (_spotify == null)
            {
                await ctx.RespondAsync($"Spotify is not loaded.");
            }
            else
            {
                //Attempt to go to the next track.
                ErrorResponse response = _spotify.SkipPlaybackToNext();

                if (response.HasError())
                {
                    await ctx.RespondAsync($"Could go to next track. {response.Error.Message}");
                }
            }
        }
    }
}
