using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Design;
using rJordanBot.Core.Methods;
using rJordanBot.Core.Preconditions;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static rJordanBot.Resources.Datatypes.Setting;

namespace rJordanBot.Core.Commands
{
    public class Owner : InteractiveBase<SocketCommandContext>
    {
        [Command("test", RunMode = RunMode.Async)]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Test()
        {
            // TEST CODE STARTS HERE
            
            // TEST CODE ENDS HERE
            IEmote yes = Constants.IEmojis.Tick;
            await Context.Message.AddReactionAsync(yes);
        }

        [Command("reload")]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Reload()
        {
            await Data.ReloadJSON();
            await Context.Message.AddReactionAsync(Constants.IEmojis.Tick);
        }

        [Command("deldm")]
        [RequireOwner]
        [RequireBotChannel]
        public async Task DelDM(int msgcount)
        {
            IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
            IEnumerable<IMessage> msgs = dm.GetMessagesAsync(msgcount).FlattenAsync().Result;
            foreach (IMessage msg in msgs)
            {
                await dm.DeleteMessageAsync(msg);
            }
            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }

        [Command("ping")]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Ping()
        {
            string ans = Environment.GetEnvironmentVariable("SystemType");

            await ReplyAsync($"[{ans.ToUpper()}] Pong!");
            Console.ForegroundColor = ConsoleColor.Green;
            string msg = $"[{DateTime.Now} at Commands] A ping in {Context.Channel} by {Context.User}";
            Console.WriteLine(msg);
            await (Constants.IGuilds.Jordan(Context).Channels.Where(x => x.Id == 642475027123404811).FirstOrDefault() as SocketTextChannel).SendMessageAsync(msg);
            Console.ResetColor();
        }

        [Command("resetchannels"), Alias("rc")]
        public async Task ResetChannels()
        {
            if (Context.User.Id != ESettings.Owner && Context.User.Id != Context.Client.CurrentUser.Id) return;
            await Methods.Data.ResetChannels(Context);

            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }

        [Group("roleembed"), Alias("re")]
        [RequireOwner]
        [RequireBotChannel]
        public class RoleEmbed : InteractiveBase<SocketCommandContext>
        {
            [Command("create")]
            public async Task Create()
            {
                EmbedBuilder embed = new EmbedBuilder();
                SocketTextChannel channel = Constants.IGuilds.Jordan(Context).Channels.FirstOrDefault(x => x.Id == Methods.Data.GetChnlId("role-selection")) as SocketTextChannel;

                embed.WithTitle("General Roles");
                embed.WithColor(114, 137, 218);
                embed.AddField("There are no general roles available at the moment.", "Check back later for another event.");
                await channel.SendMessageAsync("", false, embed.Build());

                embed = new EmbedBuilder();
                embed.WithTitle("Color Roles");
                embed.WithColor(114, 137, 218);
                embed.AddField("There are no color roles available at the moment.", "Check back later for another event.");
                embed.WithFooter("Note: if there are multiple roles in this list, you will only be able to select one.");
                await channel.SendMessageAsync("", false, embed.Build());

                embed = new EmbedBuilder();
                embed.WithTitle("Notification Roles");
                embed.WithColor(114, 137, 218);
                embed.AddField("There are no notification roles available at the moment.", "Check back later for another event.");
                await channel.SendMessageAsync("", false, embed.Build());
            }

            [Command("load")]
            public async Task Load(string role)
            {
                RoleSetting roleSetting = Methods.Data.GetRoleSetting(role);
                SocketTextChannel channel = Constants.IGuilds.Jordan(Context).Channels.Where(x => x.Id == Methods.Data.GetChnlId("role-selection")).FirstOrDefault() as SocketTextChannel;
                IUserMessage msg = channel.GetMessageAsync(roleSetting.id).Result as IUserMessage;
                Embed embed = msg.Embeds.FirstOrDefault() as Embed;
                EmbedBuilder embedBuilder = embed.ToEmbedBuilder();
                if (embed.Fields.FirstOrDefault().Name == $"There are no {roleSetting.group.ToLower()} roles available at the moment.")
                {
                    //Message is empty
                    EmbedBuilder emptybuilder = new EmbedBuilder();
                    embedBuilder.Fields = emptybuilder.Fields;
                }

                string rolename = Constants.IGuilds.Jordan(Context).Roles.Where(x => x.Id == roleSetting.roleid).FirstOrDefault().Name;
                foreach (EmbedField field in embed.Fields)
                {
                    if (field.Name == rolename)
                    {
                        //Role is loaded
                        await ReplyAsync(":x: Role already loaded.");
                        return;
                    }
                }
                embedBuilder.AddField(rolename, $"React with {roleSetting.emote} for this role!");
                await msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                await ReplyAsync(":white_check_mark: Loaded.");

                Emoji emote = new Emoji("");
                emote = new Emoji(Methods.Data.GetRoleSetting(role).emoji);
                await msg.AddReactionAsync(emote);
            }

