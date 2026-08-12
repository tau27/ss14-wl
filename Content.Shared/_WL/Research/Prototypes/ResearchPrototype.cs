using Content.Shared.Research.Prototypes;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Prototypes;

[Prototype]
public sealed partial class ResearchPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    private LocId Name { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public ProtoId<TechDisciplinePrototype> Discipline;

    [DataField(required: true)]
    public int Tier;

    [DataField(required: true)]
    public List<ProtoId<ResearchPrototype>> ParentsResearches = new();

    [DataField(required: true)]
    public Dictionary<ProtoId<ResearchPointsTypePrototype>, double> PointsCost = new();

    [DataField]
    public bool Hidden;

    [DataField]
    public int PackagesCost = 120;

    [DataField]
    public List<ProtoId<LatheRecipePrototype>> RecipeUnlocks = new();
}
