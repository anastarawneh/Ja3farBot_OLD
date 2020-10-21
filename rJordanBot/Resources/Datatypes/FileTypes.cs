using System.Collections.Generic;

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
        public Announcement announcement { get; set; }
        public string mysql_server { get; set; }
        public string mysql_username { get; set; }
        public string mysql_password { get; set; }
        public string mysql_dbname { get; set; }
        public COVID covid { get; set; }

        public class Announcement
        {
            public string title { get; set; }
            public string desc { get; set; }
            public Field[] fields { get; set; }

            public class Field
            {
                public string title { get; set; }
                public string content { get; set; }
            }
        }

        public class COVID
        {
            public int locals { get; set; }
            public int amman { get; set; }
            public int irbid { get; set; }
            public int zarqa { get; set; }
            public int mafraq { get; set; }
            public int ajloun { get; set; }
            public int jerash { get; set; }
            public int madaba { get; set; }
            public int balqa { get; set; }
            public int karak { get; set; }
            public int tafileh { get; set; }
            public int maan { get; set; }
            public int aqaba { get; set; }
            public int totalcases { get; set; }
            public int recoveries { get; set; }
            public int casualties { get; set; }
            public int totalcasualties { get; set; }
        }
    }

    public class ConfigFile
    {
        public string token { get; set; }
        public ulong owner { get; set; }
        public List<ulong> reportbanned { get; set; }
        public int starboardmin { get; set; }
        public bool modappsactive { get; set; }
        public bool eventsactive { get; set; }
        public List<string> invitewhitelist { get; set; }
        public ulong verifyid { get; set; }
        public Announcement announcement { get; set; }
        public string mysql_server { get; set; }
        public string mysql_username { get; set; }
        public string mysql_password { get; set; }
        public string mysql_dbname { get; set; }
        public COVID covid { get; set; }

        public class Announcement
        {
            public string title { get; set; }
            public string desc { get; set; }
            public Field[] fields { get; set; }

            public class Field
            {
                public string title { get; set; }
                public string content { get; set; }
            }
        }

        public class COVID
        {
            public int locals { get; set; }
            public int amman { get; set; }
            public int irbid { get; set; }
            public int zarqa { get; set; }
            public int mafraq { get; set; }
            public int ajloun { get; set; }
            public int jerash { get; set; }
            public int madaba { get; set; }
            public int balqa { get; set; }
            public int karak { get; set; }
            public int tafileh { get; set; }
            public int maan { get; set; }
            public int aqaba { get; set; }
            public int totalcases { get; set; }
            public int recoveries { get; set; }
            public int casualties { get; set; }
            public int totalcasualties { get; set; }
        }
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
