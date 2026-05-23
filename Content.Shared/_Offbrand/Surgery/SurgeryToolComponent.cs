using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Content.Shared.Damage;
using Content.Shared._WL._Offbrand.Wounds;
using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.Surgery;

[RegisterComponent]
[Access(typeof(SurgeryToolSystem))]
public sealed partial class SurgeryToolComponent : Component
{
    [DataField(required: true)]
    public SlotFlags SlotsToCheck;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField(required: true)]
    public LocId SlotsDenialPopup;

    [DataField(required: true)]
    public LocId DownDenialPopup;

    // WL-Changes: Getto-surgery start
    [DataField(required: true)]
    public ProtoId<LocalizedDatasetPrototype> FailPopups;

    [DataField]
    public float SuccessChance { get; set; } = 1.0f;

    [DataField]
    public float WoundChance { get; set; } = 0.0f;

    [DataField]
    public DamageSpecifier? FailDamage;

    [DataField]
    public float? SpeedModifier;

    [DataField]
    public List<SimpleWoundSpecifier> FailWounds = new List<SimpleWoundSpecifier>();
    // WL-Changes: Getto-surgery end
}
