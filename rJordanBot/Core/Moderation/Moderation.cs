using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Core.Preconditions;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Moderation
{
    public class Moderation : InteractiveBase<SocketCommandContext>
    {
        [Command("modhelp")]
        [RequireMod]
        public async Task ModHelp()
        {
            await ReplyAsync(
                ":question: Mod commands:\n" +
                "``^modhelp``: Displays this message.\n" +
                "``^userinfo <user>``: Displays information about a user.\n" +
                "*``^kick <user> [reason]``: Kicks a user for an optional reason.*\n" +
                "*``^ban <user> [reason]``: Bans a user for an optional reason.*\n" +
                "*``^mute <user> <time> [reason]``: Temporarily mute a reason for an optional reason. Time format is `_s, _m, _h, or _d`.\n" +
                "*``^unmute <user>``: Unmutes a user.*\n" +
                "*``^warn <user> <reason>``: Warns a user for a required reason.*\n" +
                "*``^warn get <user>``: Gets the user's warnings.*\n" +
                "*``^warn list``: Lists all users with warnings.*\n" +
                "``^mutefix <id>``: Fixes a user's mute timer on bot reboot. Only use when prompted.\n" +
                $"``^clean``: Cleans #{MentionUtils.MentionChannel(Data.GetChnlId("verification"))}.\n" +
                $"*Commands with asterisks are functional mod only.*"
            );
        }
        
        [Command("userinfo")]
        [Alias("uinfo", "ui")]
        [RequireMod]
        public async Task UserInfo(SocketUser param = null)
        {
            if (param == null)
            {
                await ReplyAsync(":x: Please mention a user.");
                return;
            }

            IUser userInfo = param as IUser;
            using SqliteDbContext DbContext = new SqliteDbContext();

            string roles = "";
            List<SocketRole> roles_ = (userInfo as SocketGuildUser).Roles.ToList();
            foreach (SocketRole role in roles_)
            {
                if (role != roles_[0]) roles += $"{role.Name}\n";
            }
            if (roles == "") roles = "None";

            int strikes = Data.GetStrikes(userInfo.Id);

            User user_ = DbContext.Users.First(x => x.ID == param.Id) ?? new User { ID = 0, Verified = false };
            bool verified = user_.Verified;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(40, 200, 150);
            embed.WithThumbnailUrl(userInfo.GetAvatarUrl());
            embed.WithTitle(userInfo.ToString());
            embed.AddField("Mention", userInfo.Mention);
            embed.AddField("ID", userInfo.Id);
            embed.AddField("Roles", roles);
            embed.AddField("Status", userInfo.Status);
            embed.AddField("Warnings", strikes);
            embed.AddField("Verified", verified);

            await ReplyAsync("", false, embed.Build());

            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }

        [Command("kick")]
        [RequireFuncMod]
        public async Task Kick(SocketGuildUser user, [Remainder]string reason = "")
        {
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

            SocketTextChannel logChannel = (SocketTextChannel)Constants.IGuilds.Jordan(Context).Channels.First(x => x.Id == Methods.Data.GetChnlId("moderation-log"));
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("ban")]
        [RequireFuncMod]
        public async Task Ban(SocketGuildUser user, [Remainder]string reason = "")
        {
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

            SocketTextChannel logChannel = (SocketTextChannel)Constants.IGuilds.Jordan(Context).Channels.First(x => x.Id == Methods.Data.GetChnlId("moderation-log"));
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("mute")]
        [RequireMod]
        public async Task Mute(SocketGuildUser user, string time, [Remainder] string reason = null)
        {
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

            SocketTextChannel logChannel = (SocketTextChannel)Constants.IGuilds.Jordan(Context).Channels.First(x => x.Id == Methods.Data.GetChnlId("moderation-log"));
            RestUserMessage msg = logChannel.SendMessageAsync("", false, embed.Build()).Result;

            while (seconds > 0)
            {
                await Task.Delay(1000);
                seconds--;
            }

            await Context.Guild.DownloadUsersAsync();
            if (Context.Guild.Users.Contains(user))
            {
                embed.WithColor(0, 0, 255);
                embed.WithTitle("User Muted => User Left");
            }
            user = Context.Guild.GetUser(user.Id); // reload user roles if changed
            IReadOnlyCollection<SocketRole> roles = user.Roles;
            if (!roles.Contains(muted)) return;

            await user.RemoveRoleAsync(muted);

            embed.WithColor(0, 255, 0);
            embed.WithTitle("User Muted => User Unmuted");

            await msg.ModifyAsync(x => x.Embed = embed.Build());
        }

        [Command("unmute")]
        [RequireMod]
        public async Task Unmute(SocketGuildUser user)
        {
            IReadOnlyCollection<SocketRole> roles = user.Roles;
            foreach (SocketRole role in roles)
            {
                if (role.Name == "Muted")
                {
                    goto cont;
                }
            }
            await ReplyAsync($":x: User {user.Mention} isn't muted.");
            return;

        cont:
            SocketRole muted = Constants.IGuilds.Jordan(Context).Roles.First(x => x.Name == "Muted");
            await user.RemoveRoleAsync(muted);

            SocketTextChannel channel = Context.Guild.Channels.First(x => x.Id == Methods.Data.GetChnlId("moderation-log")) as SocketTextChannel;
            IEnumerable<IMessage> messages = channel.GetMessagesAsync(10).FlattenAsync().Result;
            foreach (IMessage message in messages)
            {
                if (message.Embeds.First().Title == "User Muted" &&
                    message.Embeds.First().Fields.First(x => x.Name == "User").Value == user.Username + "#" + user.Discriminator)
                {
                    EmbedBuilder embed = message.Embeds.First().ToEmbedBuilder();

                    embed.WithColor(0, 255, 0);
                    embed.WithTitle("User Muted => User Unmuted");
                    embed.Fields.First(x => x.Name == "Duration").Value += $" (unmuted manually by {Context.User.Mention})";

                    await (message as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());

                    await Context.Message.AddReactionAsync(Constants.IEmojis.Tick);
                }
            }
        }

        [Group("warn")]
        [RequireFuncMod]
        public class Warn : InteractiveBase<SocketCommandContext>
        {
            [Command("add"), Alias("")]
            public async Task AddWarning(SocketGuildUser user = null, [Remainder] string reason = null)
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
                embed.WithColor(Constants.IColors.Blue);
                embed.WithDescription($"[Link to message]({response.GetJumpUrl()})");
                embed.AddField("User", user);
                embed.AddField("Reason", reason);
                embed.WithCurrentTimestamp();
                embed.WithFooter($"UserID: {user.Id}");

                SocketGuild guild = Constants.IGuilds.Jordan(Context);
                SocketTextChannel logChannel = guild.Channels.First(x => x.Id == Methods.Data.GetChnlId("moderation-log")) as SocketTextChannel;
                await logChannel.SendMessageAsync("", false, embed.Build());
            }

            [Command("get")]
            public async Task GetWarning(SocketGuildUser user = null)
            {
                // Checks
                if (user == null)
                {
                    await ReplyAsync(":x: Please mention a user to be warned. `^warn <user> <reason>`");
                    return;
                }

                // Execution
                switch (Methods.Data.GetStrikes(user.Id))
                {
                    case 1:
                        await ReplyAsync($":white_check_mark: User {user.Mention} has `{Methods.Data.GetStrikes(user.Id)}` warning.");
                        break;
                    default:
                        await ReplyAsync($":white_check_mark: User {user.Mention} has `{Methods.Data.GetStrikes(user.Id)}` warnings.");
                        break;
                }
            }

            [Command("list")]
            public async Task List(int page = 1)
            {
                using SqliteDbContext DbContext = new SqliteDbContext();
                IOrderedQueryable<Strike> strikes = DbContext.Strikes.AsQueryable().OrderByDescending(x => x.Amount);
                EmbedBuilder embed = new EmbedBuilder();

                embed.WithTitle("Warning Leaderboard");
                embed.WithColor(Constants.IColors.Blue);
                embed.WithFooter($"\nPage {page}/{strikes.Count() / 5 + 1}");

                string list = "```WARNING LEADERBOARDS:\n\n";
                List<Strike> strikes_ = strikes.ToList();

                if (5 * page >= strikes.Count())
                {
                    for (int x = 5 * (page - 1); x < strikes.Count(); x++)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(strikes_[x].UserId);
                        embed.AddField($"#{x + 1} {user.ToString()}", strikes_[x].Amount);
                        list += $"#{x + 1} {user} >> {strikes_[x].Amount}\n";
                    }
                }
                else for (int x = 5 * (page - 1); x < 5 * page; x++)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(strikes_[x].UserId);
                        embed.AddField($"#{x + 1} {user.ToString()}", strikes_[x].Amount);
                        list += $"#{x + 1} {user} >> {strikes_[x].Amount}\n";
                    }

                list += $"\nPage {page}/{strikes.Count() / 5 + 1}```";

                //await ReplyAsync(list);
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("mutefix")]
        [RequireMod(Group = "group")]
        [RequireBot(Group = "group")]
        public async Task MuteFix(ulong msgID)
        {
            if (Context.Channel is IDMChannel) return;
            SocketGuildUser self = Context.User as SocketGuildUser;

            SocketTextChannel modlog = Context.Guild.Channels.First(x => x.Id == Data.GetChnlId("moderation-log")) as SocketTextChannel;
            IMessage message = modlog.GetMessageAsync(msgID).Result;
            IEmbed embed = message.Embeds.First();
            if (embed.Title != "User Muted")
            {
                await ReplyAsync($":x: User is not muted, no need to fix this.");
                return;
            }
            SocketGuildUser user = Context.Guild.GetUser(ulong.Parse(embed.Footer.Value.Text.Replace("UserID: ", "")));

            ulong id = ulong.Parse(embed.Footer.Value.Text.Replace("UserID: ", ""));
            if (Context.Guild.GetUser(id) == null)
            {
                EmbedBuilder embedBuilder = message.Embeds.First().ToEmbedBuilder();

                embedBuilder.WithColor(0, 255, 0);
                embedBuilder.WithTitle("User Muted => User Unmuted");
                embedBuilder.Fields.First(x => x.Name == "Duration").Value += $" (unmuted manually by {Context.Client.CurrentUser.Mention}, user left)";
                await (message as IUserMessage).ModifyAsync(x => x.Embed = embedBuilder.Build());

                await Context.Message.AddReactionAsync(Constants.IEmojis.Tick);
                return;
            }

            DateTimeOffset mutestart = message.Timestamp.ToLocalTime();
            DateTimeOffset mutefinish = mutestart;
            string timestring = embed.Fields.First(x => x.Name == "Duration").Value;
            char timesymbol = timestring[^1];
            int time = int.Parse(timestring.Replace(timesymbol.ToString(), ""));
            switch (timesymbol)
            {
                case 'd':
                    mutefinish = mutefinish.AddDays(time);
                    break;
                case 'h':
                    mutefinish = mutefinish.AddHours(time);
                    break;
                case 'm':
                    mutefinish = mutefinish.AddMinutes(time);
                    break;
                case 's':
                    mutefinish = mutefinish.AddSeconds(time);
                    break;
                default:
                    await ReplyAsync(":x: Time must be in the same format as the duration in the embed.");
                    return;
            }

            Console.WriteLine(timestring);
            Console.WriteLine(timesymbol);
            Console.WriteLine(time);
            Console.WriteLine(mutestart);
            Console.WriteLine(mutefinish);
            Console.WriteLine(DateTimeOffset.Now.ToLocalTime());

            if (mutefinish <= DateTimeOffset.Now.ToLocalTime())
            {
                SocketRole muted = Constants.IGuilds.Jordan(Context).Roles.First(x => x.Name == "Muted");
                await self.RemoveRoleAsync(muted);

                EmbedBuilder embedBuilder = message.Embeds.First().ToEmbedBuilder();

                embedBuilder.WithColor(0, 255, 0);
                embedBuilder.WithTitle("User Muted => User Unmuted");
                embedBuilder.Fields.First(x => x.Name == "Duration").Value += $" (unmuted manually by {Context.Client.CurrentUser.Mention})";
                await (message as IUserMessage).ModifyAsync(x => x.Embed = embedBuilder.Build());

                await Context.Message.AddReactionAsync(Constants.IEmojis.Tick);
                return;
            }

            TimeSpan duration = mutefinish - DateTimeOffset.Now.ToLocalTime();
            await ReplyAsync($":white_check_mark: Fixed {user.Mention}'s mute, it ends in {duration.ToString(@"d\:hh\:mm\:ss")}.");

            while (mutefinish > DateTimeOffset.Now.ToLocalTime())
            {

            }

            user = Context.Guild.GetUser(user.Id); // reload user roles if changed
            SocketRole mutedrole = Constants.IGuilds.Jordan(Context).Roles.First(x => x.Name == "Muted");
            IReadOnlyCollection<SocketRole> roles = user.Roles;
            if (!roles.Contains(mutedrole)) return;

            await user.RemoveRoleAsync(mutedrole);

            EmbedBuilder embedBuilder2 = message.Embeds.First().ToEmbedBuilder();

            embedBuilder2.WithColor(0, 255, 0);
            embedBuilder2.WithTitle("User Muted => User Unmuted");

            await (message as IUserMessage).ModifyAsync(x => x.Embed = embedBuilder2.Build());

            await Context.Message.AddReactionAsync(Constants.IEmojis.Tick);
        }

        [Command("clean")]
        [RequireMod]
        public async Task Clean()
        {
            ITextChannel channel = Context.Channel as ITextChannel;
            if (channel.Id != Data.GetChnlId("verification") || channel is IDMChannel) return;

            IEnumerable<IMessage> messages = channel.GetMessagesAsync().FlattenAsync().Result;
            int count = messages.Count() - 1;
            var deletable = channel.GetMessagesAsync(count).FlattenAsync().Result;
            await channel.DeleteMessagesAsync(deletable);
        }
    }
}
