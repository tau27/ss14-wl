using Content.Shared.Anomaly.Components;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class AnomalySeverityResearchConditionSystem : ResearchConditionSystem<AnomalyComponent, AnomalySeverityResearchCondition>
{
    protected override void Condition(Entity<AnomalyComponent> entity, ref ResearchConditionEvent<AnomalySeverityResearchCondition> args)
    {
        args.Result = entity.Comp.Severity;

        if (entity.Comp.Severity > args.Condition.SeverityThreshold)
        {
            args.Points.PointsDict.Add(args.Condition.ExtrimalType, 100 * args.Result);
        }

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class AnomalySeverityResearchCondition : ResearchConditionBase<AnomalySeverityResearchCondition>
{
    [DataField]
    public float SeverityThreshold = 1f;
}
