using Discord;
using Discord.WebSocket;

namespace rJordanBot.Resources.MySQL
{
    public class Channel
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
        public ChannelType ChannelType { get; set; }
    }
    public class ChannelType
    {
        private string Value;
        private ChannelType(string value)
        {
            Value = value;
        }

        public static implicit operator ChannelType(string s)
        {
            return new ChannelType(s);
        }

        public static ChannelType TextChannel { get { return new ChannelType("TextChannel"); } }
        public static ChannelType VoiceChannel { get { return new ChannelType("VoiceChannel"); } }
        public static ChannelType NewsChannel { get { return new ChannelType("NewsChannel"); } }
        public static ChannelType CategoryChannel { get { return new ChannelType("CategoryChannel"); } }
    }
    /* --- */
    public class CustomVC
    {
        public ulong UserID { get; set; }
        public ulong ChannelID { get; set; }
        public int Slots { get; set; }
        public int Bitrate { get; set; }

        public CustomVC() { }
        public CustomVC(ulong userID)
        {
            UserID = userID;
            ChannelID = 0;
            Slots = 0;
            Bitrate = 64000;
        }
    }
    /* --- */
    public class Invite
    {
        public string Code { get; set; }
        public ulong UserID { get; set; }
        public int Uses { get; set; }
    }
    /* --- */
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
        public bool EventVerified { get; set; }
        public bool SuggestionDenied { get; set; }
    }
    /* --- */
    public class UserInvite
    {
        public ulong UserID { get; set; }
        public string Code { get; set; }
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
