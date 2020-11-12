using Dapper;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using rJordanBot.Core.Methods;
using rJordanBot.Resources.MySQL;
using System;
using System.Threading.Tasks;

namespace rJordanBot.Resources.Services
{
    public class CustomVCService
    {
        private DiscordSocketClient _client;
        public CustomVCService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task Initialize()
        {
            _client.UserVoiceStateUpdated += UserLeftVC;

            return Task.CompletedTask;
        }

        public async Task<SocketGuildUser> GetCustomVCOwner(SocketVoiceChannel vc)
        {
            if (await IsCustomVC(vc)) return null;
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"SELECT UserID FROM CustomVCs WHERE ChannelID={vc.Id}";
            ulong userID = await connection.ExecuteScalarAsync<ulong>(query);
            return vc.Guild.GetUser(userID);
        }

        private async Task<CustomVC> GetCustomVC(SocketGuildUser user)
        {
            if (!await HasCustomVC(user.Id)) return null;
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"SELECT * FROM CustomVCs WHERE UserID={user.Id}";
            CustomVC vc = await connection.QueryFirstAsync<CustomVC>(query);
            return vc;
        }
        private async Task<CustomVC> GetCustomVC(SocketVoiceChannel voice)
        {
            if (!await IsCustomVC(voice)) return null;
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"SELECT * FROM CustomVCs WHERE ChannelID={voice.Id}";
            CustomVC vc = await connection.QueryFirstAsync<CustomVC>(query);
            return vc;
        }

        private async Task<CustomVC> CreateCustomVC(SocketGuildUser user)
        {
            if (await HasCustomVC(user.Id)) return null;
            CustomVC vc = new CustomVC(user.Id);
            await Insert(vc);
            return vc;
        }

        public async Task<CustomVC> GetOrCreateCustomVC(SocketGuildUser user)
        {
            if (await HasCustomVC(user.Id)) return await GetCustomVC(user);
            else return await CreateCustomVC(user);
        }

        public async Task ModifyCustomVC(CustomVC vc, int slots)
        {
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"UPDATE CustomVCs SET Slots={slots} WHERE UserID={vc.UserID}";
            await connection.ExecuteAsync(query);
            if (await IsActive(vc)) await Constants.IGuilds.Jordan(_client).GetVoiceChannel(vc.ChannelID).ModifyAsync(x =>
            {
                x.UserLimit = slots;
            });
        }
        public async Task<string> ModifyCustomVC(CustomVC vc, string setting, int value)
        {
            if (!await HasCustomVC(vc.UserID)) return ":x: You do not have a CustomVC to edit.";

            Action<VoiceChannelProperties> action;
            switch (setting.ToLower())
            {
                case "slots":
                    if (value > 99 || value < 0) return ":x: Slots range from `0` (unlimited) to `99`.";
                    action = x => x.UserLimit = value;
                    break;
                case "bitrate":
                    if (value > 8000 || value < 128000) return ":x: Slots range from `8000` to `128000`.";
                    action = x => x.Bitrate = value;
                    break;
                default:
                    return ":x: Available settings are: `slots, bitrate`.";
            }

            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"UPDATE CustomVCs SET {setting}={value} WHERE UserID={vc.UserID}";
            await connection.ExecuteAsync(query);
            if (await IsActive(vc)) await Constants.IGuilds.Jordan(_client).GetVoiceChannel(vc.ChannelID).ModifyAsync(action);
            return ":white_check_mark: CustomVC edited.";
        }

        public async Task<string> Load(CustomVC vc)
        {
            if (await IsActive(vc)) return ":x: Custom VC is already loaded.";
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"SELECT ChannelID FROM CustomVCs WHERE UserID={vc.UserID}";
            ulong channelid = await connection.ExecuteScalarAsync<ulong>(query);
            SocketGuildUser user = Constants.IGuilds.Jordan(_client).GetUser(vc.UserID);
            RestVoiceChannel voice = await Constants.IGuilds.Jordan(_client).CreateVoiceChannelAsync($"{user.Username}'s VC",
                x =>
                {
                    x.CategoryId = Data.GetChnlId("Voice Booth", MySQL.ChannelType.CategoryChannel);
                    if (vc.Slots != 0) x.UserLimit = vc.Slots;
                    else x.UserLimit = null;
                    x.Bitrate = vc.Bitrate;
                });
            query = $"UPDATE CustomVCs SET ChannelID={voice.Id} WHERE UserID={vc.UserID}";
            await connection.ExecuteAsync(query);
            OverwritePermissions perms = new OverwritePermissions(
                muteMembers: PermValue.Allow,
                deafenMembers: PermValue.Allow,
                moveMembers: PermValue.Allow
            );
            await voice.AddPermissionOverwriteAsync(user, perms);
            return ":white_check_mark: Loaded Custom VC.";
        }

        public async Task<string> Unload(CustomVC vc)
        {
            if (!await IsActive(vc)) return ":x: Custom VC is not loaded.";
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"UPDATE CustomVCs SET ChannelID=0 WHERE UserID={vc.UserID}";
            await connection.ExecuteAsync(query);
            await Constants.IGuilds.Jordan(_client).GetVoiceChannel(vc.ChannelID).DeleteAsync();
            return ":white_check_mark: Unloaded Custom VC.";
        }

        private async Task<bool> IsActive(CustomVC vc)
        {
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"SELECT ChannelID FROM CustomVCs WHERE UserID={vc.UserID}";
            bool exists = await connection.ExecuteScalarAsync<bool>(query);
            return exists;
        }

        private async Task<bool> IsCustomVC(SocketVoiceChannel voiceChannel)
        {
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"SELECT COUNT(1) FROM CustomVCs WHERE ChannelID={voiceChannel.Id}";
            return await connection.ExecuteScalarAsync<bool>(query);
        }

        public async Task<bool> HasCustomVC(ulong UserID)
        {
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"SELECT COUNT(1) FROM CustomVCs WHERE UserID={UserID}";
            return await connection.ExecuteScalarAsync<bool>(query);
        }

        private async Task Insert(CustomVC vc)
        {
            if (await HasCustomVC(vc.UserID)) return;
            using MySqlConnection connection = MySQL.MySQL.getConnection();
            string query = $"INSERT INTO CustomVCs (UserID, ChannelID) VALUES ({vc.UserID}, {vc.ChannelID})";
            await connection.ExecuteAsync(query);
        }


        public async Task UserLeftVC(SocketUser socketUser, SocketVoiceState preState, SocketVoiceState postState)
        {
            if (preState.VoiceChannel == null || !await IsCustomVC(preState.VoiceChannel)) return;
            SocketVoiceChannel voice = preState.VoiceChannel;
            if (voice.Users.Count != 0) return;

            CustomVC vc = await GetCustomVC(voice);
            await Unload(vc);
        }
    }
}
