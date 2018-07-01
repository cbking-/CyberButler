using CyberButler.DatabaseRecords;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CyberButler.Commands
{
    [Group("restaurant", CanInvokeWithoutSubcommand = true),
        Aliases("foodlibrary")]
    public class Restaurant
    {
        [Description("I'm gonna tell you where to eat but you probably won't listen.")]
        public async Task ExecuteGroupAsync(CommandContext _ctx)
        {
            var server = _ctx.Guild.Id.ToString();
            var response = RestaurantResponse(server);

            if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
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
                                response = "the food trucks.";
                            }
                        }
                    }
                }
            }

            await _ctx.RespondAsync($"Go eat at {response}");
        }

        private String RestaurantResponse(String _server)
        {
            return new RestaurantRecord().SelectRandom(_server).Restaurant;
        }

        [Command("add"),
            Description("!restaurant add \"New Restaurant\". It does not check for duplicates.")]
        public async Task Add(CommandContext _ctx,
            [Description("Restaruant to add."), RemainingText]String _restaurant)
        {
            var server = _ctx.Guild.Id.ToString();
            var record = new RestaurantRecord
            {
                Server = server,
                Restaurant = _restaurant
            };

            record.Insert();

            await _ctx.RespondAsync($"Added \"{record.Restaurant}\".");
        }
    }
}