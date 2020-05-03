using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.GeneralJSON;
using rJordanBot.Resources.Services;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class Music : InteractiveBase<SocketCommandContext>
    {
        private MusicService _musicService;

        public Music(MusicService musicService)
        {
            _musicService = musicService;
        }

        [Command("join")]
        public async Task Join()
        {
            if (!(Context.User as SocketGuildUser).IsModerator()) return;

            if (Context.Channel is IDMChannel) return;
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            SocketGuildUser bot = Constants.IConversions.GuildUser(Context);
            if (bot.VoiceChannel != null)
            {
                await ReplyAsync($":x: Already connected to {bot.VoiceChannel.Name}.");
                return;
            }

            await ReplyAsync($":ok: Connected to {user.VoiceChannel.Name}.");
            await _musicService.ConnectAsync(user.VoiceChannel, Context.Channel as SocketTextChannel);
        }

        [Command("leave"), Alias("dc")]
        public async Task Leave()
        {
            if (!(Context.User as SocketGuildUser).IsModerator()) return;

            if (Context.Channel is IDMChannel) return;
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            SocketGuildUser bot = Constants.IConversions.GuildUser(Context);
            if (bot.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            if (user.VoiceChannel != bot.VoiceChannel)
            {
                await ReplyAsync(":x: Not connected to the same voice channel.");
                return;
            }

            await ReplyAsync($":eject: Disconnected from {bot.VoiceChannel}.");
            await _musicService.LeaveAsync(bot.VoiceChannel);
        }

        [Command("play"), Alias("p")]
        public async Task Play([Remainder]string query = null)
        {
            if (!(Context.User as SocketGuildUser).IsModerator()) return;

            if (Context.Channel is IDMChannel) return;
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            if (query == null)
            {
                await ReplyAsync(":x: Please specify the search query for the track to be played.");
            }
            SocketGuildUser bot = Constants.IConversions.GuildUser(Context);
            if (bot.VoiceChannel != null && user.VoiceChannel != bot.VoiceChannel)
            {
                await ReplyAsync(":x: Not connected to the same voice channel.");
                return;
            }

            string result = _musicService.PlayAsync(query, Context.Guild.Id, bot, user.VoiceChannel, Context.Channel as SocketTextChannel).Result;
            await ReplyAsync(result);
        }

        [Command("stop")]
        public async Task Stop()
        {
            if (!(Context.User as SocketGuildUser).IsModerator()) return;

            if (Context.Channel is IDMChannel) return;
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            SocketGuildUser bot = Constants.IConversions.GuildUser(Context);
            if (bot.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            if (user.VoiceChannel != bot.VoiceChannel)
            {
                await ReplyAsync(":x: Not connected to the same voice channel.");
                return;
            }

            string result = await _musicService.StopAsync();
            await ReplyAsync(result);
        }

        [Command("skip")]
        public async Task Skip()
        {
            if (!(Context.User as SocketGuildUser).IsModerator()) return;

            if (Context.Channel is IDMChannel) return;
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            SocketGuildUser bot = Constants.IConversions.GuildUser(Context);
            if (bot.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            if (user.VoiceChannel != bot.VoiceChannel)
            {
                await ReplyAsync(":x: Not connected to the same voice channel.");
                return;
            }

            string result = _musicService.Skip();
            await ReplyAsync(result);
        }

        [Command("pause")]
        public async Task Pause()
        {
            if (!(Context.User as SocketGuildUser).IsModerator()) return;

            if (Context.Channel is IDMChannel) return;
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            SocketGuildUser bot = Constants.IConversions.GuildUser(Context);
            if (bot.VoiceChannel == null)
            {
                await ReplyAsync(":x: Not connected to a voice channel.");
                return;
            }
            if (user.VoiceChannel != bot.VoiceChannel)
            {
                await ReplyAsync(":x: Not connected to the same voice channel.");
                return;
            }

            string result = _musicService.PauseAsync().Result;
            await ReplyAsync(result);
        }

        [Command("queue"), Alias("q")]
        public async Task Queue()
        {
            if (!(Context.User as SocketGuildUser).IsModerator()) return;

            if (Context.Channel is IDMChannel) return;
            await ReplyAsync(_musicService.Queue());
        }
    }
}
