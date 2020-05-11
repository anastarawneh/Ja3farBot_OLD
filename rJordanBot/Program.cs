using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Services;
using rJordanBot.Resources.Settings;
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
            await Data.InitJSON();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true
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

            // Main Handlers
            {
                _client.MessageReceived += Client_CommandHandler;
                _client.Ready += Client_Ready;
                //_client.Log += Client_Log;
                //_cmdService.Log += Client_Log;
                _cmdService.CommandExecuted += SocialsExceptionHandler;
            }

            // Secondary Handlers
            {
                EventHandlers eventHandlers = new EventHandlers(_client);

                _client.ReactionAdded += eventHandlers.Roles_ReactionAdded;
                _client.ReactionAdded += eventHandlers.Events_ReactionAdded;
                _client.UserJoined += eventHandlers.Invites_UserJoined;
                _client.ReactionAdded += eventHandlers.Starboard_ReactionAddedOrRemoved;
                _client.ReactionRemoved += eventHandlers.Starboard_ReactionAddedOrRemoved;
                _client.MessageReceived += Bot_CommandHandler;
                _client.UserLeft += eventHandlers.JSON_UserLeft;
                _client.MessageReceived += eventHandlers.InviteDeletion;
                _client.Ready += eventHandlers.MuteFixing;
                _client.UserJoined += eventHandlers.JoinVerification;
            }

            // Log Handlers
            {
                LogEventHandlers logEventHandlers = new LogEventHandlers(_client);

                _client.MessageUpdated += logEventHandlers.MessageEdited;
                _client.MessageDeleted += logEventHandlers.MessageDeleted;
                _client.UserUpdated += logEventHandlers.NameOrDiscrimChanged;
                _client.GuildMemberUpdated += logEventHandlers.RoleAdded;
                _client.GuildMemberUpdated += logEventHandlers.RoleRemoved;
                _client.GuildMemberUpdated += logEventHandlers.NicknameChanged;
                _client.ChannelCreated += logEventHandlers.ChannelCreated;
                _client.ChannelDestroyed += logEventHandlers.ChannelDestroyed;
                _client.UserJoined += logEventHandlers.UserJoined;
                _client.UserLeft += logEventHandlers.UserLeft;
                _client.ChannelUpdated += logEventHandlers.ChannelNameChanged;
                _client.MessagesBulkDeleted += logEventHandlers.MessagesBulkDeleted;
            }

            await _client.LoginAsync(TokenType.Bot, ESettings.Token);
            await _client.StartAsync();

            _provider = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_cmdService)
                .AddSingleton<InteractiveService>()
                .AddSingleton<LavaNode>()
                .AddSingleton(_lavaConfig)
                .AddSingleton<MusicService>()
                .BuildServiceProvider();

            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            await _provider.GetRequiredService<MusicService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            await _client.SetStatusAsync(UserStatus.Online);
            if (Environment.GetEnvironmentVariable("SystemType") == "aws") await _client.SetGameAsync("^help", null, ActivityType.Listening);
            else
            {
                await _client.SetStatusAsync(UserStatus.DoNotDisturb);
                await _client.SetGameAsync("In Maintenance! Not listening.", null, ActivityType.Playing);
            };

            await Data.SetInvitesBefore(Constants.IGuilds.Jordan(_client).Users.FirstOrDefault(x => x.Id == _client.CurrentUser.Id));
        }

        private async Task Client_CommandHandler(SocketMessage MessageParam)
        {
            if (MessageParam is SocketSystemMessage) return;
            SocketUserMessage Message = MessageParam as SocketUserMessage;
            SocketCommandContext Context = new SocketCommandContext(_client, Message);

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;
            if (!(Context.Channel.Id == Data.GetChnlId("bot-commands")) && !(Context.User.Id == ESettings.Owner) && !(Context.User.Id == 362299141587599360) && !(Context.User as SocketGuildUser).IsModerator() && !(Context.Channel is IDMChannel)) return;
            if (Environment.GetEnvironmentVariable("SystemType") == "win" && Context.User != Constants.IGuilds.Jordan(Context).Owner) return;

            int ArgPos = 0;
            if (!(Message.HasStringPrefix("^", ref ArgPos)/* || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos)*/)) return;

            IResult Result = await _cmdService.ExecuteAsync(Context, ArgPos, _provider);
            if (!Result.IsSuccess)
            {
                await Command_Log_Message(Message, Result);
            }
        }

        public async Task Client_Log(LogMessage Message)
        {
            switch (Message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    await Log_Message(Message, true);
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    await Log_Message(Message, true);
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    await Log_Message(Message, true);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    if (Environment.GetEnvironmentVariable("SystemType") == "aws")
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else Console.ForegroundColor = ConsoleColor.DarkGray;
                    await Log_Message(Message, false);
                    break;
            }
        }


        public async Task Log_Message(LogMessage message, bool WithAsync)
        {
            string errormsg = $"[{DateTime.Now} at {message.Source}] {message.Message}";
            string errormsg_ = $"[{DateTime.Now} at {message.Source}] {message.Message}";
            if (message.Severity == LogSeverity.Warning) errormsg_ = $"[{DateTime.Now} at {message.Source}] **{message.Message}**";

            if (message.Exception != null)
            {
                await LogExceptionHandler(message.Exception);
                return;
            }

            Console.WriteLine(errormsg);
            Console.ResetColor();

            SocketGuild Guild = Constants.IGuilds.Jordan(Client);
            SocketTextChannel Channel = Guild.Channels.Where(x => x.Id == 642475027123404811).FirstOrDefault() as SocketTextChannel;

            if (WithAsync == true)
            {
                await Channel.SendMessageAsync(errormsg_);
            }
        }

        public async Task CommandExceptionHandler(Optional<CommandInfo> optional, ICommandContext context, IResult result)
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

                await context.Channel.SendMessageAsync($"Catastrophic faliure! Paging {context.Guild.GetOwnerAsync().Result.Mention}.");

                /*string errormsg = $"[{DateTime.Now} at ExceptionHandler]\n" +
                $"```{ex}```";

                if (ex.ToString().Contains("Server requested a reconnect")) errormsg = $"[{DateTime.Now} at ExceptionHandler] Server requested a reconnect";

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(errormsg);
                Console.ResetColor();

                DiscordSocketClient Client = context.Client as DiscordSocketClient;
                SocketGuild Guild = Constants.IGuilds.Jordan(Client);
                SocketTextChannel Channel = Guild.Channels.First(x => x.Id == Data.GetChnlId("bot-log")) as SocketTextChannel;
                await Channel.SendMessageAsync(errormsg);*/
            }
        }
        public async Task LogExceptionHandler(Exception ex)
        {
            string errormsg = $"[{DateTime.Now} at ExecptionHandler] \n" +
                $"```{ex}```";
            if (ex.ToString().Contains("Server requested a reconnect")) errormsg = $"[{DateTime.Now} at ExceptionHandler] Server requested a reconnect";
            if (ex.ToString().Contains("WebSocket connection was closed")) errormsg = $"[{DateTime.Now} at ExceptionHandler] WebSocket connection was closed";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(errormsg);
            Console.ResetColor();

            SocketGuild Guild = Client.Guilds.First();
            SocketTextChannel Channel = Guild.Channels.First(x => x.Id == Data.GetChnlId("bot-log")) as SocketTextChannel;
            await Channel.SendMessageAsync(errormsg);
        }

        public async Task Command_Log_Message(SocketUserMessage message, IResult result)
        {
            string errormsg = $"[{DateTime.Now} at Commands] Command error: | Command: {message.Content} | User: {message.Author} | Reason: {result.ErrorReason}\nError: {result.Error}";

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(errormsg);
            Console.ResetColor();

            SocketGuild Guild = Constants.IGuilds.Jordan(_client);
            SocketTextChannel Channel = Guild.Channels.Where(x => x.Id == Data.GetChnlId("bot-log")).FirstOrDefault() as SocketTextChannel;

            await Channel.SendMessageAsync(errormsg);
        }

        public async Task Bot_CommandHandler(SocketMessage message)
        {
            if (message is SocketSystemMessage) return;
            SocketUserMessage Message = message as SocketUserMessage;
            SocketCommandContext Context = new SocketCommandContext(_client, Message);
            string[] commands = { "resetchannels" };

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.Id != _client.CurrentUser.Id) return;
            if (Context.Channel.Id != Data.GetChnlId("commands")) return;
            if (!commands.Contains(Context.Message.Content.Substring(2))) return;

            int ArgPos = 0;
            if (!Message.HasStringPrefix("^^", ref ArgPos)) return;

            IResult Result = await _cmdService.ExecuteAsync(Context, ArgPos, _provider);
            if (!Result.IsSuccess)
            {
                await Command_Log_Message(Message, Result);
            }
        }
    }
}
