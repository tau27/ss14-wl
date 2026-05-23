using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Content.Shared.Damage;

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
    [DataField]
    public float Chance { get; set; } = 1.0f;

    [DataField]
    public DamageSpecifier? FailDamage;

    [DataField]
    public float? SpeedModifier;
    // WL-Changes: Getto-surgery end
}
