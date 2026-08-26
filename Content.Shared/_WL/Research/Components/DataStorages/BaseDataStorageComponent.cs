using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

public abstract partial class BaseDataStorageComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public double LocalSize;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public double ExpiredLocalSize = 0;
}
