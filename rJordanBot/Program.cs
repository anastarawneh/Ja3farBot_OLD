using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using rJordanBot.Core.Data;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.GeneralJSON;
using rJordanBot.Resources.Services;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
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

        private DiscordSocketClient Client;
        private CommandService Commands;
        private IServiceProvider Services;

        private async Task MainAsync()
        {
            await Data.InitJSON();

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Info
            });

            // Main Handlers
            {
                Client.MessageReceived += Client_CommandHandler;
                Client.Ready += Client_Ready;
                Client.Log += Client_Log;
                Commands.CommandExecuted += CommandExceptionHandler;
            }

            // Secondary Handlers
            {
                EventHandlers eventHandlers = new EventHandlers(Client);

                Client.ReactionAdded += eventHandlers.Roles_ReactionAdded;
                Client.ReactionAdded += eventHandlers.Events_ReactionAdded;
                Client.UserJoined += eventHandlers.Invites_UserJoined;
                Client.ReactionAdded += eventHandlers.Starboard_ReactionAddedOrRemoved;
                Client.ReactionRemoved += eventHandlers.Starboard_ReactionAddedOrRemoved;
                Client.MessageReceived += Bot_CommandHandler;
                Client.UserLeft += eventHandlers.JSON_UserLeft;
                Client.MessageReceived += eventHandlers.InviteDeletion;
                Client.Ready += eventHandlers.MuteFixing;
                Client.UserJoined += eventHandlers.JoinVerification;
            }

            // Log Handlers
            {
                LogEventHandlers logEventHandlers = new LogEventHandlers(Client);

                Client.MessageUpdated += logEventHandlers.MessageEdited;
                Client.MessageDeleted += logEventHandlers.MessageDeleted;
                Client.UserUpdated += logEventHandlers.NameOrDiscrimChanged;
                Client.GuildMemberUpdated += logEventHandlers.RoleAdded;
                Client.GuildMemberUpdated += logEventHandlers.RoleRemoved;
                Client.GuildMemberUpdated += logEventHandlers.NicknameChanged;
                Client.ChannelCreated += logEventHandlers.ChannelCreated;
                Client.ChannelDestroyed += logEventHandlers.ChannelDestroyed;
                Client.UserJoined += logEventHandlers.UserJoined;
                Client.UserLeft += logEventHandlers.UserLeft;
                Client.ChannelUpdated += logEventHandlers.ChannelNameChanged;
                Client.MessagesBulkDeleted += logEventHandlers.MessagesBulkDeleted;
            }

            await Client.LoginAsync(TokenType.Bot, ESettings.Token);
            await Client.StartAsync();

            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<InteractiveService>()
                .AddSingleton<LavaRestClient>()
                .AddSingleton<LavaSocketClient>()
                .AddSingleton<MusicService>()
                .BuildServiceProvider();

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

            await Services.GetRequiredService<MusicService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private async Task Client_CommandHandler(SocketMessage MessageParam)
        {
            if (MessageParam is SocketSystemMessage) return;
            SocketUserMessage Message = MessageParam as SocketUserMessage;
            SocketCommandContext Context = new SocketCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;
            if (!(Context.Channel.Id == Data.GetChnlId("bot-commands")) && !(Context.User.Id == ESettings.Owner) && !(Context.User.Id == 362299141587599360) && !(Context.User as SocketGuildUser).IsModerator() && !(Context.Channel is IDMChannel)) return;
            if (Environment.GetEnvironmentVariable("SystemType") == "win" && Context.User != Constants.IGuilds.Jordan(Context).Owner) return;

            int ArgPos = 0;
            if (!(Message.HasStringPrefix("^", ref ArgPos)/* || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos)*/)) return;

            IResult Result = await Commands.ExecuteAsync(Context, ArgPos, Services);
            if (!Result.IsSuccess)
            {
                await Command_Log_Message(Message, Result);
            }
        }

        private async Task Client_Ready()
        {
            await Client.SetStatusAsync(UserStatus.Online);
            if (Environment.GetEnvironmentVariable("SystemType") == "aws") await Client.SetGameAsync("^help", null, ActivityType.Listening);
            else
            {
                await Client.SetStatusAsync(UserStatus.DoNotDisturb);
                await Client.SetGameAsync("In Maintenance! Not listening.", null, ActivityType.Playing);
            };

            await Data.SetInvitesBefore(Constants.IGuilds.Jordan(Client).Users.FirstOrDefault(x => x.Id == Client.CurrentUser.Id));

            /*if (Client.Guilds.First().Roles.First(x => x.Name == "Muted").Members.Count() > 0)
            {
                IEnumerable<SocketGuildUser> muteds = Client.Guilds.First().Roles.First(x => x.Name == "Muted").Members;
                if (muteds.Count() == 1) await (Client.Guilds.First().Channels.First(x => x.Id == Data.GetChnlId("mod-commands")) as SocketTextChannel).SendMessageAsync($"{Client.Guilds.First().Owner.Mention}, there is a muted user right now, and I've lost track of the time: {muteds.First().Mention}");
                else
                {
                    string users = "";
                    foreach (SocketGuildUser muted in muteds)
                    {
                        users += $"{muted.Mention} ";
                    }
                    await (Client.Guilds.First().Channels.First(x => x.Id == Data.GetChnlId("mod-commands")) as SocketTextChannel).SendMessageAsync($"{Client.Guilds.First().Owner.Mention}, there are muted users right now, and I've lost track of the time: {users}");
                }
            }*/
            /*
#if !DEBUG
            Console.WriteLine("RELEASE");
#endif

#if DEBUG
            Console.WriteLine("DEBUG");
#endif
            */
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
                
                string errormsg = $"[{DateTime.Now} at ExceptionHandler]\n" +
                $"```{ex}```";

                if (ex.ToString().Contains("Server requested a reconnect")) errormsg = $"[{DateTime.Now} at ExceptionHandler] Server requested a reconnect";

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(errormsg);
                Console.ResetColor();

                DiscordSocketClient Client = context.Client as DiscordSocketClient;
                SocketGuild Guild = Constants.IGuilds.Jordan(Client);
                SocketTextChannel Channel = Guild.Channels.First(x => x.Id == Data.GetChnlId("bot-log")) as SocketTextChannel;
                await Channel.SendMessageAsync(errormsg);
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

            SocketGuild Guild = Constants.IGuilds.Jordan(Client);
            SocketTextChannel Channel = Guild.Channels.Where(x => x.Id == Data.GetChnlId("bot-log")).FirstOrDefault() as SocketTextChannel;

            await Channel.SendMessageAsync(errormsg);
        }


        public async Task Bot_CommandHandler(SocketMessage message)
        {
            if (message is SocketSystemMessage) return;
            SocketUserMessage Message = message as SocketUserMessage;
            SocketCommandContext Context = new SocketCommandContext(Client, Message);
            string[] commands = { "resetchannels" };

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.Id != Client.CurrentUser.Id) return;
            if (Context.Channel.Id != Data.GetChnlId("commands")) return;
            if (!commands.Contains(Context.Message.Content.Substring(2))) return;

            int ArgPos = 0;
            if (!Message.HasStringPrefix("^^", ref ArgPos)) return;

            IResult Result = await Commands.ExecuteAsync(Context, ArgPos, Services);
            if (!Result.IsSuccess)
            {
                await Command_Log_Message(Message, Result);
            }
        }
    }
}
