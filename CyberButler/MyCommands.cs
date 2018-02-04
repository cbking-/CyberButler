using System;
using System.Collections.Generic;
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
        private static SpotifyWebAPI _spotify = null;
        private static string playlistID = "";

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

            playlistID = cfgjson.SpotifyPlaylistID;

            WebAPIFactory webApiFactory = new WebAPIFactory(
                   "http://localhost",
                   8000,
                   cfgjson.SpotifyClientID,
                   Scope.UserReadPlaybackState | Scope.PlaylistModifyPublic | Scope.UserModifyPlaybackState,
                   TimeSpan.FromSeconds(20)
              );

            try
            {
                _spotify = await webApiFactory.GetWebApi();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong. {ex.Message}");
            }
        }
        
        [Command("add")]
        public async Task Add(CommandContext ctx, string URI)
        {
            if (_spotify == null)
                return;
            else
            {
                var profile = _spotify.GetPrivateProfile();

                var userId = profile.Id;
                var playlistID = "";

                ErrorResponse response = _spotify.AddPlaylistTrack(userId, playlistID, URI);
                if (!response.HasError())
                    await ctx.RespondAsync($"Track added.");
                else
                    await ctx.RespondAsync($"Could not add track. {response.Error.Message}");

            }
        }
    }
}
