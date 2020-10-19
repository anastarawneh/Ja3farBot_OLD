using Dapper;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Resources.MySQL
{
    public static class ModeratorFunctions
    {
        public static bool IsModerator(this SocketGuildUser user)
        {
            using MySqlConnection connection = MySQL.getConnection();
            connection.Open();
            string query = $"SELECT COUNT(1) FROM Moderators WHERE ID={user.Id}";
            MySqlCommand command = new MySqlCommand(query, connection);

            long result = (long)command.ExecuteScalar();
            connection.Close();

            return result > 0;
        }

        public static bool IsFuncModerator(this SocketGuildUser user)
        {
            if (!user.IsModerator()) return false;
            if (user.ToModerator().modType == ModType.FunMod) return false;
            return true;
        }

        public static Moderator ToModerator(this SocketGuildUser user)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"SELECT * FROM Moderators where ID={user.Id}";
            MySqlCommand command = new MySqlCommand(query, connection);

            if (user.IsModerator()) return connection.Query<Moderator>(query).FirstOrDefault();
            return null;
        }
    }

    public static class StarboardFunctions
    {
        public static bool starboardExists(this StarboardMessage message)
        {
            using MySqlConnection connection = MySQL.getConnection();
            if (connection.State != System.Data.ConnectionState.Open) connection.Open();
            string query = $"SELECT COUNT(1) FROM Starboards WHERE MsgID={message.message.Id}";
            bool result = connection.ExecuteScalar<bool>(query);
            connection.Close();
            return result;
        }

        public static async Task Save(this StarboardMessage message)
        {
            using MySqlConnection connection = MySQL.getConnection();
            await connection.OpenAsync();
            string query = "";
            if (message.stars >= ESettings.StarboardMin && !message.starboardExists())
            {
                query = $"INSERT INTO Starboards (MsgID, ChannelID, UserID, SBMessageID) " +
                   $"VALUES ({message.message.Id}, {message.channel.Id}, {message.author.Id}, {message.starboardid})";
            }
            else
            {
                query = $"DELETE FROM Starboards WHERE MsgID={message.message.Id}";
            }
            await connection.ExecuteAsync(query);
            await connection.CloseAsync();
        }

        public static async Task<Starboard> getStarboardByMsgID(ulong msgID)
        {
            using MySqlConnection connection = MySQL.getConnection();
            await connection.OpenAsync();
            string query = $"SELECT * FROM Starboards WHERE MsgID={msgID}";
            Starboard result = await connection.QueryFirstAsync<Starboard>(query);
            await connection.CloseAsync();
            return result;
        }
    }

    public static class SocialFunctions
    {
        public static bool socialExists(ulong UserID)
        {
            using MySqlConnection connection = MySQL.getConnection();
            if (connection.State != System.Data.ConnectionState.Open) connection.OpenAsync();
            string query = $"SELECT COUNT(1) FROM Socials WHERE UserID={UserID}";
            bool result = connection.ExecuteScalarAsync<bool>(query).Result;
            connection.CloseAsync();
            return result;
        }

        public static string GetSocial(ulong UserID, string site)
        {
            using MySqlConnection connection = MySQL.getConnection();
            if (connection.State != System.Data.ConnectionState.Open) connection.Open();
            if (!socialExists(UserID))
            {
                return "None";
            }
            string query = $"SELECT * FROM Socials WHERE UserID={UserID}";
            Social result = connection.QueryFirstAsync<Social>(query).Result;
            connection.Close();
            switch (site)
            {
                default:
                    return "None";
                case "twitter":
                    return result.Twitter;
                case "instagram":
                    return result.Instagram;
                case "snapchat":
                    return result.Snapchat;
            }
        }

        public static async Task SetSocials(ulong UserID, string site = null, string link = null, SocketCommandContext context = null)
        {
            using MySqlConnection connection = MySQL.getConnection();
            if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
            string query = "";
            if (!socialExists(UserID))
            {
                query = $"INSERT INTO Socials (UserID, Twitter, Instagram, Snapchat, MsgID) " +
                    $"VALUES ({UserID}, 'None', 'None', 'None', 0)";
                await connection.ExecuteAsync(query);
            }

            query = $"SELECT * FROM Socials WHERE UserID={UserID}";
            Social current = await connection.QueryFirstAsync<Social>(query);
            switch (site.ToLower())
            {
                case "twitter":
                    current.Twitter = link;
                    break;
                case "instagram":
                    current.Instagram = link;
                    break;
                case "snapchat":
                    current.Snapchat = link;
                    break;
                default:
                    Exception ex = new Exception(message: ":x: Please specify a vaild site. Available sites are (Twitter/Instagram/Snapchat).");
                    throw ex;
            }

            query = $"UPDATE Socials SET Twitter='{current.Twitter}', Instagram='{current.Instagram}', " +
                $"Snapchat='{current.Snapchat}' WHERE UserID={UserID}";
            await connection.ExecuteAsync(query);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(context.User.ToString(), context.User.GetAvatarUrl());
            embed.WithColor(114, 137, 218);
            if (GetSocial(UserID, "twitter") == null || GetSocial(UserID, "twitter") == "None") embed.AddField("Twitter", "None");
            else if (GetSocial(UserID, "twitter") == "None") embed.AddField("Twitter", $"[{GetSocial(UserID, "twitter")}](https://twitter.com/" + GetSocial(UserID, "twitter") + ")");
            else embed.AddField("Twitter", $"[@{GetSocial(UserID, "twitter")}](https://twitter.com/" + GetSocial(UserID, "twitter") + ")");
            if (GetSocial(UserID, "instagram") == null || GetSocial(UserID, "instagram") == "None") embed.AddField("Instagram", "None");
            else if (GetSocial(UserID, "instagram") == "None") embed.AddField("Instagram", $"[{GetSocial(UserID, "instagram")}](https://instagram.com/" + GetSocial(UserID, "instagram") + ")");
            else embed.AddField("Instagram", $"[@{GetSocial(UserID, "instagram")}](https://instagram.com/" + GetSocial(UserID, "instagram") + ")");
            if (GetSocial(UserID, "snapchat") == null) embed.AddField("Snapchat", "None");
            else embed.AddField("Snapchat", $"{GetSocial(UserID, "snapchat")}");

            ulong chnlid = Data.GetChnlId("socials"); ;
            SocketTextChannel socialchnl = Constants.IGuilds.Jordan(context).Channels.Where(x => x.Id == chnlid).FirstOrDefault() as SocketTextChannel;
            IEnumerable<IMessage> msgs = await socialchnl.GetMessagesAsync(100).FlattenAsync();
            foreach (IMessage msg in msgs)
            {
                if (msg.Id == GetMsgId(context.User.Id))
                {
                    await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
                    return;
                }
            }

            RestUserMessage msg_ = await socialchnl.SendMessageAsync("", false, embed.Build());
            query = $"UPDATE Socials SET MsgID={msg_.Id} WHERE UserID={UserID}";
            await connection.ExecuteAsync(query);
        }

        public static ulong GetMsgId(ulong UserID)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"SELECT MsgID FROM Socials WHERE UserID={UserID}";
            return connection.QueryFirst<ulong>(query);
        }
    }

    public static class UserFunctions
    {
        public static bool IsRegistered(this SocketGuildUser user)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"SELECT COUNT(1) FROM Users WHERE ID={user.Id}";

            long result = connection.ExecuteScalarAsync<long>(query).Result;

            return result > 0;
        }

        private static User Register(this SocketGuildUser user)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"INSERT INTO Users (ID) VALUES ({user.Id})";
            connection.ExecuteAsync(query);

            query = $"SELECT * FROM Users WHERE ID={user.Id}";
            return connection.QueryFirst<User>(query);
        }

        public static User ToUser(this SocketGuildUser user)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"SELECT * FROM Users WHERE ID={user.Id}";

            if (user.IsRegistered()) return connection.Query<User>(query).FirstOrDefault();
            else return user.Register();
        }

        public static IEnumerable<User> List()
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"SELECT * FROM Users";
            return connection.Query<User>(query);
        }

        public static async Task Delete(this User user)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"DELETE FROM Users WHERE ID={user.ID}";
            await connection.ExecuteAsync(query);
        }

        public static async Task SetVerified(this User user, bool verified)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"UPDATE Users SET EventVerified={verified} WHERE ID={user.ID}";
            await connection.ExecuteAsync(query);
        }
    }

    public static class WarningFunctions
    {
        public static async Task<int> getWarningCount(ulong UserID)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"SELECT COUNT(*) FROM Warnings WHERE UserID={UserID}";
            return await connection.ExecuteScalarAsync<int>(query);
        }
        public static async Task<int> getWarningCount(this SocketGuildUser user)
        {
            return await getWarningCount(user.Id);
        }

        public static async Task saveWarning(this Warning warning)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"INSERT INTO Warnings (UserID, ChannelID, MessageID, Timestamp, Reason, ModID) " +
                $"VALUES ({warning.UserID}, {warning.ChannelID}, {warning.MessageID}, {warning.Timestamp}, " +
                $"'{warning.Reason}', {warning.ModID})";
            await connection.ExecuteAsync(query);
        }

        public static async Task<IEnumerable<Warning>> getWarnings(ulong UserID)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"SELECT * FROM Warnings WHERE UserID={UserID}";
            return await connection.QueryAsync<Warning>(query);
        }
        public static async Task<IEnumerable<Warning>> getWarnings(this SocketGuildUser user)
        {
            return await getWarnings(user.Id);
        }
    }
}
