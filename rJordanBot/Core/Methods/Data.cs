using Dapper;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.MySQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using YamlDotNet.Serialization;

namespace rJordanBot.Core.Methods
{
    public static class Data
    {
        public static Task InitYML()
        {
            string YML = "";

            string ConfigLocation;
            switch (Assembly.GetEntryAssembly().Location)
            {
                default:
                    Console.WriteLine("The bot is running locally on Windows.");
                    ConfigLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.0\rJordanBot.dll", @"Data\config.yml");
                    Environment.SetEnvironmentVariable("SystemType", "win");
                    break;
                case "/home/ubuntu/rJordanBot/publish/rJordanBot.dll":
                    Console.WriteLine("The bot is running remotely on AWS.");
                    ConfigLocation = Path.Combine("rJordanBot", "Data", "config.yml");
                    Environment.SetEnvironmentVariable("SystemType", "aws");
                    break;
            }
            Environment.SetEnvironmentVariable("SettingsLocation", ConfigLocation);

            using (FileStream Stream = new FileStream(ConfigLocation, FileMode.Open, FileAccess.Read))
            using (StreamReader ReadSettings = new StreamReader(Stream))
            {
                YML = ReadSettings.ReadToEnd();
            }

            IDeserializer deserializer = new DeserializerBuilder().Build();
            ConfigFile configFile = deserializer.Deserialize<ConfigFile>(YML);

            Config.Token = configFile.token;
            Config.Owner = configFile.owner;
            Config.ReportBanned = configFile.reportbanned;
            Config.StarboardMin = configFile.starboardmin;
            Config.ModAppsActive = configFile.modappsactive;
            Config.EventsActive = configFile.eventsactive;
            Config.InviteWhitelist = configFile.invitewhitelist;
            Config.VerifyID = configFile.verifyid;
            Config.BannedWords = configFile.bannedwords;
            Config.Music = configFile.music;
            Config.Announcement = configFile.announcement;
            Config.mysql_server = configFile.mysql_server;
            Config.mysql_username = configFile.mysql_username;
            Config.mysql_password = configFile.mysql_password;
            Config.mysql_dbname = configFile.mysql_dbname;
            Config.IgnoredVerificationMessages = configFile.ignoredverfmsgs;

            return Task.CompletedTask;
        }

        public static Task ReloadYML()
        {
            string YML = "";

            string SettingsLocation = Environment.GetEnvironmentVariable("SettingsLocation");
            using (FileStream Stream = new FileStream(SettingsLocation, FileMode.Open, FileAccess.Read))
            using (StreamReader ReadSettings = new StreamReader(Stream))
            {
                YML = ReadSettings.ReadToEnd();
            }

            IDeserializer deserializer = new DeserializerBuilder().Build();
            ConfigFile configFile = deserializer.Deserialize<ConfigFile>(YML);

            Config.Token = configFile.token;
            Config.Owner = configFile.owner;
            Config.ReportBanned = configFile.reportbanned;
            Config.StarboardMin = configFile.starboardmin;
            Config.ModAppsActive = configFile.modappsactive;
            Config.EventsActive = configFile.eventsactive;
            Config.InviteWhitelist = configFile.invitewhitelist;
            Config.VerifyID = configFile.verifyid;
            Config.BannedWords = configFile.bannedwords;
            Config.Music = configFile.music;
            Config.Announcement = configFile.announcement;
            Config.mysql_server = configFile.mysql_server;
            Config.mysql_username = configFile.mysql_username;
            Config.mysql_password = configFile.mysql_password;
            Config.mysql_dbname = configFile.mysql_dbname;

            return Task.CompletedTask;
        }

        public static ulong GetChnlId(string Name, Resources.MySQL.ChannelType Type = null)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query;
            if (Type == null) query = $"SELECT * FROM Channels";
            else query = $"SELECT * FROM Channels WHERE ChannelType='{Type}'";
            IEnumerable<Channel> channels = connection.Query<Channel>(query);

            foreach (Channel channel in channels)
            {
                if (channel.Name == Name) return channel.ID;
            }
            return 0;
        }

