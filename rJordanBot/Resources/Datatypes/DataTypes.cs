using Discord;
using Discord.WebSocket;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Resources.Datatypes
{
    public class DateTimeSpan
    {
        public int Years { get; set; }
        public int Months { get; set; }
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }

        public string Duration()
        {
            string Y = "";
            string M = "";
            string D = "";
            string h = "";
            string m = "";
            string s = "";
            string result = "";

            if (Years == 0 && Months == 0 && Days == 0 && Hours == 0 && Minutes == 0)
            {
                if (Seconds == 1) s = $"{Seconds} second";
                else s = $"{Seconds} seconds";

                result = s;
            }
            else if (Years == 0 && Months == 0 && Days == 0 && Hours == 0)
            {
                if (Seconds == 1) s = $"{Seconds} second";
                else s = $"{Seconds} seconds";
                if (Minutes == 1) m = $"{Minutes} minute";
                else m = $"{Minutes} minutes";
                result = $"{m} & {s}";
            }
            else if (Years == 0 && Months == 0 && Days == 0)
            {
                if (Seconds == 1) s = $"{Seconds} second";
                else s = $"{Seconds} seconds";
                if (Minutes == 1) m = $"{Minutes} minute";
                else m = $"{Minutes} minutes";
                if (Hours == 1) h = $"{Hours} hour";
                else h = $"{Hours} hours";
                result = $"{h}, {m} & {s}";
            }
            else if (Years == 0 && Months == 0)
            {
                if (Minutes == 1) m = $"{Minutes} minute";
                else m = $"{Minutes} minutes";
                if (Hours == 1) h = $"{Hours} hour";
                else h = $"{Hours} hours";
                if (Days == 1) D = $"{Days} day";
                else D = $"{Days} days";
                result = $"{D}, {h} & {m}";
            }
            else if (Years == 0)
            {
                if (Hours == 1) h = $"{Hours} hour";
                else h = $"{Hours} hours";
                if (Days == 1) D = $"{Days} day";
                else D = $"{Days} days";
                if (Months == 1) M = $"{Months} month";
                else M = $"{Months} months";
                result = $"{M}, {D} & {h}";
            }
            else
            {
                if (Days == 1) D = $"{Days} day";
                else D = $"{Days} days";
                if (Months == 1) M = $"{Months} month";
                else M = $"{Months} months";
                if (Years == 1) Y = $"{Years} year";
                else Y = $"{Years} years";
                result = $"{Y}, {M} & {D}";
            }
            return result + " ago.";
        }
    }

    public class StarboardMessage
    {
        public IUserMessage message { get; set; }
        public SocketTextChannel channel { get; set; }
        public IUser author { get; set; }
        public int stars { get; set; }
        public ulong starboardid { get; set; }

        public Task Save()
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            Starboard starboard = new Starboard
            {
                MsgID = message.Id,
                ChannelID = channel.Id,
                UserID = author.Id,
                SBMessageID = starboardid
            };

            if (stars >= ESettings.StarboardMin) DbContext.Starboards.Add(starboard);
            else if (DbContext.Starboards.Contains(starboard) && stars < ESettings.StarboardMin) DbContext.Starboards.Remove(starboard);
            DbContext.SaveChanges();

            return Task.CompletedTask;
        }
    }
}
