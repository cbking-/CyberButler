using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;

namespace CyberButler
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult(); 
        }

        static async Task MainAsync(string[] args)
        {
            //Since this is running from Ubuntu Server using Mono, this line is a SSL certificate validation override.
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

            //Load configuration
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            {
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    json = await sr.ReadToEndAsync();
                }
            }
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            //Create the Discord client
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfgjson.DiscordToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            //Since this is running from Ubuntu Server using Mono, a different web socket is needed.
            discord.SetWebSocketClient<WebSocketSharpClient>();

            //CyberButler will emote some business terms. He only likes touching bases at the moment.
            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().Contains("touch base"))
                    await e.Message.RespondAsync(":right_facing_fist: :left_facing_fist: :right_facing_fist: :left_facing_fist:");
            };

            //Create the commands configuration suing the prefix defined in the config file
            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = cfgjson.CommandPrefix
            });

            commands.CommandErrored += Commands_CommandErrored;
            commands.RegisterCommands<MyCommands>();
            commands.RegisterCommands<SpotifyGroup>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "CyberButler", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }
    }
}
