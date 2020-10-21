using MySql.Data.MySqlClient;
using rJordanBot.Resources.Datatypes;
using System;
using System.Threading.Tasks;

namespace rJordanBot.Resources.MySQL
{
    public class MySQL
    {
        private static string server = Config.mysql_server;
        private static string username = Config.mysql_username;
        private static string password = Config.mysql_password;
        private static string dbname = Config.mysql_dbname;
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
