using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeResearches()
    {
    }

    public void Sync(EntityUid primaryUid, EntityUid otherUid, ResearchDatabaseComponent? primaryDb = null, ResearchDatabaseComponent? otherDb = null)
    {
        if (!Resolve(primaryUid, ref primaryDb) || !Resolve(otherUid, ref otherDb))
            return;

        primaryDb.SupportedDisciplines = otherDb.SupportedDisciplines;
        primaryDb.Researches = otherDb.Researches;
        primaryDb.UnlockedRecipes = otherDb.UnlockedRecipes;

        Dirty(primaryUid, primaryDb);

        var ev = new TechnologyDatabaseSynchronizedEvent();
        RaiseLocalEvent(primaryUid, ref ev);
    }

    public void SyncClientWithServer(EntityUid uid, ResearchDatabaseComponent? databaseComponent = null, ResearchClientNewComponent? clientComponent = null)
    {
        if (!Resolve(uid, ref databaseComponent, ref clientComponent, false))
            return;

        if (!TryComp<ResearchDatabaseComponent>(clientComponent.Server, out var serverDatabase))
            return;

        Sync(uid, clientComponent.Server.Value, databaseComponent, serverDatabase);
    }

    public void UpdateResearchesStatus(EntityUid uid, ResearchDatabaseComponent? database = null, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref database) || !Resolve(uid, ref server))
            return;

        foreach (var (proto, state) in database.Researches)
        {
            if (state.Status != ResearchStatus.NotResearched)
                continue;

            state.DepsState = GetResearchState(uid, proto, server);
        }

        Dirty(uid, database);
    }

    public void UpdateResearchesProgress(EntityUid uid, ResearchDatabaseComponent? database = null, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref database) || !Resolve(uid, ref server))
            return;

        var mainResearchId = server.ResearchQueue[0];
        var mainResearchProto = ProtoMan.Index<ResearchPrototype>(mainResearchId);

        if (!database.Researches.TryGetValue(mainResearchProto, out var mainResearchState))
            return;

        var researchSpeed = GetResearchSpeed(uid, server);
        var modeProto = ProtoMan.Index<ResearchModePrototype>(mainResearchState.ModeId);

        var cost = mainResearchProto.PackagesCost * modeProto.PackagesModifier;

        mainResearchState.ResearchedPackages = Math.Min(mainResearchState.ResearchedPackages + researchSpeed, cost);

        if (mainResearchState.ResearchedPackages >= cost)
        {
            FinishResearch(mainResearchProto);
            mainResearchState.Status = Researched;

            server.ResearchQueue.RemoveAt(0);

            if (server.ResearchQueue.Length > 0 ||
                database.Researches.TryGetValue(server.ResearchQueue[0], out var newResearchState))
                    newResearchState.Status = ResearchStatus.Researching;
        }

        Dirty(uid, server);
        Dirty(uid, database);
    }

    private void FinishResearch(EntityUid uid, ProtoId<ResearchPrototype> researchProto, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref server))
            return;

        //TODO: add finish research event
    }

    public ResearchDepsStatus GetResearchState(EntityUid uid, ProtoId<ResearchPrototype> researchProto, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref server))
            return;

        var research = ProtoMan.Index<ResearchPrototype>(researchProto);

        foreach (var parent in research.ParentsResearches)
        {
            if (!server.Researches.TryGetValue(parent, out var parentState) ||
                    parentState.Status != Researched)
                return ResearchDepsStatus.ParentsReq;
        }

        foreach (var (type, value) in research.PointsCost)
        {
            if (!server.PointsDict.TryGetValue(type, out var serverValue) ||
                    value > serverValue)
                return ResearchDepsStatus.PointsReq;
        }

        return ResearchDepsStatus.Allowed;
    }
}
