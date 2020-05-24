﻿using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Preconditions;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.Services;
using System;
using System.Collections.Generic;
using System.Text;
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

        [Group("suggestmod")]
        [RequireMod]
        public class SuggestMod : InteractiveBase<SocketCommandContext>
        {
            private SuggestionService _suggestionService;
            public SuggestMod(SuggestionService suggestionService)
            {
                _suggestionService = suggestionService;
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
        public async Task Approve(int num, [Remainder]string reason = "No reason given")
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
        public async Task Deny(int num, [Remainder]string reason = "No reason given")
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
        public async Task Implemented(int num, [Remainder]string reason = "No reason given")
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
        public async Task Consider(int num, [Remainder]string reason = "No reason given")
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