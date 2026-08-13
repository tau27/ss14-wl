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

    public Dictionary<ProtoId<ResearchPrototype>, ResearchState> ResearchesData;

    public ResearchMainConsoleBoundInterfaceState(Dictionary<ProtoId<ResearchPointsTypePrototype>, (double, double, double)> pointsData,
            Dictionary<ProtoId<ResearchPrototype>, ResearchState> researchesData)
    {
        PointsData = pointsData;
        ResearchesData = researchesData;
    }
}

[Serializable, NetSerializable]
public sealed class TerminalStartResearchMessage : BoundUserInterfaceMessage
{
    public string Id;

    public TerminalStartResearchMessage(string id)
    {
        Id = id;
    }
}


[Serializable, NetSerializable]
public enum ResearchMainConsoleUiKey : byte
{
    Key
}