        public static async Task ResetChannels(SocketCommandContext Context)
        {
            using MySqlConnection connection = MySQL.getConnection();
            string query = $"DELETE FROM Channels";
            await connection.ExecuteAsync(query);
            IReadOnlyCollection<SocketGuildChannel> channels = Context.Guild.Channels;
            query = "INSERT INTO Channels (ID, Name, ChannelType) VALUES ";
            foreach (SocketGuildChannel channel in channels)
            {
                query += $"({channel.Id}, \"{channel.Name}\", '{channel.GetType().Name.Replace("Socket", "")}'), ";
            }
            query = query.Substring(0, query.Length - 2);
            await connection.ExecuteAsync(query);
        }

        public static RoleSetting GetRoleSetting(string role)
        {
            string ans = Environment.GetEnvironmentVariable("SystemType");
            string XmlLocation = Environment.GetEnvironmentVariable("SettingsLocation").Replace("config.yml", "RoleMessages.xml");
            /*switch (ans)
            {
                default:
                    break;
                case "win":
                    XmlLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.0\rJordanBot.dll", @"Data\RoleMessages.xml");
                    break;
                case "aws":
                    XmlLocation = Path.Combine("Data", "RoleMessages.xml");
                    break;
            }*/

            if (!File.Exists(XmlLocation))
                return null;

            FileStream Stream = new FileStream(XmlLocation, FileMode.Open, FileAccess.Read);
            XmlDocument Doc = new XmlDocument();
            Doc.Load(Stream);
            Stream.Dispose();

            RoleSetting roleSetting = new RoleSetting();
            ulong id = 0;

            foreach (XmlNode typenode in Doc.DocumentElement)
            {
                foreach (XmlNode rolenode in typenode)
                {
                    if (rolenode.Name == "id")
                    {
                        id = ulong.Parse(rolenode.InnerText);
                    }
                    if (rolenode.Name == role)
                    {
                        roleSetting.id = id;
                        foreach (XmlNode inrolenode in rolenode)
                        {
                            switch (inrolenode.Name)
                            {
                                case "roleid":
                                    roleSetting.roleid = ulong.Parse(inrolenode.InnerText);
                                    break;

                                case "emote":
                                    roleSetting.emote = inrolenode.InnerText;
                                    break;

                                case "emoji":
                                    roleSetting.emoji = inrolenode.InnerText;
                                    break;
                            }
                        }

                        roleSetting.group = typenode.Name;
                    }
                }
            }

            return roleSetting;
        }

        public static RoleSetting GetEmojiRoleSetting(string emoji)
        {
            string ans = Environment.GetEnvironmentVariable("SystemType");
            string XmlLocation = Environment.GetEnvironmentVariable("SettingsLocation").Replace("config.yml", "RoleMessages.xml");
            /*switch (ans)
            {
                default:
                    break;
                case "win":
                    XmlLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.0\rJordanBot.dll", @"Data\RoleMessages.xml");
                    break;
                case "aws":
                    XmlLocation = Path.Combine("Data", "RoleMessages.xml");
                    break;
            }*/

            FileStream Stream = new FileStream(XmlLocation, FileMode.Open, FileAccess.Read);
            XmlDocument Doc = new XmlDocument();
            Doc.Load(Stream);
            Stream.Dispose();

            string role;

            foreach (XmlNode typenode in Doc.DocumentElement)
            {
                foreach (XmlNode rolenode in typenode)
                {
                    foreach (XmlNode inrolenode in rolenode)
                    {
                        if (inrolenode.Name == "emoji" && inrolenode.InnerText == emoji)
                        {
                            role = rolenode.Name;
                            return GetRoleSetting(role);
                        }
                    }
                }
            }
            Console.WriteLine("NULL 3");
            return null;
        }

        public static async Task SetInvitesBefore(SocketGuildUser user)
        {
            IReadOnlyCollection<RestInviteMetadata> invites = user.Guild.GetInvitesAsync().Result;

            using MySqlConnection connection = MySQL.getConnection();
            string query = "DELETE FROM Invites";
            await connection.ExecuteAsync(query);

            query = "INSERT INTO Invites (Code, UserID, Uses) VALUES ";
            foreach (RestInviteMetadata invite in invites)
            {
                query += $"('{invite.Code}', {invite.Inviter.Id}, {invite.Uses}), ";
            }
            query = query.Substring(0, query.Length - 2);
            await connection.OpenAsync();
            await connection.ExecuteAsync(query);
        }

