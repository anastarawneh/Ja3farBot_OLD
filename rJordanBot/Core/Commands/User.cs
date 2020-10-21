using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Core.Preconditions;
using rJordanBot.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class User_cmd : InteractiveBase<SocketCommandContext>
    {
        [Command("help")]
        [RequireBotChannel]
        public async Task Help()
        {
            await ReplyAsync(
                ":question: Commands:\n" +
                "``^help``: Displays this message.\n" +
                "``^event``: Event command system. Enter ``^event help`` for more.\n" +
                "``^socials``: Socials command system. Enter ``^socials help`` for more.\n" +
                "``^report``: DM user report system. Only use in DMs.\n" +
                "``^suggest <suggestion>``: Submit a suggestion."/* +
                "``^music``: Music command system."*/
            );
        }

        [Command("report")]
        public async Task Report()
        {
            if (Config.ReportBanned.Contains(Context.User.Id))
            {
                await ReplyAsync(":x: You have been banned from using the report system.");
                return;
            }
            if (!(Context.Channel is IDMChannel))
            {
                await ReplyAsync(":x: Please only use the report system in DMs.");
                return;
            }

            await ReplyAsync(":exclamation: Welcome to the report system. We're sorry for the problem you are dealing with. You can reply with `cancel` at any time to cancel your report.");
            await ReplyAsync(":exclamation: NOTE: Misusing or spamming the report system will get you banned from using it.");
        //Ask if anonymous
        anonymous:
            await ReplyAsync("Do you wish to remain anonymous? Your User ID will be recorded regardless for security reasons. (`Y`/`N`)");
            SocketMessage msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
            if (msg.Content == "cancel") goto cancel;
            bool anon = false;
            switch (msg.Content.ToLower())
            {
                case "y":
                    anon = true;
                    break;
                case "n":
                    anon = false;
                    break;
                default:
                    await ReplyAsync(":x: I don't understand. Please answer with `Y` or `N`.");
                    goto anonymous;
            }

            //Get user(s) reported
            await ReplyAsync("Please specify the user(s) reported, each in one message. You can enter usernames, but IDs are preferred.");
            await ReplyAsync("Please reply with `done` after all usernames have been entered.");
            List<string> userlist = new List<string>();
            while (true)
            {
            user2:
                msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                if (msg == null) goto user2;
                if (msg.Content == "cancel") goto cancel;
                if (msg.Content != "done") userlist.Add(msg.Content);
                else if (userlist.Count() != 0) break;
                else
                {
                    await ReplyAsync(":x: Please specify at least one user.");
                    goto user2;
                }
                IEmote emote = new Emoji("✅");
                await (msg as IUserMessage).AddReactionAsync(emote);
            }

            //Get messages
            await ReplyAsync("Please specify any offending messages, each in one message. Please use message links, or IDs.");
            await ReplyAsync("Please reply with `done` after all messages have been entered.");
            List<string> msglist = new List<string>();
            while (true)
            {
            messages2:
                msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                if (msg == null) goto messages2;
                if (msg.Content == "cancel") goto cancel;
                if (msg.Content != "done") msglist.Add(msg.Content);
                else if (msglist.Count() != 0) break;
                else
                {
                    await ReplyAsync(":x: Please specify at least one message.");
                    goto messages2;
                }
                IEmote emote = new Emoji("✅");
                await (msg as IUserMessage).AddReactionAsync(emote);
            }

            //Get notes
            await ReplyAsync("Any notes you want to add? Type `none` if not.");
            string notes = "N/A";
        notes2:
            msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
            if (msg == null) goto notes2;
            if (msg.Content == "cancel") goto cancel;
            if (msg.Content.ToLower() != "none")
            {
                notes = msg.Content;
                IEmote emote = new Emoji("✅");
                await (msg as IUserMessage).AddReactionAsync(emote);
            }

            //Finish up
            await ReplyAsync(":white_check_mark: Thank you for your report. We'll look into your case as soon as possible.");

            //Prep embed
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Report");
            switch (anon)
            {
                case true:
                    embed.WithDescription("User: `anonymous`.");
                    break;
                case false:
                    embed.WithDescription($"User: `{Context.User.Username}`");
                    break;
            }
            foreach (string user in userlist)
            {
                embed.AddField("Offending user", user);
            }
            foreach (string message in msglist)
            {
                embed.AddField("Message", message);
            }
            embed.AddField("Notes", notes);
            embed.WithFooter($"{DateTime.Now} | User ID: {Context.User.Id}");

            SocketGuild guild = Context.Client.Guilds.FirstOrDefault();
            SocketGuildChannel channel = guild.Channels.FirstOrDefault(x => x.Id == Methods.Data.GetChnlId("moderation-log"));
            await (channel as SocketTextChannel).SendMessageAsync($"{guild.Roles.Where(x => x.Name == "Moderator").FirstOrDefault().Mention}", false, embed.Build());
            return;

        cancel:
            IEmote emote_ = new Emoji("❌");
            await (msg as IUserMessage).AddReactionAsync(emote_);
            return;
        }

        [Command("apply")]
        public async Task Apply()
        {
            if (Config.ModAppsActive == false)
            {
                await ReplyAsync(":x: Thank you, but we closed moderator applications for now.");
                return;
            }

            if (Context.User.IsBot) return;
            if (!(Context.Channel is IDMChannel))
            {
                await ReplyAsync(":x: To apply for the moderator position, please use this command in a DM channel with the bot.");
                return;
            }
            if (Context.Message.Timestamp.AddHours(2).Day == 17) return;

            SocketGuild guild = Context.Client.Guilds.First();
            SocketTextChannel moderationchannel = (SocketTextChannel)guild.Channels.First(x => x.Id == Methods.Data.GetChnlId("moderation-log"));

            EmbedBuilder moderationembed = new EmbedBuilder();
            moderationembed.WithAuthor(Context.User);
            moderationembed.WithColor(114, 137, 218);
            moderationembed.WithTitle("Moderation Application");
            moderationembed.WithFooter($"Started at {DateTime.Now.ToString("h:mm")}");

            RestUserMessage moderationmsg = await moderationchannel.SendMessageAsync("", false, moderationembed.Build());

            await ReplyAsync("Thank you for applying to moderate the r/Jordan Discord! We are going to ask you a few questions to make sure you are fit for the job. You can reply with `cancel` at any time to cancel the application. Waiting for more than 5 minutes to answer a question will cancel the application. Troll applications are not tolerated.");
            IUserMessage response;

            // Questions
            {
                await ReplyAsync("First of all, how old are you?");
                response = (IUserMessage)await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                if (response == null || response.Content.ToLower() == "cancel")
                {
                    goto cancel;
                }
                moderationembed.AddField("First of all, how old are you?", response.Content);
                await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());

                await ReplyAsync("What is the timezone of your current place of residence?");
                response = (IUserMessage)await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                if (response == null || response.Content.ToLower() == "cancel")
                {
                    goto cancel;
                }
                moderationembed.AddField("What is the timezone of your current place of residence?", response.Content);
                await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());

                await ReplyAsync("Tell us a little about yourself.");
                response = (IUserMessage)await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                if (response == null || response.Content.ToLower() == "cancel")
                {
                    goto cancel;
                }
                moderationembed.AddField("Tell us a little about yourself.", response.Content);
                await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());

                await ReplyAsync("How many hours a day do you average on Discord?");
                response = (IUserMessage)await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                if (response == null || response.Content.ToLower() == "cancel")
                {
                    goto cancel;
                }
                moderationembed.AddField("How many hours a day do you average on Discord?", response.Content);
                await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());

                await ReplyAsync("What do you think you could bring to the server as a moderator?");
                response = (IUserMessage)await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                if (response == null || response.Content.ToLower() == "cancel")
                {
                    goto cancel;
                }
                moderationembed.AddField("What do you think you could bring to the server as a moderator?", response.Content);
                await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());

                await ReplyAsync("Do you have any previous moderation experiences on or off Discord?");
                response = (IUserMessage)await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                if (response == null || response.Content.ToLower() == "cancel")
                {
                    goto cancel;
                }
                moderationembed.AddField("Do you have any previous moderation experiences on or off Discord?", response.Content);
                await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());
            }

            await ReplyAsync("Your application has been recorded. Thank you for your time.");
            moderationembed.WithFooter(moderationembed.Footer.Text + $" | Finished at {DateTime.Now.ToString("h:mm")}");
            await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());
            return;

        cancel:
            IEmote x = new Emoji("❌");
            if (response != null) await response.AddReactionAsync(x);
            else await ReplyAndDeleteAsync("Moderation application canceled.", false, null, TimeSpan.FromSeconds(30));

            moderationembed.WithFooter(moderationembed.Footer.Text + $" | Canceled at {DateTime.Now.ToString("h:mm")}");
            await moderationmsg.ModifyAsync(x => x.Embed = moderationembed.Build());
            return;
        }

        [Command("music"), Alias("music help")]
        [RequireBotChannel]
        public async Task MusicHelp()
        {
            await ReplyAsync(
                ":question: Music commands:\n" +
                "``^play``: Plays a track from a YouTube search query or link, or adds it to the queue. ``^play <query>``\n" +
                "``^pause``: Pauses or unpauses the current track.\n" +
                "``^stop``: Stops and clears the current queue.\n" +
                "``^skip``: Skips to the next track in the queue.\n" +
                "``^join``: Joins the voice channel the user is in.\n" +
                "``^dc``: Disconnects from the voice channel it's in.\n" +
                "``^loop``: Loops the current track.\n" +
                "``^queue``: Views the current queue."
            );
        }

        [Command("covid")]
        [RequireBotChannel]
        public async Task CovidStats()
        {
            ConfigFile.COVID stats = Config.Covid;
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"COVID-19 stats for {DateTime.Today.ToString("dd/MM/yyyy")}");
            embed.WithColor(Constants.IColors.Blurple);
            embed.WithDescription($"{stats.locals} new local cases, {stats.casualties} casualties and {stats.recoveries} recoveries.");
            embed.AddField("Amman", stats.amman, true);
            embed.AddField("Irbid", stats.irbid, true);
            embed.AddField("Zarqa", stats.zarqa, true);
            embed.AddField("Mafraq", stats.mafraq, true);
            embed.AddField("Ajloun", stats.ajloun, true);
            embed.AddField("Jerash", stats.jerash, true);
            embed.AddField("Madaba", stats.madaba, true);
            embed.AddField("Balqa", stats.balqa, true);
            embed.AddField("Karak", stats.karak, true);
            embed.AddField("Tafileh", stats.tafileh, true);
            embed.AddField("Ma'an", stats.maan, true);
            embed.AddField("Aqaba", stats.aqaba, true);
            embed.WithFooter($"Total cases: {stats.totalcases} - Total casualties: {stats.totalcasualties}");
            await ReplyAsync("", false, embed.Build());

            if (Context.Channel.Name == "news")
            {
                await Context.Message.DeleteAsync();
            }
        }
    }
}
