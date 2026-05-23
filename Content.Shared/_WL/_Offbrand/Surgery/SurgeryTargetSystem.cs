//using Content.Shared._Offbrand.Surgery;
//using Content.Shared.Construction;
//using Content.Shared.DoAfter;
//using Content.Shared.Popups;
//using Content.Shared.Random.Helpers;
//using Robust.Shared.Random;
//using Robust.Shared.Timing;
//
//namespace Content.Shared._WL._Offbrand.Surgery;
//
//public sealed partial class SurgeryTargetSystem : EntitySystem
//{
//    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
//    [Dependency] private IGameTiming _timing = default!;
//    [Dependency] private SharedPopupSystem _popup = default!;
//
//    public override void Initialize()
//    {
//        SubscribeLocalEvent<SurgeryTargetComponent, ConstructionInteractDoAfterEvent>(OnAfterSurgery);
//    }
//
//    private void OnAfterSurgery(Entity<SurgeryTargetComponent> ent, ref ConstructionInteractDoAfterEvent args)
//    {
//        var seed = SharedRandomExtensions.HashCodeCombine((int)_timing.CurTick.Value, GetNetEntity(ent).Id);
//        var rand = new System.Random(seed);
//
//        if (!rand.Prob(0.5))
//        {
//            _popup.PopupEntity(Loc.GetString("SURGERY FAILED"), ent);
//            _doAfterSystem.Cancel(ent, args.DoAfter.Index);
//        }
//    }
//}
