using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace rJordanBot.Resources.Database
{
    public class User
    {
        [Key]
        public ulong ID { get; set; }
        public bool Verified { get; set; }
    }

    public static class User_Extensions
    {
        public static User ToUser(this SocketGuildUser user)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            User result = DbContext.Users.First(x => x.ID == user.Id) ?? DbContext.Users.Add(new User { ID = user.Id, Verified = false }).Entity;
            DbContext.SaveChangesAsync();
            return result;
        }
    }
}
