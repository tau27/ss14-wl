using Robust.Shared.Physics.Components;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class GridSpeedResearchConditionSystem : ResearchConditionSystem<TransformComponent, GridSpeedResearchCondition>
{
    protected override void Condition(Entity<TransformComponent> entity, ref ResearchConditionEvent<GridSpeedResearchCondition> args)
    {
        if (entity.Comp.GridUid is not { } grid || !TryComp<PhysicsComponent>(grid, out var physics))
            return;

        args.Result = physics.LinearVelocity.Length();

        if (args.Result > args.Condition.MaxSpeed)
        {
            args.Points.PointsDict.Add(args.Condition.ExtrimalType, args.Condition.MaxSpeed - args.Result);
        }

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class GridSpeedResearchCondition : ResearchConditionBase<GridSpeedResearchCondition>
{
    [DataField]
    public float MaxSpeed = float.PositiveInfinity;
}
