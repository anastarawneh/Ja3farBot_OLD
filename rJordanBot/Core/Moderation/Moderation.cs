using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.GeneralJSON;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Moderation
{
    public class Moderation : InteractiveBase<SocketCommandContext>
    {
        [Command("reload"), Summary("Reloads the Settings.json file while running")]
        public async Task Reload()
        {
            //Checks
            if (!(Context.User.Id == ESettings.Owner))
            {
                await Context.Channel.SendMessageAsync(":x: Insufficient Permissions.");
                return;
            }

            //Execution
            await Data.Data.ReloadJSON();
            await Context.Message.AddReactionAsync(Constants.IEmojis.Tick);
        }

        [Command("userinfo")]
        [Alias("uinfo", "ui")]
        public async Task UserInfo(SocketUser param = null)
        {
            if (Context.User.Id != ESettings.Owner) return;

            if (param == null)
            {
                await ReplyAsync(":x: Please mention a user.");
                return;
            }

            IUser userInfo = param as IUser;
            IDMChannel dmchnl = await Context.User.GetOrCreateDMChannelAsync();

            string roles = "";
            List<SocketRole> roles_ = (userInfo as SocketGuildUser).Roles.ToList();
            foreach (SocketRole role in roles_)
            {
                if (role != roles_[0]) roles = roles + $"{role.Name}\n";
            }
            if (roles == "") roles = "None";

            int strikes = Data.Data.GetStrikes(userInfo.Id);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(40, 200, 150);
            embed.WithThumbnailUrl(userInfo.GetAvatarUrl());
            embed.WithTitle(userInfo.ToString());
            embed.AddField("Mention", userInfo.Mention);
            embed.AddField("ID", userInfo.Id);
            embed.AddField("Roles", roles);
            embed.AddField("Status", userInfo.Status);
            embed.AddField("Strikes", strikes);

            IUserMessage msg = await dmchnl.SendMessageAsync("", false, embed.Build());
            ulong id = msg.Id;

            EmbedBuilder embedB = msg.Embeds.FirstOrDefault().ToEmbedBuilder();
            embedB.WithFooter($"ID: {id}");
            await msg.ModifyAsync(x => x.Embed = embedB.Build());

            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }

        [Command("kick")]
        public async Task Kick(SocketGuildUser user, [Remainder]string reason = "")
        {
            SocketGuildUser user_ = Context.User as SocketGuildUser;
            if (!user_.IsModerator() && user_.Id != ESettings.Owner) return;

            await user.KickAsync();

            if (reason == "") await ReplyAsync($":white_check_mark: {user.Mention} has been kicked.");
            else await ReplyAsync($":white_check_mark: {user.Mention} has been kicked. | Reason: {reason}");

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Kicked");
            embed.WithAuthor(Context.User);
            embed.WithColor(255, 0, 0);
            embed.AddField("User", user);
            embed.WithFooter($"UserID: {user.Id}");
            if (reason != "") embed.AddField("Reason", reason);

            SocketTextChannel logChannel = (SocketTextChannel)Constants.IGuilds.Jordan(Context).Channels.First(x => x.Id == Data.Data.GetChnlId("moderation-log"));
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("ban")]
        public async Task Ban(SocketGuildUser user, [Remainder]string reason = "")
        {
            SocketGuildUser user_ = Context.User as SocketGuildUser;
            if (!user_.IsModerator() && user_.Id != ESettings.Owner) return;

            await user.BanAsync();

            if (reason == "") await ReplyAsync($":white_check_mark: {user.Mention} has been banned.");
            else await ReplyAsync($":white_check_mark: {user.Mention} has been banned. | Reason: {reason}");

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Banned");
            embed.WithAuthor(Context.User);
            embed.WithColor(255, 0, 0);
            embed.AddField("User", user);
            embed.WithFooter($"UserID: {user.Id}");
            if (reason != "") embed.AddField("Reason", reason);

            SocketTextChannel logChannel = (SocketTextChannel)Constants.IGuilds.Jordan(Context).Channels.First(x => x.Id == Data.Data.GetChnlId("moderation-log"));
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("mute")]
        public async Task Mute(SocketGuildUser user, string time, [Remainder] string reason = null)
        {
            SocketGuildUser user_ = Context.User as SocketGuildUser;
            if (!user_.IsModerator() && user_.Id != ESettings.Owner) return;

            int seconds;
            int time_ = int.Parse(time.Replace("d", "").Replace("h", "").Replace("m", "").Replace("s", ""));
            SocketRole muted = Constants.IGuilds.Jordan(Context).Roles.First(x => x.Name == "Muted");
            switch (time[^1])
            {
                case 'd':
                    seconds = time_ * 60 * 60 * 24;
                    break;
                case 'h':
                    seconds = time_ * 60 * 60;
                    break;
                case 'm':
                    seconds = time_ * 60;
                    break;
                case 's':
                    seconds = time_;
                    break;
                default:
                    await ReplyAsync(":x: Please enter the time in this format: `_h` or `_m`");
                    return;
            }

            await user.AddRoleAsync(muted);
            await Context.Message.AddReactionAsync(Constants.IEmojis.Tick);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Muted");
            embed.WithAuthor(Context.User);
            embed.WithColor(255, 0, 0);
            embed.AddField("User", user, true);
            embed.AddField("Duration", time, true);
            if (reason != null) embed.AddField("Reason", reason);
            embed.WithFooter($"UserID: {user.Id}");

            SocketTextChannel logChannel = (SocketTextChannel)Constants.IGuilds.Jordan(Context).Channels.First(x => x.Id == Data.Data.GetChnlId("moderation-log"));
            RestUserMessage msg = logChannel.SendMessageAsync("", false, embed.Build()).Result;

            while (seconds > 0)
            {
                await Task.Delay(1000);
                seconds--;
            }

            await user.RemoveRoleAsync(muted);

            embed.WithColor(0, 255, 0);
            embed.WithTitle("User Muted => User Unmuted");

            await msg.ModifyAsync(x => x.Embed = embed.Build());
        }

        [Command("warn")]
        public async Task Warn(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            // Checks
            if (user == null)
            {
                await ReplyAsync(":x: Please mention a user to be warned. `^warn <user> <reason>`");
                return;
            }
            if (reason == null)
            {
                await ReplyAsync(":x: Please mention a reason for the warning. `^warn <user> <reason>`");
                return;
            }

            // Execution
            using SqliteDbContext DbContext = new SqliteDbContext();
            if (DbContext.Strikes.Where(x => x.UserId == user.Id).Count() < 1)
            {
                DbContext.Strikes.Add(new Strike
                {
                    UserId = user.Id,
                    Amount = 0
                });
                await DbContext.SaveChangesAsync();
            }
            Strike strikeEntry = DbContext.Strikes.First(x => x.UserId == user.Id);
            strikeEntry.Amount++;
            DbContext.Update(strikeEntry);
            await DbContext.SaveChangesAsync();

            // Response
            await Context.Message.DeleteAsync();
            IUserMessage response = await ReplyAsync($":white_check_mark: User {user.Mention} has been warned for `{reason}`. Total warnings: `{strikeEntry.Amount}`.");
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Warning issued");
            embed.WithAuthor(Context.User);
            embed.WithColor(Constants.IColors.Blurple);
            embed.WithDescription($"[Link to message]({response.GetJumpUrl()})");
            embed.AddField("User", user);
            embed.AddField("Reason", reason);
            embed.WithCurrentTimestamp();
            embed.WithFooter($"UserID: {user.Id}");

            SocketGuild guild = Constants.IGuilds.Jordan(Context);
            SocketTextChannel logChannel = guild.Channels.First(x => x.Id == Data.Data.GetChnlId("moderation-log")) as SocketTextChannel;
            await logChannel.SendMessageAsync("", false, embed.Build());
        }
    }
}
