using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchDetectorComponent : Component
{
    [DataField("detectType"), required: true]
    public ProtoId<ResearchTypePrototype> ResearchType;

    [DataField("extrimalType")]
    public ProtoId<ResearchPointsTypePrototype> ExtrimalPointsType = "Experimental";

    [DataField("pointsType")]
    public ProtoId<ResearchPointsTypePrototype>? DefaultPointsType = null;

    [DataField]
    public double ResearchPercent = 0.01;

    [DataField]
    public float Range = 10f;

    [DataField]
    public bool Active = true;
}

[ByRefEvent]
public record struct GetResearchDataEvent
{
    public EntityUid Detector;

    public ProtoId<ResearchTypePrototype> ResearchType;

    public Dictionary<ProtoId<ResearchCategoryPrototype>, (double, Dictionary<ProtoId<ResearchPointsTypePrototype>, double>)> ResearchData;

    public GetServerResearchEvent(EntityUid detector, ProtoId<ResearchTypePrototype> researchType)
    {
        Detector = detector;
        ResearchType = researchType;
        ResearchData = new();
    }
}
