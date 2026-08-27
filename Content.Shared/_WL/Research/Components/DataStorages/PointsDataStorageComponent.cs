using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class PointsDataStorageComponent : BaseDataStorageComponent
{
    [ViewVariables(VVAccess.ReadOnly)]
    public ResearchPointsSpecifier Points { get; set; } = new();

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<ResearchPointsTypePrototype>> AllowedPointsTypes = new();
}
