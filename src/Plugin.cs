using System;
using System.Text.Json.Serialization;
using MySqlConnector;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace KNZUserSystem;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("Host")] public string Host { get; set; } = "localhost";
    [JsonPropertyName("Database")] public string Database { get; set; } = "nume_baza";
    [JsonPropertyName("User")] public string User { get; set; } = "root";
    [JsonPropertyName("Password")] public string Password { get; set; } = "parola";
    [JsonPropertyName("Port")] public int Port { get; set; } = 3306;
    [JsonPropertyName("TableName")] public string TableName { get; set; } = "user_system";
    [JsonPropertyName("CommandFlag")] public string CommandFlag { get; set; } = "@knz/user";
    [JsonPropertyName("ChatPrefix")] public string ChatPrefix { get; set; } = "{purple}[KNZ] {default}";
}

public class UserData
{
    public int userid { get; set; }
    public string steamid64 { get; set; } = "";
    public string name { get; set; } = "";
    public string ip { get; set; } = "";
}

public class KNZUserSystemPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "[KNZ] User System (MySQL)";
    public override string ModuleVersion => "1.3.0";
    public override string ModuleAuthor => "KNZ Development";

    public PluginConfig Config { get; set; } = new();
    private string ConnectionString => $"Server={Config.Host};Port={Config.Port};Database={Config.Database};User Id={Config.User};Password={Config.Password};";

    public void OnConfigParsed(PluginConfig config) { Config = config; }

    public override void Load(bool hotReload)
    {
        InitializeDatabase();
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
    }

    private void InitializeDatabase()
    {
        try {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            string query = $@"CREATE TABLE IF NOT EXISTS `{Config.TableName}` (
                `id` INT AUTO_INCREMENT PRIMARY KEY,
                `userid` INT UNIQUE NOT NULL,
                `steamid64` VARCHAR(32) NOT NULL,
                `name` VARCHAR(128),
                `ip` VARCHAR(45),
                `last_seen` DATETIME
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            using var cmd = new MySqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        } catch (Exception ex) { Console.WriteLine($"[KNZ-MYSQL-ERROR] {ex.Message}"); }
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;
        
        string steamid = player.AuthorizedSteamID?.SteamId64.ToString() ?? "";
        if (!string.IsNullOrEmpty(steamid)) 
            ProcessPlayer(steamid, player.PlayerName, player.IpAddress?.Split(':')[0] ?? "0.0.0.0");
            
        return HookResult.Continue;
    }

    private void ProcessPlayer(string steamid, string name, string ip)
    {
        try {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            
            var checkCmd = new MySqlCommand($"SELECT id FROM `{Config.TableName}` WHERE steamid64 = @s", conn);
            checkCmd.Parameters.AddWithValue("@s", steamid);
            var exists = checkCmd.ExecuteScalar();

            if (exists != null) {
                var upCmd = new MySqlCommand($"UPDATE `{Config.TableName}` SET name = @n, ip = @i, last_seen = NOW() WHERE steamid64 = @s", conn);
                upCmd.Parameters.AddWithValue("@n", name); upCmd.Parameters.AddWithValue("@i", ip); upCmd.Parameters.AddWithValue("@s", steamid);
                upCmd.ExecuteNonQuery();
            } else {
                var idCmd = new MySqlCommand($"SELECT IFNULL(MAX(userid), 0) + 1 FROM `{Config.TableName}`", conn);
                int nextId = Convert.ToInt32(idCmd.ExecuteScalar());
                var insCmd = new MySqlCommand($"INSERT INTO `{Config.TableName}` (userid, steamid64, name, ip, last_seen) VALUES (@u, @s, @n, @i, NOW())", conn);
                insCmd.Parameters.AddWithValue("@u", nextId); insCmd.Parameters.AddWithValue("@s", steamid); insCmd.Parameters.AddWithValue("@n", name); insCmd.Parameters.AddWithValue("@i", ip);
                insCmd.ExecuteNonQuery();
            }
        } catch (Exception ex) { Console.WriteLine($"[KNZ-MYSQL-ERROR] {ex.Message}"); }
    }

    [ConsoleCommand("css_userid", "Info player online")]
    public void OnCmdUser(CCSPlayerController? p, CommandInfo c)
    {
        if (!HasPerm(p)) return;
        if (c.ArgCount < 2) { Reply(p, "{prefix}{red}Usage: css_userid <name>"); return; }
        var target = GetPlayer(c.ArgByIndex(1));
        if (target == null) { Reply(p, Localizer["PlayerNotFound"]); return; }
        var d = GetDbData("steamid64", target.AuthorizedSteamID!.SteamId64.ToString());
        if (d != null) Reply(p, Localizer["DataUserId"], target.PlayerName, d.userid.ToString(), d.steamid64, d.ip);
    }

    [ConsoleCommand("css_steamid", "Info by UserID")]
    public void OnCmdSteam(CCSPlayerController? p, CommandInfo c)
    {
        if (!HasPerm(p)) return;
        if (c.ArgCount < 2) { Reply(p, "{prefix}{red}Usage: css_steamid <id>"); return; }
        var d = GetDbData("userid", c.ArgByIndex(1));
        if (d != null) Reply(p, Localizer["DataSteamId"], d.userid.ToString(), d.name, d.steamid64, d.ip);
        else Reply(p, Localizer["UseridInvalid"]);
    }

    [ConsoleCommand("css_offid", "Info by SteamID")]
    public void OnCmdOff(CCSPlayerController? p, CommandInfo c)
    {
        if (!HasPerm(p)) return;
        if (c.ArgCount < 2) { Reply(p, "{prefix}{red}Usage: css_offid <steamid64>"); return; }
        var d = GetDbData("steamid64", c.ArgByIndex(1));
        if (d != null) Reply(p, Localizer["DataOffId"], d.steamid64, d.name, d.userid.ToString(), d.ip);
        else Reply(p, Localizer["SteamidInvalid"]);
    }

    private UserData? GetDbData(string col, string val)
    {
        try {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            var cmd = new MySqlCommand($"SELECT userid, steamid64, name, ip FROM `{Config.TableName}` WHERE {col} = @v", conn);
            cmd.Parameters.AddWithValue("@v", val);
            using var r = cmd.ExecuteReader();
            if (r.Read()) return new UserData { userid = r.GetInt32(0), steamid64 = r.GetString(1), name = r.GetString(2), ip = r.GetString(3) };
        } catch (Exception ex) { Console.WriteLine($"[KNZ-MYSQL-ERROR] {ex.Message}"); }
        return null;
    }

    private bool HasPerm(CCSPlayerController? p) {
        if (p == null) return true;
        if (!AdminManager.PlayerHasPermissions(p, Config.CommandFlag)) { Reply(p, Localizer["NoPermission"]); return false; }
        return true;
    }

    private CCSPlayerController? GetPlayer(string n) {
        foreach (var p in Utilities.GetPlayers()) if (p.IsValid && !p.IsBot && p.PlayerName.Contains(n, StringComparison.OrdinalIgnoreCase)) return p;
        return null;
    }

    private void Reply(CCSPlayerController? p, string m, params object[] args) {
        try {
            string msg = m;
            for (int i = 0; i < args.Length; i++) msg = msg.Replace("{" + i + "}", args[i]?.ToString() ?? "");
            msg = msg.Replace("{prefix}", Config.ChatPrefix).Replace("{purple}", "\x03").Replace("{gold}", "\x10").Replace("{green}", "\x04").Replace("{default}", "\x01").Replace("{red}", "\x02");
            if (p == null) Console.WriteLine(msg); else p.PrintToChat(msg);
        } catch (Exception ex) { Console.WriteLine($"[KNZ-REPLY-ERROR] {ex.Message}"); }
    }
}