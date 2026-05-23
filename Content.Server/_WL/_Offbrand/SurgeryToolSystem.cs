using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Tools.Components;
using Content.Shared.Whitelist;
using Content.Shared.Tools.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Random.Helpers;
using Content.Shared._WL._Offbrand.Surgery;
using Content.Shared._Offbrand.Surgery;
using Content.Shared._WL.Construction;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._WL._Offbrand.Surgery;

// this code needs to use predicted popups when construction gets predicted
public sealed partial class ServerSurgeryToolSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StandingStateSystem _standingState = default!;

    // WL-Changes: Getto-surg start
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    // WL-Changes: Getto-surg end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryTargetComponent, ToolConstructAttemptedEvent>(OnToolAttemptUse);
    }

    private void OnToolAttemptUse(Entity<SurgeryTargetComponent> ent, ref ToolConstructAttemptedEvent args)
    {
        // WL-Changes: Getto-surgery start
        if (!HasComp<SurgeryTargetComponent>(ent))
            return;
        // WL-Changes: Getto-surgery end

        var seed = SharedRandomExtensions.HashCodeCombine((int)_timing.CurTick.Value, GetNetEntity(ent).Id);
        var rand = new System.Random(seed);

        if (!rand.Prob(0.5))
        {
            _popup.PopupEntity(Loc.GetString("SURGERY FAILED"), ent);
            args.Cancel();
        }
    }
}
