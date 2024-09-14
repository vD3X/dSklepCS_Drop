using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using static dSklepCS_Drop.dSklepCS_Drop;

namespace dSklepCS_Drop;

public static class Utils
{
    private static readonly Random Random = new();
    private static readonly HttpClient Client = new();

    public static void RollForWPLN(float chanceToWin)
    {
        if (Utilities.GetPlayers().Count < Config.config.Settings.MinPlayers)
        {
            foreach (var player in Utilities.GetPlayers().Where(p => p.Connected == PlayerConnectedState.PlayerConnected))
            {
                if (Random.NextDouble() <= chanceToWin)
                {
                    _ = SendWPLNToPlayer(player);
                }
            }
        }
        else
        {
            Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} DROP {ChatColors.Green}] {ChatColors.Lime}Jest za mało graczy na serwerze aby włączyć {ChatColors.LightRed}drop {ChatColors.Lime}{Config.config.Settings.Currency_Name}.");
        }
    }

    public static async Task SendWPLNToPlayer(CCSPlayerController player)
    {
        if(player == null) return;

        string playerName = player.PlayerName;

        var parameters = new Dictionary<string, string>
        {
            { "api", Config.config.Settings.Api_Key },
            { "tag_serwera", Config.config.Settings.Server_Tag },
            { "dodaj-walute", "1" },
            { "steam64", player.SteamID.ToString() },
            { "ip", "0" },
            { "client", "0" },
            { "client_admin", "0" },
            { "steam64_admin", "0" },
            { "ver", "143" },
            { "waluta", (Config.config.Settings.Drop_PLN * 100).ToString() },
        };

        try
        {
            var content = new FormUrlEncodedContent(parameters);
            string queryString = await content.ReadAsStringAsync();

            var response = await Client.GetAsync($"https://sklepcs.pl/api.php?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && responseContent.Contains("ok"))
            {
                string playerSaldo = await GetPlayerSaldo(player.SteamID.ToString());
                
                Server.NextFrame(() =>
                {
                    player.PrintToChat($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} DROP {ChatColors.Green}] Gratulacje! {ChatColors.Lime}Wygrałeś {ChatColors.DarkRed}{Config.config.Settings.Drop_PLN} {ChatColors.Lime}{Config.config.Settings.Currency_Name} do sklepu!");
                    player.PrintToChat($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} DROP {ChatColors.Green}] {ChatColors.Lime}Twoje aktualne saldo: {ChatColors.DarkRed}{playerSaldo} {Config.config.Settings.Currency_Name}");
                });
            }
            else
            {
                Server.NextFrame(() =>
                {
                    player.PrintToChat($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} DROP {ChatColors.Green}] {ChatColors.LightRed}Wystąpił błąd podczas dodawania {Config.config.Settings.Currency_Name}. Spróbuj ponownie później. (Zaloguj się do sklepu)");
                });
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.LogError($"Błąd podczas wysyłania {Config.config.Settings.Currency_Name} do gracza {playerName}: {ex.Message}");
        }
    }

    public static async Task<string> GetPlayerSaldo(string steamID64)
    {
        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "api", Config.config.Settings.Api_Key },
                { "serwer", Config.config.Settings.Server_Tag },
                { "ver", "142" },
                { "operacja", "3" },
                { "client", "0" },
                { "sid64", steamID64 }
            };

            var content = new FormUrlEncodedContent(parameters);
            string queryString = await content.ReadAsStringAsync();

            var response = await Client.GetAsync($"https://sklepcs.pl/api_server_uslugi.php?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && responseContent.Contains(";"))
            {
                string[] parts = responseContent.Split(';');
                if (parts.Length == 2 && int.TryParse(parts[1], out int saldo))
                {
                    float saldoInPLN = saldo / 100f;
                    return saldoInPLN.ToString("F2");
                }
                else
                {
                    Instance.Logger.LogWarning($"Nie udało się sparsować salda: {responseContent}");
                    return "Błąd parsowania salda";
                }
            }
            else
            {
                Instance.Logger.LogWarning($"Błąd podczas pobierania salda. Odpowiedź serwera: {responseContent}");
                return "Błąd pobierania salda";
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.LogError($"Błąd podczas pobierania salda dla gracza {steamID64}: {ex.Message}");
            return "Błąd pobierania salda";
        }
    }
}