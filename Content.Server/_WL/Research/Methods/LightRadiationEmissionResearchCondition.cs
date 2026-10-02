using Content.Shared._WL.Research.Methods;
using Content.Shared.Radiation.Components;
using Robust.Server.GameObjects;

namespace Content.Server._WL.Research.Methods;

public sealed partial class LightRadiationEmissionResearchConditionSystem : ResearchConditionSystem<RadiationSourceComponent, LightRadiationEmissionResearchCondition>
{
    protected override void Condition(Entity<RadiationSourceComponent> entity, ref ResearchConditionEvent<LightRadiationEmissionResearchCondition> args)
    {
        var hasLight = HasComp<PointLightComponent>(entity.Owner);

        if (!hasLight)
            return;

        var radiationIntensity = entity.Comp.Intensity;

        args.Result = entity.Comp.Intensity;


        if (radiationIntensity > args.Condition.MaxEmission)
        {
            args.Points.PointsDict.Add(args.Condition.ExtrimalType, (radiationIntensity - args.Condition.MaxEmission) * 10f);
        }

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}
