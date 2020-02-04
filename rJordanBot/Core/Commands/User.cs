using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class User : InteractiveBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync(
                ":question: Commands:\n" +
                "``^help``: Displays this message.\n" +
                "``^event``: Event command system. Enter ``^event help`` for more.\n" +
                "``^socials``: Socials command system. Enter ``^socials help`` for more.\n" +
                "``^report``: DM report system. Only use in DMs."
            );
        }

        [Command("report")]
        public async Task Report()
        {
            if (ESettings.ReportBanned.Contains(Context.User.Id))
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
                    await ReplyAsync(":x: Please specify at least one user.");
                    goto messages2;
                }
                IEmote emote = new Emoji("✅");
                await (msg as IUserMessage).AddReactionAsync(emote);
            }

            //Get notes
            await ReplyAsync("Any notes you want to add? Type `none` if not.");
            string notes = "";
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
            SocketGuildChannel channel = guild.Channels.FirstOrDefault(x => x.Id == Data.Data.GetChnlId("moderation-log"));
            await (channel as SocketTextChannel).SendMessageAsync($"{guild.Roles.Where(x => x.Name == "Moderator").FirstOrDefault().Mention}", false, embed.Build());
            return;

        cancel:
            IEmote emote_ = new Emoji("❌");
            await (msg as IUserMessage).AddReactionAsync(emote_);
            return;
        }
    }
}
