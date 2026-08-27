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

    public bool TryWritePoints(
            EntityUid uid,
            ref ResearchPointsSpecifier writePoints,
            out ResearchPointsSpecifier writedPoints,
            PointsDataStorageComponent? storage = null)
    {
        writedPoints = new();

        if (!Resolve(uid, ref storage))
            return false;

        writedPoints = storage.Points.SizedAdd(ProtoMan, writePoints, storage.LocalSize - storage.ExpiredLocalSize);

        storage.ExpiredLocalSize += writedPoints.GetSize(ProtoMan);

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
