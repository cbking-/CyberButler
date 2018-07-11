using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CyberButler.Commands
{
    internal class RainbowGroup
    {
        [Group("rainbow", CanInvokeWithoutSubcommand = false)]
        internal class CustomCommand
        {
            private double seconds = Convert.ToDouble(Configuration.Config["RainbowInterval"]);

            [Command("toggle"),
                Description("Turn the rainbow group on or off")]
            public async Task Toggle(CommandContext _ctx)
            {
                //Could create a table and record for the state of the command but since this is the only one,
                // a file will do. If there is another togglable command in the future, we can create a table
                // for those toggles.

                var rainbowRole = _ctx.Guild.Roles.FirstOrDefault(
                    role => role.Name == Configuration.Config["RainbowRole"]);

                if (File.Exists("rainbow"))
                {
                    File.Delete("rainbow");
                    await _ctx.RespondAsync("Rainbow off.");
                }
                else
                {
                    File.Create("rainbow");
                    await _ctx.RespondAsync("Rainbow on.");

                    var colors = new List<DiscordColor>
                    {
                        DiscordColor.Red,
                        DiscordColor.Orange,
                        DiscordColor.Yellow,
                        DiscordColor.Green,
                        DiscordColor.Blue,
                        DiscordColor.Violet
                    };

                    var colorEnum = colors.GetEnumerator();

                    //it's too damn fast
                    //the system sometimes doesn't release the lock on the file after creation quick enough
                    //give it a second
                    Thread.Sleep(2000);

                    while (File.Exists("rainbow"))
                    {
                        if (colorEnum.MoveNext())
                        {
                            await _ctx.Guild.UpdateRoleAsync(rainbowRole, color: colorEnum.Current);
                            await Task.Delay((int)(seconds * 1000));
                        }
                        else
                        {
                            colorEnum = colors.GetEnumerator();
                        }
                    }
                }
            }

            [Command("party"),
               Description("You tarts like a good rainbow party. " +
                "Lasts one minute. Usable 5 times per 10 minutes per user"),
                Cooldown(5,600,CooldownBucketType.User)]
            public async Task Party(CommandContext _ctx)
            {
                seconds = 0.25;
                var rainbow = DiscordEmoji.FromName(_ctx.Client, ":rainbow:");
                var sparkles = DiscordEmoji.FromName(_ctx.Client, ":sparkles:");

                await _ctx.RespondAsync($"{rainbow}{sparkles}!! XD PARTY TIEM XD!! {sparkles}{rainbow}");
                await Task.Delay(10 * 1000);
                await _ctx.RespondAsync("<('-'<) ^(' - ')^(>'-')><('-'<) ^(' - ')^(>'-')>");
                await Task.Delay(10 * 1000);
                await _ctx.RespondAsync("<('-'<) ^(' - ')^(>'-')><('-'<) ^(' - ')^(>'-')>");
                await Task.Delay(10 * 1000);
                await _ctx.RespondAsync("<('-'<) ^(' - ')^(>'-')><('-'<) ^(' - ')^(>'-')>");
                await Task.Delay(10 * 1000);
                await _ctx.RespondAsync("<('-'<) ^(' - ')^(>'-')><('-'<) ^(' - ')^(>'-')>");
                await Task.Delay(10 * 1000);
                await _ctx.RespondAsync($"{rainbow}{sparkles}!! XD PARTY TIEM XD!! {sparkles}{rainbow}");
                await Task.Delay(10 * 1000);
                await _ctx.RespondAsync("party over :(");
                seconds = Convert.ToDouble(Configuration.Config["RainbowInterval"]);
            }
        }
    }
}