using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared._WL.Languages.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LanguagesSpeekingComponent : Component
{
    [DataField]
    public bool IsUnderstanding = true;

    [DataField]
    public bool IsSpeeking = true;

    [DataField]
    public List<ProtoId<LanguagePrototype>> UnderstandingLanguages = [];

    [DataField]
    public List<ProtoId<LanguagePrototype>> SpeekingLanguages = [];

    [DataField]
    public string CurrentLanguage = "";

    [Serializable, NetSerializable]
    public sealed class State : ComponentState
    {
        public bool IsUnderstanding = default!;
        public bool IsSpeeking = default!;
        public string CurrentLanguage = default!;
        public List<ProtoId<LanguagePrototype>> SpeekingLanguages = default!;
        public List<ProtoId<LanguagePrototype>> UnderstangingLanguages = default!;
    }
}
