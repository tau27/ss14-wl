using Content.Shared.Lathe;
using Content.Shared.Research.Prototypes;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedResearchNewSystem), typeof(SharedLatheSystem)), AutoGenerateComponentState]
public sealed partial class ResearchDatabaseComponent : Component
{
    [AutoNetworkedField]
    [DataField]
    public List<ProtoId<TechDisciplinePrototype>> SupportedDisciplines = new();

    [AutoNetworkedField]
    [DataField]
    public Dictionary<ProtoId<ResearchPrototype>, ResearchState> Researches = new();

    /// todo: if you unlock all the recipes in a tech, it doesn't count as unlocking the tech. sadge
    [AutoNetworkedField]
    [DataField]
    public List<ProtoId<LatheRecipePrototype>> UnlockedRecipes = new();
}

/// <summary>
/// Event raised on the database whenever its
/// technologies or recipes are modified.
/// </summary>
/// <remarks>
/// This event is forwarded from the
/// server to all of it's clients.
/// </remarks>
[ByRefEvent]
public readonly record struct ResearchDatabaseModifiedEvent(List<string>? NewlyUnlockedRecipes);

/// <summary>
/// Event raised on a database after being synchronized
/// with the values from another database.
/// </summary>
[ByRefEvent]
public readonly record struct ResearchDatabaseSynchronizedEvent;

[Serializable, NetSerializable]
public struct ResearchState
{
    public ResearchStatus Status { get; set; }

    public ResearchDepsStatus DepsState { get; set; }

    public double ResearchedPackages = 0;

    public ProtoId<ResearchModePrototype> ModeId = "Default";

    public ResearchState()
    {
        Status = ResearchStatus.NotResearched;
        DepsState = ResearchDepsStatus.Allowed;
    }
}

[Serializable, NetSerializable]
public enum ResearchStatus: byte
{
    NotResearched = 0,
    Researching = 1,
    InQueue = 2,
    Researched = 3,
    Invalid = 4
}

[Serializable, NetSerializable]
public enum ResearchDepsStatus: byte
{
    Allowed = 0,
    SpecialReq = 1,
    ParentsReq = 2,
    PointsReq = 3,
    Invalid = 4
}
