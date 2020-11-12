using Dapper;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using rJordanBot.Core.Methods;
using rJordanBot.Core.Preconditions;
using rJordanBot.Resources.MySQL;
using rJordanBot.Resources.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class CustomVCs : InteractiveBase<SocketCommandContext>
    {
        [Group("customvc"), Alias("cvc")]
        [RequireBotChannel]
        public class CustomVCModule : InteractiveBase<SocketCommandContext>
        {
            private CustomVCService _customVCService;

            public CustomVCModule(CustomVCService customVCService)
            {
                _customVCService = customVCService;
            }

            [Command("help"), Alias("")]
            public async Task Help()
            {
                await ReplyAsync(
                    ":question: CustomVC commands:\n" +
                    "``^customvc help``: Displays this message.\n" +
                    "``^customvc create``: Creates a CustomVC.\n" +
                    "``^customvc load``: Loads your CustomVC.\n" +
                    "``^customvc unload``: Unloads your CustomVC.\n" +
                    "``^customvc edit <property> <value>``: Edits your CustomVC."
                );
            }

            [Command("create")]
            public async Task Create()
            {
                if (await _customVCService.HasCustomVC(Context.User.Id))
                {
                    await ReplyAsync(":x: You already have a Custom VC.");
                    return;
                }
                IUserMessage msg = await ReplyAsync("How many slots should your Custom VC contain? (0-99, 0 to disable the limit)");
                SocketMessage response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                if (response == null)
                {
                    await msg.AddReactionAsync(Constants.IEmojis.X);
                    return;
                }
                int slots;
                if (int.TryParse(response.Content, out int result) && result < 100) slots = result;
                else
                {
                    await ReplyAsync(":x: Invalid response.");
                    return;
                }
                CustomVC vc = await _customVCService.GetOrCreateCustomVC(Context.User as SocketGuildUser);
                await _customVCService.ModifyCustomVC(vc, slots);
                await ReplyAsync(":white_check_mark: Custom VC created.");
            }

            [Command("load")]
            public async Task Load()
            {
                CustomVC vc = await _customVCService.GetOrCreateCustomVC(Context.User as SocketGuildUser);
                await ReplyAsync(await _customVCService.Load(vc));
            }

            [Command("unload")]
            public async Task Unload()
            {
                CustomVC vc = await _customVCService.GetOrCreateCustomVC(Context.User as SocketGuildUser);
                await ReplyAsync(await _customVCService.Unload(vc));
            }

            [Command("edit")]
            public async Task Edit(string setting, int value)
            {
                CustomVC vc = await _customVCService.GetOrCreateCustomVC(Context.User as SocketGuildUser);
                await ReplyAsync(await _customVCService.ModifyCustomVC(vc, setting, value));
            }
        }
    }
}
