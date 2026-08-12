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
        SubscribeLocalEvent<ResearchMainConsoleComponent, ResearchServerNewUpdatedEvent>(OnServerUpdated);
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
        if (TryGetClientServer(uid, out _, out var serverComponent, clientComponent) &&
                clientComponent.ConnectedToServer)
            pointsData = serverComponent.PointsDict;

        state = new ResearchMainConsoleBoundInterfaceState(pointsData);

        _uiSystem.SetUiState(uid, ResearchMainConsoleUiKey.Key, state);
    }
}
