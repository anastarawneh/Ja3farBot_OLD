﻿using System.Collections.Generic;

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
        public ulong verifyid { get; set; }
    }

    public class RoleSetting
    {
        public ulong id { get; set; }
        public ulong roleid { get; set; }
        public string emote { get; set; }
        public string group { get; set; }
        public string emoji { get; set; }
    }
}
