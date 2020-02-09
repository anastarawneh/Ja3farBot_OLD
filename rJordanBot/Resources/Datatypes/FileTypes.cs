using Discord;
using Discord.WebSocket;
using rJordanBot.Core.Data;
using rJordanBot.Resources.GeneralJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace rJordanBot.Resources.Datatypes
{
    public class Setting
    {
        public string token { get; set; }
        public ulong owner { get; set; }
        public List<ulong> reportbanned { get; set; }
        public int starboardmin { get; set; }
    }

    public class e_Verified
    {
        public List<ulong> allowed { get; set; }
        public List<ulong> denied { get; set; }
    }

    public class RoleSetting
    {
        public ulong id { get; set; }
        public ulong roleid { get; set; }
        public string emote { get; set; }
        public string group { get; set; }
        public string emoji { get; set; }
    }

    public class GeneralJsonInitializer
    {
        public List<UserInitializer> users { get; set; }
    }

    public class UserInitializer
    {
        public ulong id { get; set; }
        public string username { get; set; }
        public string discrim { get; set; }
        public bool verified { get; set; }
        public List<ulong> roles { get; set; }
    }
}
