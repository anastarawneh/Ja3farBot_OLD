﻿using Discord;
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
        public bool modappsactive { get; set; }
        public bool eventsactive { get; set; }
        public List<string> invitewhitelist { get; set; }
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
        public List<ModeratorInitializer> moderators { get; set; }
    }

    public class UserInitializer
    {
        public ulong id { get; set; }
        public string username { get; set; }
        public string discrim { get; set; }
        public bool verified { get; set; }
        public List<ulong> roles { get; set; }
    }

    public class ModeratorInitializer
    {
        public ulong id { get; set; }
        public string timezone { get; set; }
        public ModType modtype { get; set; }
    }
}