        public static async Task CompareInvites(SocketGuildUser user)
        {
            IReadOnlyCollection<RestInviteMetadata> newInvites = user.Guild.GetInvitesAsync().Result;
            Invite sureInvite = new Invite
            {
                Code = "null",
                UserID = 1000,
                Uses = 10000
            };
            using MySqlConnection connection = MySQL.getConnection();
            string query = "SELECT * FROM Invites";
            IEnumerable<Invite> invites = await connection.QueryAsync<Invite>(query);
            foreach (RestInviteMetadata newInvite in newInvites)
            {
                foreach (Invite DbInvite in invites)
                {
                    if (DbInvite.Code == newInvite.Code && DbInvite.Uses == (newInvite.Uses.Value - 1))
                    {
                        sureInvite = DbInvite;
                        UpdateUserInvite(user, sureInvite);
                        await SetInvitesBefore(user);
                        return;
                    }
                }
            }
            foreach (RestInviteMetadata newInvite in newInvites)
            {
                Invite newInvite_ = new Invite
                {
                    Code = newInvite.Code,
                    UserID = newInvite.Inviter.Id,
                    Uses = newInvite.Uses.Value - 1
                };

                query = $"SELECT COUNT(1) FROM Invites WHERE Code='{newInvite_.Code}'";
                bool contains = await connection.ExecuteScalarAsync<bool>(query);
                if (!contains && newInvite.Uses == 1)
                {
                    UpdateUserInvite(user, newInvite_);
                    await SetInvitesBefore(user);
                    return;
                }
            }

            UpdateUserInvite(user, new Invite { Code = "Unknown", UserID = 1, Uses = 0 }, true);
            await SetInvitesBefore(user);
        }

        public static DateTimeSpan GetDuration(DateTime DT1, DateTime DT2)
        {
            // DT1 must be older than DT2

            int s = DT2.Second - DT1.Second;
            int m = DT2.Minute - DT1.Minute;
            int h = DT2.Hour - DT1.Hour;
            int D = DT2.Day - DT1.Day;
            int M = DT2.Month - DT1.Month;
            int Y = DT2.Year - DT1.Year;

            if (s < 0)
            {
                s += 60;
                m--;
            }
            if (m < 0)
            {
                m += 60;
                h--;
            }
            if (h < 0)
            {
                h += 24;
                D--;
            }
            if (D < 0)
            {
                switch (DT1.Month)
                {
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                    case 8:
                    case 10:
                    case 12:
                        D += 31;
                        break;
                    case 4:
                    case 6:
                    case 9:
                    case 11:
                        D += 30;
                        break;
                    case 2:
                        if (DT1.Year % 4 == 0) D += 29;
                        else D += 28;
                        break;
                }
                M--;
            }
            if (M < 0)
            {
                M += 12;
                Y--;
            }

            DateTimeSpan span = new DateTimeSpan
            {
                Seconds = s,
                Minutes = m,
                Hours = h,
                Days = D,
                Months = M,
                Years = Y
            };

            return span;
        }

        public static void UpdateUserInvite(SocketGuildUser user, Invite invite, bool unknown = false)
        {
            using MySqlConnection connection = MySQL.getConnection();

            if (unknown) invite = new Invite
            {
                Code = "Unknown",
                UserID = 1,
                Uses = 0
            };

            string query = "SELECT * FROM UserInvites";
            IEnumerable<UserInvite> userInvites = connection.Query<UserInvite>(query);
            foreach (UserInvite UserInvite in userInvites)
            {
                if (UserInvite.UserID == user.Id)
                {
                    // Remove it if it exists.
                    query = $"DELETE FROM UserInvites WHERE Code={UserInvite.Code}";
                    connection.Execute(query);
                    return;
                }
            }

            // Add it because it doesn't exist.

            UserInvite userInvite = new UserInvite
            {
                UserID = user.Id,
                Code = invite.Code
            };

            query = $"INSERT INTO UserInvites (UserID, Code) VALUES ({userInvite.UserID}, '{userInvite.Code}')";
            connection.Execute(query);
        }

