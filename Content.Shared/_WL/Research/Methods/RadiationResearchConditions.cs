namespace Content.Shared._WL.Research.Methods;

public sealed partial class LightRadiationEmissionResearchCondition : ResearchConditionBase<LightRadiationEmissionResearchCondition>
{
    [DataField]
    public float MaxEmission = float.PositiveInfinity;
}

public sealed partial class ReceivedRadiationResearchCondition : ResearchConditionBase<ReceivedRadiationResearchCondition>
{
    [DataField]
    public float MaxRadiation = float.PositiveInfinity;
}
