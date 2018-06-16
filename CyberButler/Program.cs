using System;
using System.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CyberButler.Commands;
using CyberButler.DatabaseRecords;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;

namespace CyberButler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DiscordClient discord;
            CommandsNextModule commands;

            //Since this is running from Ubuntu Server using Mono, this line is a SSL certificate validation override.
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

            //Create the Discord client
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings["DiscordToken"].ToString(),
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            //Since this is running from Ubuntu Server using Mono, a different web socket is needed.
            discord.SetWebSocketClient<WebSocketSharpClient>();

            discord.UseInteractivity(new InteractivityConfiguration
            {
                // default pagination behaviour to just ignore the reactions
                PaginationBehaviour = TimeoutBehaviour.Ignore,

                // default pagination timeout to 5 minutes
                PaginationTimeout = TimeSpan.FromMinutes(5),

                // default timeout for other actions to 2 minutes
                Timeout = TimeSpan.FromMinutes(2)
            });

            //Create the commands configuration using the prefix defined in the config file
            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ConfigurationManager.AppSettings["CommandPrefix"].ToString()
            });

            commands.CommandErrored += Commands_CommandErrored;
            commands.RegisterCommands<MyCommands>();
            commands.RegisterCommands<Restaurant>();

            discord.MessageCreated += MessageCreated;
            discord.GuildMemberUpdated += DisplayNameChanged;
            discord.MessageReactionAdded += ReactionAdded;
            discord.MessageReactionRemoved += ReactionRemoved;

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task MessageCreated(MessageCreateEventArgs e)
        {
            var rgx = new Regex(@"touch(ing|ed)? base(s)?");
            var author = (DiscordMember)e.Author;
            var client = (DiscordClient)e.Client;

            if (rgx.IsMatch(e.Message.Content.ToLower()))
            {
                await e.Message.RespondAsync(":right_facing_fist: :left_facing_fist: :right_facing_fist: :left_facing_fist:");
            }

            if (e.Message.ToString().ToLower().Contains("donger") && !author.IsBot)
            {
                await e.Message.RespondAsync($"ヽ༼ຈل͜ຈ༽ﾉ raise your dongers ヽ༼ຈل͜ຈ༽ﾉ");
            }

            if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
            {
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":wednesday:"));
            }

            if (author.Nickname.ToLower().Contains("goat"))
            {
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":goat:"));
            }
        }

        private static async Task DisplayNameChanged(GuildMemberUpdateEventArgs e)
        {
            if (e.NicknameAfter != e.NicknameBefore)
            {
                var record = new UsernameHistoryRecord
                {
                    Server = e.Guild.Name,
                    UserID = e.Member.Username + '#' + e.Member.Discriminator,
                    NameBefore = e.NicknameBefore,
                    NameAfter = e.NicknameAfter
                };

                record.Insert();
            }

            await Task.CompletedTask;
        }

        private static async Task ReactionAdded(MessageReactionAddEventArgs e)
        {
            await Task.CompletedTask;
        }

        private static async Task ReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            await Task.CompletedTask;
        }
        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.DebugLogger.LogMessage(
                LogLevel.Error, 
                "CyberButler", 
                $"{e.Context.User.Username} tried executing '" +
                $"{e.Command?.QualifiedName ?? "<unknown command>"}' " +
                $"but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}." +
                $"Stack trace:{e.Exception.StackTrace}"
                , DateTime.Now);

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

        public async void ScheduleAction(Action action, DateTime ExecutionTime)
        {
            await Task.Delay((int)ExecutionTime.Subtract(DateTime.Now).TotalMilliseconds);
            action();
        }
    }
}
