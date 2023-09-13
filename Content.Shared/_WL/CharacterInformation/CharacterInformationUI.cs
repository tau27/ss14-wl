using Content.Shared._WL.Preferences;
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
    public NetEntity Uid;
    public string CharacterName;
    public string FlavorText;
    public string? OocText;
    public ErpStatus? ErpStatus;

    public CharacterInformationBuiState(
        NetEntity uid,
        string characterName,
        string flavorText,
        string? oocText,
        ErpStatus? erpStatus)
    {
        Uid = uid;
        CharacterName = characterName;
        FlavorText = flavorText;
        OocText = oocText;
        ErpStatus = erpStatus;
    }
}
