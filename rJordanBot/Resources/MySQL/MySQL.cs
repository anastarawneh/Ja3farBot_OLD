using MySql.Data.MySqlClient;
using rJordanBot.Resources.Settings;
using System;
using System.Threading.Tasks;

namespace rJordanBot.Resources.MySQL
{
    public class MySQL
    {
        private static string server = ESettings.mysql_server;
        private static string username = ESettings.mysql_username;
        private static string password = ESettings.mysql_password;
        private static string dbname = ESettings.mysql_dbname;
        private static MySqlConnection connection;


        public static Task Initialize()
        {
            connection = new MySqlConnection($"server={server};userid={username};password={password};database={dbname};");

            Console.WriteLine("MySql setup complete.");

            return Task.CompletedTask;
        }

        public static MySqlConnection getConnection()
        {
            return connection;
        }
    }
}
