using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.GeneralJSON;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace rJordanBot.Core.Commands
{
    public class Owner : InteractiveBase<SocketCommandContext>
    {
        [Command("test", RunMode = RunMode.Async)]
        public async Task Test(string time, string giveaway)
        {
            if (Context.User.Id != ESettings.Owner) return;
            try
            {
                SocketGuild Guild = Context.Guild;
                SocketTextChannel Channel = Context.Channel as SocketTextChannel;
                SocketUserMessage Message = Context.Message;
                EmbedBuilder Embed = new EmbedBuilder();
                Random Random = new Random();

                // Test code starts here.
                int seconds = 10;
                while (seconds > 0)
                {
                    await Task.Delay(1000);
                    Console.WriteLine(seconds);
                    seconds--;
                }
                // Test code ends here.

                IEmote emoji = new Emoji("✅");
                await Context.Message.AddReactionAsync(emoji);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"[{DateTime.Now} at Test] {ex.ToString()}");
                Console.ResetColor();

                IEmote emoji = new Emoji("❌");
                await Context.Message.AddReactionAsync(emoji);
            }
        }

        [Command("deldm")]
        public async Task DelDM(int msgcount)
        {
            if (Context.User.Id != ESettings.Owner) return;
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
        public async Task Ping()
        {
            if (Context.User.Id != ESettings.Owner) return;

            string ans = Environment.GetEnvironmentVariable("SystemType");

            await ReplyAsync($"[{ans.ToUpper()}] Pong!");
            Console.ForegroundColor = ConsoleColor.Green;
            string msg = $"[{DateTime.Now} at Commands] A ping in {Context.Channel} by {Context.User}";
            Console.WriteLine(msg);
            await (Context.Guild.Channels.Where(x => x.Id == 642475027123404811).FirstOrDefault() as SocketTextChannel).SendMessageAsync(msg);
            Console.ResetColor();
        }

        [Command("resetchannels"), Alias("rc")]
        public async Task ResetChannels()
        {
            if (Context.User.Id != ESettings.Owner && Context.User.Id != Context.Client.CurrentUser.Id) return;
            await Data.Data.ResetChannels(Context);

            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }

        [Group("roleembed"), Alias("re")]
        public class RoleEmbed : InteractiveBase<SocketCommandContext>
        {
            [Command("create")]
            public async Task Create()
            {
                if (Context.User.Id != ESettings.Owner) return;

                EmbedBuilder embed = new EmbedBuilder();
                SocketTextChannel channel = Context.Guild.Channels.FirstOrDefault(x => x.Id == Data.Data.GetChnlId("role-selection")) as SocketTextChannel;

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
                if (Context.User.Id != ESettings.Owner) return;

                RoleSetting roleSetting = Data.Data.GetRoleSetting(role);
                SocketTextChannel channel = Context.Guild.Channels.Where(x => x.Id == Data.Data.GetChnlId("role-selection")).FirstOrDefault() as SocketTextChannel;
                IUserMessage msg = channel.GetMessageAsync(roleSetting.id).Result as IUserMessage;
                Embed embed = msg.Embeds.FirstOrDefault() as Embed;
                EmbedBuilder embedBuilder = embed.ToEmbedBuilder();
                if (embed.Fields.FirstOrDefault().Name == $"There are no {roleSetting.group.ToLower()} roles available at the moment.")
                {
                    //Message is empty
                    EmbedBuilder emptybuilder = new EmbedBuilder();
                    embedBuilder.Fields = emptybuilder.Fields;
                }

                string rolename = Context.Guild.Roles.Where(x => x.Id == roleSetting.roleid).FirstOrDefault().Name;
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
                emote = new Emoji(Data.Data.GetRoleSetting(role).emoji);
                await msg.AddReactionAsync(emote);
            }

            [Command("unload"), Alias("ul")]
            public async Task Unload(string role)
            {
                if (Context.User.Id != ESettings.Owner) return;

                RoleSetting roleSetting = Data.Data.GetRoleSetting(role);
                SocketTextChannel channel = Context.Guild.Channels.Where(x => x.Id == Data.Data.GetChnlId("role-selection")).FirstOrDefault() as SocketTextChannel;
                IUserMessage msg = channel.GetMessageAsync(roleSetting.id).Result as IUserMessage;
                Embed embed = msg.Embeds.FirstOrDefault() as Embed;

                if (embed.Fields.FirstOrDefault().Name == $"There are no {roleSetting.group.ToLower()} roles available at the moment.")
                {
                    //Message is empty
                    await ReplyAsync(":x: Role is not loaded.");
                }

                string rolename = Context.Guild.Roles.Where(x => x.Id == roleSetting.roleid).FirstOrDefault().Name;
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
                        emote = new Emoji(Data.Data.GetRoleSetting(role).emoji);
                        await msg.RemoveReactionAsync(emote, Context.Client.CurrentUser as IUser);
                        return;
                    }
                }
            }

            [Command("ban")]
            public async Task Ban(SocketGuildUser user)
            {
                if (Context.User.Id != ESettings.Owner) return;

                SocketGuild guild = user.Guild;
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
                if (Context.User.Id != ESettings.Owner) return;

                SocketGuild guild = user.Guild;
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

        [Command("stop")]
        public async Task Stop()
        {
            if (Context.User.Id != ESettings.Owner) return;
            SocketTextChannel channel = Context.Guild.Channels.FirstOrDefault(x => x.Id == Data.Data.GetChnlId("bot-log")) as SocketTextChannel;
            await channel.SendMessageAsync($"[{DateTime.Now} at Commands] Stopping [{Environment.GetEnvironmentVariable("SystemType").ToUpper()}] instance...");
            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
            Environment.Exit(1);
        }

        [Command("setgame"), Alias("sg")]
        public async Task SetGame(string state, [Remainder]string game = null)
        {
            if (Context.User.Id != ESettings.Owner) return;
            ActivityType activity = new ActivityType();
            switch (state)
            {
                case "-l":
                    activity = ActivityType.Listening;
                    break;
                default:
                case "-p":
                    activity = ActivityType.Playing;
                    break;
                case "-s":
                    activity = ActivityType.Streaming;
                    break;
                case "-q":
                    activity = ActivityType.Watching;
                    break;
            }
            await Context.Client.SetGameAsync(game, null, activity);

            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }

        [Command("announce")]
        public async Task Announce()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("The new Mod Team!");
            embed.WithDescription($"Good evening everyone. We are proud to announce the new moderation team, {Context.Guild.Users.First(x => x.Id == 321275495218020362).Mention} and {Context.Guild.Users.First(x => x.Id == 252526202982498305).Mention}! Let's welcome them with a warm round of applause! :clap:\nThank you to everyone else who applied, there will be many opportunities for you in the future to try again.");
            embed.WithFooter("Please leave any feedback about this update in #feedback.");
            embed.WithColor(114, 137, 218);

            IUserMessage testmsg = await ReplyAsync("", false, embed.Build());
            IUserMessage confirmationmessage = await ReplyAsync($"Please type `confirm` within 30 seconds to send this message to <#{Data.Data.GetChnlId("announcements")}>.");

            var reply = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
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
                        SocketTextChannel AnnouncementChannel = Context.Guild.Channels.FirstOrDefault(x => x.Id == Data.Data.GetChnlId("announcements")) as SocketTextChannel;
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
        public async Task Say(SocketTextChannel channel, [Remainder] string message)
        {
            if (Context.User.Id != ESettings.Owner) return;
            
            await channel.SendMessageAsync(message);
            await Context.Message.DeleteAsync();
        }

        [Group("json")]
        public class JSON : InteractiveBase<SocketCommandContext>
        {
            [Group("user")]
            public class User : InteractiveBase<SocketCommandContext>
            {
                [Command("add")]
                public async Task Add(SocketGuildUser user)
                {
                    await GeneralJson.AddUser(user);

                    await ReplyAsync($":white_check_mark: User {user.Mention} was added to the JSON file.");
                }

                [Command("remove")]
                public async Task Remove(SocketGuildUser user)
                {
                    await GeneralJson.RemoveUser(user);

                    await ReplyAsync($":white_check_mark: User {user.Mention} was removed from the JSON file.");
                }

                [Command("list"), Alias("l")]
                public async Task List()
                {
                    string list = "";
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithColor(0, 255, 0);
                    embed.WithTitle("List of users in JSON file");
                    foreach (Resources.GeneralJSON.User user in GeneralJson.users)
                    {
                        list += $"{user.Username}#{user.Discriminator}\n";
                    }
                    embed.WithDescription(list);

                    await ReplyAsync("", false, embed.Build());
                }
            }
        }
    }
}
