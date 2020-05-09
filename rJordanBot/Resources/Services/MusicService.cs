using Discord;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Interfaces;
using Victoria.Responses.Rest;

namespace rJordanBot.Resources.Services
{
    public class MusicService
    {
        private LavaNode _lavaNode;
        private DiscordSocketClient _client;
        private LavaPlayer _player;
        /*private List<LavaTrack> queue = new List<LavaTrack>();
        private LavaTrack track;
        private IVoiceChannel channel;
        private ITextChannel tchannel;*/
        private bool isItOn = false;
        private bool loop = false;

        public MusicService(LavaNode lavaNode, DiscordSocketClient client)
        {
            _lavaNode = lavaNode;
            _client = client;
        }

        /*public async Task OnDisconnect(Exception ex)
        {
            if (_player == null) return;
            if (_player.IsPlaying)
            {
                isItOn = true;
                foreach (IQueueObject queueObject in _player.Queue.Items)
                {
                    LavaTrack lavaTrack = queueObject as LavaTrack;
                    queue.Add(lavaTrack);
                }
                track = _player.CurrentTrack;
                channel = _player.VoiceChannel;
                tchannel = _player.TextChannel;
                await _player.StopAsync();
            }
            else isItOn = false;

            _oldlavaRestClient = _lavaRestClient;
            _oldlavaSocketClient = _lavaSocketClient;
        }

        public async Task OnReconnect()
        {
            await _lavaSocketClient.StartAsync(_client, new Configuration
            {
                LogSeverity = LogSeverity.Debug,
                SelfDeaf = false
            });

            if (isItOn)
            {
                await _lavaSocketClient.ConnectAsync(channel, tchannel);
                _player = _lavaSocketClient.GetPlayer(_client.Guilds.First().Id);
                await _player.PlayAsync(track);
                await _player.SeekAsync(track.Position);
                Console.WriteLine(queue.Count().ToString());
                foreach (LavaTrack item in queue)
                {
                    _player.Queue.Enqueue(item);
                }
                queue.Clear();
            }

            isItOn = false;
        }*/

        public Task InitializeAsync()
        {
            Program program = new Program();

            _client.Ready += ClientReadyAsync;
            _lavaNode.OnLog += program.Client_Log;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _client.UserVoiceStateUpdated += CilentVoiceStateChanged;
            // _client.Disconnected += OnDisconnect;
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, SocketTextChannel textChannel)
            => await _lavaNode.JoinAsync(voiceChannel, textChannel);

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
        {
            if (_player != null && _player.PlayerState == PlayerState.Playing) await _player.StopAsync();
            _player.Queue.Clear();
            await _lavaNode.LeaveAsync(voiceChannel);
            loop = false;
        }

        public async Task<string> PlayAsync(string query, IGuild guild, SocketGuildUser bot, SocketVoiceChannel vc, SocketTextChannel tc)
        {
            SearchResponse result;
            if (query.Contains("https://")) result = _lavaNode.SearchAsync(query).Result;
            else result = _lavaNode.SearchYouTubeAsync(query).Result;
            if (result.LoadStatus == LoadStatus.LoadFailed) return ":x: Search failed.";
            if (result.LoadStatus == LoadStatus.NoMatches) return ":x: No matches found.";
            PlaylistInfo playlist;

            LavaTrack track = result.Tracks.First();

            if (bot.VoiceChannel == null)
            {
                await ConnectAsync(vc, tc);
            }

            _player = _lavaNode.GetPlayer(guild);

            if (result.LoadStatus == LoadStatus.PlaylistLoaded)
            {
                playlist = result.Playlist;

                if (_player.PlayerState == PlayerState.Playing)
                {
                    foreach (LavaTrack resultTrack in result.Tracks)
                    {
                        _player.Queue.Enqueue(resultTrack);
                    }
                    return $":1234: Playlist `{playlist.Name}` loaded in queue.";
                }

                await _player.PlayAsync(track);
                foreach (LavaTrack resultTrack in result.Tracks)
                {
                    if (resultTrack != track) _player.Queue.Enqueue(resultTrack);
                }
                return $":1234: Playlist `{playlist.Name}` loaded in queue ({result.Tracks.Count()} tracks).\n" +
                        $":arrow_forward: Now playing: `{track.Title}`";
            }

            if (_player.PlayerState == PlayerState.Playing)
            {
                _player.Queue.Enqueue(track);
                return $":fast_forward: `{track.Title}` added to the queue in position {_player.Queue.Items.ToList().IndexOf(track) + 1}.";
            }

            await _player.PlayAsync(track);
            return $":arrow_forward: Now playing: `{track.Title}`";
        }

