using Discord;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Data
{
    public static class LogEventHandlers
    {
        static readonly ulong LogID = Data.GetChnlId("ja3far-logs");

        // Message Edited
        public static async Task MessageEdited(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
        {
            if (channel is IDMChannel) return;
            if (message.Author.IsBot) return;
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;

            List<SocketTextChannel> Blacklist = new List<SocketTextChannel>
            {
                guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("suggestions")) as SocketTextChannel
            };

            if (Blacklist.Contains(channel as SocketTextChannel)) return;

            IMessage msgBefore = cacheable.Value;
            IMessage msgAfter = message as IMessage;

            if (msgBefore.Content == msgAfter.Content) return;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Message edited in #{message.Channel}");
            embed.WithDescription($"[Link to message]({message.GetJumpUrl()})");
            embed.WithAuthor(message.Author);
            embed.AddField("Before:", msgBefore.Content);
            embed.AddField("After:", msgAfter.Content);
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"MsgID: {message.Id}");
            if (message.Attachments.Count > 0)
            {
                string links = "";
                foreach (Attachment attachment in message.Attachments)
                {
                    links += $"{attachment.Url}\n";
                }
                embed.AddField("Attachments:", links);
            }

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Message Deleted
        public static async Task MessageDeleted(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel channel)
        {
            if (channel is IDMChannel) return;
            if (cacheable.Value == null) return;
            if (cacheable.Value.Author.IsBot) return;

            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            IMessage message = cacheable.GetOrDownloadAsync().Result;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Message deleted in #{message.Channel}");
            embed.WithAuthor(message.Author);
            if (message.Content == "" || message.Content == null) embed.AddField("Message:", "None");
            else embed.AddField("Message:", message.Content);
            embed.WithCurrentTimestamp();
            embed.WithColor(221, 95, 83);
            embed.WithFooter($"MsgID: {message.Id}");
            if (message.Attachments.Count > 0)
            {
                string links = "";
                foreach (IAttachment attachment in message.Attachments)
                {
                    links += $"{attachment.Url}\n";
                }
                embed.AddField("Attachments:", links);
            }

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Name Changed
        public static async Task NameChanged(SocketUser oldUser, SocketUser newUser)
        {
            if (oldUser.Username == newUser.Username) return;
            if ((oldUser as SocketGuildUser) == null) return;

            SocketGuildUser oldUser_ = oldUser as SocketGuildUser;
            SocketGuild guild = oldUser_.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Name changed");
            embed.WithAuthor(newUser);
            embed.AddField("Before:", oldUser.Username);
            embed.AddField("After:", newUser.Username);
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"UserID: {newUser.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Discriminator Changed
        public static async Task DiscriminatorChanged(SocketUser oldUser, SocketUser newUser)
        {
            if (oldUser.Discriminator == newUser.Discriminator) return;

            SocketGuildUser oldUser_ = oldUser as SocketGuildUser;
            SocketGuild guild = oldUser_.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Name changed");
            embed.WithAuthor(newUser);
            embed.AddField("Before:", oldUser.Discriminator);
            embed.AddField("After:", newUser.Discriminator);
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"UserID: {newUser.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Role Added
        public static async Task RoleAdded(SocketGuildUser oldUser, SocketGuildUser newUser)
        {
            SocketGuild guild = newUser.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            SocketRole role = guild.EveryoneRole;

            if (oldUser.Roles.Count >= newUser.Roles.Count) return;

            foreach (SocketRole newRole in newUser.Roles)
            {
                if (!oldUser.Roles.Contains(newRole))
                {
                    role = newRole;
                    break;
                }
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Role Added");
            embed.WithAuthor(newUser);
            embed.AddField("Role:", role.Mention);
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"UserID: {newUser.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Role Removed
        public static async Task RoleRemoved(SocketGuildUser oldUser, SocketGuildUser newUser)
        {
            SocketGuild guild = newUser.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            SocketRole role = guild.EveryoneRole;

            if (oldUser.Roles.Count <= newUser.Roles.Count) return;

            foreach (SocketRole oldRole in oldUser.Roles)
            {
                if (!newUser.Roles.Contains(oldRole))
                {
                    role = oldRole;
                    break;
                }
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Role Removed");
            embed.WithAuthor(newUser);
            embed.AddField("Role:", role.Mention);
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"UserID: {newUser.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Nickname Changed
        public static async Task NicknameChanged(SocketGuildUser oldUser, SocketGuildUser newUser)
        {
            if (oldUser.Nickname == newUser.Nickname) return;
            SocketGuild guild = newUser.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Nickname changed");
            embed.WithAuthor(newUser);
            if (oldUser.Nickname == "" || oldUser.Nickname == null) embed.AddField("Before:", "None");
            else embed.AddField("Before:", oldUser.Nickname);
            if (newUser.Nickname == "" || newUser.Nickname == null) embed.AddField("After:", "None");
            else embed.AddField("After:", newUser.Nickname);
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"UserID: {newUser.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Channel Created
        public static async Task ChannelCreated(SocketChannel channel)
        {
            if (channel is IDMChannel) return;
            if (channel is ICategoryChannel) return;
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            string group = "None";
            string type = "Unknown";

            foreach (SocketCategoryChannel category in guild.CategoryChannels)
            {
                if (category.Channels.Contains(channel)) group = category.Name;
            }

            if (channel is ITextChannel) type = "Text Channel";
            else type = "Voice Channel";

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Channel created");
            embed.AddField("Name:", (channel as SocketGuildChannel).Name);
            embed.AddField("Type:", type);
            embed.AddField("Category:", group);
            embed.WithCurrentTimestamp();
            embed.WithColor(83, 221, 172);
            embed.WithFooter($"ChannelID: {channel.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());

            SocketTextChannel CommandChannel = guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("commands")) as SocketTextChannel;
            await CommandChannel.SendMessageAsync("^^resetchannels");
        }

        // Channel Deleted
        public static async Task ChannelDestroyed(SocketChannel channel)
        {
            if (channel is IDMChannel) return;
            if (channel is ICategoryChannel) return;
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            string group = "None";
            string type = "Unknown";

            if (channel is ITextChannel) type = "Text Channel";
            else type = "Voice Channel";

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Channel deleted");
            embed.AddField("Name:", (channel as SocketGuildChannel).Name);
            embed.AddField("Type:", type);
            embed.AddField("Category:", group);
            embed.WithCurrentTimestamp();
            embed.WithColor(221, 95, 83);
            embed.WithFooter($"ChannelID: {channel.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());

            SocketTextChannel CommandChannel = guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("commands")) as SocketTextChannel;
            await CommandChannel.SendMessageAsync("^^resetchannels");
        }

        // User Joined
        public static async Task UserJoined(SocketGuildUser user)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();

            string invite = "Unknown";
            ulong inviterID = 0;
            string inviter = "Unknown";

            foreach (UserInvite UserInvite in DbContext.UserInvites)
            {
                if (UserInvite.UserID == user.Id)
                {
                    invite = UserInvite.Code;
                    inviterID = DbContext.Invites.FirstOrDefault(x => x.Text == UserInvite.Code).UserId;
                }
            }

            if (inviterID > 0)
            {
                inviter = user.Guild.Users.FirstOrDefault(x => x.Id == inviterID).Mention;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"User joined");
            embed.WithAuthor(user);
            embed.AddField("Account created:", Data.GetDuration(user.CreatedAt.DateTime.AddHours(2), DateTime.Now).Duration());
            embed.AddField("Invite link:", invite);
            embed.AddField("Invited by:", inviter);
            embed.WithCurrentTimestamp();
            embed.WithColor(83, 221, 172);
            embed.WithFooter($"UserID: {user.Id} | User count: {user.Guild.Users.Count}");

            SocketTextChannel LogChannel = user.Guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // User Left
        public static async Task UserLeft(SocketGuildUser user)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"User left");
            embed.WithAuthor(user);
            if (user.JoinedAt == null) embed.AddField("User joined:", "Unknown");
            else embed.AddField("User joined:", Data.GetDuration(user.JoinedAt.Value.DateTime.AddHours(2), DateTime.Now).Duration());
            embed.WithCurrentTimestamp();
            embed.WithColor(255, 245, 175);
            embed.WithFooter($"UserID: {user.Id} | User count: {user.Guild.Users.Count}");

            SocketTextChannel LogChannel = user.Guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());

            using SqliteDbContext DbContext = new SqliteDbContext();
            foreach (UserInvite UserInvite in DbContext.UserInvites)
            {
                if (UserInvite.UserID == user.Id)
                {
                    DbContext.UserInvites.Remove(UserInvite);
                    await DbContext.SaveChangesAsync();
                }
            }
            IReadOnlyCollection<Discord.Rest.RestInviteMetadata> Invites = user.Guild.GetInvitesAsync().Result;
            foreach (RestInviteMetadata Invite in Invites)
            {
                if (Invite.Inviter.Id == user.Id) await Invite.DeleteAsync();
            }
            await Data.SetInvitesBefore(user);
        }

        // Emoji Created
        public static async Task EmojiCreated(SocketGuild guildBefore, SocketGuild guildAfter)
        {
            try
            {
                if (guildBefore.Emotes.Count >= guildAfter.Emotes.Count) return;

                GuildEmote emote;
                foreach (IEmote Emote in guildAfter.Emotes)
                {
                    if (!guildBefore.Emotes.Contains(Emote))
                    {
                        emote = Emote as GuildEmote;

                        EmbedBuilder embed = new EmbedBuilder();
                        embed.WithTitle($"Emote Created");
                        embed.WithAuthor(guildAfter.Users.FirstOrDefault(x => x.Id == emote.CreatorId));
                        embed.AddField("Name:", emote.Name);
                        embed.AddField("Animated:", emote.Animated);
                        embed.WithCurrentTimestamp();
                        embed.WithColor(83, 221, 172);
                        embed.WithImageUrl(emote.Url);
                        embed.WithFooter($"EmoteID: {emote.Id}");

                        SocketTextChannel LogChannel = guildAfter.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
                        await LogChannel.SendMessageAsync("", false, embed.Build());
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Emoji Deleted
        public static async Task EmojiDeleted(SocketGuild guildBefore, SocketGuild guildAfter)
        {
            if (guildBefore.Emotes.Count! > guildAfter.Emotes.Count) return;

            GuildEmote emote;
            foreach (IEmote Emote in guildBefore.Emotes)
            {
                if (!guildAfter.Emotes.Contains(Emote))
                {
                    emote = Emote as GuildEmote;

                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithTitle($"Emote Deleted");
                    embed.WithAuthor(guildAfter.Users.FirstOrDefault(x => x.Id == emote.CreatorId));
                    embed.AddField("Name:", emote.Name);
                    embed.AddField("Animated:", emote.Animated);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(255, 245, 175);
                    embed.WithImageUrl(emote.Url);
                    embed.WithFooter($"EmoteID: {emote.Id}");

                    SocketTextChannel LogChannel = guildAfter.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
                    await LogChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }
            }
        }

        // Channel Name Changed
        public static async Task ChannelNameChanged(SocketChannel channelBefore, SocketChannel channelAfter)
        {
            SocketGuildChannel channelBefore_ = channelBefore as SocketGuildChannel;
            SocketGuildChannel channelAfter_ = channelAfter as SocketGuildChannel;

            if (channelBefore_.Name == channelAfter_.Name) return;

            string type = "Unknown";
            if (channelAfter is ITextChannel) type = "Text";
            else if (channelAfter is IVoiceChannel) type = "Voice";
            else return;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"{type} Channel changed");
            embed.AddField("Before:", channelBefore_.Name);
            embed.AddField("After:", channelAfter_.Name);
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"ChannelID: {channelAfter.Id}");

            SocketGuild guild = channelAfter_.Guild;
            SocketTextChannel LogChannel = (SocketTextChannel)guild.Channels.First(x => x.Id == LogID);
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }
    }
}