        public static async Task UpdateStarboard(StarboardMessage starboardMessage)
        {
            Starboard starboard = new Starboard
            {
                MsgID = starboardMessage.message.Id,
                ChannelID = starboardMessage.channel.Id,
                UserID = starboardMessage.author.Id
            };

            SocketGuild guild = starboardMessage.channel.Guild;
            SocketTextChannel starboardChannel = guild.Channels.FirstOrDefault(x => x.Id == GetChnlId("starboard")) as SocketTextChannel;

            // If starboard message does not exist
            if (!starboardMessage.starboardExists() && starboardMessage.stars >= Config.StarboardMin)
            {
                SocketTextChannel channel = guild.Channels.First(x => x.Id == starboardMessage.channel.Id) as SocketTextChannel;
                string link = channel.GetMessageAsync(starboardMessage.message.Id).Result.GetJumpUrl();

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor(starboardMessage.message.Author);
                embed.WithColor(Color.Gold);
                if (starboardMessage.message.Content != null && starboardMessage.message.Content != "") embed.AddField("Content", starboardMessage.message.Content);
                embed.AddField("Channel:", starboardMessage.channel.Mention);
                if (starboardMessage.message.Attachments.Count == 1) embed.WithImageUrl(starboardMessage.message.Attachments.First().Url);
                embed.WithDescription($"[Link to original message]({link})");

                RestUserMessage msg = await starboardChannel.SendMessageAsync($"{starboardMessage.stars} :star2:", false, embed.Build());
                starboardMessage.starboardid = msg.Id;

                await starboardMessage.Save();
            }
            // If starboard message exists and needs editing
            else if (starboardMessage.starboardExists() && starboardMessage.stars >= Config.StarboardMin)
            {
                Starboard starboard1 = await StarboardFunctions.getStarboardByMsgID(starboardMessage.message.Id);

                IUserMessage msg = starboardChannel.GetMessageAsync(starboard1.SBMessageID).Result as IUserMessage;
                await msg.ModifyAsync(x => x.Content = $"{starboardMessage.stars} :star2:");

                await starboardMessage.Save();
            }
            // If starboard message needs removal
            else if (starboardMessage.starboardExists() && starboardMessage.stars < Config.StarboardMin)
            {
                Starboard starboard1 = await StarboardFunctions.getStarboardByMsgID(starboardMessage.message.Id);

                IUserMessage msg = starboardChannel.GetMessageAsync(starboard1.SBMessageID).Result as IUserMessage;
                await msg.DeleteAsync();

                await starboardMessage.Save();
            }
        }

        public static async Task<T> APIHttpRequest<T>(string URL, string method)
        {
            if (Environment.GetEnvironmentVariable("SystemType") == "win")
            {
                URL = URL.Replace("https://api.anastarawneh.live", "http://localhost:8000");
            }

            HttpWebRequest request = WebRequest.CreateHttp(URL);
            request.Method = method;
            WebResponse response = await request.GetResponseAsync();

            using Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string result = await reader.ReadToEndAsync();
            AnasAPIObject obj = JsonConvert.DeserializeObject<AnasAPIObject>(result);
            string data = ((JObject) obj.data).ToString();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{DateTime.Now} at Network] Sent {method} request to {URL}, returned code {obj.code}");
            Console.ResetColor();
            T typereturn = JsonConvert.DeserializeObject<T>(data);
            return typereturn;
        }

        public static async Task SetListeningStatus(DiscordSocketClient client, bool toListen)
        {
            if (toListen)
            {
                await client.SetStatusAsync(UserStatus.Online);
                await client.SetGameAsync("^help", null, ActivityType.Listening);
            }
            else
            {
                await client.SetStatusAsync(UserStatus.DoNotDisturb);
                await client.SetGameAsync("In Maintenance! Not listening.", null, ActivityType.Playing);
            };
        }
    }
}