        public async Task<string> StopAsync()
        {
            if (_player is null || _player.Track is null) return ":x: Nothing to stop.";
            _player.Queue.Clear();
            await _player.StopAsync();
            loop = false;
            return ":stop_button: Stopped.";
        }

        public string Skip()
        {
            if (_player is null || _player.Track == null) return ":x: Nothing to skip.";
            LavaTrack skippedTrack = _player.Track;
            loop = false;
            if (_player.Queue.Items.Count() == 0 && _player.Track != null)
            {
                _player.StopAsync();
                return ":stop_button: There are no more tracks in the queue.";
            }
            _player.SkipAsync();
            return $":track_next: Skipped `{skippedTrack.Title}`\n" +
                $":arrow_forward: Now playing: `{_player.Track.Title}`";
        }

        public async Task<string> PauseAsync()
        {
            if (_player.PlayerState != PlayerState.Paused)
            {
                await _player.PauseAsync();
                return ":pause_button: Paused player.";
            }
            await _player.ResumeAsync();
            return ":arrow_forward: Resumed player.";
        }

        public string Queue(int page)
        {
            if (_player is null || _player.Queue.Equals(null) || (_player.Queue.Items.Count() == 0 && _player.Track == null))
            {
                return ":x: Queue is empty.";
            }

            string result = "```stylus\n";
            foreach (IQueueable queueObject in _player.Queue.Items)
            {
                LavaTrack track = queueObject as LavaTrack;
                result += $"{_player.Queue.Items.ToList().IndexOf(queueObject) + 1}) {track.Title} -> {track.Duration.ToString(@"m\:ss")}\n";
            }
            if (loop) result += $"\n0) [LOOPING] {_player.Track.Title} -> {(_player.Track.Duration - _player.Track.Position).ToString(@"m\:ss")} left\n```";
            else result += $"\n0) {_player.Track.Title} -> {(_player.Track.Duration - _player.Track.Position).ToString(@"m\:ss")} left\n```";

            if (result.Count() > 2000)
            {
                result = "```stylus\n";
                if (10 * page >= _player.Queue.Items.Count())
                {
                    for (int x = 10 * (page - 1); x < _player.Queue.Items.Count(); x++)
                    {
                        LavaTrack track = _player.Queue.Items.ElementAt(x) as LavaTrack;
                        result += $"{x + 1}) {track.Title} -> {track.Duration.ToString(@"m\:ss")}\n";
                    }
                }
                else for (int x = 10 * (page - 1); x < 10 * page; x++)
                    {
                        LavaTrack track = _player.Queue.Items.ElementAt(x) as LavaTrack;
                        result += $"{x + 1}) {track.Title} -> {track.Duration.ToString(@"m\:ss")}\n";
                    }

                if (loop) result += $"\n0) [LOOPING] {_player.Track.Title} -> {(_player.Track.Duration - _player.Track.Position).ToString(@"m\:ss")} left\n";
                else result += $"\n0) {_player.Track.Title} -> {(_player.Track.Duration - _player.Track.Position).ToString(@"m\:ss")} left\n";

                result += $"\nPage {page}/{_player.Queue.Items.Count() / 10 + 1}```";
            }

            return result;
        }

        public string Loop()
        {
            if (_player is null || _player.Track == null) return ":x: Nothing to loop.";
            if (!loop)
            {
                loop = true;
                return $":repeat: Looping track `{_player.Track.Title}`";
            }
            loop = false;
            return $":repeat: Stopped looping track `{_player.Track.Title}`";
        }


        private async Task ClientReadyAsync()
        {
            await _lavaNode.ConnectAsync();

            _client.Ready -= ClientReadyAsync;
            /*_client.Ready += OnReconnect;*/
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            TrackEndReason reason = args.Reason;
            LavaPlayer player = args.Player;
            LavaTrack track = args.Track;

            if (!reason.ShouldPlayNext()) return;
            if (loop)
            {
                await player.PlayAsync(track);
                await player.TextChannel.SendMessageAsync($":arrow_forward: Now playing: `{track.Title}`");
                return;
            }
            if (!player.Queue.TryDequeue(out IQueueable queueObject) || !(queueObject is LavaTrack nextTrack))
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

            if (preType == 1 && _player.PlayerState == PlayerState.Paused) return;
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
