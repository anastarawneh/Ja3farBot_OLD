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


        public static void Information(string Source, string Message)
            => DirectLog(Source, Message, LogSeverity.Info);
        public static void Warning(string Source, string Message)
            => DirectLog(Source, Message, LogSeverity.Warning);
        public static void Error(string Source, string Message)
            => DirectLog(Source, Message, LogSeverity.Error);
        public static void Exception(Exception ex)
        {
            string errormsg = $"[{DateTime.Now} at ExecptionHandler] \n{ex}";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errormsg);
            Console.ResetColor();
        }
        public static void Critical(string Source, string Message)
            => DirectLog(Source, Message, LogSeverity.Critical);
        private static Task DirectLog(string Source, string Message, LogSeverity Severity)
        {
            switch (Severity)
            {
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }
            Console.WriteLine($"[{DateTime.Now} at {Source}] {Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }


        private Task GenericLog(LogMessage logMessage)
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

        private void ExceptionLog(LogMessage logMessage)
        {
            Exception ex = logMessage.Exception;
            string errormsg = $"[{DateTime.Now} at ExecptionHandler] \n{ex}";
            
            Console.WriteLine(errormsg);
            Console.ResetColor();
        }

        private async Task ExceptionPing(Optional<CommandInfo> optional, ICommandContext context, IResult result)
        {
            if (result is ExecuteResult result_)
            {
                Exception ex = result_.Exception;
                if (ex == null) return;
                if (ex.Message == ":x: Please specify a vaild site. Available sites are (Twitter/Instagram/Snapchat).") return;

                await context.Channel.SendMessageAsync($"Catastrophic faliure! Paging {context.Guild.GetOwnerAsync().Result.Mention}.");
            }
        }

        private void SetColor(LogSeverity severity)
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
