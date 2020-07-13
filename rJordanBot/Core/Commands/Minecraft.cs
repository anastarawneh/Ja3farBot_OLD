using Discord.Addons.Interactive;
using Discord.Commands;
using MulticraftLib;
using rJordanBot.Core.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Core.Commands
{
    public class Minecraft : InteractiveBase
    {
        [Command("list")]
        [MinecraftCommand]
        public async Task List()
        {
            getServerStatus.apiData.apiPlayer[] players = Multicraft.GetServerStatus().Data.Players;
            if (players.Count() == 0) await ReplyAsync("There are 0 players online.");
            else if (players.Count() == 1) await ReplyAsync($"There is 1 player online:\n- {players.First().Name}");
            else
            {
                string response = $"There are {players.Count()} players online:";
                foreach (var player in players) response += $"\n- {player.Name}";
                await ReplyAsync(response);
            }
        }
    }
}
