using Robust.Shared.Serialization;

namespace Content.Shared._WL.CharacterInformation;

[Serializable, NetSerializable]
public enum CharacterInformationUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class CharacterInformationBuiState : BoundUserInterfaceState
{
    public EntityUid Uid;
    public string CharacterName;
    public string FlavorText;
    public string? OocText;

    public CharacterInformationBuiState(EntityUid uid, string characterName, string flavorText, string? oocText)
    {
        Uid = uid;
        CharacterName = characterName;
        FlavorText = flavorText;
        OocText = oocText;
    }
}
