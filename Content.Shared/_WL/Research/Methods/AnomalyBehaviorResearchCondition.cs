using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class AnomalyBehaviorResearchConditionSystem : ResearchConditionSystem<AnomalyComponent, AnomalyBehaviorResearchCondition>
{
    protected override void Condition(Entity<AnomalyComponent> entity, ref ResearchConditionEvent<AnomalyBehaviorResearchCondition> args)
    {
        if (entity.Comp.CurrentBehavior != args.Condition.Behavior)
            return;

        args.Result = 1;
        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class AnomalyBehaviorResearchCondition : ResearchConditionBase<AnomalyBehaviorResearchCondition>
{
    [DataField(required: true)]
    public ProtoId<AnomalyBehaviorPrototype> Behavior;
}
