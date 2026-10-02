using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class TotalDamageResearchConditionSystem : ResearchConditionSystem<DamageableComponent, TotalDamageResearchCondition>
{
    [Dependency] private DamageableSystem _damageable = default!;

    protected override void Condition(Entity<DamageableComponent> entity, ref ResearchConditionEvent<TotalDamageResearchCondition> args)
    {
        args.Result = _damageable.GetTotalDamage(entity.Owner);

        var extrimalPoints = args.Result > args.Condition.MaxDamage ? args.Result : 0;

        if (args.Result > args.Condition.MaxDamage)
        {
            args.Points.PointsDict.Add(args.Condition.ExtrimalType, 100);
        }

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class TotalDamageResearchCondition : ResearchConditionBase<TotalDamageResearchCondition>
{
    [DataField]
    public float MaxDamage = float.PositiveInfinity;
}