            [Command("unload"), Alias("ul")]
            public async Task Unload(string role)
            {
                RoleSetting roleSetting = Methods.Data.GetRoleSetting(role);
                SocketTextChannel channel = Constants.IGuilds.Jordan(Context).Channels.Where(x => x.Id == Methods.Data.GetChnlId("role-selection")).FirstOrDefault() as SocketTextChannel;
                IUserMessage msg = channel.GetMessageAsync(roleSetting.id).Result as IUserMessage;
                Embed embed = msg.Embeds.FirstOrDefault() as Embed;

                if (embed.Fields.FirstOrDefault().Name == $"There are no {roleSetting.group.ToLower()} roles available at the moment.")
                {
                    //Message is empty
                    await ReplyAsync(":x: Role is not loaded.");
                }

                string rolename = Constants.IGuilds.Jordan(Context).Roles.Where(x => x.Id == roleSetting.roleid).FirstOrDefault().Name;
                foreach (EmbedField field in embed.Fields)
                {
                    if (field.Name == rolename)
                    {
                        //Role is loaded
                        EmbedBuilder embedBuilder = new EmbedBuilder();
                        embedBuilder.WithTitle(embed.Title);
                        embedBuilder.WithColor(embed.Color.Value);
                        if (embed.Fields.Count() == 1)
                        {
                            embedBuilder.AddField($"There are no {roleSetting.group.ToLower()} roles available at the moment.", "Check back later for another event.");
                        }
                        else
                        {
                            foreach (EmbedField field_ in embed.Fields)
                            {
                                if (field_.Name != rolename) embedBuilder.AddField(field_.Name, field_.Value);
                            }
                        }
                        if (embed.Footer != null) embedBuilder.WithFooter(embed.Footer.Value.Text);

                        await msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        await ReplyAsync(":white_check_mark: Role unloaded.");

                        IEmote emote = new Emoji("");
                        emote = new Emoji(Methods.Data.GetRoleSetting(role).emoji);
                        await msg.RemoveReactionAsync(emote, Context.Client.CurrentUser as IUser);
                        return;
                    }
                }
            }

            [Command("ban")]
            public async Task Ban(SocketGuildUser user)
            {
                SocketGuild guild = Constants.IGuilds.Jordan(Context);
                SocketRole role = guild.Roles.First(x => x.Name == "Role denied");

                if (role.Members.Contains(user))
                {
                    await ReplyAsync($":x: {user.Mention} is already banned from using reaction roles.");
                    return;
                }

                await user.AddRoleAsync(role);
                await ReplyAsync($":white_check_mark: {user.Mention} has been banned from using reaction roles.");
            }

            [Command("unban")]
            public async Task Unban(SocketGuildUser user)
            {
                SocketGuild guild = Constants.IGuilds.Jordan(Context);
                SocketRole role = guild.Roles.First(x => x.Name == "Role denied");

                if (!role.Members.Contains(user))
                {
                    await ReplyAsync($":x: {user.Mention} is not banned from using reaction roles.");
                    return;
                }

                await user.RemoveRoleAsync(role);
                await ReplyAsync($":white_check_mark: {user.Mention} has been unbanned from using reaction roles.");
            }
        }

        [Command("botstop")]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Stop()
        {
            SocketTextChannel channel = Constants.IGuilds.Jordan(Context).Channels.FirstOrDefault(x => x.Id == Methods.Data.GetChnlId("bot-log")) as SocketTextChannel;
            await channel.SendMessageAsync($"[{DateTime.Now} at Commands] Stopping [{Environment.GetEnvironmentVariable("SystemType").ToUpper()}] instance...");
            await Context.Client.LogoutAsync();
            await Task.Delay(2500);
            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
            Environment.Exit(1);
        }

