using Discord;
using Discord.WebSocket;
using Org.BouncyCastle.Asn1.Mozilla;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.MySQL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Resources.Services
{
    public class AutomodService
    {
        private DiscordSocketClient _client;
        
        public AutomodService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task Initialize()
        {
            _client.MessageReceived += DeleteBannedWords;
            
            return Task.CompletedTask;
        }

        public async Task DeleteBannedWords(SocketMessage message)
        {
            if (message is SocketSystemMessage) return;
            SocketUserMessage Message = message as SocketUserMessage;
            if (Message.Author.IsBot) return;
            if (!(Message.Channel is SocketGuildChannel)) return;
            if ((Message.Author as SocketGuildUser).IsModerator()) return;
            foreach (string phrase in Config.BannedWords)
            {
                if (Message.Content.Contains(phrase))
                {
                    await Message.DeleteAsync();
                    goto cont;
                }
            }
            return;

        cont:
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Automod Action");
            embed.WithColor(Constants.IColors.Red);
            embed.WithAuthor(Message.Author);
            embed.WithDescription($"Message sent in {MentionUtils.MentionChannel(Message.Channel.Id)}");
            embed.AddField("Message", Message.Content);
            embed.AddField("Reason", "Banned word");
            embed.WithFooter($"MessageID: {Message.Id}");
            embed.WithCurrentTimestamp();

            SocketTextChannel channel = Constants.IGuilds.Jordan(_client).GetTextChannel(Data.GetChnlId("automod-log"));
            await channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
