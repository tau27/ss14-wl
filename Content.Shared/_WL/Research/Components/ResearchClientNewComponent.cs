using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components
{
    [RegisterComponent]
    public sealed partial class ResearchClientNewComponent : Component
    {
        public bool ConnectedToServer => Server != null;

        /// <summary>
        /// The server the client is connected to
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public EntityUid? Server { get; set; }
    }

    /// <summary>
    /// Raised on the client whenever its server is changed
    /// </summary>
    /// <param name="Server">Its new server</param>
    [ByRefEvent]
    public readonly record struct ResearchRegistrationNewChangedEvent(EntityUid? Server);

    /// <summary>
    ///     Sent to the server when the client deselects a research server.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ResearchClientServerNewDeselectedMessage : BoundUserInterfaceMessage
    {
    }

    /// <summary>
    ///     Sent to the server when the client chooses a research server.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ResearchClientServerNewSelectedMessage : BoundUserInterfaceMessage
    {
        public int ServerId;

        public ResearchClientServerNewSelectedMessage(int serverId)
        {
            ServerId = serverId;
        }
    }

    /// <summary>
    ///     Request that the server updates the client.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ResearchClientNewSyncMessage : BoundUserInterfaceMessage
    {
    }

    [NetSerializable, Serializable]
    public enum ResearchClientUiKey
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class ResearchClientNewBoundInterfaceState : BoundUserInterfaceState
    {
        public int ServerCount;
        public string[] ServerNames;
        public int[] ServerIds;
        public int SelectedServerId;

        public ResearchClientNewBoundInterfaceState(int serverCount, string[] serverNames, int[] serverIds, int selectedServerId = -1)
        {
            ServerCount = serverCount;
            ServerNames = serverNames;
            ServerIds = serverIds;
            SelectedServerId = selectedServerId;
        }
    }
}
