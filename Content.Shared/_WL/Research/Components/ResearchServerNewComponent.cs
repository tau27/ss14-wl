using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchServerNewComponent : Component
{
    /// <summary>
    /// The name of the server
    /// </summary>
    [AutoNetworkedField]
    [DataField("serverName"), ViewVariables(VVAccess.ReadWrite)]
    public string ServerName = "RDMAINSERVER";

    [AutoNetworkedField]
    [DataField("points")]
    public Dictionary<ProtoId<ResearchPointsTypePrototype>, (double, double, double)> PointsDict { get; set; } = new(); // current, max, per second

    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public int Id;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<ResearchPointsTypePrototype>> AllowedPointsTypes = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Clients = new();

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<ProtoId<ResearchCategoryPrototype>, ResearchField> ResearchedData = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<ResearchPrototype>> ResearchQueue = new();

    [DataField("nextUpdateTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField("researchConsoleUpdateTime"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ResearchConsoleUpdateTime = TimeSpan.FromSeconds(1);
}

[ByRefEvent]
public record struct GetServerResearchEvent
{
    public EntityUid Server;
    public Dictionary<ProtoId<ResearchCategoryPrototype>, (Dictionary<ProtoId<ResearchTypePrototype>, double>, double, ProtoId<ResearchPointsTypePrototype>)> ResearchData;

    public GetServerResearchEvent(EntityUid server)
    {
        Server = server;
        ResearchData = new();
    }
}

[ByRefEvent]
public readonly record struct GetResearchSpeedEvent(int Speed);

[ByRefEvent]
public readonly record struct ResearchServerNewUpdatedEvent();
