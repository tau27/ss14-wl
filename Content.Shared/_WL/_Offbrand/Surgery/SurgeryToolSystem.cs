using Content.Shared._WL._Offbrand.Surgery;
using Content.Shared._Offbrand.Surgery;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared._WL._Offbrand.Surgery;

// this code needs to use predicted popups when construction gets predicted
public sealed partial class SharedSurgeryToolSystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryToolComponent, GetVerbsEvent<ExamineVerb>>(OnDetailedExamine);
    }

    private void OnDetailedExamine(Entity<SurgeryToolComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        var iconTexture = "/Textures/_WL/Interface/VerbIcons/scalpel.png";

        _examine.AddHoverExamineVerb(args,
            ent.Comp,
            Loc.GetString("surgery-tool-verb-text"),
            Loc.GetString("surgery-tool-verb-text-message", ("successChance", ent.Comp.SuccessChance)),
            iconTexture
        );
    }
}
