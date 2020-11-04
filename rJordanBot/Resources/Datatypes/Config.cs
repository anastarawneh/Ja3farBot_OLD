using System;
using System.Collections.Generic;
using System.Text;
using static rJordanBot.Resources.Datatypes.ConfigFile;

namespace rJordanBot.Resources.Datatypes
{
    public static class Config
    {
        public static string Token;
        public static ulong Owner;
        public static List<ulong> ReportBanned;
        public static int StarboardMin;
        public static bool ModAppsActive;
        public static bool EventsActive;
        public static List<string> InviteWhitelist;
        public static ulong VerifyID;
        public static List<string> BannedWords;
        public static Announcement Announcement;
        public static string mysql_server;
        public static string mysql_username;
        public static string mysql_password;
        public static string mysql_dbname;
    }
}
