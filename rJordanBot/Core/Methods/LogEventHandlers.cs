using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using rJordanBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Victoria;

namespace rJordanBot.Core.Methods
{
    public class LogEventHandlers
    {
        static readonly ulong LogID = Data.GetChnlId("ja3far-logs");
        private readonly DiscordSocketClient _client;
        public LogEventHandlers(DiscordSocketClient client) => _client = client;

        public Task Initialize()
        {
            _client.MessageUpdated += MessageEdited;
            _client.MessageDeleted += MessageDeleted;
            _client.UserUpdated += NameOrDiscrimChanged;
            _client.GuildMemberUpdated += RoleAdded;
            _client.GuildMemberUpdated += RoleRemoved;
            _client.GuildMemberUpdated += NicknameChanged;
            _client.ChannelCreated += ChannelCreated;
            _client.ChannelDestroyed += ChannelDestroyed;
            _client.UserJoined += UserJoined;
            _client.UserLeft += UserLeft;
            _client.ChannelUpdated += ChannelNameChanged;
            _client.MessagesBulkDeleted += MessagesBulkDeleted;
            _client.RoleCreated += RoleCreated;
            _client.RoleDeleted += RoleDeleted;
            _client.RoleUpdated += RoleRenamed;
            _client.RoleUpdated += RoleRecolored;
            _client.RoleUpdated += RoleUpdated;
            _client.GuildUpdated += EmoteCreated;
            _client.GuildUpdated += EmoteDeleted;
            _client.GuildUpdated += EmoteUpdated;

            return Task.CompletedTask;
        }

        // Message Edited
        public async Task MessageEdited(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
        {
            if (channel is IDMChannel) return;
            if (message.Author.IsBot) return;
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;

            List<SocketTextChannel> Blacklist = new List<SocketTextChannel>
            {
                guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("suggestions")) as SocketTextChannel,
                guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("commands")) as SocketTextChannel
            };

            if (Blacklist.Contains(channel as SocketTextChannel)) return;

            IMessage msgBefore = cacheable.Value;
            IMessage msgAfter = message as IMessage;

            if (msgBefore == null) return;
            if (msgBefore.Content == null || msgAfter.Content == null) return;
            if (msgBefore.Content == msgAfter.Content) return;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Message edited in #{message.Channel}");
            embed.WithDescription($"[Link to message]({message.GetJumpUrl()})");
            embed.WithAuthor(message.Author);
            if (msgBefore.Content == null || msgBefore.Content == "" || msgBefore == null) embed.AddField("Before:", "None");
            else embed.AddField("Before:", msgBefore.Content);
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
        public async Task MessageDeleted(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel channel)
        {
            if (channel is IDMChannel) return;
            if (cacheable.Value == null) return;
            if (cacheable.Value.Author.IsBot) return;
            SocketGuild guild = (channel as SocketGuildChannel).Guild;

            List<SocketTextChannel> Blacklist = new List<SocketTextChannel>
            {
                guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("music-boi_songrequests")) as SocketTextChannel,
                guild.Channels.FirstOrDefault(x => x.Id == Data.GetChnlId("commands")) as SocketTextChannel
            };

            if (Blacklist.Contains(channel as SocketTextChannel)) return;

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

