using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace rJordanBot.Core.Methods
{
    public struct Constants
    {
        public interface IColors
        {
            public static readonly Color Blurple = new Color(66, 134, 244);
            public static readonly Color Red = new Color(221, 95, 83);
            public static readonly Color Green = new Color(83, 221, 172);
            public static readonly Color Blurple = new Color(114, 137, 218);

            public interface ISuggestionColors
            {
                public static readonly Color Green = new Color(75, 255, 68);
                public static readonly Color Red = new Color(255, 75, 66);
                public static readonly Color Turquise = new Color(145, 251, 255);
                public static readonly Color Yellow = new Color(253, 255, 145);
            }
        }

        public interface IEmojis
        {
            public static readonly Emoji Tick = new Emoji("✅");
            public static readonly Emoji X = new Emoji("❌");
        }

        public interface IGuilds
        {
            public static SocketGuild Jordan(DiscordSocketClient Client)
            {
                return Client.Guilds.First(x => x.Id == 550848068640309259);
            }
            public static SocketGuild Jordan(SocketCommandContext Context)
            {
                return Context.Client.Guilds.First(x => x.Id == 550848068640309259);
            }
            public static IGuild Jordan(ICommandContext Context)
            {
                return (Context.Client as DiscordSocketClient).Guilds.First(x => x.Id == 550848068640309259);
            }
        }

        public interface IMacros
        {
            public static readonly string NoPerms = ":x: Insufficient permissions.";
        }

        public interface IConversions
        {
            public static SocketGuildUser GuildUser(SocketCommandContext Context)
            {
                SocketGuild guild = IGuilds.Jordan(Context);
                SocketUser user = Context.Client.CurrentUser;
                return guild.GetUser(user.Id);
            }

            public static SocketGuildUser GuildUser(DiscordSocketClient Client)
            {
                SocketGuild guild = IGuilds.Jordan(Client);
                SocketUser user = Client.CurrentUser;
                return guild.GetUser(user.Id);
            }
        }
    }
}
