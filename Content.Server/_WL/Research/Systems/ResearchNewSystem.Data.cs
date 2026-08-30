using System.Numerics;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    [Dependency] private ItemSlotsSystem _itemSlots = default!;

    public readonly ComponentRegistry StorageFormatComponents = new ComponentRegistry(new Dictionary<string, EntityPrototype.ComponentRegistryEntry>()
        {
             {"PointsDataStorage", new EntityPrototype.ComponentRegistryEntry(new PointsDataStorageComponent()) }
        }
    );

    private void InitializeData()
    {
        Subs.BuiEvents<PointsDataStorageComponent>(PointsDataReaderUiKey.Key, subs =>
        {
            subs.Event<PointsTransferMessage>(OnPointsTransferMessage);
        });
    }

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

        ent.Comp.Points = new ResearchPointsSpecifier(ent.Comp.AllowedPointsTypes, 0);
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

    private void OnPointsTransferMessage(Entity<PointsDataStorageComponent> ent, ref PointsTransferMessage args)
    {
        if (!TryComp<DataReaderComponent>(ent, out var reader) ||
                !_itemSlots.TryGetSlot(ent, reader.SlotId, out var itemSlot) ||
                itemSlot.Item is not { } disk)
            return;

        if (args.Direction)
            TryTransferPoints(ent, disk, args.Points, out _, ent.Comp);
        else
            TryTransferPoints(disk, ent, args.Points, out _, null, ent.Comp);

        UpdatePointsReaderInterface(ent, null, ent.Comp);
    }

    public bool TryDrawPoints(
            EntityUid uid,
            ref ResearchPointsSpecifier drawPoints,
            out ResearchPointsSpecifier drawedPoints,
            PointsDataStorageComponent? storage = null)
    {
        drawedPoints = new();

        if (!Resolve(uid, ref storage))
            return false;

        if (!drawPoints.AnyPositive())
            return false;

        storage.Points -= drawPoints;

        var negativePoints = ResearchPointsSpecifier.GetNegative(storage.Points);
        storage.Points -= negativePoints;
        drawedPoints = drawPoints + negativePoints;

        drawPoints -= drawedPoints;

        return true;
    }

    public bool TryWritePoints(
            EntityUid uid,
            ref ResearchPointsSpecifier writePoints,
            out ResearchPointsSpecifier writedPoints,
            PointsDataStorageComponent? storage = null)
    {
        writedPoints = new();

        if (!Resolve(uid, ref storage))
            return false;

        if (!writePoints.AnyPositive())
            return false;

        writedPoints = storage.Points.SizedAdd(ProtoMan, writePoints, storage.LocalSize - storage.ExpiredLocalSize);

        storage.ExpiredLocalSize += writedPoints.GetSize(ProtoMan);

        return true;
    }

    public bool TryTransferPoints(
            EntityUid sourceUid,
            EntityUid recipientUid,
            ResearchPointsSpecifier transferPoints,
            out ResearchPointsSpecifier tranferedPoints,
            PointsDataStorageComponent? sourceStorage = null,
            PointsDataStorageComponent? recipientStorage = null)
    {
        tranferedPoints = new();

        if (!Resolve(sourceUid, ref sourceStorage) || !Resolve(recipientUid, ref recipientStorage))
            return false;

        if (!transferPoints.AnyPositive())
            return false;

        if (!TryDrawPoints(sourceUid, ref transferPoints, out var drawedPoints, sourceStorage))
            return false;

        if (!TryWritePoints(recipientUid, ref drawedPoints, out var writedPoints, recipientStorage))
            return false;

        if (!TryWritePoints(sourceUid, ref drawedPoints, out var returnedPoints, sourceStorage))
            return false;

        transferPoints += returnedPoints;

        return true;
    }

    private void UpdatePointsReaderInterface(EntityUid uid, DataReaderComponent? reader = null, PointsDataStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref reader, ref storage, false))
            return;

        if (!_itemSlots.TryGetSlot(uid, reader.SlotId, out var itemSlot))
            return;

        var portState = PortState.Empty;
        var diskPoints = new ResearchPointsSpecifier();

        if (itemSlot.Item is { } disk)
        {
            portState = PortState.WrongFormat;
            if (TryComp<PointsDataStorageComponent>(disk, out var diskStorage))
            {
                diskPoints = diskStorage.Points;

                if (TryComp<DataStorageComponent>(disk, out var dataStorage))
                    portState = dataStorage.WriteAllowed ? PortState.ReadWrite : PortState.ReadOnly;
            }
        }

        var state = new PointsDataReadBoundUserInterfaceState(portState, storage.Points, diskPoints);

        _uiSystem.SetUiState(uid, PointsDataReaderUiKey.Key, state);
    }
}
