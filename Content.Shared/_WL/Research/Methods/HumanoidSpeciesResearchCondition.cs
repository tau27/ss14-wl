using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class HumanoidSpeciesResearchConditionSystem : ResearchConditionSystem<HumanoidProfileComponent, HumanoidSpeciesResearchCondition>
{
    protected override void Condition(Entity<HumanoidProfileComponent> entity, ref ResearchConditionEvent<HumanoidSpeciesResearchCondition> args)
    {
        if (entity.Comp.Species != args.Condition.Species)
            return;

        args.Result = 1;
        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class HumanoidSpeciesResearchCondition : ResearchConditionBase<HumanoidSpeciesResearchCondition>
{
    [DataField]
    public ProtoId<SpeciesPrototype> Species = HumanoidCharacterProfile.DefaultSpecies;
}
