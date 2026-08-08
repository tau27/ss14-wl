using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research.Components;
using Content.Shared.Research.Components;
using Robust.Shared.Utility;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeClient()
    {
        SubscribeLocalEvent<ResearchClientNewComponent, MapInitEvent>(OnClientMapInit);
        SubscribeLocalEvent<ResearchClientNewComponent, ComponentShutdown>(OnClientShutdown);
        SubscribeLocalEvent<ResearchClientNewComponent, BoundUIOpenedEvent>(OnClientUIOpen);
        SubscribeLocalEvent<ResearchClientNewComponent, AnchorStateChangedEvent>(OnClientAnchorStateChanged);

        SubscribeLocalEvent<ResearchClientNewComponent, ResearchClientSyncMessage>(OnClientSyncMessage);
        SubscribeLocalEvent<ResearchClientNewComponent, ResearchClientServerSelectedMessage>(OnClientSelected);
        SubscribeLocalEvent<ResearchClientNewComponent, ResearchClientServerDeselectedMessage>(OnClientDeselected);
        SubscribeLocalEvent<ResearchClientNewComponent, ResearchRegistrationNewChangedEvent>(OnClientRegistrationChanged);
    }

    #region UI

    private void OnClientSelected(EntityUid uid, ResearchClientNewComponent component, ResearchClientServerSelectedMessage args)
    {
        if (!TryGetServerById(uid, args.ServerId, out var serveruid, out var serverComponent))
            return;

        // Validate that we can access this server.
        if (!GetServers(uid).Contains((serveruid.Value, serverComponent)))
            return;

        UnregisterClient(uid, component);
        RegisterClient(uid, serveruid.Value, component, serverComponent);
    }

    private void OnClientDeselected(EntityUid uid, ResearchClientNewComponent component, ResearchClientServerDeselectedMessage args)
    {
        UnregisterClient(uid, clientComponent: component);
    }

    private void OnClientSyncMessage(EntityUid uid, ResearchClientNewComponent component, ResearchClientSyncMessage args)
    {
        UpdateClientInterface(uid, component);
    }
    #endregion

    private void OnClientRegistrationChanged(EntityUid uid, ResearchClientNewComponent component, ref ResearchRegistrationNewChangedEvent args)
    {
        UpdateClientInterface(uid, component);
    }

    private void OnClientMapInit(EntityUid uid, ResearchClientNewComponent component, MapInitEvent args)
    {
        Logger.Debug("Try client inig");
        if (GetServers(uid).FirstOrNull() is { } server)
            RegisterClient(uid, server, component, server);
    }

    private void OnClientShutdown(EntityUid uid, ResearchClientNewComponent component, ComponentShutdown args)
    {
        UnregisterClient(uid, component);
    }

    private void OnClientUIOpen(EntityUid uid, ResearchClientNewComponent component, BoundUIOpenedEvent args)
    {
        UpdateClientInterface(uid, component);
    }

    private void OnClientAnchorStateChanged(Entity<ResearchClientNewComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
        {
            if (ent.Comp.Server is not null)
                return;

            if (GetServers(ent).FirstOrNull() is { } server)
                RegisterClient(ent, server, ent, server);
        }
        else
        {
            UnregisterClient(ent, ent.Comp);
        }
    }

    private void UpdateClientInterface(EntityUid uid, ResearchClientNewComponent? component = null)
    {
        Logger.Debug("Try update interface");
        if (!Resolve(uid, ref component, false))
            return;

        TryGetClientServer(uid, out _, out var serverComponent, component);

        var names = GetServerNames(uid);
        var state = new ResearchClientBoundInterfaceState(
            names.Length,
            names,
            GetServerIds(uid),
            serverComponent?.Id ?? -1);

        _uiSystem.SetUiState(uid, ResearchClientUiKey.Key, state);
    }

    /// <summary>
    /// Tries to get the server belonging to a client
    /// </summary>
    /// <param name="uid">The client</param>
    /// <param name="server">It's server. Null if false.</param>
    /// <param name="serverComponent">The server's ResearchServerNewComponent. Null if false</param>
    /// <param name="component">The client's Researchclient component</param>
    /// <returns>If the server was successfully retrieved.</returns>
    public bool TryGetClientServer(EntityUid uid,
        [NotNullWhen(returnValue: true)] out EntityUid? server,
        [NotNullWhen(returnValue: true)] out ResearchServerNewComponent? serverComponent,
        ResearchClientNewComponent? component = null)
    {
        server = null;
        serverComponent = null;

        if (!Resolve(uid, ref component, false))
            return false;

        if (component.Server == null)
            return false;

        if (!TryComp(component.Server, out serverComponent))
            return false;

        server = component.Server;
        return true;
    }
}
