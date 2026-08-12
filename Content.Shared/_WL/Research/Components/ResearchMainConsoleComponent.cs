using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class ResearchMainConsoleComponent : Component
{ }

[Serializable, NetSerializable]
public sealed class ResearchMainConsoleBoundInterfaceState : BoundUserInterfaceState
{
    public Dictionary<ProtoId<ResearchPointsTypePrototype>, (double, double, double)> PointsData;
    public ResearchMainConsoleBoundInterfaceState(Dictionary<ProtoId<ResearchPointsTypePrototype>, (double, double, double)> pointsData)
    {
        PointsData = pointsData;
    }
}

[Serializable, NetSerializable]
public enum ResearchMainConsoleUiKey : byte
{
    Key
}
