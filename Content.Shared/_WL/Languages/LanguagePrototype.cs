using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages;

[Prototype]
public sealed partial class LanguagePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public string Description = string.Empty;

    [DataField(required: true)]
    public ObfuscationMethod Obfuscation = ObfuscationMethod.Default;

    [DataField]
    public string Colour = "#000000";
}
