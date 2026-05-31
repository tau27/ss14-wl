namespace Content.Shared._WL._Offbrand.Surgery;

[ByRefEvent]
public record struct ToolSpeedModifierEvent(EntityUid Tool, float Speed);
