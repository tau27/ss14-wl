using Content.Shared._WL.Languages.Components;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages;

public abstract class SharedLanguagesSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager _prototype = default!;
    [Dependency] protected readonly SharedGameTicker _ticker = default!;

    public LanguagePrototype? GetLanguagePrototype(ProtoId<LanguagePrototype> id)
    {
        _prototype.TryIndex(id, out var proto);
        return proto;
    }

    public string ObfuscateMessage(string message, ProtoId<LanguagePrototype> language)
    {
        var proto = GetLanguagePrototype(language);

        if (proto == null)
        {
            return message;
        }
        else
        {
            var obfus = proto.Obfuscation.Obfuscate(message, _ticker.RoundId);
            return obfus;
        }
    }
}
