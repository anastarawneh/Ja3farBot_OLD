using System.Collections.Generic;
using static rJordanBot.Resources.Datatypes.Setting;

namespace rJordanBot.Resources.Settings
{
    public static class ESettings
    {
        public static string Token;
        public static ulong Owner;
        public static List<ulong> ReportBanned;
        public static int StarboardMin;
        public static bool ModAppsActive;
        public static bool EventsActive;
        public static List<string> InviteWhitelist;
        public static ulong VerifyID;
        public static Announcement Announcement;
    }
}