        [Command("setgame"), Alias("sg")]
        [RequireOwner]
        [RequireBotChannel]
        public async Task SetGame(string state, [Remainder]string game = null)
        {
            ActivityType activity = new ActivityType();
            activity = state switch
            {
                "-l" => ActivityType.Listening,
                "-s" => ActivityType.Streaming,
                "-q" => ActivityType.Watching,
                _ => ActivityType.Playing,
            };
            await Context.Client.SetGameAsync(game, null, activity);

            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }

        [Command("announce")]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Announce()
        {
            Announcement ann = ESettings.Announcement;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(ann.title);
            embed.WithDescription(ann.desc);
            foreach (Announcement.Field field in ann.fields)
            {
                embed.AddField(field.title, field.content);
            }
            embed.WithFooter("Please leave any feedback about this update in #feedback.");
            embed.WithColor(114, 137, 218);

            IUserMessage testmsg = await ReplyAsync("", false, embed.Build());
            IUserMessage confirmationmessage = await ReplyAsync($"Please type `confirm` within 2 minutes to send this message to <#{Data.GetChnlId("announcements")}>.");

            var reply = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
            if (reply == null)
            {
                await testmsg.DeleteAsync();
                await confirmationmessage.AddReactionAsync(new Emoji("❌"));
                return;
            }
            else switch (reply.Content)
                {
                    case "confirm":
                        await testmsg.DeleteAsync();
                        SocketTextChannel AnnouncementChannel = Constants.IGuilds.Jordan(Context).Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("announcements")) as SocketTextChannel;
                        await AnnouncementChannel.SendMessageAsync("@everyone", false, embed.Build());
                        //await AnnouncementChannel.SendMessageAsync("", false, embed.Build());
                        break;
                    default:
                        await testmsg.DeleteAsync();
                        await confirmationmessage.AddReactionAsync(new Emoji("❌"));
                        return;
                }
        }

        [Command("say")]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Say(SocketTextChannel channel, [Remainder] string message)
        {
            await channel.SendMessageAsync(message);
            await Context.Message.DeleteAsync();
        }

        [Group("db")]
        [RequireOwner]
        [RequireBotChannel]
        public class DB : InteractiveBase<SocketCommandContext>
        {
            [Group("user")]
            public class UserInfo : InteractiveBase<SocketCommandContext>
            {
                [Command("add")]
                public async Task Add([Remainder]SocketGuildUser user)
                {
                    using SqliteDbContext DbContext = new SqliteDbContext();
                    user.ToUser();

                    await ReplyAsync($":white_check_mark: User {user.Mention} was added to the JSON file.");
                }

                [Command("list"), Alias("l")]
                public async Task List()
                {
                    using SqliteDbContext DbContext = new SqliteDbContext();
                    string list = "";
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithColor(0, 255, 0);
                    embed.WithTitle("List of users in database");
                    foreach (User user in DbContext.Users)
                    {
                        list += $"{Context.Guild.GetUser(user.ID)}\n";
                    }
                    embed.WithDescription(list);

                    await ReplyAsync("", false, embed.Build());
                }
            }

            [Group("moderator"), Alias("mod")]
            public class ModeratorInfo : InteractiveBase<SocketCommandContext>
            {
                [Command("list"), Alias("l")]
                public async Task List()
                {
                    using SqliteDbContext DbContext = new SqliteDbContext();
                    string list = "";
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithColor(0, 255, 0);
                    embed.WithTitle("List of moderators in database");
                    foreach (Moderator mod in DbContext.Moderators)
                    {
                        SocketGuildUser user = Constants.IGuilds.Jordan(Context).Users.First(x => x.Id == mod.ID);
                        list += $"{user}\n";
                    }
                    embed.WithDescription(list);

                    await ReplyAsync("", false, embed.Build());
                }

                [Command("type"), Alias("t")]
                public async Task Type([Remainder]SocketGuildUser user)
                {
                    Moderator mod = user.ToModerator();
                    if (mod == null)
                    {
                        await ReplyAsync($":x: User {user.Mention} is not a moderator.");
                        return;
                    }
                    await ReplyAsync($":white_check_mark: User {user.Mention} is a type {mod.modType.GetHashCode()} moderator.");
                }
            }
        }

