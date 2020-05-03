using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace rJordanBot.Resources.Database
{
    public class Moderator
    {
        [Key]
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
    public static class Moderator_Extensions
    {
        public static bool IsModerator(this SocketGuildUser user)
        {
            SqliteDbContext DbContext = new SqliteDbContext();
            ulong id = user.Id;

            foreach (Moderator mod in DbContext.Moderators)
            {
                if (mod.ID == id) return true;
            }

            return false;
        }

        public static bool IsFuncModerator(this SocketGuildUser user)
        {
            if (!user.IsModerator()) return false;
            if (user.ToModerator().modType == ModType.FunMod) return false;
            return true;
        }

        public static Moderator ToModerator(this SocketGuildUser user)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            if (user.IsModerator()) return DbContext.Moderators.First(x => x.ID == user.Id);
            else return null;
        }
    }
}
