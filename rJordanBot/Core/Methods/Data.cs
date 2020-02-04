using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using rJordanBot.Resources.Database;
using rJordanBot.Resources.Datatypes;
using rJordanBot.Resources.Event_Verified;
using rJordanBot.Resources.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace rJordanBot.Core.Data
{
    public static class Data
    {
        public static Task InitJSON()
        {
            string JSON = "";
            string JSON2 = "";
            //Assembly.GetEntryAssembly().Location = /home/ubuntu/linux-x64/publish/rJordanBot.dll; (ON AWS)

            string SettingsLocation = "";
            switch (Assembly.GetEntryAssembly().Location)
            {
                default:
                    Console.WriteLine("The bot is running locally on Windows.");
                    SettingsLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.0\rJordanBot.dll", @"Data\Settings.json");
                    Environment.SetEnvironmentVariable("SystemType", "win");
                    break;
                case "/home/ubuntu/linux-x64/publish/rJordanBot.dll":
                    Console.WriteLine("The bot is running remotely on AWS.");
                    SettingsLocation = Path.Combine("Data", "Settings.json");
                    Environment.SetEnvironmentVariable("SystemType", "aws");
                    break;
            }
            Environment.SetEnvironmentVariable("SettingsLocation", SettingsLocation);

            using (FileStream Stream = new FileStream(SettingsLocation, FileMode.Open, FileAccess.Read))
            using (StreamReader ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            Setting Settings = JsonConvert.DeserializeObject<Setting>(JSON);
            ESettings.Token = Settings.token;
            ESettings.Owner = Settings.owner;
            ESettings.ReportBanned = Settings.reportbanned;
            ESettings.StarboardMin = Settings.starboardmin;

            using (FileStream Stream = new FileStream(SettingsLocation.Replace("Settings", "Event_Verified"), FileMode.Open, FileAccess.ReadWrite))
            using (StreamReader ReadSettings = new StreamReader(Stream))
            {
                JSON2 = ReadSettings.ReadToEnd();
            }

            e_Verified eVerified_ = JsonConvert.DeserializeObject<e_Verified>(JSON2);
            eVerified.Allowed = eVerified_.allowed;
            eVerified.Denied = eVerified_.denied;

            return Task.CompletedTask;
        }

        public static Task ReloadJSON()
        {
            string JSON = "";

            string SettingsLocation = @"C:\Users\anast\Desktop\VS\C#\rJordanBot\rJordanBot\Data\Settings.json";
            using (FileStream Stream = new FileStream(SettingsLocation, FileMode.Open, FileAccess.Read))
            using (StreamReader ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            Setting Settings = JsonConvert.DeserializeObject<Setting>(JSON);

            ESettings.Token = Settings.token;
            ESettings.Owner = Settings.owner;
            ESettings.ReportBanned = Settings.reportbanned;
            ESettings.StarboardMin = Settings.starboardmin;

            return Task.CompletedTask;
        }

        public static int GetStrikes(ulong UserId)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            if (DbContext.Strikes.Where(x => x.UserId == UserId).Count() < 1)
            {
                return 0;
            }
            return DbContext.Strikes.Where(x => x.UserId == UserId).Select(x => x.Amount).FirstOrDefault();
        }

        public static async Task SaveStrikes(ulong UserId, int Amount, string Username)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            if (DbContext.Strikes.Where(x => x.UserId == UserId).Count() < 1)
            {
                DbContext.Strikes.Add(new Strike
                {
                    UserId = UserId,
                    Amount = Amount,
                    Username = Username
                });
            }
            else
            {
                Strike Current = DbContext.Strikes.Where(x => x.UserId == UserId).FirstOrDefault();
                Current.Amount += Amount;
                DbContext.Strikes.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }

        public static async Task SetSocials(ulong UserId = 0, string site = null, string link = null, SocketCommandContext Context = null)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            if (DbContext.Socials.Where(x => x.UserId == UserId).Count() < 1)
            {
                DbContext.Socials.Add(new Social
                {
                    UserId = UserId,
                    Instagram = "None",
                    MsgId = 0,
                    Snapchat = "None",
                    Twitter = "None"
                });
                await DbContext.SaveChangesAsync();
            }

            Social Current = DbContext.Socials.Where(x => x.UserId == UserId).FirstOrDefault();
            switch (site.ToLower())
            {
                case "twitter":
                    Current.Twitter = link;
                    break;
                case "instagram":
                    Current.Instagram = link;
                    break;
                case "snapchat":
                    Current.Snapchat = link;
                    break;
                default:
                    Exception ex = new Exception(message: ":x: Please specify a vaild site. Available sites are (Twitter/Instagram/Snapchat).");
                    throw ex;
            }

            await DbContext.SaveChangesAsync();

            ForContext lol = new ForContext();
            await lol.EmbedBuild(Context);

            await DbContext.SaveChangesAsync();
        }

        public static string GetSocial(ulong UserId, string site)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            if (DbContext.Socials.Where(x => x.UserId == UserId).Count() < 1)
            {
                return "None.";
            }
            switch (site)
            {
                default:
                    return "None";
                case "twitter":
                    return DbContext.Socials.Where(x => x.UserId == UserId).Select(x => x.Twitter).FirstOrDefault();
                case "instagram":
                    return DbContext.Socials.Where(x => x.UserId == UserId).Select(x => x.Instagram).FirstOrDefault();
                case "snapchat":
                    return DbContext.Socials.Where(x => x.UserId == UserId).Select(x => x.Snapchat).FirstOrDefault();
            }
        }

        public static ulong GetMsgId(ulong UserId)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            return DbContext.Socials.Where(x => x.UserId == UserId).FirstOrDefault().MsgId;
        }

        public static ulong GetChnlId(string Name)
        {
            using (SqliteDbContext DbContext = new SqliteDbContext())
            {
                foreach (Channel channel in DbContext.Channels)
                {
                    if (channel.Name == Name) goto cont;
                }

                return 0;

                cont:
                ulong id = DbContext.Channels.Where(x => x.Name == Name).FirstOrDefault().ID;
                return id;
            }
        }

        public static async Task ResetChannels(SocketCommandContext Context)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();
            foreach (Channel channel in DbContext.Channels)
            {
                DbContext.Channels.Remove(channel);
            }

            foreach (SocketGuildChannel channel in Context.Guild.Channels)
            {
                if (channel.GetType().ToString() == "Discord.WebSocket.SocketCategoryChannel")
                {

                }
                else
                {
                    DbContext.Channels.Add(new Channel
                    {
                        ID = channel.Id,
                        Name = channel.Name,
                        Type = channel.GetType().ToString()
                    });
                }
            }

            await DbContext.SaveChangesAsync();
        }

        public class ForContext
        {
            public async Task EmbedBuild(SocketCommandContext Context)
            {
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor(Context.User.ToString(), Context.User.GetAvatarUrl());
                embed.WithColor(114, 137, 218);
                if (GetSocial(Context.User.Id, "twitter") == null || GetSocial(Context.User.Id, "twitter") == "None") embed.AddField("Twitter", "None");
                else if (GetSocial(Context.User.Id, "twitter") == "None") embed.AddField("Twitter", $"[{GetSocial(Context.User.Id, "twitter")}](https://twitter.com/" + GetSocial(Context.User.Id, "twitter") + ")");
                else embed.AddField("Twitter", $"[@{GetSocial(Context.User.Id, "twitter")}](https://twitter.com/" + GetSocial(Context.User.Id, "twitter") + ")");
                if (GetSocial(Context.User.Id, "instagram") == null || GetSocial(Context.User.Id, "instagram") == "None") embed.AddField("Instagram", "None");
                else if (GetSocial(Context.User.Id, "instagram") == "None") embed.AddField("Instagram", $"[{GetSocial(Context.User.Id, "instagram")}](https://instagram.com/" + GetSocial(Context.User.Id, "instagram") + ")");
                else embed.AddField("Instagram", $"[@{GetSocial(Context.User.Id, "instagram")}](https://instagram.com/" + GetSocial(Context.User.Id, "instagram") + ")");
                if (GetSocial(Context.User.Id, "snapchat") == null) embed.AddField("Snapchat", "None");
                else embed.AddField("Snapchat", $"{GetSocial(Context.User.Id, "snapchat")}");

                ulong chnlid = GetChnlId("socials"); ;
                SocketTextChannel socialchnl = Context.Guild.Channels.Where(x => x.Id == chnlid).FirstOrDefault() as SocketTextChannel;
                IEnumerable<IMessage> msgs = await socialchnl.GetMessagesAsync(100).FlattenAsync();
                foreach (IMessage msg in msgs)
                {
                    if (msg.Id == GetMsgId(Context.User.Id))
                    {
                        await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
                        return;
                    }
                }
                using (SqliteDbContext DbContext = new SqliteDbContext())
                {
                    RestUserMessage msg = await socialchnl.SendMessageAsync("", false, embed.Build());
                    Social Current = DbContext.Socials.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
                    Current.MsgId = msg.Id;
                    await DbContext.SaveChangesAsync();
                }
            }
        }

        public static RoleSetting GetRoleSetting(string role)
        {
            string ans = Environment.GetEnvironmentVariable("SystemType");
            string XmlLocation = "";
            switch (ans)
            {
                default:
                    break;
                case "win":
                    XmlLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.0\rJordanBot.dll", @"Data\RoleMessages.xml");
                    break;
                case "aws":
                    XmlLocation = Path.Combine("Data", "RoleMessages.xml");
                    break;
            }

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
            string XmlLocation = "";
            switch (ans)
            {
                default:
                    break;
                case "win":
                    XmlLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.0\rJordanBot.dll", @"Data\RoleMessages.xml");
                    break;
                case "aws":
                    XmlLocation = Path.Combine("Data", "RoleMessages.xml");
                    break;
            }

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

        public static void UpdateVerified()
        {
            e_Verified eVerified_ = new e_Verified
            {
                allowed = eVerified.Allowed,
                denied = eVerified.Denied
            };

            string SettingsLocation = Environment.GetEnvironmentVariable("SettingsLocation");
            using FileStream Stream = new FileStream(SettingsLocation.Replace("Settings", "Event_Verified"), FileMode.Truncate, FileAccess.ReadWrite);
            using StreamWriter WriteSettings = new StreamWriter(Stream);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(WriteSettings, eVerified_);
        }

        public static async Task SetInvitesBefore(SocketGuildUser user)
        {
            IReadOnlyCollection<RestInviteMetadata> invites = user.Guild.GetInvitesAsync().Result;

            using SqliteDbContext DbContext = new SqliteDbContext();
            foreach (Invite invite in DbContext.Invites)
            {
                DbContext.Invites.Remove(invite);
            }

            foreach (RestInviteMetadata invite in invites)
            {
                DbContext.Invites.Add(new Invite
                {
                    UserId = invite.Inviter.Id,
                    Text = invite.Code,
                    Uses = invite.Uses.Value
                });
            }

            await DbContext.SaveChangesAsync();
        }

        public static async Task CompareInvites(SocketGuildUser user)
        {
            SocketTextChannel logchnl = user.Guild.Channels.FirstOrDefault(x => x.Id == GetChnlId("bot-log")) as SocketTextChannel;

            IReadOnlyCollection<RestInviteMetadata> newInvites = user.Guild.GetInvitesAsync().Result;
            Invite sureInvite = new Invite
            {
                Text = "null",
                UserId = 1000,
                Uses = 10000
            };
            using (SqliteDbContext DbContext = new SqliteDbContext())
            {
                foreach (RestInviteMetadata newInvite in newInvites)
                {
                    foreach (Invite DbInvite in DbContext.Invites)
                    {
                        if (DbInvite.Text == newInvite.Code && DbInvite.Uses == (newInvite.Uses.Value - 1))
                        {
                            sureInvite = DbInvite;
                            //await logchnl.SendMessageAsync($"[{DateTime.Now} at UserJoined] {user.Mention} joined through {user.Guild.Users.FirstOrDefault(x => x.Id == sureInvite.UserId).Mention}'s invite (`{sureInvite.Text}`).");
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
                        Text = newInvite.Code,
                        UserId = newInvite.Inviter.Id,
                        Uses = newInvite.Uses.Value - 1
                    };

                    if (!DbContext.Invites.Contains(newInvite_) && newInvite.Uses == 1)
                    {
                        //await logchnl.SendMessageAsync($"[{DateTime.Now} at UserJoined] {user.Mention} joined through {user.Guild.Users.FirstOrDefault(x => x.Id == newInvite_.UserId).Mention}'s invite (`{newInvite_.Text}`).");
                        UpdateUserInvite(user, newInvite_);
                        await SetInvitesBefore(user);
                        return;
                    }
                }
            }

            //await logchnl.SendMessageAsync($"[{DateTime.Now} at UserJoined] {user.Mention} joined with unknown link.");
            UpdateUserInvite(user, new Invite { Text = "Unknown", UserId = 1, Uses = 0 }, true);
            await SetInvitesBefore(user);
            return;
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
            using SqliteDbContext DbContext = new SqliteDbContext();

            if (unknown == true)
            {
                invite = new Invite
                {
                    Text = "Unknown",
                    UserId = 1,
                    Uses = 0
                };
            }

            foreach (UserInvite UserInvite in DbContext.UserInvites)
            {
                if (UserInvite.UserID == user.Id)
                {
                    // Remove it if it exists.

                    DbContext.UserInvites.Remove(UserInvite);
                    DbContext.SaveChanges();
                    return;
                }
            }

            // Add it because it doesn't exist.

            UserInvite userInvite = new UserInvite
            {
                UserID = user.Id,
                Code = invite.Text
            };

            DbContext.UserInvites.Add(userInvite);
            DbContext.SaveChanges();
        }

        public static async Task UpdateStarboard(StarboardMessage starboardMessage)
        {
            using SqliteDbContext DbContext = new SqliteDbContext();

            Starboard starboard = new Starboard
            {
                MsgID = starboardMessage.message.Id,
                ChannelID = starboardMessage.channel.Id,
                UserID = starboardMessage.author.Id
            };

            SocketGuild guild = starboardMessage.channel.Guild;
            SocketTextChannel starboardChannel = guild.Channels.FirstOrDefault(x => x.Id == GetChnlId("starboard")) as SocketTextChannel;

            // If starboard message does not exist
            if (!DbContext.Starboards.Contains(starboard) && starboardMessage.stars >= ESettings.StarboardMin)
            {
                SocketTextChannel channel = guild.Channels.First(x => x.Id == starboardMessage.channel.Id) as SocketTextChannel;
                string link = channel.GetMessageAsync(starboardMessage.message.Id).Result.GetJumpUrl();

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor(starboardMessage.message.Author);
                embed.WithColor(Color.Gold);
                if (starboardMessage.message.Content != null) embed.AddField("Content", starboardMessage.message.Content);
                embed.AddField("Channel:", starboardMessage.channel.Mention);
                if (starboardMessage.message.Attachments.Count == 1) embed.WithImageUrl(starboardMessage.message.Attachments.First().Url);
                embed.WithDescription($"[Link to original message]({link})");

                RestUserMessage msg = await starboardChannel.SendMessageAsync($"{starboardMessage.stars} :star2:", false, embed.Build());
                starboardMessage.starboardid = msg.Id;

                await starboardMessage.Save();
            }
            // If starboard message exists and needs editing
            else if (DbContext.Starboards.Contains(starboard) && starboardMessage.stars >= ESettings.StarboardMin)
            {
                Starboard starboard1 = DbContext.Starboards.FirstOrDefault(x => x.MsgID == starboardMessage.message.Id);

                IUserMessage msg = starboardChannel.GetMessageAsync(starboard1.SBMessageID).Result as IUserMessage;
                await msg.ModifyAsync(x => x.Content = $"{starboardMessage.stars} :star2:");

                await starboardMessage.Save();
            }
            // If starboard message needs removal
            else if (DbContext.Starboards.Contains(starboard) && starboardMessage.stars < ESettings.StarboardMin)
            {
                Starboard starboard1 = DbContext.Starboards.FirstOrDefault(x => x.MsgID == starboardMessage.message.Id);

                IUserMessage msg = starboardChannel.GetMessageAsync(starboard1.SBMessageID).Result as IUserMessage;
                await msg.DeleteAsync();

                await starboardMessage.Save();
            }
        }
    }
}
