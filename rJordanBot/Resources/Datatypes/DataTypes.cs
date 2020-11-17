using Discord;

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

    public class Suggestion
    {
        public IUserMessage message { get; set; }
        public int number { get; set; }
        public string author { get; set; }
        public string suggestion { get; set; }
        public SuggestionState state { get; set; }
        public string reason { get; set; }
        public string mod { get; set; }
    }
    public enum SuggestionState
    {
        Normal = 0,
        Approved = 1,
        Denied = -1,
        Implemented = 2,
        Considered = 3
    }

    public class AnasAPIObject
    {
        public int code { get; set; }
        public object data { get; set; }
    }

    public class COVID
    {
        public int cases { get; set; }
        public int localCases { get; set; }
        public int totalCases { get; set; }
        public int deaths { get; set; }
        public int totalDeaths { get; set; }
        public int recoveries { get; set; }
        public int hospitalRecoveries { get; set; }
        public int homeRecoveries { get; set; }
        public int totalRecoveries { get; set; }
        public int tests { get; set; }
        public int totalTests { get; set; }
        public int hospitalized { get; set; }
        public int totalHospitalized { get; set; }
        public int active { get; set; }
        public int critical { get; set; }
        public COVID_Cities cities { get; set; }
    }
    public class COVID_Cities
    {
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
    }
}
