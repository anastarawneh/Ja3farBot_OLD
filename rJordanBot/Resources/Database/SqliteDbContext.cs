using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Reflection;

namespace rJordanBot.Resources.Database
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<Strike> Strikes { get; set; }
        public DbSet<Social> Socials { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<UserInvite> UserInvites { get; set; }
        public DbSet<Starboard> Starboards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string ans = Environment.GetEnvironmentVariable("SystemType");
            string DbLocation = "";
            switch (ans)
            {
                default:
                    break;
                case "aws":
                    DbLocation = Path.Combine("Data", "Database.sqlite");
                    Options.UseSqlite($"DataSource={DbLocation}");
                    break;
                case "win":
                    DbLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.0\rJordanBot.dll", @"Data\");
                    Options.UseSqlite($"Data Source={DbLocation}Database.sqlite");
                    break;
            }
        }
    }
}
