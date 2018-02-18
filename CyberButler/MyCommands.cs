using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using SpotifyAPI.Web; 
using SpotifyAPI.Web.Auth; 
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

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
        static AutorizationCodeAuth auth;
        private static SpotifyWebAPI _spotify = null;
        private static Token token;
        private static string SpotifyClientSecret;
        private static string playlistID = "";
        private static string userID = "";

        public SpotifyGroup()
        {
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            SpotifyClientSecret = cfgjson.SpotifyClientSecret;

            playlistID = cfgjson.SpotifyPlaylistID;

            //Create the auth object
            auth = new AutorizationCodeAuth()
            {
                ClientId = cfgjson.SpotifyClientID,
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

            //With the token object, you can now make API calls

            var profile = _spotify.GetPrivateProfile();
            userID = profile.Id;

            //Stop the HTTP Server, done.
            auth.StopHttpServer();

            //Start playlist cleaner
            CleanPlaylist();
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
                    //refresh authorization token
                    auth.RefreshToken(token.RefreshToken, SpotifyClientSecret);

                    context = _spotify.GetPlayingTrack();

                    //Determine if Spotify has changed tracks in the playlist
                    if ((context.Item != null) && (prevTrack != context.Item.Id))
                    {
                        //remove previous track
                        ErrorResponse response = _spotify.RemovePlaylistTrack(userID, playlistID, new DeleteTrackUri($"spotify:track:{prevTrack}"));
                        if (!response.HasError())
                            prevTrack = context.Item.Id;
                        else
                            Console.WriteLine($"Could not remove track. {response.Error.Message}");
                        
                    }

                    //Do this every 30 seconds
                    await Task.Delay(30000);
                }
            }
        }

        [Command("add")]
        public async Task Add(CommandContext ctx, string URI)
        {
            if (_spotify == null)
                await ctx.RespondAsync($"Spotify is not loaded.");
            else
            {
                //refresh authorization token
                auth.RefreshToken(token.RefreshToken, SpotifyClientSecret);

                //Search Spotify if the user did not provide a URI
                if (!URI.Contains("spotify:track"))
                {
                    //Encode spaces as +
                    SearchItem item = _spotify.SearchItems(URI.Replace(' ', '+'), SearchType.Track);

                    //Use the first track found
                    URI = item.Tracks.Items[0].Uri;
                }

                //Get the playlist's tracks
                Paging<PlaylistTrack> playlist = _spotify.GetPlaylistTracks(userID, playlistID);

                //Check if the track being requested is already in the playlist. Spotify does not
                //  prevent duplicate tracks from being added to a playlist through their API.
                if (!playlist.Items.Any(track => $"spotify:track:{track.Track.Id}" == URI))
                {
                    //Attempt to add the track to the playlist
                    ErrorResponse response = _spotify.AddPlaylistTrack(userID, playlistID, URI);

                    if (!response.HasError())
                    {
                        await ctx.RespondAsync($"Track added.");

                        //Determine if the current playing context is the playlist
                        PlaybackContext context = _spotify.GetPlayingTrack();

                        //If the currently played track is not in the playlist, then the playing context needs to be switched.
                        if (!playlist.Items.Any(track => track.Track.Id == context.Item.Id))
                        {
                            //In CyberButler's case, there should only be one Spotify device so choose the first device.
                            string deviceId = _spotify.GetDevices().Devices[0].Id;

                            string contextUri = $"spotify:user:{userID}:playlist:{playlistID}";

                            //Attempt to change the playback context
                            response = _spotify.ResumePlayback(deviceId: deviceId, contextUri: contextUri);

                            if (response.HasError())
                                await ctx.RespondAsync($"Could not start playback. {response.Error.Message}");
                        }
                    }
                    else
                        await ctx.RespondAsync($"Could not add track. {response.Error.Message}");
                }
                else
                {
                    await ctx.RespondAsync($"Duplicate track.");
                } 
            }
        }

        [Command("next")]
        public async Task Next(CommandContext ctx)
        {
            if (_spotify == null)
                await ctx.RespondAsync($"Spotify is not loaded.");
            else
            {
                //refresh authorization token
                auth.RefreshToken(token.RefreshToken, SpotifyClientSecret);

                //Attempt to go to the next track.
                ErrorResponse response = _spotify.SkipPlaybackToNext();

                if (response.HasError())
                    await ctx.RespondAsync($"Could go to next track. {response.Error.Message}");
            }
        }
    }
}
