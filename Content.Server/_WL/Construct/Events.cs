using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;


namespace Content.Shared._WL.Construction;

[ByRefEvent]
public record struct ToolConstructAttemptedEvent
{
    public EntityUid User;

    public EntityUid Used;

    public bool Cancelled { get; private set; }

    public void Cancel()
    {
        Cancelled = true;
    }

    public void Uncancel()
    {
        Cancelled = false;
    }
}