        [Command("giveaway", RunMode = RunMode.Async)]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Giveaway(string time, [Remainder] string prize)
        {
            int Seconds;
            int Time = int.Parse(time.Replace("d", "").Replace("h", "").Replace("m", "").Replace("s", ""));
            string remaining;
            string field;
            Random random = new Random();
            int winner;
            ulong winnerid;

            switch (time[^1])
            {
                case 'd':
                    Seconds = Time * 24 * 60 * 60;
                    break;
                case 'h':
                    Seconds = Time * 60 * 60;
                    break;
                case 'm':
                    Seconds = Time * 60;
                    break;
                case 's':
                    Seconds = Time;
                    break;
                default:
                    await ReplyAsync(":x: Please choose a correct time format.");
                    return;
            }

            if (Seconds >= 60)
            {
                if (Seconds / 60 >= 60)
                {
                    if (Seconds / 60 / 60 >= 24)
                    {
                        remaining = $"{Seconds / 60 / 60 / 24} days";
                    }
                    else remaining = $"{Seconds / 60 / 60} hours";
                }
                else remaining = $"{Seconds / 60} minutes";
            }
            else remaining = $"{Seconds} seconds";

            field = $"\n{remaining} left.";

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Giveaway: {prize}");
            embed.WithColor(Constants.IColors.Blue);
            embed.WithDescription("React with 🎉 to enter!");
            embed.AddField("Time Remaining", field);

            SocketGuild Guild = Constants.IGuilds.Jordan(Context);
            SocketTextChannel Channel = Context.Channel as SocketTextChannel;
            IUserMessage Message = await ReplyAsync("", false, embed.Build());

            await Message.AddReactionAsync(new Emoji("🎉"));

            while (true)
            {
                await Task.Delay(1000);
                Seconds--;

                if (Seconds == 0) break;

                if (Seconds % 5 == 0)
                {
                    if (Seconds >= 60)
                    {
                        if (Seconds / 60 >= 60)
                        {
                            if (Seconds / 60 / 60 >= 24)
                            {
                                remaining = $"{Seconds / 60 / 60 / 24} days";
                            }
                            else remaining = $"{Seconds / 60 / 60} hours";
                        }
                        else remaining = $"{Seconds / 60} minutes";
                    }
                    else remaining = $"{Seconds} seconds";

                    field = $"\n{remaining} left.";

                    embed.Fields.First().Value = field;

                    await Message.ModifyAsync(x => x.Embed = embed.Build());
                }
            }

            await Message.RemoveReactionAsync(new Emoji("🎉"), Context.Client.CurrentUser);

            winner = random.Next(0, Message.GetReactionUsersAsync(new Emoji("🎉"), 100).FlattenAsync().Result.Count());

            winnerid = Message.GetReactionUsersAsync(new Emoji("🎉"), 100).FlattenAsync().Result.ElementAt(winner).Id;

            embed.Fields.First().Name = "Winner";
            embed.Fields.First().Value = $"{MentionUtils.MentionUser(winnerid)}";
            embed.WithColor(0, 255, 0);

            await Message.ModifyAsync(x => x.Embed = embed.Build());

        }

        [Command("reconnect", RunMode = RunMode.Async)]
        [RequireOwner]
        [RequireBotChannel]
        public async Task Reconnect()
        {
            DiscordSocketClient client = Context.Client;
            await client.StopAsync();

            await client.LoginAsync(TokenType.Bot, ESettings.Token);
            await client.StartAsync();
        }

        [Command("modifyrule")]
        [RequireOwner]
        public async Task ModifyRule(int index, [Remainder] string text)
        {
            ITextChannel channel = Context.Guild.GetTextChannel(Data.GetChnlId("rules"));
            IUserMessage message = channel.GetMessagesAsync(1).FlattenAsync().Result.First() as IUserMessage;

            EmbedBuilder embed = message.Embeds.First().ToEmbedBuilder();
            embed.Fields[index - 1].Value = text;

            IUserMessage confirmationmessage = await ReplyAsync($"Please `confirm` this change:\n**{embed.Fields[index - 1].Name}** {embed.Fields[index - 1].Value}");
            var reply = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
            if (reply == null)
            {
                await confirmationmessage.AddReactionAsync(new Emoji("❌"));
                return;
            }
            else switch (reply.Content)
                {
                    case "confirm":
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                        break;
                    default:
                        await confirmationmessage.AddReactionAsync(new Emoji("❌"));
                        return;
                }
        }
    }
}
