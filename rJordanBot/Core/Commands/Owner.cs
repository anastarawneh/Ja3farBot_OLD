using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class Owner : InteractiveBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task Test()
        {
            if (Context.User.Id != ESettings.Owner) return;
            try
            {
                // Test code starts here.

                // Test code ends here.
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"[{DateTime.Now} at Test] {ex.ToString()}");
                Console.ResetColor();
            }

            IEmote emoji = new Emoji("✅");
            await Context.Message.AddReactionAsync(emoji);
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
            embed.WithTitle("Server updates!");
            embed.WithDescription("Good evening everyone, we hope your day has been going well so far. We have a few server updates we would like to share with you.");
            embed.AddField("Introducing the Starboard system!", $"You showed your approval of this idea, so we implemented it! The Starboard system is going to replace our channel pins system by introducing a new channel ({MentionUtils.MentionChannel(Data.Data.GetChnlId("starboard"))}), which will archive any message you deem star-worthy. To do so, all you have to do is react to a message with a minimum of 5 :star2: reactions, which will post the message there, along with a link to make it easier for you to find the original message.");
            embed.AddField("Moderator Applications!", "Many of you have been asking about our moderator applications, so here's your answer. Applications will start sometime during the next week, and will last for a full week after. As for when the results will be announced, that is going to depend on the number of applications we receive. More info will be available when the application period starts.");
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
                    break;
                default:
                    await testmsg.DeleteAsync();
                    await confirmationmessage.AddReactionAsync(new Emoji("❌"));
                    return;
            }
        }
    }
}
