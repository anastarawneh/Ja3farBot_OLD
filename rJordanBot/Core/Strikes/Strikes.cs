using Discord;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Strikes
{
    public class Strikes : ModuleBase<SocketCommandContext>
    {
        [Group("strike"), Alias("st"), Summary("Strike Group")]
        public class StrikesGroup : ModuleBase<SocketCommandContext>
        {
            [Command("get")]
            public async Task Get(IUser User = null)
            {
                if (User == null)
                {
                    //No mention
                    await Context.Channel.SendMessageAsync(":x: You didn't mention a user. ``^strike get <user>``");
                    return;
                }

                if (User.IsBot)
                {
                    //Mention is a bot
                    await Context.Channel.SendMessageAsync(":x: You can't strike a bot. ``^strike get <user>``");
                    return;
                }

                SocketGuildUser User1 = Context.User as SocketGuildUser;
                if (!User1.Roles.Contains(User1.Roles.FirstOrDefault(x => x.Name == "Moderator")) && User1.Id != ESettings.Owner)
                {
                    //No perms
                    await Context.Channel.SendMessageAsync(":x: You don't have sufficient permissions. ``^strike get <user>``");
                    return;
                }

                //Execution
                await Context.Channel.SendMessageAsync($":white_check_mark: {User.Mention} has {Data.Data.GetStrikes(User.Id)} strike(s).");
            }

            [Command, Summary("Add a strike"), Alias("add")]
            public async Task Add(IUser User = null, int Amount = 1)
            {
                //Checks
                if (User == null)
                {
                    //No mention
                    await Context.Channel.SendMessageAsync(":x: You didn't mention a user. ``^strike add <user> [amount]``");
                    return;
                }

                if (User.IsBot)
                {
                    //Mention is a bot
                    await Context.Channel.SendMessageAsync(":x: You can't strike a bot. ``^strike add <user> [amount]``");
                    return;
                }

                if (Amount < 1)
                {
                    //Amount is less than 1
                    await Context.Channel.SendMessageAsync(":x: You have to specify a number greater than one. ``^strike add <user> [amount]``");
                    return;
                }

                SocketGuildUser User1 = Context.User as SocketGuildUser;
                if (!User1.Roles.Contains(User1.Roles.FirstOrDefault(x => x.Name == "Moderator")) && User1.Id != ESettings.Owner)
                {
                    //No perms
                    await Context.Channel.SendMessageAsync(":x: You don't have sufficient permissions. ``^strike add <user> [amount]``");
                    return;
                }

                //Execution
                await Context.Channel.SendMessageAsync($":white_check_mark: {User.Mention} has been given {Amount} strike(s).");

                //Saving
                await Data.Data.SaveStrikes(User.Id, Amount);

            }

            [Command("reset"), Summary("Reset strikes")]
            public async Task Reset(IUser User = null)
            {
                //Checks
                if (User == null)
                {
                    //No mention
                    await Context.Channel.SendMessageAsync(":x: You didn't mention a user. ``^strike reset <user>``");
                    return;
                }

                if (User.IsBot)
                {
                    //Mention is a bot
                    await Context.Channel.SendMessageAsync(":x: You can't strike a bot. ``^strike reset <user>``");
                    return;
                }

                SocketGuildUser User1 = Context.User as SocketGuildUser;
                if (!User1.Roles.Contains(User1.Roles.FirstOrDefault(x => x.Name == "Moderator")) && User1.Id != ESettings.Owner)
                {
                    //No perms
                    await Context.Channel.SendMessageAsync(":x: You don't have sufficient permissions. ``^strike reset <user>``");
                    return;
                }

                //Execution
                await Context.Channel.SendMessageAsync($":white_check_mark: {User.Mention}'s strikes have been reset.");

                //Saving
                using SqliteDbContext DbContext = new SqliteDbContext();
                DbContext.Strikes.RemoveRange(DbContext.Strikes.Where(x => x.UserId == User.Id));
                await DbContext.SaveChangesAsync();
            }

            [Command("set")]
            public async Task Set(IUser User = null, int Amount = 1)
            {
                //Checks
                if (User == null)
                {
                    //No mention
                    await Context.Channel.SendMessageAsync(":x: You didn't mention a user. ``^strike set <user> [amount]``");
                    return;
                }

                if (User.IsBot)
                {
                    //Mention is a bot
                    await Context.Channel.SendMessageAsync(":x: You can't strike a bot. ``^strike set <user> [amount]``");
                    return;
                }

                if (Amount < 1)
                {
                    //Amount is less than 1
                    await Context.Channel.SendMessageAsync(":x: You have to specify a number greater than one. ``^strike set <user> [amount]``");
                    return;
                }

                SocketGuildUser User1 = Context.User as SocketGuildUser;
                if (!User1.Roles.Contains(User1.Roles.FirstOrDefault(x => x.Name == "Moderator")) && User1.Id != ESettings.Owner)
                {
                    //No perms
                    await Context.Channel.SendMessageAsync(":x: You don't have sufficient permissions. ``^strike set <user> [amount]``");
                    return;
                }

                //Execution
                await Context.Channel.SendMessageAsync($":white_check_mark: {User.Mention} now has {Amount} strike(s).");

                //Saving
                await Data.Data.SetStrikes(User.Id, Amount);
            }
        }
    }
}
