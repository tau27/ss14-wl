using System.Linq;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Systems;

public abstract partial class SharedResearchNewSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnRDBInit(EntityUid uid, ResearchDatabaseComponent component, ref MapInitEvent args)
    {
        var avalibleResearches = GetAvaliableResearches(uid, component);

        foreach (var researchProto in avalibleResearches)
        {
            component.Researches.Add(researchProto, new ResearchState());
        }

        Dirty(uid, component);
    }

    public List<ResearchPrototype> GetAvaliableResearches(EntityUid uid, ResearchDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return new List<ResearchPrototype>();

        var availableTechnologies = new List<ResearchPrototype>();
        foreach (var tech in ProtoMan.EnumeratePrototypes<ResearchPrototype>())
        {
            if (component.SupportedDisciplines.Contains(tech.Discipline))
                availableTechnologies.Add(tech);
        }

        return availableTechnologies;
    }

    [SubscribeLocalEvent]
    private void OnInsertAttempt(Entity<DataReaderComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Slot.ID != ent.Comp.SlotId || args.Cancelled)
            return;

        if (HasComp<DataStorageComponent>(args.Item))
            return;

        args.Cancelled = true;
    }
}
