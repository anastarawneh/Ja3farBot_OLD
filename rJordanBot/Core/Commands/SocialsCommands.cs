using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class SocialsCommands : InteractiveBase<SocketCommandContext>
    {
        [Group("socials"), Alias("social")]
        public class SocialsGroup : InteractiveBase<SocketCommandContext>
        {
            [Command(""), Alias("help")]
            public async Task Help()
            {
                await ReplyAsync(
                    ":question: Socials commands:\n" +
                    "``^socials help``: Displays this message.\n" +
                    "``^socials set <site> <profile>``: Sets your social profile."
                );
            }

            [Command("set"), Alias("add")]
            public async Task Set(string site = null, string link = null)
            {
                //Checks
                if (site == null)
                {
                    await ReplyAsync(":x: Please specify a vaild site. Available sites are (Twitter/Instagram/Snapchat).");
                    return;
                }
                //Execution
                await ReplyAsync(":white_check_mark: Your socials have been updated.");
                //Saving
                try
                {
                    await Data.Data.SetSocials(Context.User.Id, site, link, Context);
                }
                catch (Exception ex)
                {
                    if (ex.Message == ":x: Please specify a vaild site. Available sites are (Twitter/Instagram/Snapchat).")
                    {
                        await ReplyAsync(ex.Message);
                    }
                    else
                    {
                        Program program = new Program();
                        await program.Command_Log_Message(Context.Message, ex.Message);
                    }
                }
            }
        }
    }
}
