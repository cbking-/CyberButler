using CyberButler.EntityContext;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CyberButler.Commands
{
    [Group("customcommand"), Aliases("cc")]
    internal class CustomCommand : BaseCommandModule
    {
        private readonly CyberButlerContext _dbContext;

        public CustomCommand(CyberButlerContext db)
        {
            _dbContext = db;
        }

        [Command("add"), 
            Aliases("create"), 
            Description("Example: !customcommand add test This is a test command.")]   
        public async Task Add(CommandContext _ctx,
            [Description("The command name")] string _command,
            [Description("The text to display")][RemainingText] string _text)
        {
            var server = _ctx.Guild.Id.ToString();
            var existingCustom = _dbContext.CustomCommand.Where(_ => _.Server == server && _.Command == _command).FirstOrDefault();
            var existingDelivered = _ctx.Client.GetCommandsNext().RegisteredCommands.ContainsKey(_command);

            if (existingCustom == null && !existingDelivered)
            {
                _dbContext.Add(new Entities.CustomCommand()
                {
                    Server = server,
                    Command = _command,
                    Text = _text
                });

                await _dbContext.SaveChangesAsync();

                await _ctx.RespondAsync($"Added \"{_command}\".");
            }
            else
            {
                await _ctx.RespondAsync($"\"{_command}\" already exists on {_ctx.Guild.Name}.");
            }
        }

        [Command("get"), 
            Aliases("list", "read"), 
            Description("Example: !customcommand get")]
        public async Task Get(CommandContext _ctx)
        {
            var server = _ctx.Guild.Id.ToString();
            var results = _dbContext.CustomCommand.Where(_ => _.Server == server);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{_ctx.Guild.Name} Custom Commands",
                Color = DiscordColor.Azure
            };

            foreach (var record in results)
            {
                if (embed.Fields.Count < 25)
                {
                    embed.AddField(record.Command,
                        record.Text.Length <= 1024 ? record.Text : record.Text.Substring(0, 1024));
                }
                else
                {
                    await _ctx.RespondAsync("", embed: embed);

                    embed = new DiscordEmbedBuilder
                    {
                        Title = $"{_ctx.Guild.Name} Custom Commands",
                        Color = DiscordColor.Azure
                    };
                }
            }

            if (embed.Fields.Count > 0)
            {
                await _ctx.RespondAsync("", embed: embed);
            }
        }

        [Command("modify"), 
            Aliases("update"), 
            Description("Example: !customcommand modify test New text")]
        public async Task Modify(CommandContext _ctx, 
            [Description("The command name")] string _command, 
            [Description("Updated text")][RemainingText] string _text)
        {            
            var server = _ctx.Guild.Id.ToString();

            var result = _dbContext.CustomCommand.Where(_ => _.Server == server && _.Command == _command).FirstOrDefault();

            if (result != null)
            {
                await _ctx.RespondAsync($"Are you sure you want to update {_command}? (Yes/No)");
                var interactivity = _ctx.Client.GetInteractivity();
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == _ctx.User.Id, 
                    TimeSpan.FromMinutes(1));

                if (msg.Result.Content.ToLower() == "yes")
                {
                    result.Text = _text;
                    await _dbContext.SaveChangesAsync();
                    await _ctx.RespondAsync($"\"{_command}\" updated.");
                }
                else
                {
                    await _ctx.RespondAsync($"Update canceled.");
                }
            }
            else
            {
                await _ctx.RespondAsync($"\"{_command}\" does not exist on {_ctx.Guild.Name}.");
            }
        }

        [Command("delete"), 
            Aliases("remove"), 
            Description("Example: !customcommand delete test")]
        public async Task Delete(CommandContext _ctx, 
            [Description("The command name to delete")] string _command)
        {
            var server = _ctx.Guild.Id.ToString();

            var result = _dbContext.CustomCommand.Where(_ => _.Server == server && _.Command == _command).FirstOrDefault();

            if (result != null)
            {
                await _ctx.RespondAsync($"Are you sure you want to delete {_command}? (Yes/No)");
                var interactivity = _ctx.Client.GetInteractivity();
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == _ctx.User.Id, 
                    TimeSpan.FromMinutes(1));

                if (msg.Result.Content.ToLower() == "yes")
                {
                    _dbContext.Remove(result);
                    await _dbContext.SaveChangesAsync();
                    await _ctx.RespondAsync($"\"{_command}\" deleted.");
                }
                else
                {
                    await _ctx.RespondAsync($"Delete canceled.");
                }
            }
            else
            {
                await _ctx.RespondAsync($"\"{_command}\" does not exist on {_ctx.Guild.Name}.");
            }
        }
    }
}