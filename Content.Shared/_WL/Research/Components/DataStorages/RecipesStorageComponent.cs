using Content.Shared.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RecipesStorageComponent : BaseDataStorageComponent
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<LatheRecipePrototype>> Recipes = new();

    [DataField]
    public FixedPoint2 SizePerTech = FixedPoint2.New(0.2);
}
