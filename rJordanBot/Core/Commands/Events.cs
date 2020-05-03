using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class Events : InteractiveBase
    {
        [Group("event"), Alias("events")]
        public class EventsGroup : InteractiveBase<SocketCommandContext>
        {
            [Command, Alias("help")]
            public async Task Help()
            {
                await Context.Channel.SendMessageAsync(
                    ":question: Event commands:\n" +
                    "``^event help``: Displays this message.\n" +
                    "``^event create``: Creates an event.\n" +
                    "``^event edit``: Edits an event detail. Make sure to use this syntax without <>: ``^event edit <ID> <Section> <New Message>``\n" +
                    "``^event delete``: Deletes an event. Make sure to use this syntax without <>: ``^event delete <ID> confirm``\n" +
                    "``^event verify``: Initiates the Event Verification System. Only use in DMs.\n" +
                    "``^event tos``: Displays the Event Terms of Service."
                );
            }

            [Command("create", RunMode = RunMode.Async)]
            public async Task Create()
            {
                //Variables
                string title;
                string time;
                string location;
                string notes;
                int id;
                string user;

                if (ESettings.EventsActive == false)
                {
                    await ReplyAsync(":x: The event system is not available right now.");
                    return;
                }

                if (!(Context.User as SocketGuildUser).ToUser().Verified)
                {
                    await ReplyAsync(":x: You need to be verified to be able to use the Events system.");
                    return;
                }

                //Information entering
                await ReplyAsync("Please enter an event title.");
                SocketMessage title_ = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                title = title_.Content;
                if (title == "cancel") goto cancel;

                await ReplyAsync("Please enter the date and time for the event.");
                SocketMessage time_ = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                time = time_.Content;
                if (time == "cancel") goto cancel;

                await ReplyAsync("Please enter the event loaction.");
                SocketMessage location_ = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                location = location_.Content;
                if (location == "cancel") goto cancel;

                await ReplyAsync("Please enter any additional notes. Type `none` to ignore.");
                SocketMessage notes_ = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                notes = notes_.Content;
                if (notes == "cancel") goto cancel;

                Random rnd = new Random();
                id = rnd.Next(0, 100000);

                //Embed message creation
                user = (Context.User.Username);
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor(Context.User.Username + "#" + Context.User.Discriminator, Context.User.GetAvatarUrl());
                embed.WithColor(114, 137, 218);
                embed.WithFooter($"React if you're going! | ID: {id}");
                embed.WithTitle($"{user} is hosting an event!");
                embed.AddField("Event", title);
                embed.AddField("Time", time);
                embed.AddField("Location", location);
                if (notes != "none")
                {
                    embed.AddField("Notes", notes);
                }

                //Saving
                SocketTextChannel channel = Constants.IGuilds.Jordan(Context).Channels.Where(x => x.Id == Data.Data.GetChnlId("events")).FirstOrDefault() as SocketTextChannel;
                IEmote emote = new Emoji("✅");
                SocketRole role = Constants.IGuilds.Jordan(Context).Roles.First(x => x.Id == 644974763692785664);
                RestUserMessage msg_ = await channel.SendMessageAsync($"{role.Mention}", false, embed.Build());
                await msg_.AddReactionAsync(emote);
                await Context.Channel.SendMessageAsync($":white_check_mark: Your event has been posted in {channel.Mention} with ID {id}.");
                return;

            cancel:
                await ReplyAndDeleteAsync(":white_check_mark: Canceled.");
            }

            [Command("edit")]
            public async Task Edit(int id = 0, string section = "empty", [Remainder]string message = "none")
            {
                if (ESettings.EventsActive == false)
                {
                    await ReplyAsync(":x: The event system is not available right now.");
                    return;
                }

                if (id == 0 || id >= 100000)
                {
                    await Context.Channel.SendMessageAsync(":x: Please provide a valid ID.");
                    return;
                }

                if (section == "empty" || !(section.ToLower() == "event" || section.ToLower() == "time" || section.ToLower() == "location" || section.ToLower() == "notes"))
                {
                    await ReplyAsync(":x: Please provide a valid section.");
                    return;
                }

                ulong eventschnlid = Data.Data.GetChnlId("events");
                SocketTextChannel eventschnl = Constants.IGuilds.Jordan(Context).Channels.Where(x => x.Id == eventschnlid).FirstOrDefault() as SocketTextChannel;
                IEnumerable<IMessage> msgs = await eventschnl.GetMessagesAsync(100).FlattenAsync();
                foreach (IMessage msg in msgs)
                {
                    IEnumerable<IEmbed> embeds = msg.Embeds;
                    foreach (IEmbed embed in embeds)
                    {
                        if (embed.Footer.ToString().Contains($"ID: {id}") && embed.Author.ToString().Contains(Context.User.Username + "#" + Context.User.Discriminator))
                        {
                            ulong id_ = msg.Id;
                            EmbedBuilder embedB = embed.ToEmbedBuilder();

                            switch (section.ToLower())
                            {
                                default:
                                    await ReplyAsync(":x: Please provide a valid section.");
                                    return;

                                case "event":
                                    embedB.Fields.Where(x => x.Name == "Event").FirstOrDefault().Value = message;
                                    break;

                                case "time":
                                    embedB.Fields.Where(x => x.Name == "Time").FirstOrDefault().Value = message;
                                    break;

                                case "location":
                                    embedB.Fields.Where(x => x.Name == "Location").FirstOrDefault().Value = message;
                                    break;

                                case "notes":
                                    if (embedB.Fields.Count == 3)
                                    {
                                        //If Notes are not a thing
                                        embedB.AddField("Notes", message);
                                    }
                                    embedB.Fields.Where(x => x.Name == "Notes").FirstOrDefault().Value = message;
                                    break;
                            }

                            await (msg as IUserMessage).ModifyAsync(m => m.Embed = embedB.Build());
                            await ReplyAsync(":white_check_mark: Edited your event.");
                            return;
                        }
                    }
                }
                await Context.Channel.SendMessageAsync(":x: Could not find your event within the last 5 events.");
            }

            [Command("delete")]
            public async Task Delete(int id = 0, [Remainder]string confirmation = null)
            {
                //Checks

                if (ESettings.EventsActive == false)
                {
                    await ReplyAsync(":x: The event system is not available right now.");
                    return;
                }

                if (id == 0 || id >= 100000)
                {
                    await Context.Channel.SendMessageAsync(":x: Please provide a valid ID.");
                    return;
                }
                if (confirmation != "confirm")
                {
                    await Context.Channel.SendMessageAsync(":x: Please type confirm after the ID.");
                    return;
                }

                SocketTextChannel eventschnl = Constants.IGuilds.Jordan(Context).Channels.Where(x => x.Id == Data.Data.GetChnlId("events")).FirstOrDefault() as SocketTextChannel;
                IEnumerable<IMessage> msgs = await eventschnl.GetMessagesAsync(5).FlattenAsync();
                foreach (IMessage msg in msgs)
                {
                    IEnumerable<IEmbed> embeds = msg.Embeds;
                    foreach (IEmbed embed in embeds)
                    {
                        if (embed.Footer.ToString().Contains($"ID: {id}") && embed.Author.ToString().Contains(Context.User.Username + "#" + Context.User.Discriminator))
                        {
                            //Execution

                            await msg.DeleteAsync();
                            await Context.Channel.SendMessageAsync(":white_check_mark: Deleted your event.");
                            return;
                        }
                    }
                }
                await Context.Channel.SendMessageAsync(":x: Could not find your event within the last 5 events.");
            }

            [Command("verify")]
            public async Task Verify()
            {
                if (!(Context.Channel is IDMChannel))
                {
                    await ReplyAsync(":x: For your own privacy, please use the event verification system in DMs.");
                    return;
                }
                if ((Context.User as SocketGuildUser).ToUser().Verified)
                {
                    await ReplyAsync(":x: You are already verified.");
                    return;
                }

                SocketTextChannel log = Context.Client.Guilds.FirstOrDefault().Channels.FirstOrDefault(x => x.Id == Data.Data.GetChnlId("bot-log")) as SocketTextChannel;
                string logmsg = $"[{DateTime.Now} at EVS] {Context.User.Mention} has started verification.";
                await log.SendMessageAsync(logmsg);
                Console.WriteLine(logmsg);

                await ReplyAsync(":exclamation: Welcome to the event verification system. Please type `proceed` to proceed. You can reply with `cancel` at any time to cancel the process.");
                SocketMessage msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                if (msg.Content.ToLower() == "cancel") goto cancel;
                if (msg.Content.ToLower() != "proceed")
                {
                    await ReplyAsync(":x: Verification canceled.");
                    return;
                }

                //Get photo
                await ReplyAsync(":exclamation: Please attach a photo of legal proof that you are of age 16 or older. You may censor anything else.");
                await ReplyAsync(":exclamation: We do not save any data sent through the event verification system. After verification, all pictures sent will be deleted.");
            photo:
                msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));
                if (msg.Content.ToLower() == "cancel") goto cancel;
                else if (msg.Attachments.Count < 1)
                {
                    await ReplyAsync(":x: Please attach a photo of legal proof that you are of age 16 or older. You may censor anything else.");
                    goto photo;
                }
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("Event Verification Request");
                embed.WithColor(255, 255, 0);
                embed.WithDescription($"User: {Context.User}");
                embed.WithImageUrl(msg.Attachments.FirstOrDefault().Url);

                SocketGuild guild = Constants.IGuilds.Jordan(Context);
                SocketTextChannel channel = guild.Channels.FirstOrDefault(x => x.Id == Data.Data.GetChnlId("moderation-log")) as SocketTextChannel;
                await channel.SendMessageAsync("", false, embed.Build());

                //Finish
                await ReplyAsync(":white_exclamation_mark: Thank you. You'll get a message shortly with your verification status.");
                await Context.Channel.DeleteMessageAsync(msg.Id);

                logmsg = $"[{DateTime.Now} at EVS] {Context.User.Mention} has finished verification.";
                await log.SendMessageAsync(logmsg);
                Console.WriteLine(logmsg);

                return;

            cancel:
                IEmote emote_ = new Emoji("❌");
                await (msg as IUserMessage).AddReactionAsync(emote_);

                logmsg = $"[{DateTime.Now} at EVS] {Context.User.Mention} has canceled verification.";
                await log.SendMessageAsync(logmsg);
                Console.WriteLine(logmsg);
                return;
            }

            [Command("tos"), Alias("terms")]
            public async Task TOS()
            {
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("Events Terms of Service");
                embed.WithDescription("You must be of **16 years of age or older** to participate in events held outside this Discord server. You must also be of **18 years of age or older** to participate in events that are marked 18+. You must provide legal proof of age through the Event Verification System in order to use events. The server administration (Owner and Moderators) will not be responsible for anything that goes on during the event. The server administration (Owner and Moderators) will not be responsible for anyone who doesn’t comply with the Terms above.");
                embed.WithFooter("When you use the event system, you agree to the Terms mentioned above.");
                await ReplyAsync("", false, embed.Build());
            }
        }
    }
}
