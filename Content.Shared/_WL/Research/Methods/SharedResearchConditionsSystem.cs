using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class SharedResearchConditionsSystem : EntitySystem, IResearchConditionRaiser
{
    // ResearchValue, dict of points
    public (double, Dictionary<ProtoId<ReseachPointsTypePrototype>, double>) GetCondition<T>(EntityUid target, T condition, EntityUid? sourceEnt = null) where T : ResearchCondition
    {
        return condition.RaiseEvent(target, this, sourceEnt);
    }

    public (double, Dictionary<ProtoId<ReseachPointsTypePrototype>, double>) RaiseConditionEvent<T>(EntityUid target, T effect, EntityUid? sourceEnt) where T : ResearchConditionBase<T>
    {
        var conditionEv = new ResearchConditionEvent<T>(effect, sourceEnt);
        RaiseLocalEvent(target, ref conditionEv);
        return (conditionEv.Result, conditionEv.Points);
    }
}

public abstract partial class ResearchConditionSystem<T, TCon> : EntitySystem where T : Component where TCon : ResearchConditionBase<TCon>
{
    public override void Initialize()
    {
        SubscribeLocalEvent<T, ResearchConditionEvent<TCon>>(Condition);
    }

    protected abstract void Condition(Entity<T> entity, ref ResearchConditionEvent<TCon> args);
}

public interface IResearchConditionRaiser
{
    bool RaiseConditionEvent<T>(EntityUid target, T condition, EntityUid? sourceEnt) where T : ResearchConditionBase<T>;
}

public abstract partial class ResearchConditionBase<T> : ResearchCondition where T : ResearchConditionBase<T>
{
    public override bool RaiseEvent(EntityUid target, IResearchConditionRaiser raiser, EntityUid? sourceEnt)
    {
        if (this is not T type)
            return false;

        return raiser.RaiseConditionEvent(target, type, sourceEnt);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class ResearchCondition
{
    [DataField]
    public ProtoId<ResearchPointsType> BaseType;

    [DataField]
    public ProtoId<ResearchPointsType> ExtrimalType = "Experimental";

    public abstract bool RaiseEvent(EntityUid target, IResearchConditionRaiser raiser, EntityUid? sourceEnt);

    public abstract string ResearchConditionGuidebookText(IPrototypeManager prototype);
}

[ByRefEvent]
[DataRecord]
public partial record struct ResearchConditionEvent<T>(T Condition, EntityUid? SourceEnt) where T : ResearchConditionBase<T>
{
    [DataField]
    public double Result;

    [DataField]
    public Dictionary<ProtoId<ReseachPointsTypePrototype>, double> Points;

    public readonly T Condition = Condition;

    public readonly EntityUid? SourceEnt = SourceEnt;
}
