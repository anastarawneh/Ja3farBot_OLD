using Discord;
using Discord.WebSocket;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.GeneralJSON;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Data
{
    public static class EventHandlers
    {
        public static async Task Roles_ReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IDMChannel)
            {
                return;
            }
            if (reaction.User.Value.IsBot) return;

            SocketTextChannel channel_ = channel as SocketTextChannel;
            if (channel_.Guild == null) return;
            SocketGuild guild = channel_.Guild;
            if (channel_.Id == Data.GetChnlId("role-selection") && !reaction.User.Value.IsBot)
            {
                RoleSetting role = new RoleSetting();
                role = Data.GetEmojiRoleSetting(reaction.Emote.Name);

                IMessage msg = channel_.GetMessageAsync(role.id).Result;
                IUserMessage msg_ = msg as IUserMessage;
                await msg_.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                SocketGuildUser user = reaction.User.Value as SocketGuildUser;
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

        public static async Task Events_ReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IDMChannel) return;
            if ((channel as SocketTextChannel).Name != "events") return;
            if (reaction.User.Value.IsBot) return;

            if ((reaction.User.Value as SocketGuildUser).ToUser().Verified)
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

                IDMChannel dm = reaction.User.Value.GetOrCreateDMChannelAsync().Result;
                await dm.SendMessageAsync(":x: Please verify your age using the Event Verification System before using events.");
                return;
            }
        }

        public static async Task Invites_UserJoined(SocketGuildUser user)
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

        public static async Task Starboard_ReactionAddedOrRemoved(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
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

            try
            {
                await Data.UpdateStarboard(starboardmessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task JSON_UserLeft(SocketGuildUser user)
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

            foreach (User item in GeneralJson.users)
            {
                if (item.ID == user.Id)
                {
                    await GeneralJson.RemoveUser(user);
                }
            }
        }

        public static async Task InviteDeletion(SocketMessage message)
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
    }
}