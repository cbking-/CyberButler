using CyberButler.DatabaseRecords;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace CyberButler.Commands
{
    [Group("usernamehistory", CanInvokeWithoutSubcommand = false),
        Aliases("uh")]
    internal class UsernameHistory
    {
        [Command("get"),
            Aliases("list", "read"),
            Description("Example: !usernamehistory get @UserName")]
        public async Task Get(CommandContext _ctx, 
            [Description("@ the user you want to see the history for")]DiscordMember _userId)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{_userId.DisplayName} History",
                ThumbnailUrl = _userId.AvatarUrl,
                Color = _userId.Color
            };

            var server = _ctx.Guild.Id.ToString();
            var results = new UsernameHistoryRecord().Select(server, _userId.Id.ToString());

            foreach (var record in results)
            {
                embed.Description += $"{record.NameBefore}\n";
            }

            await _ctx.RespondAsync("", embed: embed);
        }
    }
}