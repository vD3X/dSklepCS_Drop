using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace dSklepCS_Drop; 
 
public class dSklepCS_Drop : BasePlugin 
{ 
    public override string ModuleName => "[CS2] D3X - [ dSklepCS Drop wPLN ]";
    public override string ModuleAuthor => "D3X";
    public override string ModuleDescription => "Plugin na możliwość dropu wPLN do sklepCS na serwery CS2 by D3X";
    public override string ModuleVersion => "1.0.2";

    public static dSklepCS_Drop Instance { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Instance = this;
        Config.Initialize();

        AddCommand("css_wpln", "show wPLN", async (player, info) =>
        {
            if (player == null) return;

            string playerSaldo = await Utils.GetPlayerSaldo(player.SteamID.ToString());
            
            Server.NextFrame(() => player.PrintToChat($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} DROP {ChatColors.Green}] Twoje aktualne saldo: {ChatColors.DarkRed}{playerSaldo} {Config.config.Settings.Currency_Name}"));
        });
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        float timeInSeconds = Config.config.Settings.Time;
        float chance = Config.config.Settings.Chance_To_Win;

        chance /= 100.0f;

        if (timeInSeconds > 0 && chance >= 0 && chance <= 1)
        {
            Instance.AddTimer(timeInSeconds, () => Utils.RollForWPLN(chance), TimerFlags.REPEAT);
        }
        else
        {
            Instance.Logger.LogError("Wartości Time i Chance_To_Win muszą być dodatnie i prawidłowe!");
        }
    }
}