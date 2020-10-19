using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Preconditions;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.Services;
using System.Threading.Tasks;

namespace rJordanBot.Core.Moderation
{
    public class SuggestionMod : InteractiveBase<SocketCommandContext>
    {
        private SuggestionService _suggestionService;
        public SuggestionMod(SuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        [Command("suggestmodhelp")]
        [Alias("smodhelp")]
        [RequireMod]
        public async Task SuggestModHelp()
        {
            await ReplyAsync(
                ":question: Suggestion mod commands:\n" +
                "``^suggestmodhelp``: Displays this message.\n" +
                "``^suggestinfo <number>``: Displays information about a suggestion.\n" +
                "*``^approve <number> [reason]``: Approves a suggestion for an optional reason.*\n" +
                "*``^deny <number> [reason]``: Denies a suggestion for an optional reason.*\n" +
                "*``^implemented <number> [reason]``: Marks a suggestion as implemented for an optional reason.*\n" +
                "*``^consider <number> [reason]``: Considers a suggestion for an optional reason.*\n" +
                $"*Commands with asterisks are functional mod only.*"
            );
        }

        [Command("suggestinfo")]
        [RequireMod]
        public async Task SuggestInfo(int num)
        {
            if (num < 7)
            {
                await ReplyAsync(":x: Due to some stupid mistake, our suggestions start at 7.");
                return;
            }

            Suggestion suggestion = _suggestionService.GetSuggestion(num);
            string result = $"```" +
                $"Number: {suggestion.number}\n" +
                $"Author: {suggestion.author}\n" +
                $"Message: {suggestion.suggestion}\n" +
                $"Jordan: {_suggestionService.IsOwnSuggestion(suggestion)}\n" +
                $"State: {suggestion.state}\n" +
                $"Reason: {suggestion.reason}\n" +
                $"Mod: {suggestion.mod}" +
                $"```";

            await ReplyAsync(result);
        }

        [Command("approve")]
        [RequireFuncMod]
        public async Task Approve(int num, [Remainder] string reason = "No reason given")
        {
            if (num < 7)
            {
                await ReplyAsync(":x: Due to some stupid mistake, our suggestions start at 7.");
                return;
            }

            await ReplyAsync(_suggestionService.Approve(_suggestionService.GetSuggestion(num), Context.User as SocketGuildUser, reason).Result);
        }

        [Command("deny")]
        [RequireFuncMod]
        public async Task Deny(int num, [Remainder] string reason = "No reason given")
        {
            if (num < 7)
            {
                await ReplyAsync(":x: Due to some stupid mistake, our suggestions start at 7.");
                return;
            }

            await ReplyAsync(_suggestionService.Deny(_suggestionService.GetSuggestion(num), Context.User as SocketGuildUser, reason).Result);
        }

        [Command("implemented")]
        [RequireFuncMod]
        public async Task Implemented(int num, [Remainder] string reason = "No reason given")
        {
            if (num < 7)
            {
                await ReplyAsync(":x: Due to some stupid mistake, our suggestions start at 7.");
                return;
            }

            await ReplyAsync(_suggestionService.Implemented(_suggestionService.GetSuggestion(num), Context.User as SocketGuildUser, reason).Result);
        }

        [Command("consider")]
        [RequireFuncMod]
        public async Task Consider(int num, [Remainder] string reason = "No reason given")
        {
            if (num < 7)
            {
                await ReplyAsync(":x: Due to some stupid mistake, our suggestions start at 7.");
                return;
            }

            await ReplyAsync(_suggestionService.Consider(_suggestionService.GetSuggestion(num), Context.User as SocketGuildUser, reason).Result);
        }
    }
}
