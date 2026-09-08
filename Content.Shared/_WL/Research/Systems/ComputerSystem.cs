using System.Linq;
using Content.Shared.Emp;
using Content.Shared.Verbs;
using Content.Shared.UserInterface;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Systems;

public sealed partial class SharedComputerSystem : EntitySystem
{
    [Dependency] protected SharedUserInterfaceSystem UI = default!;
    [Dependency] protected SharedAppearanceSystem Appearance = default!;

    public override void Initialize()
    {
        Subs.BuiEvents<UniversalComputerComponent>(UCMenuUiKey.Key,
            subs =>
        {
            subs.Event<ProgramOpenMessage>(OnProgramOpenMessage);
        });
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<UniversalComputerComponent> ent, ref MapInitEvent args)
    {
        foreach (var programId in ent.Comp.Programs)
        {
            InitProgram(ent, programId, ent.Comp);
        }

        OpenProgram(ent, ent.Comp.MenuProgram, ent.Comp);
    }

    private void InitProgram(EntityUid uid, ProtoId<ProgramPrototype> programId, UniversalComputerComponent? computer = null)
    {
        if (!Resolve(uid, ref computer))
            return;

        var program = ProtoMan.Index(programId);

        UI.SetUi(uid, program.UIKey, program.UIData);

        EntityManager.AddComponents(uid, program.Components);
    }

    [SubscribeLocalEvent]
    private void GetVerb(Entity<UniversalComputerComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (ent.Comp.InMenu)// || !ShouldAddVerb(uid, component, args))
            return;

        args.Verbs.Add(new ActivationVerb
        {
            Act = () => OpenProgram(ent, ent.Comp.MenuProgram, ent.Comp),
            Text = Loc.GetString(ent.Comp.VerbText),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/close.svg.192dpi.png")),
        });
    }

    [SubscribeLocalEvent]
    private void GetActivationVerb(Entity<UniversalComputerComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (ent.Comp.InMenu)// || !ShouldAddVerb(uid, component, args))
            return;

        args.Verbs.Add(new ActivationVerb
        {
            Act = () => OpenProgram(ent, ent.Comp.MenuProgram, ent.Comp),
            Text = Loc.GetString(ent.Comp.VerbText),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/close.svg.192dpi.png")),
        });
    }

    private void OnProgramOpenMessage(Entity<UniversalComputerComponent> ent, ref ProgramOpenMessage args)
    {
        if (!ProtoMan.TryIndex<ProgramPrototype>(args.Program, out var program))
            return;

        OpenProgram(ent, program, ent.Comp);
    }

    /*
    private void OnEmpPulse(Entity<BarSignComponent> ent, ref EmpPulseEvent args)
    {
        if (!ProtoMan.Resolve(ent.Comp.Emped, out var empedPrototype))
            return;

        SetBarSign(ent, empedPrototype);
        args.Affected = true;
        args.Disabled = true;
    }

    private void OnBoundUIAttempt(Entity<BarSignComponent> ent, ref BoundUserInterfaceMessageAttempt args)
    {
        if (HasComp<EmpDisabledComponent>(ent))
            args.Cancel();
    }
    */

    public void OpenProgram(EntityUid uid, ProtoId<ProgramPrototype> programId, UniversalComputerComponent? computer = null)
    {
        if (!Resolve(uid, ref computer) || HasComp<EmpDisabledComponent>(uid))
            return;

        var prevProgram = ProtoMan.Index(computer.CurrentProgram);

        var program = ProtoMan.Index(programId);

        if (TryComp<ActivatableUIComponent>(uid, out var activatable))
        {
            activatable.Key = program.UIKey;
            Dirty(uid, activatable);
        }

        Appearance.SetData(uid, UnversalComputerVisuals.ProgramPrototype, programId);

        computer.CurrentProgram = programId;
        computer.InMenu = computer.MenuProgram == programId;

        UpdateComputerInterface(uid, computer);

        UI.CloseUi(uid, prevProgram.UIKey);
        UI.OpenUi(uid, program.UIKey);

        Dirty(uid, computer);
    }

    protected void UpdateComputerInterface(EntityUid uid, UniversalComputerComponent? computer = null)
    {
        if (!Resolve(uid, ref computer, false))
            return;

        var state = new UniversalComputerBoundInterfaceState(computer.Programs);

        UI.SetUiState(uid, UCMenuUiKey.Key, state);
    }
}
