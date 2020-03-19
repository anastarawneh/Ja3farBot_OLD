using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Core.Data;
using rJordanBot.Core.Methods;
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
        public async Task Test()
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
                SocketTextChannel RulesChannel = Guild.Channels.First(x => x.Id == Data.Data.GetChnlId("rules")) as SocketTextChannel;

                Embed.WithTitle("Server Rules");
                Embed.AddField("1. Be civil.", "Treat people the way you would like to be treated. Let's show our fellow neighbors a good image of Jordan.");
                Embed.AddField("2. No offensive content.", "This includes, but is not limited to: Offensive slurs, hate speech of any kind, NSFW content outside of its designated channel, doxxing, and targeting, bullying or harassing fellow users. This rule is going to be enforced a lot more than before.");
                Embed.AddField("3. No spamming.", "This includes, but is not limited to: Excessive use of emotes in text or in reactions, excessive mentions of moderators or users in general, and general spamming in text channels.");
                Embed.AddField("4. No self promotion.", "This includes, but is not limited to: advertisements of any links outside of the designated social media sharing channel, and unsolicited advertising in direct messages.");
                Embed.AddField("5. Language guidelines.", "Unless appropriate in context of a general conversation, this server communicates in English and Arabic only.");
                Embed.AddField("6. Channel topics.", "Each channel has info on how to use it in its topic, and you are expected to post your content to the appropriate channels.");
                Embed.AddField("7. Common sense.", "This means that trying to use loopholes to get out of abiding by these rules is also not allowed. In addition, moderators can and will take any action at their own discretion, in a way that benefits the server, even if said content does not \"technically\" break the server rules.");
                Embed.AddField("8. Illegal content.", "We as a community are expected to follow the law of the country, so discussion of illegal content is not allowed.");
                Embed.AddField("9. Direct Messages", "These rules also apply to direct messages including members of this server, as long as all parties involved are in explicit agreement otherwise.");
                Embed.AddField("10. Discord's Terms of Service.", "We're using the Discord platform to host this community, and we are expected to abide by its ToS. That means that you have to be over the age of 13 to use the service, and you are not allowed to use client modifications.");

                Embed.WithFooter("These rules are not up for debate. Failing to comply with them, or arguing a moderator's judgement, will lead to punitive action.\nNote: Edited and deleted messages are logged for security reasons.");

                Embed.WithColor(Constants.Colors.Blurple);

                //await RulesChannel.SendMessageAsync("", false, Embed.Build());
                RestUserMessage NewMessage = (RestUserMessage)RulesChannel.GetMessagesAsync(1).FlattenAsync().Result.First();
                await NewMessage.ModifyAsync(x => x.Embed = Embed.Build());
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
        public async Task Announce()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("We're rebranding!");
            embed.WithDescription($"Good evening everyone. It's been long overdue, but as of today, this Discord server and the r/jordan subreddit are going in separate ways. We would like to thank the subreddit team for allowing this community to grow as it did, and we're looking forward to what the future holds for this server. There will be some slight changes in {MentionUtils.MentionChannel(Data.Data.GetChnlId("welcome"))} and {MentionUtils.MentionChannel(Data.Data.GetChnlId("rules"))}, so please make sure to reread them to stay up to date on our guidelines.");
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

        [Command("giveaway", RunMode = RunMode.Async)]
        public async Task Giveaway(string time, [Remainder] string prize)
        {
            if (Context.User.Id != ESettings.Owner) return;
            try
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
                embed.WithColor(Constants.Colors.Blurple);
                embed.WithDescription("React with 🎉 to enter!");
                embed.AddField("Time Remaining", field);

                SocketGuild Guild = Context.Guild;
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
