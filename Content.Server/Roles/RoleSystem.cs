using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Roles;

public sealed class RoleSystem : SharedRoleSystem
{
    [Dependency] private readonly IServerPreferencesManager _servPrefMan = default!;
    [Dependency] private readonly IPlayerManager _playMan = default!;

    public string? MindGetBriefing(EntityUid? mindId)
    {
        if (mindId == null)
        {
            Log.Error($"MingGetBriefing failed for mind {mindId}");
            return null;
        }

        TryComp<MindComponent>(mindId.Value, out var mindComp);

        if (mindComp is null)
        {
            Log.Error($"MingGetBriefing failed for mind {mindId}");
            return null;
        }

        var ev = new GetBriefingEvent();

        // This is on the event because while this Entity<T> is also present on every Mind Role Entity's MindRoleComp
        // getting to there from a GetBriefing event subscription can be somewhat boilerplate
        // and this needs to be looked up for the event anyway so why calculate it again later
        ev.Mind = (mindId.Value, mindComp);

        // Briefing is no longer raised on the mind entity itself
        // because all the components that briefings subscribe to should be on Mind Role Entities
        foreach(var role in mindComp.MindRoles)
        {
            RaiseLocalEvent(role, ref ev);
        }

        return ev.Briefing;
    }

    public HumanoidCharacterProfile? GetProfileByEntity(EntityUid? entity)
    {
        if (entity == null)
            return null;

        _playMan.TryGetSessionByEntity(entity.Value, out var session);

        return GetProfileBySession(session);
    }

    public HumanoidCharacterProfile? GetProfileBySession(ICommonSession? session)
    {
        if (session == null)
            return null;

        var genericProfile = _servPrefMan.GetPreferencesOrNull(session.UserId)?.SelectedCharacter;

        return genericProfile as HumanoidCharacterProfile;
    }

    public string? GetSubnameByEntity(EntityUid entity, string jobId)
    {
        var profile = GetProfileByEntity(entity);
        if (profile == null)
            return null;

        if (!profile.JobSubnames.TryGetValue(jobId, out var subname))
            return null;

        if (_prototypes.TryIndex<JobPrototype>(jobId, out var proto))
            if (!proto.Subnames.Contains(subname))
                return proto.LocalizedName;

        return subname;
    }

    public string? GetSubnameBySesssion(ICommonSession? session, string jobId)
    {
        if (session == null)
            return null;

        var profile = GetProfileBySession(session);
        if (profile == null)
            return null;

        if (!profile.JobSubnames.TryGetValue(jobId, out var subname))
            return null;

        if (_prototypes.TryIndex<JobPrototype>(jobId, out var proto))
            if (!proto.Subnames.Contains(subname))
                return proto.LocalizedName;

        return subname;
    }
}

/// <summary>
/// Event raised on the mind to get its briefing.
/// Handlers can either replace or append to the briefing, whichever is more appropriate.
/// </summary>
[ByRefEvent]
public sealed class GetBriefingEvent
{
    /// <summary>
    /// The text that will be shown on the Character Screen
    /// </summary>
    public string? Briefing;

    /// <summary>
    /// The Mind to whose Mind Role Entities the briefing is sent to
    /// </summary>
    public Entity<MindComponent> Mind;

    public GetBriefingEvent(string? briefing = null)
    {
        Briefing = briefing;
    }

    /// <summary>
    /// If there is no briefing, sets it to the string.
    /// If there is a briefing, adds a new line to separate it from the appended string.
    /// </summary>
    public void Append(string text)
    {
        if (Briefing == null)
        {
            Briefing = text;
        }
        else
        {
            Briefing += "\n" + text;
        }
    }
}
