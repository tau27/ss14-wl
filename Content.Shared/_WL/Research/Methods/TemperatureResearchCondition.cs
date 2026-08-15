using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class TemperatureResearchConditionSystem : ResearchConditionSystem<TransformComponent, TemperatureResearchCondition>
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    protected override void Condition(Entity<TemperatureComponent> entity, ref ResearchConditionEvent<TemperatureResearchCondition> args)
    {
        args.Result = entity.Comp.CurrentTemperature;

        if (entity.Comp.CurrentTemperature < args.Condition.MinExtrimal)
        {
            args.Points.Add(args.Condition.ExtrimalType, args.Condition.MinExtrimal - entity.Comp.CurrentTemperature)
        }
        else if (entity.Comp.CurrentTemperature > args.Condition.MaxExtrimal)
        {
            args.Points.Add(args.Condition.ExtrimalType, entity.Comp.CurrentTemperature - args.Condition.MaxExtrimal)
        }

        args.Points.Add(args.Condition.BaseType, 100);
    }
}

/// <inheritdoc cref="ResearchCondition"/>
public sealed partial class TemperatureResearchCondition : ResearchConditionBase<TemperatureResearchCondition>
{
    [DataField]
    public float MinExtrimal = 0f;

    [DataField]
    public float MaxExtrimal = float.PositiveInfinity;
}
