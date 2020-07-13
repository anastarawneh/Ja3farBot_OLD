using Discord;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Core.Preconditions
{
    public class RequireMod : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser user)
            {
                if (user.IsModerator()) return Task.FromResult(PreconditionResult.FromSuccess());
                else return Task.FromResult(PreconditionResult.FromError(Constants.IMacros.NoPerms));
            }
            else return Task.FromResult(PreconditionResult.FromError(Constants.IMacros.NoGuild));
        }
    }

    public class RequireFuncMod : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser user)
            {
                if (user.IsFuncModerator()) return Task.FromResult(PreconditionResult.FromSuccess());
                else return Task.FromResult(PreconditionResult.FromError(Constants.IMacros.NoPerms));
            }
            else return Task.FromResult(PreconditionResult.FromError(Constants.IMacros.NoGuild));
        }
    }

    public class RequireBot : PreconditionAttribute
    { 
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == context.Client.CurrentUser.Id) return Task.FromResult(PreconditionResult.FromSuccess());
            else return Task.FromResult(PreconditionResult.FromError("User is not the bot."));
        }
    }

    public class RequireBotChannel : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!(context.Channel.Id == Data.GetChnlId("bot-commands"))
                && !(context.User.Id == ESettings.Owner)
                && !(context.User.Id == 362299141587599360)
                && !(context.User as SocketGuildUser).IsModerator()
                && !(context.Channel is IDMChannel)) return Task.FromResult(PreconditionResult.FromError("This command is restricted to #bot-commands."));
            else return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    public class MinecraftCommand : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel.Id == Data.GetChnlId("mc-chat")
                && !(context.User.Id == ESettings.Owner)
                && !(context.User.Id == 362299141587599360)
                && !(context.User as SocketGuildUser).IsModerator()) return Task.FromResult(PreconditionResult.FromError("This command is restricted to #mc-chat."));
            else return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
