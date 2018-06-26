using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;

namespace CyberButler.Commands
{
    public class MyCommands
    {
        [Command("random")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            var random = new Random();
            await ctx.RespondAsync($" Your random number is: {random.Next(min, max)}");
        }

        [Command("sweepstakes")]
        [Description("You'll never actually win anything but you'll keep entering. Predictable.")]
        public async Task Sweepstakes(CommandContext ctx)
        {
            await ctx.RespondAsync($"You have been entered to win.");
        }

        [Command("ratemywaifu")]
        [Description("Find out how crappy your waifu is.")]
        public async Task RateMyWaifu(CommandContext ctx)
        {
            await ctx.RespondAsync($"Anime was a mistake and she will never love you back. Also, you should probably wash your body pillows.");
        }

        [Command("eightball")]
        [Aliases("8ball")]
        [Description("Place important decisions in the hands of RNGesus")]
        public async Task EightBall(CommandContext ctx, [RemainingText]String _question)
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
            embed.WithThumbnailUrl(@"https://upload.wikimedia.org/wikipedia/commons/thumb/e/e3/8_ball_icon.svg/240px-8_ball_icon.svg.png");
            embed.AddField("Question:", _question);
            embed.AddField("CyberButler Says:", responses[random.Next(responses.Count)]);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("source")]
        [Description("Source code remote repository.")]
        public async Task Source(CommandContext ctx)
        {
            await ctx.RespondAsync($"https://gitlab.com/corbinking/CyberButler");
        }

        [Command("timetillwednesday")]
        [Aliases("ttw")]
        [Description("Time till the best day of the week.")]
        public async Task TimeTillWednesday(CommandContext _ctx)
        {
            var today = DateTime.Now;
            DateTime nextWednesday = GetNextWeekday(DateTime.Today, DayOfWeek.Wednesday);
            TimeSpan diff = nextWednesday - today;

            await _ctx.RespondAsync($"{diff.Days} Days, {diff.Hours} Hours, {diff.Minutes} Minutes, {diff.Seconds} Seconds");
        }

        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        [Command("setgame")]
        [RequireOwner]
        [Description("Set the bot's game")]
        [Hidden]
        public async Task SetStatus(CommandContext ctx, [RemainingText]String _game)
        {
            await ctx.Client.UpdateStatusAsync(
                new DiscordGame
                {
                    Name = _game
                }
            );
        }
    }
}
