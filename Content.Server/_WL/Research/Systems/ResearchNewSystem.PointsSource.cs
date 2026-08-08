using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeSource()
    {
        SubscribeLocalEvent<PointsSourceDebugComponent, GetServerResearchEvent>(OnGetServerResearch);
    }

    private void OnGetServerResearch(Entity<PointsSourceDebugComponent> source, ref GetServerResearchEvent args)
    {
        if (CanProduce(source))
            args.ResearchData.Add(source.Comp.DebugCategory, (source.Comp.TypesDict, source.Comp.Points, source.Comp.PointsType));
    }

    public bool CanProduce(Entity<PointsSourceDebugComponent> source)
    {
        return source.Comp.Active && this.IsPowered(source, EntityManager);
    }
}
