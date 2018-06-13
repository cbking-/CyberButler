using CyberButler.DatabaseRecords;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Threading.Tasks;

namespace CyberButler.Commands
{
    [Group("restaurant", CanInvokeWithoutSubcommand = true)]
    [Aliases("foodlibrary")]
    public class Restaurant
    {
        [Description("I'm gonna tell you where to eat but you probably won't listen.")]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string response = "";

            if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
            {
                await ctx.RespondAsync("Is it nice out? (Yes/No)");
                var interactivity = ctx.Client.GetInteractivityModule();
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(1));

                if (msg.Message.Content.ToLower() == "yes")
                {
                    response = "Food Trucks";
                }
                else
                {
                    response = RestaurantResponse(ctx.Guild.Name);
                }
            }
            else
            {
                response = RestaurantResponse(ctx.Guild.Name);
            }

            await ctx.RespondAsync($"Go eat at {response}");
        }

        private String RestaurantResponse(String _server)
        {
            return new RestaurantRecord().SelectRandom(_server);
        }

        [Command("add")]
        [Description("!restaurant add \"New Restaurant\". It does not check for duplicates.")]
        public async Task Add(CommandContext ctx, params String[] _restaurant)
        {
            var record = new RestaurantRecord
            {
                Server = ctx.Guild.Name,
                Restaurant = String.Join(" ", _restaurant)
            };

            record.Insert();

            await ctx.RespondAsync($"Added \"{record.Restaurant}\" to {record.Server}.");
        }
    }
}