        // Name or Discriminator Changed
        public async Task NameOrDiscrimChanged(SocketUser oldUser, SocketUser newUser)
        {
            if (oldUser.Username == newUser.Username && oldUser.Discriminator == newUser.Discriminator) return;
            if (oldUser.MutualGuilds.Count < 1) return;

            SocketGuild guild = Constants.IGuilds.Jordan(_client);
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Name changed");
            embed.WithAuthor(newUser);
            embed.AddField("Before:", $"{oldUser.Username}#{oldUser.Discriminator}");
            embed.AddField("After:", $"{newUser.Username}#{newUser.Discriminator}");
            embed.WithCurrentTimestamp();
            embed.WithColor(66, 134, 244);
            embed.WithFooter($"UserID: {newUser.Id}");

            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Role Added
        public async Task RoleAdded(SocketGuildUser oldUser, SocketGuildUser newUser)
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
        public async Task RoleRemoved(SocketGuildUser oldUser, SocketGuildUser newUser)
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
        public async Task NicknameChanged(SocketGuildUser oldUser, SocketGuildUser newUser)
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
        public async Task ChannelCreated(SocketChannel channel)
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
        public async Task ChannelDestroyed(SocketChannel channel)
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
        public async Task UserJoined(SocketGuildUser user)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();

            EmbedBuilder embed = new EmbedBuilder();
            string invite = "Unknown";
            ulong inviterID = 0;
            string inviter = "Unknown";

            if (!user.IsBot)
            {
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

                embed.WithTitle($"User joined");
                embed.WithAuthor(user);
                embed.AddField("Account created:", Data.GetDuration(user.CreatedAt.DateTime.ToLocalTime(), DateTime.Now.ToLocalTime()).Duration());
                embed.AddField("Invite link:", invite);
                embed.AddField("Invited by:", inviter);
                embed.WithCurrentTimestamp();
                embed.WithColor(83, 221, 172);
                embed.WithFooter($"UserID: {user.Id} | User count: {user.Guild.MemberCount}");
            }
            else
            {
                embed.WithTitle($"Bot joined");
                embed.WithAuthor(user);
                embed.WithCurrentTimestamp();
                embed.WithColor(83, 221, 172);
                embed.WithFooter($"UserID: {user.Id} | User count: {user.Guild.MemberCount}");
            }

            SocketTextChannel LogChannel = user.Guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // User Left
        public async Task UserLeft(SocketGuildUser user)
        {
            string roles = "";
            List<SocketRole> roles_ = user.Roles.ToList();
            foreach (SocketRole role in roles_)
            {
                if (role != roles_[0]) roles += $"{role.Mention}\n";
            }
            if (roles == "") roles = "None";

            EmbedBuilder embed = new EmbedBuilder();
            if (user.IsBot)
            {
                embed.WithTitle($"User left");
                embed.WithAuthor(user);
                if (user.JoinedAt == null) embed.AddField("User joined:", "Unknown");
                else embed.AddField("User joined:", Data.GetDuration(user.JoinedAt.Value.DateTime.ToLocalTime(), DateTime.Now.ToLocalTime()).Duration());
                embed.AddField("Roles:", roles);
                embed.WithCurrentTimestamp();
                embed.WithColor(255, 245, 175);
                embed.WithFooter($"UserID: {user.Id} | User count: {user.Guild.MemberCount}");
            }
            else
            {
                embed.WithTitle($"Bot left");
                embed.WithAuthor(user);
                embed.WithCurrentTimestamp();
                embed.WithColor(Constants.IColors.Red);
                embed.WithFooter($"UserID: {user.Id} | User count: {user.Guild.MemberCount}");
            }

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

        // Channel Name Changed
        public async Task ChannelNameChanged(SocketChannel channelBefore, SocketChannel channelAfter)
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

        // Messages Bulk Deleted
        public async Task MessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> collection, ISocketMessageChannel channel)
        {
            string messages = "";
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Messages bulk deleted in #{channel}");
            embed.WithColor(Constants.IColors.Red);
            embed.WithCurrentTimestamp();

            IEnumerable<Cacheable<IMessage, ulong>> revcollection = collection.Reverse();
            foreach (Cacheable<IMessage, ulong> cacheable in revcollection)
            {
                IMessage message = cacheable.Value;
                if (message.Content != null) messages += $"[<@{message.Author.Id}>]: {message.Content}\n";
            }

            embed.AddField("Messages", messages);

            await ((channel as SocketGuildChannel).Guild.Channels.First(x => x.Id == LogID) as SocketTextChannel).SendMessageAsync("", false, embed.Build());
        }

        // Role Created
        public async Task RoleCreated(SocketRole role)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Role created");
            embed.WithDescription(MentionUtils.MentionRole(role.Id));
            if (role.Color != new Color(0, 0, 0)) embed.WithColor(role.Color);
            else embed.WithColor(Constants.IColors.Blue);
            embed.AddField("Mentionable", role.IsMentionable.ToString().CapitalizeFirst());
            embed.AddField("Hoisted", role.IsHoisted.ToString().CapitalizeFirst());
            embed.AddField("Position", role.Position);
            embed.WithFooter($"RoleID: {role.Id}");
            embed.WithCurrentTimestamp();

            SocketGuild guild = role.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Role Deleted
        public async Task RoleDeleted(SocketRole role)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Role deleted");
            embed.WithDescription(role.Name);
            embed.WithColor(Constants.IColors.Red);
            embed.AddField("Mentionable", role.IsMentionable.ToString().CapitalizeFirst());
            embed.AddField("Hoisted", role.IsHoisted.ToString().CapitalizeFirst());
            embed.AddField("Position", role.Position);
            embed.AddField("Created", Data.GetDuration(role.CreatedAt.DateTime.ToLocalTime(), DateTime.Now.ToLocalTime()).Duration());
            embed.WithFooter($"RoleID: {role.Id}");
            embed.WithCurrentTimestamp();

            SocketGuild guild = role.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Role Renamed
        public async Task RoleRenamed(SocketRole role1, SocketRole role2)
        {
            if (role1.Name == role2.Name) return;
            if (role1.Color != role2.Color) return;

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Role renamed");
            embed.WithColor(Constants.IColors.Blue);
            embed.AddField("Before", role1.Name);
            embed.AddField("After", role2.Name);
            embed.WithFooter($"RoleID: {role2.Id}");
            embed.WithCurrentTimestamp();

            SocketGuild guild = role1.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Role Recolored
        public async Task RoleRecolored(SocketRole role1, SocketRole role2)
        {
            if (role1.Color == role2.Color) return;
            if (role1.Name != role2.Name) return;

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Role recolored");
            embed.WithDescription(MentionUtils.MentionRole(role2.Id));
            embed.WithColor(role2.Color);
            embed.AddField("Before", role1.Color.GetRGB());
            embed.AddField("After", role2.Color.GetRGB());
            embed.WithFooter($"RoleID: {role2.Id}");
            embed.WithCurrentTimestamp();

            SocketGuild guild = role1.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Role Updated (renamed AND colored)
        public async Task RoleUpdated(SocketRole role1, SocketRole role2)
        {
            if (role1.Name == role2.Name || role1.Color == role2.Color) return;

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Role updated");
            embed.WithColor(role2.Color);
            embed.AddField("Before", $"**Name:** {role1.Name}\n**Color:** {role1.Color.GetRGB()}", true);
            embed.AddField("After", $"**Name:** {role2.Name}\n**Color:** {role2.Color.GetRGB()}", true);
            embed.WithFooter($"RoleID: {role2.Id}");
            embed.WithCurrentTimestamp();

            SocketGuild guild = role1.Guild;
            SocketTextChannel LogChannel = guild.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Emote Created
        public async Task EmoteCreated(SocketGuild guild1, SocketGuild guild2)
        {
            if (guild1.Emotes.Count >= guild2.Emotes.Count) return;

            GuildEmote emote = guild1.Emotes.First();
            foreach (GuildEmote new_ in guild2.Emotes)
            {
                if (!guild1.Emotes.Contains(new_)) emote = new_;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Emote Created");
            embed.WithColor(Constants.IColors.Green);
            embed.WithDescription($"{emote.Name} > <:{emote.Name}:{emote.Id}>");
            embed.WithFooter($"EmoteID: {emote.Id}");
            embed.WithCurrentTimestamp();

            SocketTextChannel LogChannel = guild1.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Emote Deleted
        public async Task EmoteDeleted(SocketGuild guild1, SocketGuild guild2)
        {
            if (guild1.Emotes.Count <= guild2.Emotes.Count) return;

            GuildEmote emote = guild2.Emotes.First();
            foreach (GuildEmote old in guild1.Emotes)
            {
                if (!guild2.Emotes.Contains(old)) emote = old;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Emote Deleted");
            embed.WithColor(Constants.IColors.Red);
            embed.WithDescription(emote.Name);
            embed.WithFooter($"EmoteID: {emote.Id}");
            embed.WithCurrentTimestamp();

            SocketTextChannel LogChannel = guild1.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }

        // Emote Updated
        public async Task EmoteUpdated(SocketGuild guild1, SocketGuild guild2)
        {
            if (guild1.Emotes == guild2.Emotes) return;
            if (guild1.Emotes.Count != guild2.Emotes.Count) return;

            ulong id = 0;

            foreach (var oldemote in guild1.Emotes)
            {
                foreach (var newemote in guild2.Emotes)
                {
                    if (oldemote.Id == newemote.Id && oldemote.Name != newemote.Name) id = oldemote.Id;
                }
            }

            GuildEmote emote1 = guild1.Emotes.First(x => x.Id == id);
            GuildEmote emote2 = guild2.Emotes.First(x => x.Id == id);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Emote Renamed");
            embed.WithDescription($"<:{emote2.Name}:{emote2.Id}>");
            embed.WithColor(Constants.IColors.Blue);
            embed.AddField("Before", emote1.Name);
            embed.AddField("After", emote2.Name);
            embed.WithFooter($"EmoteID: {emote2.Id}");
            embed.WithCurrentTimestamp();

            SocketTextChannel LogChannel = guild1.Channels.FirstOrDefault(x => x.Id == LogID) as SocketTextChannel;
            await LogChannel.SendMessageAsync("", false, embed.Build());
        }
    }
}
