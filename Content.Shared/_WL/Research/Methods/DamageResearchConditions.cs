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

        if (args.Result > args.Condition.MaxDamage)
        {
            args.Points.PointsDict.Add(args.Condition.ExtrimalType, args.Result - args.Condition.MaxDamage);
        }

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class TotalDamageResearchCondition : ResearchConditionBase<TotalDamageResearchCondition>
{
    [DataField]
    public float MaxDamage = float.PositiveInfinity;
}

public sealed partial class DamageTypeResearchConditionSystem : ResearchConditionSystem<DamageableComponent, DamageTypeResearchCondition>
{
    [Dependency] private DamageableSystem _damageable = default!;

    protected override void Condition(Entity<DamageableComponent> entity, ref ResearchConditionEvent<DamageTypeResearchCondition> args)
    {
        var damage = _damageable.GetAllDamage(entity.Owner);

        if (damage.DamageDict.TryGetValue(args.Condition.DamageType, out var value))
            args.Result = value;

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class DamageTypeResearchCondition : ResearchConditionBase<DamageTypeResearchCondition>
{
    [DataField(required: true)]
    public ProtoId<DamageTypePrototype> DamageType;
}
