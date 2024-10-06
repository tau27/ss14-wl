using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
[CVarDefs]
public sealed class WLCVars
{
    /*
     *  Game
     */
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

    /*
     *  Discord
     */
    /// <summary>
    /// Через какое время все токены на подключение аккаунта игры к дискорду будут недействительны.
    /// </summary>
    public static readonly CVarDef<int> DiscordAuthTokensExpirationTime =
        CVarDef.Create("discord.auth_tokens_expiration_time", 300, CVar.REPLICATED);

    /*
     * Poly
     */
    /// <summary>
    /// Интервал, через который Поли™ будет готова выбрать новое сообщение!
    /// </summary>
    public static readonly CVarDef<int> PolyMessageChooseCooldown =
        CVarDef.Create("poly.choose_cooldown_time", 3600, CVar.SERVERONLY);

    /// <summary>
    /// Нужна ли очистка выбранных Поли™ сообщений после РАУНДА.
    /// </summary>
    public static readonly CVarDef<bool> PolyNeededRoundEndCleanup =
        CVarDef.Create("poly.round_end_cleanup", false, CVar.SERVERONLY);
}
