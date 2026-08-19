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

        var ev = new ResearchDatabaseSynchronizedEvent();
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

    private void UpdateResearchesStatus(EntityUid uid, ResearchDatabaseComponent? database = null, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref database) || !Resolve(uid, ref server))
            return;

        foreach (var (proto, state) in database.Researches)
        {
            if (state.Status != ResearchStatus.NotResearched)
                continue;

            var bufferState = database.Researches[proto];
            bufferState.DepsState = GetResearchState(uid, proto, server, database);

            database.Researches[proto] = bufferState;
        }

        Dirty(uid, database);
    }

    private void UpdateResearchesProgress(EntityUid uid, ResearchDatabaseComponent? database = null, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref database) || !Resolve(uid, ref server))
            return;

        if (server.ResearchQueue.Count == 0)
            return;

        var mainResearchId = server.ResearchQueue[0];
        var mainResearchProto = ProtoMan.Index<ResearchPrototype>(mainResearchId);

        if (!database.Researches.TryGetValue(mainResearchProto, out var mainResearchState))
            return;

        var researchSpeed = GetResearchSpeed(uid, server);
        var modeProto = ProtoMan.Index<ResearchModePrototype>(mainResearchState.ModeId);

        int cost = (int)(mainResearchState.PackagesCostModed);

        mainResearchState.ResearchedPackages = Math.Min(mainResearchState.ResearchedPackages + researchSpeed, cost);

        if (mainResearchState.ResearchedPackages >= cost)
        {
            FinishResearch(uid, mainResearchProto, server);
            mainResearchState.Status = ResearchStatus.Researched;

            server.ResearchQueue.RemoveAt(0);

            if (server.ResearchQueue.Count > 0 &&
                database.Researches.TryGetValue(server.ResearchQueue[0], out var newResearchState))
            {
                newResearchState.Status = ResearchStatus.Researching;
                database.Researches[server.ResearchQueue[0]] = newResearchState;
            }
        }

        database.Researches[mainResearchProto] = mainResearchState;

        Dirty(uid, server);
        Dirty(uid, database);
    }

    private void FinishResearch(EntityUid uid, ProtoId<ResearchPrototype> researchProto, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref server))
            return;

        //TODO: add finish research event
    }

    private bool TryStartResearch(EntityUid uid, ProtoId<ResearchPrototype> researchId, ResearchServerNewComponent? server = null, ResearchDatabaseComponent? database = null, ProtoId<ResearchModePrototype>? modeId = null)
    {
        if (!Resolve(uid, ref database) || !Resolve(uid, ref server))
            return false;

        if (!database.Researches.TryGetValue(researchId, out var researchState))
            return false;

        if (GetResearchState(uid, researchId, server, database) != ResearchDepsStatus.Allowed)
            return false;

        modeId ??= researchState.ModeId;

        var modeProto = ProtoMan.Index<ResearchModePrototype>(modeId);
        var researchProto = ProtoMan.Index<ResearchPrototype>(researchId);

        foreach (var (type, value) in researchProto.PointsCost)
        {
            if (!TryModifyPoints(uid, type, -value * modeProto.PointsModifier, false, server))
                return false;
        }

        server.ResearchQueue.Add(researchId);

        if (server.ResearchQueue.Count == 1)
            researchState.Status = ResearchStatus.Researching;
        else
            researchState.Status = ResearchStatus.InQueue;

        researchState.ModeId = modeId.Value;
        researchState.PackagesCostModed = modeProto.PackagesModifier * researchProto.PackagesCost;

        database.Researches[researchId] = researchState;

        Dirty(uid, server);
        Dirty(uid, database);

        return true;
    }

    public ResearchDepsStatus GetResearchState(EntityUid uid, ProtoId<ResearchPrototype> researchProto, ResearchServerNewComponent? server = null, ResearchDatabaseComponent? database = null)
    {
        if (!Resolve(uid, ref database) || !Resolve(uid, ref server))
            return ResearchDepsStatus.Invalid;

        var research = ProtoMan.Index<ResearchPrototype>(researchProto);

        foreach (var parent in research.ParentsResearches)
        {
            if (!database.Researches.TryGetValue(parent, out var parentState) ||
                    parentState.Status != ResearchStatus.Researched)
                return ResearchDepsStatus.ParentsReq;
        }

        foreach (var (type, value) in research.PointsCost)
        {
            if (!server.PointsDict.TryGetValue(type, out var serverValue) ||
                    value > serverValue.Item1)
                return ResearchDepsStatus.PointsReq;
        }

        return ResearchDepsStatus.Allowed;
    }
}
