using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PointsSourceDebugComponent : Component
{
    [AutoNetworkedField]
    [DataField("points")]
    public Dictionary<ProtoId<ResearchPointsTypePrototype>, int> PointsDict { get; set; } = new();
}
