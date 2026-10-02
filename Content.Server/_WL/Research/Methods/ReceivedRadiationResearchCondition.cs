using Content.Server.Radiation.Components;
using Content.Shared._WL.Research.Methods;

namespace Content.Server._WL.Research.Methods;

public sealed partial class ReceivedRadiationResearchConditionSystem : ResearchConditionSystem<RadiationReceiverComponent, ReceivedRadiationResearchCondition>
{
    protected override void Condition(Entity<RadiationReceiverComponent> entity, ref ResearchConditionEvent<ReceivedRadiationResearchCondition> args)
    {
        args.Result = entity.Comp.CurrentRadiation;

        if (args.Result > args.Condition.MaxRadiation)
        {
            args.Points.PointsDict.Add(args.Condition.ExtrimalType, args.Result - args.Condition.MaxRadiation);
        }

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}
