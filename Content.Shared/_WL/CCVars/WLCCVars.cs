using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
[CVarDefs]
public sealed class WLCVars
{
    /// <summary>
    /// Через сколько времени(в секундах) появится кнопка возвращения в лобби.
    /// </summary>
    public static readonly CVarDef<int> GhostReturnToLobbyButtonCooldown =
        CVarDef.Create("ghost.return_to_lobby_button_cooldown", 1200, CVar.SERVERONLY);

    /// <summary>
    /// Нужно ли проверять игрока на возраст при выборе роли.
    /// </summary>
    public static readonly CVarDef<bool> IsAgeCheckNeeded =
        CVarDef.Create("game.is_age_check_needed", true, CVar.REPLICATED);

    /// <summary>
    /// Токен для авторизации htpp-запросов на api сервера.
    /// </summary>
    public static readonly CVarDef<string> WLApiToken =
        CVarDef.Create(
            "admin.wl_api_token", "92132c05009d46c25ffa1d7263b8f24226abef8a7503ce7c26175b0f0e3db61dc82907a3bd7b72f8321206fc42576bb2896c9a937714d2cbf422d3507fc078492cedd3fa6300eb8fa75f4ceffe8577c6790bc0a93ea989e9cbc15e090dff97eb",
            CVar.SERVERONLY | CVar.CONFIDENTIAL,
            "Строковой токен, использующийся для авторизации HTTP-запросов, отправленных на API сервера.");


    /// <summary>
    /// Через какое время все токены на подключение аккаунта игры к дискорду будут недействительны.
    /// </summary>
    public static readonly CVarDef<int> DiscordAuthTokensExpirationTime =
        CVarDef.Create("discord.auth_tokens_expiration_time", 300, CVar.REPLICATED);
}
