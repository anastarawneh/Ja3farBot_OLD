using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class Suggestions : InteractiveBase<SocketCommandContext>
    {
        private SuggestionService _suggestionService;
        public Suggestions(SuggestionService suggestionService) => _suggestionService = suggestionService;

        [Command("suggest")]
        public async Task Suggest([Remainder]string text = null)
        {
            if (text == null)
            {
                await ReplyAsync(":x: Please enter a suggestion.");
                return;
            }

            SocketTextChannel channel = Context.Guild.Channels.First(x => x.Id == Data.GetChnlId("suggestions")) as SocketTextChannel;
            IEnumerable<IMessage> msgs = channel.GetMessagesAsync(1).FlattenAsync().Result;
            
            int number = 0;

            if (msgs.Count() < 1) number = 1;
            else
            {
                IUserMessage msg = msgs.First() as IUserMessage;
                number = _suggestionService.GetNumber(msg) + 1;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(Context.User);
            embed.WithTitle($"Suggestion #{number}");
            embed.WithDescription(text);
            embed.WithColor(Constants.IColors.Blurple);

            RestUserMessage usermsg = await channel.SendMessageAsync("", false, embed.Build());
            Emoji[] reactions = new Emoji[]
            {
                Constants.IEmojis.Up,
                Constants.IEmojis.Down
            };
            await usermsg.AddReactionsAsync(reactions);

            await ReplyAsync(":white_check_mark: Thanks for your suggestion!");
        }
    }
}
