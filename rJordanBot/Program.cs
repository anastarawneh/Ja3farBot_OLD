using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MulticraftLib;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.MySQL;
using rJordanBot.Resources.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;

namespace rJordanBot
{
    class Program
    {
        static void Main(/*string[] args*/)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _cmdService;
        private IServiceProvider _provider;
        private LavaConfig _lavaConfig;

        private async Task MainAsync()
        {
            await Data.InitYML();
            await MySQL.Initialize();
            if (!Multicraft.Initialize(Environment.GetEnvironmentVariable("SystemType"), BotType.Ja3farBot))
            {
                Console.WriteLine("FALSE");
                return;
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true,
                ExclusiveBulkDelete = true
            });

            _cmdService = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Info
            });

            _lavaConfig = new LavaConfig
            {
                LogSeverity = LogSeverity.Debug,
                SelfDeaf = false
            };

            _client.MessageReceived += Client_CommandHandler;
            _client.Ready += Client_Ready;
            _cmdService.CommandExecuted += SocialsExceptionHandler;
            _client.MessageReceived += Bot_CommandHandler;

            await _client.LoginAsync(TokenType.Bot, Config.Token);
            await _client.StartAsync();

            _provider = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_cmdService)
                .AddSingleton<InteractiveService>()
                .AddSingleton<LavaNode>()
                .AddSingleton(_lavaConfig)
                .AddSingleton<MusicService>()
                .AddSingleton<LoggerService>()
                .AddSingleton<EventHandlers>()
                .AddSingleton<LogEventHandlers>()
                .AddSingleton<SuggestionService>()
                .AddSingleton<AutomodService>()
                .AddSingleton<CustomVCService>()
                .AddSingleton<WebSocketService>()
                .BuildServiceProvider();

            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            if (Config.Music) await _provider.GetRequiredService<MusicService>().Initialize();
            await _provider.GetRequiredService<LoggerService>().Initialize();
            await _provider.GetRequiredService<EventHandlers>().Initialize();
            await _provider.GetRequiredService<LogEventHandlers>().Initialize();
            await _provider.GetRequiredService<SuggestionService>().Initialize();
            await _provider.GetRequiredService<AutomodService>().Initialize();
            await _provider.GetRequiredService<CustomVCService>().Initialize();
            _provider.GetRequiredService<WebSocketService>().Initialize();

            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            await _client.SetStatusAsync(UserStatus.Online);
            if (Environment.GetEnvironmentVariable("SystemType") == "aws") await Data.SetListeningStatus(_client, true);
            else await Data.SetListeningStatus(_client, false);

            await Data.SetInvitesBefore(Constants.IGuilds.Jordan(_client).Users.FirstOrDefault(x => x.Id == _client.CurrentUser.Id));
        }

        private async Task Client_CommandHandler(SocketMessage MessageParam)
        {
            if (MessageParam is SocketSystemMessage) return;
            SocketUserMessage Message = MessageParam as SocketUserMessage;
            SocketCommandContext Context = new SocketCommandContext(_client, Message);
            int ArgPos = 0;

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;

            if (_client.Status == UserStatus.DoNotDisturb && Context.User != Constants.IGuilds.Jordan(Context).Owner) return;
            if (MessageParam.Content.EndsWith('^')) return;

            if (!(Message.HasStringPrefix("^", ref ArgPos)/* || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos)*/)) return;

            IResult Result = await _cmdService.ExecuteAsync(Context, ArgPos, _provider);
            if (!Result.IsSuccess)
            {
                await _provider.GetRequiredService<LoggerService>().CommandErrorLog(Message, Result);
                await MessageParam.Channel.SendMessageAsync($":x: Command error: `{Result.ErrorReason}`");
            }
        }

        public async Task SocialsExceptionHandler(Optional<CommandInfo> optional, ICommandContext context, IResult result)
        {
            if (result is ExecuteResult Result)
            {
                Exception ex = Result.Exception;
                if (Result.Exception == null) return;

                if (ex.Message == ":x: Please specify a vaild site. Available sites are (Twitter/Instagram/Snapchat).")
                {
                    await context.Channel.SendMessageAsync(ex.Message);
                    return;
                }
            }
        }

        public async Task Bot_CommandHandler(SocketMessage message)
        {
            if (message is SocketSystemMessage) return;
            SocketUserMessage Message = message as SocketUserMessage;
            SocketCommandContext Context = new SocketCommandContext(_client, Message);
            string[] commands = { "resetchannels", "mutefix" };

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.Id != _client.CurrentUser.Id) return;
            if (Context.Channel.Id != Data.GetChnlId("commands")) return;
            if (!commands.Contains(Context.Message.Content.Substring(2).Split(' ')[0])) return;

            int ArgPos = 0;
            if (!Message.HasStringPrefix("^^", ref ArgPos)) return;

            IResult Result = await _cmdService.ExecuteAsync(Context, ArgPos, _provider);
            if (!Result.IsSuccess)
            {
                await _provider.GetRequiredService<LoggerService>().CommandErrorLog(Message, Result);
            }
        }
    }
}
