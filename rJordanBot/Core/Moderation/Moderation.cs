using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Resources.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Moderation
{
    public class Moderation : InteractiveBase<SocketCommandContext>
    {
        [Command("reload"), Summary("Reloads the Settings.json file while running")]
        public async Task Reload()
        {
            //Checks
            if (!(Context.User.Id == ESettings.Owner))
            {
                await Context.Channel.SendMessageAsync(":x: Insufficient Permissions.");
                return;
            }

            //Execution
            await Data.Data.ReloadJSON();
        }

        [Command("userinfo")]
        [Alias("uinfo", "ui")]
        public async Task UserInfo(SocketUser param = null)
        {
            if (Context.User.Id != ESettings.Owner) return;

            if (param == null)
            {
                await ReplyAsync(":x: Please mention a user.");
                return;
            }

            IUser userInfo = param as IUser;
            IDMChannel dmchnl = await Context.User.GetOrCreateDMChannelAsync();

            string roles = "";
            List<SocketRole> roles_ = (userInfo as SocketGuildUser).Roles.ToList();
            foreach (SocketRole role in roles_)
            {
                if (role != roles_[0]) roles = roles + $"{role.Name}\n";
            }
            if (roles == "") roles = "None";

            int strikes = Data.Data.GetStrikes(userInfo.Id);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(40, 200, 150);
            embed.WithThumbnailUrl(userInfo.GetAvatarUrl());
            embed.WithTitle(userInfo.ToString());
            embed.AddField("Mention", userInfo.Mention);
            embed.AddField("ID", userInfo.Id);
            embed.AddField("Roles", roles);
            embed.AddField("Status", userInfo.Status);
            embed.AddField("Strikes", strikes);

            IUserMessage msg = await dmchnl.SendMessageAsync("", false, embed.Build());
            ulong id = msg.Id;

            EmbedBuilder embedB = msg.Embeds.FirstOrDefault().ToEmbedBuilder();
            embedB.WithFooter($"ID: {id}");
            await msg.ModifyAsync(x => x.Embed = embedB.Build());

            IEmote emote = new Emoji("✅");
            await Context.Message.AddReactionAsync(emote);
        }
    }
}
