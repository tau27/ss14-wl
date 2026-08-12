using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeServer()
    {
        SubscribeLocalEvent<ResearchServerNewComponent, ComponentStartup>(OnServerStartup);
        SubscribeLocalEvent<ResearchServerNewComponent, ComponentShutdown>(OnServerShutdown);
    }

    private void OnServerStartup(Entity<ResearchServerNewComponent> ent, ref ComponentStartup args)
    {
        foreach (var type in ent.Comp.AllowedPointsTypes)
        {
            ent.Comp.PointsDict.TryAdd(type, (0, 0, 0));
        }

        var unusedId = EntityQuery<ResearchServerNewComponent>(true)
            .Max(s => s.Id) + 1;
        ent.Comp.Id = unusedId;

        Dirty(ent, ent.Comp);
    }

    private void OnServerShutdown(Entity<ResearchServerNewComponent> ent, ref ComponentShutdown args)
    {
        foreach (var client in new List<EntityUid>(ent.Comp.Clients))
        {
            UnregisterClient(client, ent, dirtyServer: false);
        }
    }

    private bool CanRun(EntityUid uid)
    {
        return this.IsPowered(uid, EntityManager);
    }

    private void UpdateServer(EntityUid uid, int time, ResearchServerNewComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!CanRun(uid))
            return;

        MakeResearch(uid, component);
    }

    private void MakeResearch(EntityUid ent, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(ent, ref server))
            return;

        foreach (var (key, (value, maxValue, _)) in server.PointsDict)
        {
            server.PointsDict[key] = (value, maxValue, 0);
        }

        var ev = new GetServerResearchEvent(ent);
        foreach (var client in server.Clients)
        {
            RaiseLocalEvent(client, ref ev);
        }

        foreach (var (categoryId, (rawData, points, pointsType)) in ev.ResearchData)
        {
            var (pointsValue, maxPoints, pointsPS) = server.PointsDict[pointsType];
            double addValue;

            if (server.ResearchedData.ContainsKey(categoryId))
            {
                addValue = server.ResearchedData[categoryId].ResearchData(rawData, points);
            }
            else
            {
                var categoryProto = ProtoMan.Index(categoryId);
                server.ResearchedData.Add(categoryId, new ResearchField(categoryProto.ResearchTypes.ToArray(), categoryProto.MaxPoints));

                addValue = server.ResearchedData[categoryId].ResearchData(rawData, points);
            }

            server.PointsDict[pointsType] = (pointsValue + addValue, maxPoints + addValue, pointsPS + addValue);
        }

        Dirty(ent, server);

        var ev2 = new ResearchServerNewUpdatedEvent();
        foreach (var client in server.Clients)
        {
            RaiseLocalEvent(client, ref ev2);
        }
    }

    /// <summary>
    /// Registers a client to the specified server.
    /// </summary>
    /// <param name="client">The client being registered</param>
    /// <param name="server">The server the client is being registered to</param>
    /// <param name="clientComponent"></param>
    /// <param name="serverComponent"></param>
    /// <param name="dirtyServer">Whether or not to dirty the server component after registration</param>
    public void RegisterClient(EntityUid client, EntityUid server, ResearchClientNewComponent? clientComponent = null,
        ResearchServerNewComponent? serverComponent = null,  bool dirtyServer = true)
    {
        Logger.Debug("Try register");
        if (!Resolve(client, ref clientComponent, false) || !Resolve(server, ref serverComponent, false))
            return;

        if (serverComponent.Clients.Contains(client))
            return;

        serverComponent.Clients.Add(client);
        clientComponent.Server = server;
        // SyncClientWithServer(client, clientComponent: clientComponent);

        if (dirtyServer && !TerminatingOrDeleted(server))
            Dirty(server, serverComponent);

        var ev = new ResearchRegistrationNewChangedEvent(server);
        RaiseLocalEvent(client, ref ev);
    }

    /// <summary>
    /// Unregisterse a client from its server
    /// </summary>
    /// <param name="client"></param>
    /// <param name="clientComponent"></param>
    /// <param name="dirtyServer"></param>
    public void UnregisterClient(EntityUid client, ResearchClientNewComponent? clientComponent = null, bool dirtyServer = true)
    {
        if (!Resolve(client, ref clientComponent))
            return;

        if (clientComponent.Server is not { } server)
            return;

        UnregisterClient(client, server, clientComponent, dirtyServer: dirtyServer);
    }

    /// <summary>
    /// Unregisters a client from its server
    /// </summary>
    /// <param name="client"></param>
    /// <param name="server"></param>
    /// <param name="clientComponent"></param>
    /// <param name="serverComponent"></param>
    /// <param name="dirtyServer"></param>
    public void UnregisterClient(EntityUid client, EntityUid server, ResearchClientNewComponent? clientComponent = null,
        ResearchServerNewComponent? serverComponent = null, bool dirtyServer = true)
    {
        if (!Resolve(client, ref clientComponent, false) || !Resolve(server, ref serverComponent, false))
            return;

        serverComponent.Clients.Remove(client);
        clientComponent.Server = null;
        // SyncClientWithServer(client, clientComponent: clientComponent);

        if (dirtyServer && !TerminatingOrDeleted(server))
        {
            Dirty(server, serverComponent);
        }

        var ev = new ResearchRegistrationNewChangedEvent(null);
        RaiseLocalEvent(client, ref ev);
    }
}
