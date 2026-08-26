using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeTerminal()
    {
        SubscribeLocalEvent<ResearchMainConsoleComponent, TerminalStartResearchMessage>(OnStartResearch);
        SubscribeLocalEvent<ResearchMainConsoleComponent, ResearchServerNewUpdatedEvent>(OnServerUpdated);
    }

    private void OnStartResearch(EntityUid uid, ResearchMainConsoleComponent component, TerminalStartResearchMessage args)
    {
        var act = args.Actor;

        if (!this.IsPowered(uid, EntityManager))
            return;

        if (!ProtoMan.TryIndex<ResearchPrototype>(args.Id, out var technologyPrototype))
            return;

        if(!TryGetClientServer(uid, out var serverUid, out var serverComp))
            return;

        if (!TryStartResearch(serverUid.Value, args.Id, serverComp))
            return;

        SyncClientWithServer(uid);
        UpdateConsoleInterface(uid, component);
    }

    private void OnServerUpdated(EntityUid uid, ResearchMainConsoleComponent component, ref ResearchServerNewUpdatedEvent args)
    {
        if (!_uiSystem.IsUiOpen(uid, ResearchMainConsoleUiKey.Key))
            return;

        UpdateConsoleInterface(uid, component);
    }

    private void UpdateConsoleInterface(EntityUid uid, ResearchMainConsoleComponent? component = null, ResearchClientNewComponent? clientComponent = null)
    {
        if (!Resolve(uid, ref component, ref clientComponent, false))
            return;

        ResearchMainConsoleBoundInterfaceState state;

        var pointsData = new Dictionary<ProtoId<ResearchPointsTypePrototype>, (double, double, double)>();
        var researchData = new Dictionary<ProtoId<ResearchPrototype>, ResearchState>();

        if (TryGetClientServer(uid, out var server, out var serverComponent, clientComponent) &&
                clientComponent.ConnectedToServer &&
                TryComp<PointsDataStorageComponent>(server, out var serverStorage))
            foreach (var (key, value) in serverStorage.PointsDict)
            {
                if (serverComponent.PointsStatistic.TryGetValue(key, out var statistics))
                    pointsData.TryAdd(key, (value, statistics.Item1, statistics.Item2));
                else
                    pointsData.TryAdd(key, (value, 0, 0));
            }

        if (TryComp<ResearchDatabaseComponent>(uid, out var database))
            researchData = database.Researches;

        state = new ResearchMainConsoleBoundInterfaceState(pointsData, researchData);

        _uiSystem.SetUiState(uid, ResearchMainConsoleUiKey.Key, state);
    }
}
