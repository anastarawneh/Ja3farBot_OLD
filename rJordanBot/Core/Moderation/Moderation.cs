using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
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
            try
            {
                await Data.Data.ReloadJSON();
                await Context.Message.AddReactionAsync(Constants.Emojis.Tick);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
            if (!user.IsModerator() || user.Id != ESettings.Owner) return;

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

            SocketTextChannel logChannel = (SocketTextChannel)Context.Guild.Channels.First(x => x.Id == Data.Data.GetChnlId("moderation-log"));
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("ban")]
        public async Task Ban(SocketGuildUser user, [Remainder]string reason = "")
        {
            if (!user.IsModerator() || user.Id != ESettings.Owner) return;

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

            SocketTextChannel logChannel = (SocketTextChannel)Context.Guild.Channels.First(x => x.Id == Data.Data.GetChnlId("moderation-log"));
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("mute")]
        public async Task Mute(SocketGuildUser user, string time)
        {
            try
            {
                int seconds;
                int time_ = int.Parse(time.Replace("h", "").Replace("m", ""));
                SocketRole muted = Context.Guild.Roles.First(x => x.Name == "Muted");
                switch (time[^1])
                {
                    case 'h':
                        seconds = time_ * 60 * 60;
                        break;
                    case 'm':
                        seconds = time_ * 60;
                        break;
                    default:
                        await ReplyAsync(":x: Please enter the time in this format: `_h` or `_m`");
                        return;
                }

                await user.AddRoleAsync(muted);

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("User Muted");
                embed.WithAuthor(Context.User);
                embed.WithColor(255, 0, 0);
                embed.AddField("User", user, true);
                embed.AddField("Duration", time, true);
                embed.WithFooter($"UserID: {user.Id}");

                SocketTextChannel logChannel = (SocketTextChannel)Context.Guild.Channels.First(x => x.Id == Data.Data.GetChnlId("moderation-log"));
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
