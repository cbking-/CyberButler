using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Web;

namespace CyberButler.Commands
{
    public class MyCommands
    {
        [Command("random"),
            Description("Generate a random number between the provided min and max.")]
        public async Task Random(CommandContext ctx, 
            [Description("Minimum number")]int min,
            [Description("Maxiumum number")]int max)
        {
            var random = new Random();
            await ctx.RespondAsync($" Your random number is: {random.Next(min, max)}");
        }

        [Command("sweepstakes"),
            Description("You'll never actually win anything but you'll keep entering. Predictable.")]
        public async Task Sweepstakes(CommandContext ctx)
        {
            await ctx.RespondAsync($"You have been entered to win.");
        }

        [Command("ratemywaifu"),
            Description("Find out how crappy your waifu is.")]
        public async Task RateMyWaifu(CommandContext ctx)
        {
            var response = $"Anime was a mistake and she will never love you back. ";
            response += "Also, you should probably wash your body pillows.";
            await ctx.RespondAsync(response);
        }

        [Command("eightball")]
        [Aliases("8ball")]
        [Description("Place important decisions in the hands of RNGesus")]
        public async Task EightBall(CommandContext ctx, 
            [Description("Question you want CyberButler to answer."), RemainingText]String _question)
        {
            var responses = new List<String>
            {
                "It is certain",
                "It is decidedly so",
                "Without a doubt",
                "Yes definitely",
                "You may rely on it",
                "As I see it, yes",
                "Most likely",
                "Outlook good",
                "Yes",
                "Signs point to yes",
                "Reply hazy try again",
                "Ask again later",
                "Better not tell you now",
                "Cannot predict now",
                "Concentrate and ask again",
                "Don't count on it",
                "My reply is no",
                "My sources say no",
                "Outlook not so good",
                "Very doubtful"
            };

            var random = new Random();

            var embed = new DiscordEmbedBuilder();
            var url = @"https://emojipedia-us.s3.amazonaws.com/thumbs/120/microsoft/135/billiards_1f3b1.png";
            embed.WithThumbnailUrl(url);
            embed.AddField("Question:", _question);
            embed.AddField("CyberButler Says:", responses[random.Next(responses.Count)]);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("source")]
        [Description("Source code remote repository.")]
        public async Task Source(CommandContext ctx)
        {
            await ctx.RespondAsync($"https://github.com/cbking-/CyberButler");
        }

        [Command("timetillwednesday")]
        [Aliases("ttw")]
        [Description("Time till the best day of the week.")]
        public async Task TimeTillWednesday(CommandContext _ctx)
        {
            var response = "";
            var today = DateTime.Now;
            DateTime nextWednesday = GetNextWeekday(DateTime.Today, DayOfWeek.Wednesday);
            TimeSpan diff = nextWednesday - today;

            if (diff.Ticks < 0)
            {
                var emoji = DiscordEmoji.FromName(_ctx.Client, ":wednesday:");
                response = $"{emoji} It is Wednesday, my dudes. {emoji}";
            }
            else
            {
                response = $"{diff.Days} Days, {diff.Hours} Hours, {diff.Minutes} Minutes, {diff.Seconds} Seconds";
            }

            await _ctx.RespondAsync(response);
        }

        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        [Command("setgame"),
            RequireOwner,
            Description("Set the bot's game"),
            Hidden]
        public async Task SetStatus(CommandContext ctx,
            [Description("Game to be set."), RemainingText]String _game)
        {
            await ctx.Client.UpdateStatusAsync(
                new DiscordGame
                {
                    Name = _game
                }
            );
        }

        [Command("restaurant")]
        [Aliases("foodlibrary")]
        [Description("I'm gonna tell you where to eat but you probably won't listen.")]
        public async Task Restaurant(CommandContext _ctx, [RemainingText]string location)
        {
            var server = _ctx.Guild.Id.ToString();
            var embed = RestaurantResponse(server, _ctx, location).Result;

            if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday && DateTime.Now.Month >= 6 && DateTime.Now.Month <= 9)
            {
                var openWeatherURL = $"https://api.openweathermap.org/data/2.5/weather?zip=45840&appid=";
                openWeatherURL += Configuration.Config["OpenWeatherMapKey"];

                var request = (HttpWebRequest)WebRequest.Create(openWeatherURL);

                using (HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = JsonConvert.DeserializeObject<dynamic>(await reader.ReadToEndAsync());
                            var temp = result["main"]["temp"] * (9 / 5) - 459.67;
                            var rain = result["weather"][0]["description"].ToString().Contains("rain");

                            if (temp < 80 && !rain)
                            {
                                embed = new DiscordEmbedBuilder
                                {
                                    Title = "Food Trucks",
                                    Color = DiscordColor.Blurple,
                                    Description = "Go eat at the food trucks."
                                };
                            }
                        }
                    }
                }
            }

            await _ctx.RespondAsync(embed: embed);
        }

        private async Task<DiscordEmbedBuilder> RestaurantResponse(string _server, CommandContext _ctx, string location)
        {
            var pick = 0;
            var embed = new DiscordEmbedBuilder();

            var url = Configuration.Config["YelpAPIRoot"] + "/businesses/search";
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["location"] = location;
            query["categories"] = "restaurants";
            uriBuilder.Query = query.ToString();
            url = uriBuilder.ToString();

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers["Authorization"] = "Bearer " + Configuration.Config["YelpAPIKey"];

            using (HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (var stream = webResponse.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var result = JsonConvert.DeserializeObject<dynamic>(await reader.ReadToEndAsync());
                        pick = new Random().Next((int)result["total"]);

                        if (pick < result["businesses"].Count)
                        {
                            embed = BuildEmbed(result["businesses"][pick]);
                        }
                    }
                }
            }

            if (embed.Title == null)
            {
                query["limit"] = "1";
                query["offset"] = pick.ToString();
                uriBuilder.Query = query.ToString();
                url = uriBuilder.ToString();
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers["Authorization"] = "Bearer " + Configuration.Config["YelpAPIKey"];

                using (HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = JsonConvert.DeserializeObject<dynamic>(await reader.ReadToEndAsync());

                            embed = BuildEmbed(result["businesses"][0]);
                        }
                    }
                }
            }

            return embed;
        }

        private DiscordEmbedBuilder BuildEmbed(dynamic _business)
        {
            var categories = new List<string>();
            var rating = (string)_business["rating"];
            var reviews = (string)_business["review_count"];
            var price = (string)_business["price"] ?? "N/A";
            var address = String.Join("\n",_business["location"]["display_address"].ToObject<string[]>());
            var mapUrl = $"https://www.google.com/maps/search/?api=1&query={WebUtility.UrlEncode(address)}";

            foreach (var category in _business["categories"])
            {
                categories.Add((string)category["title"]);
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = (string)_business["name"],
                Color = DiscordColor.Blurple,
                Url = (string)_business["url"],
                Description = String.Join(", ", categories),
                ThumbnailUrl = "http://icons.iconarchive.com/icons/webalys/kameleon.pics/512/Food-Dome-icon.png",
            };

            embed.AddField("Rating (Reviews)", $"{rating} ({reviews})", true);
            embed.AddField("Price", price, true);
            embed.AddField("Address", $"[{address}]({mapUrl})");

            return embed;
        }
    }
}