using System.Numerics;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    public readonly ComponentRegistry StorageFormatComponents = new ComponentRegistry(new Dictionary<string, EntityPrototype.ComponentRegistryEntry>()
        {
             {"PointsDataStorage", new EntityPrototype.ComponentRegistryEntry(new PointsDataStorageComponent()) }
        }
    );

    private void InitializeData()
    { }

    [SubscribeLocalEvent]
    private void OnDataStorageInit(Entity<DataStorageComponent> ent, ref MapInitEvent args)
    {
        TryFormatStorage(ent, true);
    }

    private bool TryFormatStorage(Entity<DataStorageComponent> ent, bool initFormat = false)
    {
        if (!ent.Comp.CanBeFormatted && !initFormat)
            return false;

        EntityManager.RemoveComponents(ent, StorageFormatComponents);

        var formatProto = ProtoMan.Index(ent.Comp.CurrentStorageFormat);

        ent.Comp.LocalSize = ent.Comp.Size / formatProto.StoragesList.Values.Count;

        EntityManager.AddComponents(ent, formatProto.StoragesList);

        return true;
    }

    [SubscribeLocalEvent]
    private void OnPointsStorageStartup(Entity<PointsDataStorageComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<DataStorageComponent>(ent, out var dataStorage))
            return;

        ent.Comp.LocalSize = dataStorage.LocalSize;

        foreach (var type in ent.Comp.AllowedPointsTypes)
        {
            ent.Comp.PointsDict.TryAdd(type, 0);
        }
    }

    [SubscribeLocalEvent]
    private void OnDiskInserted(Entity<DataReaderComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdatePointsReaderInterface(ent.Owner, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnDataReaderStartup(Entity<DataReaderComponent> ent, ref ComponentStartup args)
    {
        UpdatePointsReaderInterface(ent.Owner, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnExpiredSizeUpdate(Entity<DataStorageComponent> ent, ref ExpiredSizeUpdatedEvent args)
    {
        if (args.Count + ent.Comp.ExpiredSize > ent.Comp.Size)
            args.CanUpdate = false;
        else
            ent.Comp.ExpiredSize += args.Count;
    }

    public double GetPointsSize(Dictionary<ProtoId<ResearchPointsTypePrototype>, double> points)
    {
        double size = 0;

        foreach (var (pointsId, value) in points)
        {
            var pointsProto = ProtoMan.Index(pointsId);

            size += value / pointsProto.PointsSizeCoof;
        }

        return size;
    }

    public double GetPointsSize(ProtoId<ResearchPointsTypePrototype> pointsId, double pointsValue)
    {
        var pointsProto = ProtoMan.Index(pointsId);

        var size = pointsValue / pointsProto.PointsSizeCoof;

        return size;
    }

    public bool TryWritePoints(
            EntityUid uid,
            ref Dictionary<ProtoId<ResearchPointsTypePrototype>, double> writePoints,
            out Dictionary<ProtoId<ResearchPointsTypePrototype>, double> writedPoints,
            PointsDataStorageComponent? storage = null)
    {
        var pointsSize = GetPointsSize(writePoints);
        writedPoints = new Dictionary<ProtoId<ResearchPointsTypePrototype>, double>();

        if (!Resolve(uid, ref storage) ||
                pointsSize + storage.ExpiredLocalSize > storage.LocalSize)
            return false;

        if (writePoints.Count == 0)
            return true;

        storage.ExpiredLocalSize += pointsSize;

        var ev = new ExpiredSizeUpdatedEvent(pointsSize);
        RaiseLocalEvent(uid, ref ev);

        if (!ev.CanUpdate)
            return false;

        foreach (var (type, value) in writePoints)
        {
            storage.PointsDict[type] += value;
            writePoints[type] -= value;
            writedPoints.Add(type, value);
        }

        if (writePoints.Values.Sum() != 0)
            return false;

        return true;
    }

    private void UpdatePointsReaderInterface(EntityUid uid, DataReaderComponent? reader = null, PointsDataStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref reader, ref storage, false))
            return;

        if (!_itemSlots.TryGetSlot(uid, comp.SlotId, out var itemSlot))
            return;

        var portState = PortState.Empty;
        var diskPoints = new Dictionary<ProtoId<ResearchPointsPrototype>, double>();

        if (itemSlot.Item is { } disk)
        {
            portState = PortState.WrongFormat;
            if (TryComp<PointsDataStorageComponent>(disk, out var diskStorage))
            {
                diskPoints = diskStorage.PointsDict;

                if (TryComp<DataStorageComponent>(disk, out var dataStorage))
                    portState = dataStorage.WriteAllowed ? PortState.ReadWrite : PortState.ReadOnly;
            }
        }

        var state = new PointsDataReadBoundUserInterfaceState(portState, storage.PointsDict, diskPoints);

        _uiSystem.SetUiState(uid, PointsDataReaderUiKey.Key, state);
    }
}
