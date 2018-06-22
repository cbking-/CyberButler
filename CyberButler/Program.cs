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
        static async Task Main()
        {
            DiscordClient discord;
            CommandsNextModule commands;

            //Since this is running from Ubuntu Server using Mono, this line is a SSL certificate validation override.
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

            //Create the Discord client
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings["DiscordToken"],
                TokenType = TokenType.Bot
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
                StringPrefix = ConfigurationManager.AppSettings["CommandPrefix"]
            });

            commands.CommandErrored += Commands_CommandErrored;
            commands.RegisterCommands<MyCommands>();
            commands.RegisterCommands<Restaurant>();
            commands.RegisterCommands<CustomCommand>();

            discord.MessageCreated += MessageCreated;
            discord.GuildMemberUpdated += DisplayNameChanged;
            discord.MessageReactionAdded += ReactionAdded;
            discord.MessageReactionRemoved += ReactionRemoved;

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        static async Task MessageCreated(MessageCreateEventArgs e)
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

        static async Task DisplayNameChanged(GuildMemberUpdateEventArgs e)
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

        static async Task ReactionAdded(MessageReactionAddEventArgs e)
        {
            await Task.CompletedTask;
        }

        static async Task ReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            await Task.CompletedTask;
        }

        static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException)
            {
                var text = new CommandRecord().SelectOne(e.Context.Guild.Name, e.Context.Message.Content.Substring(1));

                if (text != "")
                {
                    await e.Context.RespondAsync(text);
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"{e.Context.Message.Content.Split(' ')[0].Substring(1)} not found.",
                        Color = DiscordColor.Red
                    };

                    await e.Context.RespondAsync("", embed: embed);
                }
            }  
            else if (e.Exception is ChecksFailedException ex)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = DiscordColor.Red
                };

                await e.Context.RespondAsync("", embed: embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = $"{e.Exception.Message}",
                    Color = DiscordColor.Red
                };

                await e.Context.RespondAsync("", embed: embed);
            }
        }
    }
}
