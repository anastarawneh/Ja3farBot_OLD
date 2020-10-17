using Discord;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.MySQL;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Methods
{
    public class EventHandlers
    {
        private readonly DiscordSocketClient _client;
        public EventHandlers(DiscordSocketClient client) => _client = client;

        public Task Initialize()
        {
            _client.ReactionAdded += Roles_ReactionAdded;
            _client.ReactionAdded += Events_ReactionAdded;
            _client.UserJoined += Invites_UserJoined;
            _client.ReactionAdded += Starboard_ReactionAddedOrRemoved;
            _client.ReactionRemoved += Starboard_ReactionAddedOrRemoved;
            _client.UserLeft += JSON_UserLeft;
            _client.MessageReceived += InviteDeletion;
            _client.Ready += MuteFixing;
            //_client.UserJoined += JoinVerification;
            _client.GuildMemberUpdated += Greeting_GuildMemberUpdated;

            return Task.CompletedTask;
        }


        public async Task Roles_ReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IDMChannel) return;

            SocketGuildUser user = Constants.IGuilds.Jordan(_client).GetUser(reaction.UserId);
            
            SocketTextChannel channel_ = channel as SocketTextChannel;
            SocketGuild guild = channel_.Guild;
            if (channel_.Id == Data.GetChnlId("role-selection") && !user.IsBot)
            {
                RoleSetting role = new RoleSetting();
                role = Data.GetEmojiRoleSetting(reaction.Emote.Name);

                IMessage msg = channel_.GetMessageAsync(role.id).Result;
                IUserMessage msg_ = msg as IUserMessage;
                await msg_.RemoveReactionAsync(reaction.Emote, user);

                foreach (SocketRole role_ in user.Roles)
                {
                    if (role_.Id == role.roleid)
                    {
                        //User has the role
                        //=> remove the role
                        await user.RemoveRoleAsync(role_);
                        IDMChannel dm_ = await user.GetOrCreateDMChannelAsync();
                        await dm_.SendMessageAsync($"You have been removed from the {role_.Name} role.");
                        return;
                    }
                }
                //User does not have the role
                //=> add the role
                SocketRole role__ = guild.Roles.Where(x => x.Id == role.roleid).FirstOrDefault();
                await user.AddRoleAsync(role__);
                IDMChannel dm = await user.GetOrCreateDMChannelAsync();
                await dm.SendMessageAsync($"You have been added to the {role__.Name} role.");
            }
        }

        public async Task Events_ReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IDMChannel) return;
            if ((channel as SocketTextChannel).Name != "events") return;

            SocketGuildUser user = Constants.IGuilds.Jordan(_client).GetUser(reaction.UserId);

            if (user.IsBot) return;
            IEmbed embed = cacheable.GetOrDownloadAsync().Result.Embeds.First();
            if (embed.Fields.First(x => x.Name == "Location").Value == "Discord") return;

            if (user.ToUser().EventVerified)
            {
                // User is allowed.
                return;
            }
            /*else if (eVerified.Denied.Contains(reaction.User.Value.Id))
            {
                // User is denied.

                IMessage msg = (channel as SocketTextChannel).GetMessageAsync(cacheable.Id).Result;
                await (msg as IUserMessage).RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                IDMChannel dm = reaction.User.Value.GetOrCreateDMChannelAsync().Result;
                await dm.SendMessageAsync(":x: You are denied from using the events system.");
                return;
            }*/
            else
            {
                // User is not verified.

                IMessage msg = (channel as SocketTextChannel).GetMessageAsync(cacheable.Id).Result;
                await (msg as IUserMessage).RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                IDMChannel dm = user.GetOrCreateDMChannelAsync().Result;
                await dm.SendMessageAsync(":x: Please verify your age using the Event Verification System before using events.");
                return;
            }
        }

        public async Task Invites_UserJoined(SocketGuildUser user)
        {
            try
            {
                await Data.CompareInvites(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task Starboard_ReactionAddedOrRemoved(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name != "🌟") return;
            if (channel.Id == Data.GetChnlId("starboard")) return;

            List<ulong> blacklist = new List<ulong>
            {
                Data.GetChnlId("welcome"),
                Data.GetChnlId("rules"),
                Data.GetChnlId("announcements"),
                Data.GetChnlId("role-selection"),
                Data.GetChnlId("suggestions"),
                Data.GetChnlId("events"),
                Data.GetChnlId("starboard")
            };

            if (blacklist.Contains(channel.Id)) return;

            IUserMessage message = cacheable.GetOrDownloadAsync().Result;

            StarboardMessage starboardmessage = new StarboardMessage
            {
                message = message,
                channel = channel as SocketTextChannel,
                author = message.Author,
                stars = message.Reactions.FirstOrDefault(x => x.Key.Name == "🌟").Value.ReactionCount
            };

            if (message.Attachments.Count > 1 || message.Author == reaction.User.Value)
            {
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                return;
            }

            await Data.UpdateStarboard(starboardmessage);
        }

        public async Task JSON_UserLeft(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            SocketTextChannel channel = guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("bot-log")) as SocketTextChannel;

            /*if (eVerified.Allowed.Contains(user.Id))
            {
                eVerified.Allowed.Remove(user.Id);
                Data.UpdateVerified();

                await channel.SendMessageAsync($"[{DateTime.Now} at Events] {user.Mention} was removed from the event allowed list for leaving.");
            }
            else if (eVerified.Denied.Contains(user.Id))
            {
                eVerified.Denied.Remove(user.Id); ;
                Data.UpdateVerified();

                await channel.SendMessageAsync($"[{DateTime.Now} at Events] {user.Mention} was removed from the event denied list for leaving.");
            }*/

            foreach (User item in UserFunctions.List())
            {
                if (item.ID == user.Id)
                {
                    await item.Delete();
                }
            }
        }

        public async Task InviteDeletion(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            foreach (string whitelist in ESettings.InviteWhitelist)
            {
                if (message.Content.Contains(whitelist)) return;
            }
            if (message.Content.Contains("discord.gg") || message.Content.Contains("discordapp.com"))
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync(":x: Please don't send Discord server invites in this server.");
            }
        }

        public async Task MuteFixing()
        {
            _client.Ready -= MuteFixing;

            SocketGuild guild = Constants.IGuilds.Jordan(_client);
            SocketTextChannel botlog = guild.Channels.First(x => x.Id == Data.GetChnlId("bot-log")) as SocketTextChannel;
            SocketTextChannel modlog = guild.Channels.First(x => x.Id == Data.GetChnlId("moderation-log")) as SocketTextChannel;
            IEnumerable<IMessage> messages = modlog.GetMessagesAsync(20).FlattenAsync().Result;

            await botlog.SendMessageAsync($"[{DateTime.Now} at Gateway] First launch");

            foreach (IMessage message in messages)
            {
                if (message is IUserMessage msg)
                {
                    if (message.Embeds.First().Title == "User Muted")
                    {
                        await (guild.Channels.First(x => x.Id == Data.GetChnlId("commands")) as SocketTextChannel)
                            .SendMessageAsync($"^^mutefix {message.Id}");
                    }
                }
            }
        }

        /*public async Task JoinVerification(SocketGuildUser user)
        {
            if (user.IsBot) return;
            SocketRole role = Constants.IGuilds.Jordan(_client).Roles.First(x => x.Id == 705470408522072086);
            await user.AddRoleAsync(role);
        }*/

        public async Task Greeting_GuildMemberUpdated(SocketGuildUser user1, SocketGuildUser user2)
        {
            SocketGuild guild = user1.Guild;
            SocketRole verification = guild.Roles.First(x => x.Name == "Verification");
            if (!(user1.Roles.Contains(verification) && !user2.Roles.Contains(verification))) return;

            SocketTextChannel general = guild.Channels.First(x => x.Id == Data.GetChnlId("general")) as SocketTextChannel;
            await general.SendMessageAsync($"{user2.Mention} has joined! Say hello everyone!");
        }
    }
}