using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.Chat;
using Content.Shared.Speech;
using Content.Shared.IdentityManagement;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._WL.Languages;

public sealed class LanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logMan = default!;
    private ISawmill _sawmill = default!;
    public const string SawmillName = "languages.sys";

    public string GetHeardMessage(string message, EntityUid source, EntityUid listener)
    {
        if (!TryComp<LanguagesSpeekingComponent>(source, out var source_lang))
            return message;

        if (!TryComp<LanguagesSpeekingComponent>(listener, out var listen_lang))
            return message;

        var message_language = source_lang.CurrentLanguage;
        if (listen_lang.IsUnderstanding && listen_lang.IsSpeeking && listen_lang.UnderstandingLanguages.Contains(message_language))
        {
            return message;
        }
        else
        {
            var obfus = ObfuscateMessage(message, message_language);
            return obfus;
        }
    }

    public string ObfuscateMessageFromSource(string message, EntityUid source)
    {
        if (!TryComp<LanguagesSpeekingComponent>(source, out var source_lang))
            return message;
        else
        {
            var message_language = source_lang.CurrentLanguage;
            var obfus = ObfuscateMessage(message, message_language);
            return obfus;
        }
    }

    public bool CanUnderstand(EntityUid source, EntityUid listener)
    {
        if (!TryComp<LanguagesSpeekingComponent>(source, out var source_lang))
            return true;

        if (!TryComp<LanguagesSpeekingComponent>(listener, out var listen_lang))
            return true;

        var message_language = source_lang.CurrentLanguage;
        return listen_lang.IsUnderstanding && listen_lang.IsSpeeking && listen_lang.UnderstandingLanguages.Contains(message_language);
    }

    public bool IsObfusEmoting(EntityUid source)
    {
        _sawmill = _logMan.GetSawmill(SawmillName);
        _sawmill.Info("TRY TO EMOTE");
        if (!TryComp<LanguagesSpeekingComponent>(source, out var source_lang))
            return false;
        else
        {
            var message_language = source_lang.CurrentLanguage;
            var proto = GetLanguagePrototype(message_language);
            if (proto == null)
                return false;
            else
            {
                _sawmill.Info("EMOTES PROMOTES");
                if (proto.Obfuscation.IsEmoting())
                    _sawmill.Info("VANSUSI");
                return proto.Obfuscation.IsEmoting();
            }
        }
    }

    public string GetObfusWrappedMessage(string message, EntityUid source, SpeechVerbPrototype speech, string name)
    {
        //_sawmill = _logMan.GetSawmill(SawmillName);
        var obfusMessage = ObfuscateMessageFromSource(message, source);
        if (IsObfusEmoting(source))
        {
            var ent = Identity.Entity(source, EntityManager);
            var wrappedMessage = Loc.GetString("chat-manager-entity-me-wrap-message",
                ("entityName", name),
                ("entity", ent),
                ("message", FormattedMessage.RemoveMarkupOrThrow(obfusMessage))
            );
            return wrappedMessage;
        }
        else
        {
            var wrappedMessage = Loc.GetString(speech.Bold ? "chat-manager-entity-say-bold-wrap-message" : "chat-manager-entity-say-wrap-message",
                    ("entityName", name),
                    ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
                    ("fontType", speech.FontId),
                    ("fontSize", speech.FontSize),
                    ("message", FormattedMessage.EscapeText(obfusMessage)
                )
            );
            return wrappedMessage;
        }
    }
}
