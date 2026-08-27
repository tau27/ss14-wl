using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class DataReaderComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string SlotId = "storage_slot";
}

[Serializable, NetSerializable]
public enum PointsDataReaderUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum PortState : byte
{
    Empty = 0,
    WrongFormat = 1,
    ReadOnly = 2,
    ReadWrite = 3
}

[Serializable, NetSerializable]
public sealed class PointsDataReadBoundUserInterfaceState : BoundUserInterfaceState
{
    public PortState PortState;

    public ResearchPointsSpecifier StoragePointsData;

    public ResearchPointsSpecifier DiskPointsData;

    public PointsDataReadBoundUserInterfaceState(
            PortState portState,
            ResearchPointsSpecifier storagePointsData,
            ResearchPointsSpecifier? diskPointsData = null
        )
    {
        PortState = portState;
        StoragePointsData = storagePointsData;
        DiskPointsData = diskPointsData ?? new ResearchPointsSpecifier();
    }
}
