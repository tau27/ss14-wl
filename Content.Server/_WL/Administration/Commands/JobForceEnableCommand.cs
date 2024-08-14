using Content.Server.Administration;
using Content.Server.Preferences.Managers;
using Content.Shared.Administration;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._WL.Administration.Commands
{
    [AdminCommand(AdminFlags.QuestionnaireSpecialist)]
    public sealed partial class JobForceEnableCommand : LocalizedCommands
    {
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IPlayerManager _playMan = default!;
        [Dependency] private readonly IServerPreferencesManager _prefMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;

        public override string Command => "jobforceenable";
        public override string Description => "Позволяет насильно разблокировать проверку должности на возраст.";
        public override string Help => "jobforceenable <player_username> <character_name> <job_name> [True/False]";

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var players = _playMan.GetAllPlayerData();

                return CompletionResult.FromHintOptions(players.Select(p => p.UserName), "<username>");
            }
            else if (args.Length == 2)
            {
                var username = args[0];
                if (_playMan.TryGetUserId(username, out var user_id))
                {
                    var characters = _prefMan.GetPreferencesOrNull(user_id)?.Characters;
                    if (characters != null)
                        return CompletionResult.FromHintOptions(characters.Select(c => c.Value.Name), "<character_name>");
                }
            }
            else if (args.Length == 3)
            {
                var prototypes = _protoMan.EnumeratePrototypes<JobPrototype>();
                return CompletionResult.FromHintOptions(prototypes.Select(p => p.ID), "<job_id>");
            }
            else if (args.Length == 4)
            {
                return CompletionResult.FromHint("[True/False]");
            }

            return CompletionResult.Empty;
        }

        public override async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            try
            {
                if (args.Length != 4 && args.Length != 3)
                {
                    shell.WriteError(LocalizationManager.GetString("shell-wrong-arguments-number"));
                    return;
                }

                var username = args[0];
                var character_name = args[1];
                var job_id = args[2];
                var string_value = args.Length == 4
                    ? args[3]
                    : null;

                if (!_protoMan.TryIndex<JobPrototype>(job_id, out var jobProto))
                {
                    shell.WriteError($"Указанный ID прототипа {nameof(JobPrototype)} не был найден!");
                    return;
                }

                if (!_playMan.TryGetUserId(username, out var netUserId))
                {
                    shell.WriteError($"Игрока с никнеймом [color=yellow]{username}[/color] не найдено.");
                    return;
                }

                var characters = _prefMan.GetPreferencesOrNull(netUserId);
                if (characters == null)
                {
                    shell.WriteError("Произошла неизвестная ошибка при получении профиля игрока!");
                    return;
                }

                var character_profile_and_slot = characters.Characters
                    .FirstOrNull(kv => kv.Value.Name.Equals(character_name, StringComparison.CurrentCultureIgnoreCase));

                var character_profile = (HumanoidCharacterProfile?)character_profile_and_slot?.Value;
                var character_slot = character_profile_and_slot?.Key;

                if (character_profile_and_slot == null || character_profile == null || character_slot == null)
                {
                    shell.WriteError($"Персонаж [color=blue]{character_name}[/color] игрока [color=yellow]{username}[/color] не был найден!");
                    return;
                }

                if (string_value != null)
                {
                    if (!bool.TryParse(string_value, out var value))
                    {
                        shell.WriteError($"Не удалось преобразовать [color=gray]{string_value}[/color] в [color=blue]{nameof(Boolean)}[/color]");
                        return;
                    }

                    var newProfile = character_profile.WithJobUnblocking(job_id, value);

                    await _prefMan.SetProfile(netUserId, character_slot.Value, newProfile);
                }
                else
                {
                    var off_on_string = "";

                    if (character_profile.JobUnblockings.TryGetValue(job_id, out var value))
                        off_on_string = value
                            ? "выключена"
                            : "включена";
                    else off_on_string = "включена";

                    var text = $"Для персонажа [color=green]{character_name}[/color] пользователя " +
                        $"[color=yellow]{username}[/color] в должности [color=gray]{jobProto.LocalizedName}[/color] " +
                        $"проверка на возраст [color=blue]{off_on_string}[/color].";

                    shell.WriteLine(
                        FormattedMessage.FromMarkupOrThrow(text)
                    );
                }
            }
            catch (Exception ex)
            {
                shell.WriteError(ex.Message);
            }
        }
    }
}
