using Discord;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
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

            _cmdService.CommandExecuted += ExceptionPing;

            return Task.CompletedTask;
        }

        public Task GenericLog(LogMessage logMessage)
        {
            SetColor(logMessage.Severity);
            string errormsg = $"[{DateTime.Now} at {logMessage.Source}] {logMessage.Message}";
            if (logMessage.Exception != null)
            {
                ExceptionLog(logMessage);
                return Task.CompletedTask;
            }

            Console.WriteLine(errormsg);
            Console.ResetColor();
            return Task.CompletedTask;
        }

        public void ExceptionLog(LogMessage logMessage)
        {
            Exception ex = logMessage.Exception;
            string errormsg = $"[{DateTime.Now} at ExecptionHandler] \n{ex}";
            
            Console.WriteLine(errormsg);
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
                    break;
                case LogSeverity.Debug:
                    if (Environment.GetEnvironmentVariable("SystemType") == "win") Console.ForegroundColor = ConsoleColor.DarkGray;
                    else Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    if (Environment.GetEnvironmentVariable("SystemType") == "win") Console.ForegroundColor = ConsoleColor.White;
                    else Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }
        }

        public Task CommandErrorLog(SocketUserMessage message, IResult result)
        {
            string errormsg = $"[{DateTime.Now} at Commands] Command error: | Command: {message.Content} | User: {message.Author} | Reason: {result.ErrorReason}\nError: {result.Error}";

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(errormsg);
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}
