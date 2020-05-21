using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Internal;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Resources.Services
{
    public class SuggestionService
    {
        private DiscordSocketClient _client;
        private ITextChannel channel;
        private ITextChannel logChannel;

        public SuggestionService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task Initialize()
        {
            _client.GuildAvailable += OnGuildLoaded;

            return Task.CompletedTask;
        }

        public Task OnGuildLoaded(SocketGuild guild)
        {
            channel = _client.Guilds.First().Channels.First(x => x.Id == Data.GetChnlId("suggestions")) as ITextChannel;
            logChannel = _client.Guilds.First().Channels.First(x => x.Id == Data.GetChnlId("suggestion-log")) as ITextChannel;
            _client.GuildAvailable -= OnGuildLoaded;
            return Task.CompletedTask;
        }


        public async Task<string> Approve(Suggestion suggestion, SocketGuildUser mod, string reason)
        {
            if (suggestion.state == SuggestionState.Approved) return $":x: Suggestion #{suggestion.number} is already approved.";
            EmbedBuilder embed = suggestion.message.Embeds.First().ToEmbedBuilder();

            if (suggestion.state != SuggestionState.Normal) 
            {
                embed.Fields.Clear();
            }
            embed.AddField($"Reason from {mod}", reason);
            embed.WithColor(Constants.IColors.ISuggestionColors.Green);
            embed.WithTitle($"Suggestion #{suggestion.number} Approved");

            await logChannel.SendMessageAsync("", false, embed.Build());
            await suggestion.message.ModifyAsync(x => x.Embed = embed.Build());
            return $":white_check_mark: Approved suggestion #{suggestion.number}.";
        }
        public async Task<string> Deny(Suggestion suggestion, SocketGuildUser mod, string reason)
        {
            if (suggestion.state == SuggestionState.Denied) return $":x: Suggestion #{suggestion.number} is already denied.";
            EmbedBuilder embed = suggestion.message.Embeds.First().ToEmbedBuilder();

            if (suggestion.state != SuggestionState.Normal)
            {
                embed.Fields.Clear();
            }
            embed.AddField($"Reason from {mod}", reason);
            embed.WithColor(Constants.IColors.ISuggestionColors.Red);
            embed.WithTitle($"Suggestion #{suggestion.number} Denied");

            await logChannel.SendMessageAsync("", false, embed.Build());
            await suggestion.message.ModifyAsync(x => x.Embed = embed.Build());
            return $":white_check_mark: Denied suggestion #{suggestion.number}.";
        }
        public async Task<string> Implemented(Suggestion suggestion, SocketGuildUser mod, string reason)
        {
            if (suggestion.state == SuggestionState.Implemented) return $":x: Suggestion #{suggestion.number} is already implemented.";
            EmbedBuilder embed = suggestion.message.Embeds.First().ToEmbedBuilder();

            if (suggestion.state != SuggestionState.Normal)
            {
                embed.Fields.Clear();
            }
            embed.AddField($"Reason from {mod}", reason);
            embed.WithColor(Constants.IColors.ISuggestionColors.Turquise);
            embed.WithTitle($"Suggestion #{suggestion.number} Implemented");

            await logChannel.SendMessageAsync("", false, embed.Build());
            await suggestion.message.ModifyAsync(x => x.Embed = embed.Build());
            return $":white_check_mark: Implemented suggestion #{suggestion.number}.";
        }
        public async Task<string> Consider(Suggestion suggestion, SocketGuildUser mod, string reason)
        {
            if (suggestion.state == SuggestionState.Considered) return $":x: Suggestion #{suggestion.number} is already considered.";
            EmbedBuilder embed = suggestion.message.Embeds.First().ToEmbedBuilder();

            if (suggestion.state != SuggestionState.Normal)
            {
                embed.Fields.Clear();
            }
            embed.AddField($"Reason from {mod}", reason);
            embed.WithColor(Constants.IColors.ISuggestionColors.Yellow);
            embed.WithTitle($"Suggestion #{suggestion.number} Considered");

            await logChannel.SendMessageAsync("", false, embed.Build());
            await suggestion.message.ModifyAsync(x => x.Embed = embed.Build());
            return $":white_check_mark: Considered suggestion #{suggestion.number}.";
        }


        public Suggestion GetSuggestion(int num)
        {
            IEnumerable<IMessage> msgs = channel.GetMessagesAsync(100).FlattenAsync().Result;
            IUserMessage msg = msgs.First(x => x.Embeds.First().Title.Contains($"Suggestion #{num}")) as IUserMessage;

            Suggestion result = new Suggestion
            {
                author = msg.Embeds.First().Author.Value.Name,
                message = msg,
                number = num,
                suggestion = msg.Embeds.First().Description
            };

            result.state = GetState(result);
            result.reason = GetReason(result);
            result.mod = GetMod(result);

            return result;
        }

        public bool IsOwnSuggestion(Suggestion suggestion)
        {
            if (suggestion.message.Author == _client.CurrentUser) return true;
            return false;
        }

        public int GetNumber(IUserMessage msg)
        {
            string title = msg.Embeds.First().Title;
            string num = title.Split('#')[1].Split(' ')[0];
            return int.Parse(num);
        }

        public SuggestionState GetState(Suggestion suggestion)
        {
            string title = suggestion.message.Embeds.First().Title;
            if (title.Contains("Approved")) return SuggestionState.Approved;
            if (title.Contains("Denied")) return SuggestionState.Denied;
            if (title.Contains("Considered")) return SuggestionState.Considered;
            if (title.Contains("Implemented")) return SuggestionState.Implemented;
            return SuggestionState.Normal;
        }

        public string GetReason(Suggestion suggestion)
        {
            if (suggestion.state == SuggestionState.Normal) return "NULL";
            return suggestion.message.Embeds.First().Fields.First().Value;
        }

        public string GetMod(Suggestion suggestion)
        {
            if (suggestion.state == SuggestionState.Normal) return "NULL";
            return suggestion.message.Embeds.First().Fields.First().Name.Substring(12);
        }
    }
}
