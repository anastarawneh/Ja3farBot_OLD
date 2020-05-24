using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
