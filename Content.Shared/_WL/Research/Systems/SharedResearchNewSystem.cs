using System.Linq;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Systems;

public abstract partial class SharedResearchNewSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResearchDatabaseComponent, MapInitEvent>(OnRDBInit);
    }

    private void OnRDBInit(EntityUid uid, ResearchDatabaseComponent component, MapInitEvent args)
    {
        var avalibleResearches = GetAvailableResearches(uid, component);

        foreach (var researchProto in avalibleResearches)
        {
            component.Researches.Add(researchProto, new ResearchState());
        }

        Dirty(uid, component);
    }

    public List<ResearchPrototype> GetAvailableResearches(EntityUid uid, ResearchDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return new List<ResearchPrototype>();

        var availableTechnologies = new List<ResearchPrototype>();
        foreach (var tech in ProtoMan.EnumeratePrototypes<ResearchPrototype>())
        {
            if (!component.SupportedDisciplines.Contains(tech.Discipline))
                availableTechnologies.Add(tech);
        }

        return availableTechnologies;
    }
}
