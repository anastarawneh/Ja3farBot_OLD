using Discord.WebSocket;
using rJordanBot.Core.Data;
using rJordanBot.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Resources.GeneralJSON
{
    public static class GeneralJson
    {
        public static List<User> users;

        public static Task AddUser(SocketGuildUser user)
        {
            foreach (User item in users)
            {
                if (item.ID == user.Id) return Task.CompletedTask;
            }

            List<ulong> roles = new List<ulong>();
            foreach (SocketRole role in user.Roles)
            {
                roles.Add(role.Id);
            }

            bool verified = false;
            foreach (User item in users)
            {
                if (item.ID == user.Id)
                {
                    verified = item.Verified;
                    break;
                }
            }
            /*if (eVerified.Allowed.Contains(user.Id)) verified = true;
            else verified = false;*/

            users.Add(new User
            {
                ID = user.Id,
                Username = user.Username,
                Discriminator = user.Discriminator,
                Verified = verified,
                Roles = roles
            });

            Data.SaveGeneralJSON();

            return Task.CompletedTask;
        }
        public static Task RemoveUser(SocketGuildUser user)
        {
            foreach (User item in users)
            {
                if (item.ID == user.Id)
                {
                    users.Remove(users.First(x => x.ID == user.Id));

                    Data.SaveGeneralJSON();

                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
        
        public static GeneralJsonInitializer ToInitForm()
        {
            List<UserInitializer> usersinit = new List<UserInitializer>();
            foreach (User user in users)
            {
                usersinit.Add(new UserInitializer
                {
                    id = user.ID,
                    username = user.Username,
                    discrim = user.Discriminator,
                    roles = user.Roles,
                    verified = user.Verified
                });
            }

            return new GeneralJsonInitializer
            {
                users = usersinit
            };
        }
    }

    public class User
    {
        public ulong ID;
        public string Username;
        public string Discriminator;
        public bool Verified;
        public List<ulong> Roles;

        public int DiscriminatorValue()
        {
            return int.Parse(Discriminator);
        }
    }

    public static class Extensions
    {
        public static User ToUser(this SocketGuildUser user)
        {
            foreach (User item in GeneralJson.users)
            {
                if (user.Id == item.ID) return item;
            }

            bool verified;
            /*if (eVerified.Allowed.Contains(user.Id)) verified = true;
            else*/ if (GeneralJson.users.First(x => x.ID == user.Id).Verified) verified = true;
            else verified = false;
            List<ulong> roles = new List<ulong>();
            foreach (SocketRole role in user.Roles) roles.Add(role.Id);
            return new User
            {
                ID = user.Id,
                Username = user.Username,
                Discriminator = user.Discriminator,
                Verified = verified,
                Roles = roles
            };
        }
    }
}
