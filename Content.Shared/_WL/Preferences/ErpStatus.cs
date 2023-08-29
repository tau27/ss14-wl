namespace Content.Shared._WL.Preferences;

/// <summary>
///     Displays OOC player intention for ERP interactions.
/// </summary>
public enum ErpStatus
{
    // These enum values HAVE to match the ones in DbErpStatus in Server.Database
    Ask = 0,
    CheckOOC,
    No,
    Yes,
    YesDom,
    YesSub,
    YesSwitch,
}
