using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace rJordanBot.Resources.MySQL
{
    public class Moderator
    {
        public ulong ID { get; set; }
        public ModType modType { get; set; }
        public string Timezone { get; set; }
    }
    public enum ModType
    {
        Owner = 1,
        Moderator = 2,
        FunMod = 3
    }
    /* --- */
    public class Social
    {
        public ulong UserId { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Snapchat { get; set; }
        public ulong MsgId { get; set; }
    }
    /* --- */
    public class Starboard
    {
        public ulong MsgID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong UserID { get; set; }
        public ulong SBMessageID { get; set; }
    }
    public class StarboardMessage
    {
        public IUserMessage message { get; set; }
        public SocketTextChannel channel { get; set; }
        public IUser author { get; set; }
        public int stars { get; set; }
        public ulong starboardid { get; set; }
    }
    /* --- */
    public class User
    {
        public ulong ID { get; set; }
        public bool Verified { get; set; }
        public bool SuggestionDenied { get; set; }
    }
    /* --- */
    public class Warning
    {
        public ulong WarningID { get; set; }
        public ulong UserID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong MessageID { get; set; }
        public long Timestamp { get; set; }
        public string Reason { get; set; }
        public ulong ModID { get; set; }
    }
}
