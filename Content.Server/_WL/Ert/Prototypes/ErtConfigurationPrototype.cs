using Content.Shared._WL.Ert;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._WL.Ert.Prototypes
{
    [Prototype("ertConfig")]
    public sealed partial class ErtConfigurationPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public Dictionary<ErtType, ErtConfigEntry> Entry { get; private set; } = new();

        [DataDefinition]
        [UsedImplicitly]
        public sealed partial class ErtConfigEntry
        {
            [DataField(required: true)]
            public ResPath ShuttlePath { get; private set; }

            [DataField]
            public float ShuttleSpawnOffset { get; private set; } = 300;
        }

        public float ShuttleOffset(ErtType ert)
        {
            return Entry[ert].ShuttleSpawnOffset;
        }

        public ResPath ShuttlePath(ErtType ert)
        {
            return Entry[ert].ShuttlePath;
        }
    }
}
