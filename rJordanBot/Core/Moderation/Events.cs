﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.GeneralJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rJordanBot.Core.Moderation
{
    class Events
    {
        [Group("eventsmod"), Alias("emod")]
        public class EventsMod : InteractiveBase
        {
            [Command("verify"), Alias("v")]
            public async Task Verify([Remainder]IUser user = null)
            {
                if (Context.User != Constants.IGuilds.Jordan(Context).Owner) return;

                ulong id = user.Id;

                if (user is null)
                {
                    await ReplyAsync(":x: Please enter a user.");
                    return;
                }

                if ((user as SocketGuildUser).ToUser().Verified)
                {
                    await ReplyAsync(":x: User is already verified.");
                    return;
                }

                /*eVerified.Allowed.Add(id);
                eVerified.Denied.Remove(id);
                Data.Data.UpdateVerified();*/

                    (user as SocketGuildUser).ToUser().Verified = true;
                await Data.Data.SaveGeneralJSON();

                await ReplyAsync($"{user.Mention} is now verified.");

                IDMChannel channel = user.GetOrCreateDMChannelAsync().Result;
                await channel.SendMessageAsync(":white_check_mark: You have been verified.");
            }

            [Command("unverify"), Alias("uv", "deny")]
            public async Task Unverify([Remainder]IUser user = null)
            {
                if (Context.User != Constants.IGuilds.Jordan(Context).Owner) return;
                ulong id = user.Id;

                if (user is null)
                {
                    await ReplyAsync(":x: Please enter a user.");
                    return;
                }

                if (!(user as SocketGuildUser).ToUser().Verified)
                {
                    await ReplyAsync(":x: User is already denied.");
                    return;
                }

                /*eVerified.Denied.Add(id);
                eVerified.Allowed.Remove(id);
                Data.Data.UpdateVerified();*/

                (user as SocketGuildUser).ToUser().Verified = false;
                await Data.Data.SaveGeneralJSON();

                await ReplyAsync($"{user.Mention} is now denied.");

                IDMChannel channel = user.GetOrCreateDMChannelAsync().Result;
                await channel.SendMessageAsync(":x: Your verification was denied.");
            }

            [Command("get"), Alias("g", "is")]
            public async Task Get([Remainder]IUser user = null)
            {
                if (Context.User != Constants.IGuilds.Jordan(Context).Owner) return;
                if (user is null)
                {
                    await ReplyAsync(":x: Please enter a user.");
                    return;
                }

                if ((user as SocketGuildUser).ToUser().Verified) await ReplyAsync($":white_check_mark: {user.Mention} is verified.");
                //else if (eVerified.Denied.Contains(user.Id)) await ReplyAsync($":white_check_mark: {user.Mention} is denied.");
                else await ReplyAsync($":x: {user.Mention} is not verified.");
            }

            [Command("delete"), Alias("del", "d")]
            public async Task Delete([Remainder]IUser user = null)
            {
                await ReplyAsync(":x: This command doesn't work anymore. See `^eventsmod unverify`.");
                return;

                /*if (Context.User != Constants.IGuilds.Jordan(Context).Owner) return;
                if (user is null)
                {
                    await ReplyAsync(":x: Please enter a user.");
                    return;
                }

                if (eVerified.Allowed.Contains(user.Id))
                {
                    eVerified.Allowed.Remove(user.Id);
                    Data.Data.UpdateVerified();
                    await ReplyAsync($":white_check_mark: {user.Mention} (Previously verified) has been deleted.");
                }
                else if (eVerified.Denied.Contains(user.Id))
                {
                    eVerified.Denied.Remove(user.Id);
                    Data.Data.UpdateVerified();
                    await ReplyAsync($":white_check_mark: {user.Mention} (Previously denied) has been deleted.");
                }
                else
                {
                    await ReplyAsync($":x: {user.Mention} is not verified.");
                }*/
            }

            /*[Group("list"), Alias("l")]
            public class List : InteractiveBase
            {
                [Command("verified"), Alias("v")]
                public async Task ListVerified()
                {
                    SocketRole modrole = Constants.IGuilds.Jordan(Context).Roles.FirstOrDefault(x => x.Name == "Discord Mods");
                    if (!(Context.User as SocketGuildUser).Roles.Contains(modrole)) return;

                    List<ulong> list = eVerified.Allowed;
                    string userlist = "None";

                    if (list.Count > 1) userlist = "";
                    foreach (ulong user in list)
                    {
                        userlist = userlist + Constants.IGuilds.Jordan(Context).Users.FirstOrDefault(x => x.Id == user) + "\n";
                    }

                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithTitle("List of Verified Users");
                    embed.WithDescription(userlist);
                    embed.WithColor(0, 255, 0);

                    await ReplyAsync("", false, embed.Build());
                }

                [Command("denied"), Alias("d")]
                public async Task ListDenied()
                {
                    SocketRole modrole = Constants.IGuilds.Jordan(Context).Roles.FirstOrDefault(x => x.Name == "Discord Mods");
                    if (!(Context.User as SocketGuildUser).Roles.Contains(modrole)) return;

                    List<ulong> list = eVerified.Denied;
                    string userlist = "None";

                    if (list.Count > 1) userlist = "";
                    foreach (ulong user in list)
                    {
                        userlist = userlist + Constants.IGuilds.Jordan(Context).Users.FirstOrDefault(x => x.Id == user) + "\n";
                    }

                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithTitle("List of Denied Users");
                    embed.WithDescription(userlist);
                    embed.WithColor(255, 0, 0);

                    await ReplyAsync("", false, embed.Build());
                }
            }*/

            [Command("list"), Alias("l")]
            public async Task List()
            {
                SocketRole modrole = Constants.IGuilds.Jordan(Context).Roles.FirstOrDefault(x => x.Name == "Discord Mods");
                if (!(Context.User as SocketGuildUser).Roles.Contains(modrole)) return;

                //List<ulong> list = eVerified.Allowed;
                string userlist = "None";

                if (GeneralJson.users.Count > 1) userlist = "";
                foreach (User user in GeneralJson.users)
                {
                    if (user.Verified) userlist = userlist + Constants.IGuilds.Jordan(Context).Users.FirstOrDefault(x => x.Id == user.ID) + "\n";
                }

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("List of Verified Users");
                embed.WithDescription(userlist);
                embed.WithColor(0, 255, 0);

                await ReplyAsync("", false, embed.Build());
            }
        }
    }
}
