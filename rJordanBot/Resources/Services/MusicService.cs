using Discord;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;
using Victoria.Queue;

namespace rJordanBot.Resources.Services
{
    public class MusicService
    {
        private LavaRestClient _lavaRestClient;
        private LavaSocketClient _lavaSocketClient;
        private DiscordSocketClient _client;
        private LavaPlayer _player;

        public MusicService(LavaRestClient lavaRestClient, LavaSocketClient lavaSocketClient, DiscordSocketClient client)
        {
            _lavaRestClient = lavaRestClient;
            _lavaSocketClient = lavaSocketClient;
            _client = client;
        }

        public Task InitializeAsync()
        {
            Program program = new Program();

            _client.Ready += ClientReadyAsync;
            _lavaSocketClient.Log += program.Client_Log;
            _lavaSocketClient.OnTrackFinished += OnTrackFinished;
            _client.UserVoiceStateUpdated += CilentVoiceStateChanged;
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, SocketTextChannel textChannel)
            => await _lavaSocketClient.ConnectAsync(voiceChannel, textChannel);

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
            => await _lavaSocketClient.DisconnectAsync(voiceChannel);

        public async Task<string> PlayAsync(string query, ulong guildID, SocketGuildUser bot, SocketVoiceChannel vc, SocketTextChannel tc)
        {
            SearchResult results = _lavaRestClient.SearchYouTubeAsync(query).Result;
            if (results.LoadType == LoadType.LoadFailed) return ":x: Search failed.";
            if (results.LoadType == LoadType.NoMatches) return ":x: No matches found.";

            LavaTrack track = results.Tracks.First();

            if (bot.VoiceChannel == null)
            {
                await ConnectAsync(vc, tc);
            }

            _player = _lavaSocketClient.GetPlayer(guildID);

            if (_player.IsPlaying)
            {
                _player.Queue.Enqueue(track);
                return $":fast_forward: `{track.Title}` added to the queue in position {_player.Queue.Items.ToList().IndexOf(track) + 1}.";
            }

            await _player.PlayAsync(track);
            return $":arrow_forward: Now playing: `{track.Title}`";
        }

        public async Task<string> StopAsync()
        {
            if (_player is null || _player.CurrentTrack is null) return ":x: Nothing to stop.";
            await _player.StopAsync();
            return ":stop_button: Stopped.";
        }

        public string Skip()
        {
            if (_player is null || _player.CurrentTrack == null) return ":x: Nothing to skip.";
            LavaTrack skippedTrack = _player.CurrentTrack;
            if (_player.Queue.Items.Count() == 0 && _player.CurrentTrack != null)
            {
                _player.StopAsync();
                return ":stop_button: There are no more tracks in the queue.";
            }
            _player.SkipAsync();
            return $":track_next: Skipped `{skippedTrack.Title}`\n" +
                $":arrow_forward: Now playing: `{_player.CurrentTrack.Title}`";
        }

        public async Task<string> PauseAsync()
        {
            if (!_player.IsPaused)
            {
                await _player.PauseAsync();
                return ":pause_button: Paused player.";
            }
            await _player.ResumeAsync();
            return ":arrow_forward: Resumed player.";
        }

        public string Queue()
        {
            if (_player is null || (_player.Queue.Items.Count() == 0 && _player.CurrentTrack == null))
            {
                return ":x: Queue is empty.";
            }

            string result = "```stylus\n";
            foreach (IQueueObject queueObject in _player.Queue.Items)
            {
                LavaTrack track = queueObject as LavaTrack;
                result += $"{_player.Queue.Items.ToList().IndexOf(queueObject) + 1}) {track.Title} -> {track.Length.ToString(@"m\:ss")}\n";
            }
            result += $"\n0) {_player.CurrentTrack.Title} -> {(_player.CurrentTrack.Length - _player.CurrentTrack.Position).ToString(@"m\:ss")} left\n```";

            return result;
        }


        private async Task ClientReadyAsync()
        {
            await _lavaSocketClient.StartAsync(_client, new Configuration { 
                LogSeverity = LogSeverity.Debug,
                SelfDeaf = false
            });
        }

        private async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (!reason.ShouldPlayNext()) return;
            if (!player.Queue.TryDequeue(out IQueueObject queueObject) || !(queueObject is LavaTrack nextTrack))
            {
                await player.TextChannel.SendMessageAsync(":x: There are no more tracks in the queue.");
                return;
            }

            await player.PlayAsync(nextTrack);
            await player.TextChannel.SendMessageAsync($":arrow_forward: Now playing: `{nextTrack.Title}`");
        }

        private async Task CilentVoiceStateChanged(SocketUser user, SocketVoiceState preState, SocketVoiceState postState)
        {
            if (user.IsBot) return;
            if (_player is null) return;
            int preType = -1; // 1 for bot, 2 for not bot, 0 for null
            int postType = -1;

            if (preState.VoiceChannel is null) preType = 0;
            else if (preState.VoiceChannel.Users.Contains(Constants.IConversions.GuildUser(_client))) preType = 1;
            else preType = 2;

            if (postState.VoiceChannel is null) postType = 0;
            else if (postState.VoiceChannel.Users.Contains(Constants.IConversions.GuildUser(_client))) postType = 1;
            else postType = 2;

            if (preType == 1 && _player.IsPaused) return;
            if ((preType == 0 && postType == 0) || (preType == 2 && postType == 2) || (preType == 0 && postType == 2) || (preType == 2 && postType == 0)) return;
            if (preType == 1 && preState.VoiceChannel.Users.Count != 1) return;
            if (postType == 1 && postState.VoiceChannel.Users.Count != 2) return;

            if ((preType == 1 && postType == 0) || (preType == 1 && postType == 2))
            {
                Console.WriteLine($"Pause, from {preType} to {postType}");
                await PauseAsync();
                await _player.TextChannel.SendMessageAsync(":pause_button: Paused playback; the voice channel is empty.");
            }

            if ((preType == 0 && postType == 1) || (preType == 2 && postType == 1))
            {
                Console.WriteLine($"Pause, from {preType} to {postType}");
                await PauseAsync();
                await _player.TextChannel.SendMessageAsync(":arrow_forward: Resumed playback.");
            }

            /*if (preState.VoiceChannel == null && postState.VoiceChannel == null) return;
            if (preState.VoiceChannel != null &&
                !preState.VoiceChannel.Users.Contains(Constants.IConversions.GuildUser(_client)) &&
                postState.VoiceChannel != null &&
                !postState.VoiceChannel.Users.Contains(Constants.IConversions.GuildUser(_client))) return;
            
            if (preState.VoiceChannel != null && preState.VoiceChannel.Users.Contains(user) && ((!postState.VoiceChannel.Users.Contains(user) && postState.VoiceChannel.Users.Count == 1 &&) || postState.VoiceChannel == null) && !_player.IsPaused)
            {
                await PauseAsync();
                await _player.TextChannel.SendMessageAsync(":pause_button: Paused playback; the voice channel is empty.");
            }
            if (postState.VoiceChannel != null && postState.VoiceChannel.Users.Contains(user) && (!preState.VoiceChannel.Users.Contains(user) && preState.VoiceChannel.Users.Count == 1) || preState.VoiceChannel == null)
            {
                await PauseAsync();
                await _player.TextChannel.SendMessageAsync(":arrow_forward: Resumed playback.");
            }*/
        }
    }
}
