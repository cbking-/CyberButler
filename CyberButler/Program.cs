using CyberButler.Commands;
using CyberButler.DatabaseRecords;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace CyberButler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StartDiscord(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task StartDiscord(string[] args)
        {
            DiscordClient discord;
            CommandsNextModule commands;

            //Create the Discord client
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Configuration.Config["DiscordToken"],
                TokenType = TokenType.Bot
            });

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
                StringPrefix = Configuration.Config["CommandPrefix"],
                CaseSensitive = Boolean.Parse(Configuration.Config["CommandCaseSensitive"])
            });

            commands.CommandErrored += Commands_CommandErrored;
            commands.RegisterCommands<MyCommands>();
            commands.RegisterCommands<CustomCommand>();
            commands.RegisterCommands<UsernameHistory>();
            commands.RegisterCommands<RainbowGroup>();

            discord.MessageCreated += MessageCreated;
            discord.GuildMemberUpdated += DisplayNameChanged;
            discord.MessageReactionAdded += ReactionAdded;
            discord.MessageReactionRemoved += ReactionRemoved;
            await discord.ConnectAsync();
            await Task.Delay(-1);

        }

        private static async Task MessageCreated(MessageCreateEventArgs e)
        {
            var touchBase = new Regex(@"touch(ing|ed)? base(s)?");
            var ripAndTear = new Regex(@"rip and tear");
            var niche = new Regex(@"(n i c h e|niche)");

            var author = (DiscordMember)e.Author;
            var client = (DiscordClient)e.Client;

            if (touchBase.IsMatch(e.Message.Content.ToLower()))
            {
                await e.Message.RespondAsync(":right_facing_fist: :left_facing_fist: :right_facing_fist: :left_facing_fist:");
            }

            if (ripAndTear.IsMatch(e.Message.Content.ToLower()))
            {
                var giphyURL = $"https://api.giphy.com/v1/gifs/random?";
                giphyURL += $"api_key={Configuration.Config["GiphyAPIKey"]}";
                giphyURL += $"&limit=1";
                giphyURL += $"&tag=doomguy";

                var request = (HttpWebRequest)WebRequest.Create(giphyURL);

                using (HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = JsonConvert.DeserializeObject<dynamic>(await reader.ReadToEndAsync());
                            await e.Message.RespondAsync(result.data["url"].ToString());
                        }
                    }
                }
            }

            if (niche.IsMatch(e.Message.Content.ToLower()))
            {
                await e.Message.RespondAsync("【﻿ＮＩＣＨＥ】");
            }

            if (e.Message.ToString().ToLower().Contains("donger") && !author.IsBot)
            {
                await e.Message.RespondAsync($"ヽ༼ຈل͜ຈ༽ﾉ raise your dongers ヽ༼ຈل͜ຈ༽ﾉ");
            }

            //if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
            //{
            //    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":wednesday:"));
            //}

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
                    Server = e.Guild.Id.ToString(),
                    UserID = e.Member.Id.ToString(),
                    NameBefore = e.NicknameBefore,
                    NameAfter = e.NicknameAfter,
                    InsertDateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                };

                record.Insert();
            }

            await Task.CompletedTask;
        }

        private static async Task ReactionAdded(MessageReactionAddEventArgs e)
        {
            var theWayReact = e.Message.Reactions.FirstOrDefault(react => react.Emoji.Name == "theway");
            var notTheWayReact = e.Message.Reactions.FirstOrDefault(react => react.Emoji.Name == "nottheway");

            if (theWayReact != null && theWayReact.Count > 3)
            {
                var random = new Random();
                var theWay = DiscordEmoji.FromName((DiscordClient)e.Client, ":theway:");
                var click = DiscordEmoji.FromName((DiscordClient)e.Client, ":click:");

                string[] responses = {$"{theWay}{theWay}{theWay} THIS IS THE WAY {theWay}{theWay}{theWay}",
                                 $"{click}{click}{click}{click}{click}{click}{click}{click}{click}" };

                for (var i = 0; i < 30; i++)
                {
                    await e.Message.RespondAsync(responses[random.Next(2)]);
                    await Task.Delay(1000);
                }
            }

            if (notTheWayReact != null && notTheWayReact.Count > 3)
            {
                var random = new Random();
                var notTheWay = DiscordEmoji.FromName((DiscordClient)e.Client, ":nottheway:");
                var spit = DiscordEmoji.FromName((DiscordClient)e.Client, ":sweat_drops:");

                string[] responses = {$"{notTheWay}{notTheWay}{notTheWay} THIS IS NOT THE WAY " +
                                        $"{notTheWay}{notTheWay}{notTheWay}",
                                      $"{notTheWay}{notTheWay}{notTheWay} SPIT ON THE FALSE QUEEN" +
                                        $"{notTheWay}{notTheWay}{notTheWay}",
                                      $"{notTheWay}{spit}{notTheWay}{spit}{notTheWay}{spit}{notTheWay}{spit}" +
                                      $"{notTheWay}{spit}{notTheWay}{spit}{notTheWay}{spit}{notTheWay}{spit}" +
                                      $"{notTheWay}{spit}{notTheWay}{spit}{notTheWay}{spit}{notTheWay}{spit}"};

                for (var i = 0; i < 30; i++)
                {
                    await e.Message.RespondAsync(responses[random.Next(3)]);
                    await Task.Delay(1000);
                }
            }

            await Task.CompletedTask;
        }

        private static async Task ReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            await Task.CompletedTask;
        }

        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException)
            {
                var command = e.Context.Message.Content.Substring(1);
                var server = e.Context.Guild.Id.ToString();

                if (!Boolean.Parse(Configuration.Config["CommandCaseSensitive"]))
                {
                    command = command.ToLower();
                }

                var record = new CommandRecord().SelectOne(server, command);

                if (record != null)
                {
                    await e.Context.RespondAsync(record.Text);
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