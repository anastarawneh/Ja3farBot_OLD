using Discord;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;

namespace rJordanBot.Resources.Services
{
    public class LoggerService
    {
        private DiscordSocketClient _client;
        private CommandService _cmdService;
        private LavaNode _lavaNode;
        private ITextChannel _logChannel;
        private bool _serverLog = false;
        private bool _hasServer = false;
        private bool rateLimited = false;

        public LoggerService(DiscordSocketClient client, CommandService cmdService, LavaNode lavaNode)
        {
            _client = client;
            _cmdService = cmdService;
            _lavaNode = lavaNode;
        }

        public Task Initialize()
        {
            _client.Log += GenericLog;
            _cmdService.Log += GenericLog;
            _lavaNode.OnLog += GenericLog;

            _client.GuildAvailable += InitGuild;

            _cmdService.CommandExecuted += ExceptionPing;

            return Task.CompletedTask;
        }

        public Task InitGuild(SocketGuild guild)
        {
            _logChannel = guild.Channels.First(x => x.Id == Data.GetChnlId("bot-log")) as ITextChannel;
            _hasServer = true;
            return Task.CompletedTask;
        }

        public async Task GenericLog(LogMessage logMessage)
        {
            SetColor(logMessage.Severity);
            string errormsg = $"[{DateTime.Now} at {logMessage.Source}] {logMessage.Message}";
            string errormsg_ = $"[{DateTime.Now} at {logMessage.Source}] {logMessage.Message}";
            if (logMessage.Severity == LogSeverity.Warning) errormsg_ = $"[{DateTime.Now} at {logMessage.Source}] **{logMessage.Message}**";
            if (logMessage.Message.Contains("Rate limit triggered")) rateLimited = true;

            if (logMessage.Exception != null)
            {
                await ExceptionLog(logMessage);
                return;
            }

            Console.WriteLine(errormsg);
            
            int c = 0;
            if (rateLimited)
            {
                if (c == 0) await _logChannel.SendMessageAsync($"Hey {MentionUtils.MentionUser(ESettings.Owner)}! I'm rate-limited! I'm not logging anymore.");
                c++;
                return;
            }
            else
            {
                if (_hasServer && _serverLog) await _logChannel.SendMessageAsync(errormsg_);
                c = 0;
            }

            Console.ResetColor();
        }

        public async Task ExceptionLog(LogMessage logMessage)
        {
            Exception ex = logMessage.Exception;
            string errormsg = $"[{DateTime.Now} at ExecptionHandler] \n{ex}";
            string errormsg_ = $"[{DateTime.Now} at ExecptionHandler] \n```{ex}```";

            if (ex.ToString().Contains("Server requested a reconnect")) errormsg_ = $"[{DateTime.Now} at ExceptionHandler] Server requested a reconnect";
            if (ex.ToString().Contains("WebSocket connection was closed")) errormsg_ = $"[{DateTime.Now} at ExceptionHandler] WebSocket connection was closed";

            Console.WriteLine(errormsg);
            if (_hasServer && _serverLog && !rateLimited) await _logChannel.SendMessageAsync(errormsg_);

            Console.ResetColor();
        }

        public async Task ExceptionPing(Optional<CommandInfo> optional, ICommandContext context, IResult result)
        {
            if (result is ExecuteResult result_)
            {
                Exception ex = result_.Exception;
                if (ex == null) return;
                if (ex.Message == ":x: Please specify a vaild site. Available sites are (Twitter/Instagram/Snapchat).") return;

                await context.Channel.SendMessageAsync($"Catastrophic faliure! Paging {context.Guild.GetOwnerAsync().Result.Mention}.");
            }
        }

        public void SetColor(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    _serverLog = true;
                    break;
                case LogSeverity.Debug:
                    if (Environment.GetEnvironmentVariable("SystemType") == "win") Console.ForegroundColor = ConsoleColor.DarkGray;
                    else Console.ForegroundColor = ConsoleColor.White;
                    _serverLog = false;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    _serverLog = true;
                    break;
                case LogSeverity.Info:
                    if (Environment.GetEnvironmentVariable("SystemType") == "win") Console.ForegroundColor = ConsoleColor.White;
                    else Console.ForegroundColor = ConsoleColor.Green;
                    _serverLog = true;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    _serverLog = false;
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    _serverLog = true;
                    break;
            }
        }

        public async Task CommandErrorLog(SocketUserMessage message, IResult result)
        {
            string errormsg = $"[{DateTime.Now} at Commands] Command error: | Command: {message.Content} | User: {message.Author} | Reason: {result.ErrorReason}\nError: {result.Error}";

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(errormsg);
            Console.ResetColor();

            SocketGuild Guild = Constants.IGuilds.Jordan(_client);
            SocketTextChannel Channel = Guild.Channels.Where(x => x.Id == Data.GetChnlId("bot-log")).FirstOrDefault() as SocketTextChannel;

            await Channel.SendMessageAsync(errormsg);
        }


        public void ReopenLogs() => rateLimited = false;
    }
}
