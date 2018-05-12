using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;

namespace CyberButler
{

    public class MyCommands
    {
        [Command("restaurant")]
        [Aliases("foodlibrary")]
        [Description("You meat bags need a man of steel to tell them where to eat, huh?")]
        public async Task Restaurant(CommandContext ctx)
        {
            var restaurants = new List<String>
            {
                "Tony Paco's",
                "Subway",
                "Arby's",
                "Wendy's",
                "Chipotle",
                "Bangcock Kitchen",
                "Steak Escape",
                "Buffalo Wild Wings",
                "Taco Bell",
                "Burger King",
                "Five Guys",
                "McDonald's",
                "Jimmy Johns",
                "Penn Station",
                "Tropical Smoothy Cafe",
                "Hot Head Burritos",
                "Chick-fil-A"
            };

            var random = new Random();

            await ctx.RespondAsync(restaurants[random.Next(restaurants.Count)]);
        }

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
            await ctx.RespondAsync($"Anime was a mistake and she love never you back. Also, you should probably wash your body pillows.");
        }

        [Command("eightball")]
        [Aliases("8ball")]
        [Description("Place important descisions in the hands of RNGesus")]
        public async Task EightBall(CommandContext ctx)
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

            await ctx.RespondAsync(responses[random.Next(responses.Count)]);
        }
    }

}
