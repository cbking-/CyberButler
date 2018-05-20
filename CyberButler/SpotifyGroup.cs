using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CyberButler
{
    [Group("spotify", CanInvokeWithoutSubcommand = false)]
    public class SpotifyGroup
    {
        private static SpotifyWebAPI _spotify = null;

        public SpotifyGroup()
        {
            string SpotifyClientSecret = ConfigurationManager.AppSettings["SpotifyClientSecret"].ToString();
            string SpotifyClientId = ConfigurationManager.AppSettings["SpotifyClientID"].ToString();
            string SpotifyPlaylistId = ConfigurationManager.AppSettings["SpotifyPlaylistID"].ToString();

            //Create the auth object
            AutorizationCodeAuth auth = new AutorizationCodeAuth()
            {
                ClientId = SpotifyClientId,
                RedirectUri = "http://localhost:8000",
                Scope = Scope.UserReadPlaybackState | Scope.PlaylistModifyPublic | Scope.UserModifyPlaybackState
            };

            //This will be called, if the user cancled/accept the auth-request
            auth.OnResponseReceivedEvent += async (AutorizationCodeAuthResponse response) =>
            {

                Token token = auth.ExchangeAuthCode(response.Code, SpotifyClientSecret);

                _spotify = new SpotifyWebAPI()
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken
                };

                //Stop the HTTP Server, done.
                auth.StopHttpServer();

                //Refresh token as needed
                await TokenRefreshAsync(auth, token.RefreshToken, SpotifyClientSecret);

                //Start playlist cleaner
                await CleanPlaylistAsync(SpotifyPlaylistId);
            };

            //a local HTTP Server will be started (Needed for the response)
            auth.StartHttpServer(8000);
            //This will open the spotify auth-page. The user can decline/accept the request
            auth.DoAuth();
        }

        private static async Task TokenRefreshAsync(AutorizationCodeAuth auth, string RefreshToken, string SpotifyClientSecret)
        {
            //While this async method isn't awaited, it still yields control due to the Task.Delay() call.
            //Method is based on https://blogs.msdn.microsoft.com/benwilli/2016/06/30/asynchronous-infinite-loops-instead-of-timers/

            while (true)
            {
                Token newToken = auth.RefreshToken(RefreshToken, SpotifyClientSecret);
                _spotify.AccessToken = newToken.AccessToken;
                await Task.Delay(newToken.ExpiresIn * 1000);
            }
        }

        private static async Task CleanPlaylistAsync(string playlistID)
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
                        string userID = _spotify.GetPrivateProfile().Id;
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
            if (_spotify == null)
            {
                await ctx.RespondAsync($"Spotify is not loaded.");
            }
            else
            {
                try
                {
                    if (!URI.Contains("spotify:track"))
                    {
                        URI = FindTrack(URI);
                    }

                    string playlistID = ConfigurationManager.AppSettings["SpotifyPlaylistID"].ToString();
                    string userID = _spotify.GetPrivateProfile().Id;

                    string trackAdded = AddTrackToPlaylist(userID, playlistID, URI);
                    await ctx.RespondAsync(trackAdded);
                    SwitchPlayingContext(userID, playlistID);
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync(ex.Message);
                }

            }
        }

        private string FindTrack(string query)
        {
            SearchItem item = _spotify.SearchItems(query.Replace(' ', '+'), SearchType.Track);

            if (item.Tracks == null)
            {
                throw new Exception($"Could not find: {query}");
            }
            else
            {
                return item.Tracks.Items[0].Uri;
            }
        }

        private string AddTrackToPlaylist(string userID, string playlistID, string URI)
        {

            Paging<PlaylistTrack> playlist = _spotify.GetPlaylistTracks(userID, playlistID);

            //Check if the track being requested is already in the playlist. Spotify does not
            //  prevent duplicate tracks from being added to a playlist through their API.
            if ((playlist.Items == null) || (!playlist.Items.Any(track => $"spotify:track:{track.Track.Id}" == URI)))
            {
                ErrorResponse response = _spotify.AddPlaylistTrack(userID, playlistID, URI);

                if (!response.HasError())
                {
                    FullTrack trackInfo = _spotify.GetTrack(URI.Split(':')[2]);
                    return $"\"{trackInfo.Name} - {trackInfo.Artists[0].Name}\" added.";
                }
                else
                {
                    throw new Exception($"Could not add track. {response.Error.Message}");
                }
            }
            else
            {
                throw new Exception($"Duplicate track.");
            }
        }

        private void SwitchPlayingContext(string userID, string playlistID)
        {
            Paging<PlaylistTrack> playlist = _spotify.GetPlaylistTracks(userID, playlistID);
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
                    ErrorResponse response = _spotify.ResumePlayback(deviceId: deviceId, contextUri: contextUri);

                    if (response.HasError())
                    {
                        throw new Exception($"Could not start playback. {response.Error.Message}");
                    }
                }
                else
                {
                    throw new Exception($"Could not start playback. No devices found.");
                }
            }
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
                var interactivity = ctx.Client.GetInteractivityModule();

                DiscordEmoji thumbsup = DiscordEmoji.FromName(ctx.Client, ":thumbsup:");
                DiscordEmoji thumbsdown = DiscordEmoji.FromName(ctx.Client, ":thumbsdown:");
                DiscordEmoji[] options = { thumbsup, thumbsdown };

                var poll_options = options.Select(xe => xe.ToString());

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Skip song?"
                };

                var msg = await ctx.RespondAsync(embed: embed);

                for (var i = 0; i < options.Length; i++)
                {
                    await msg.CreateReactionAsync(options[i]);
                    Thread.Sleep(500);
                }

                var poll_result = await interactivity.CollectReactionsAsync(msg, new TimeSpan(0, 0, 5));
                var results = poll_result.Reactions.Where(xkvp => options.Contains(xkvp.Key))
                                .Max(xkvp => $"{xkvp.Key}");

                if (results == thumbsup)
                {
                    ErrorResponse response = _spotify.SkipPlaybackToNext();

                    if (response.HasError())
                    {
                        await ctx.RespondAsync($"Could not go to next track. {response.Error.Message}");
                    }
                    else
                    {
                        await ctx.RespondAsync($"The :thumbsup: have it. Pick a better song next time, will ya?");
                    }
                }
                else
                {
                    await ctx.RespondAsync($"The :thumbsdown: have it. This is a good song.");
                }

            }
        }
    }
}
