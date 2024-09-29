using Robust.Shared.Serialization;

namespace Content.Shared._WL.Poly.Events
{
    [Serializable, NetSerializable]
    public sealed partial class PolyServerQueryEvent : EntityEventArgs
    {
        public readonly NetEntity Entity;
        public readonly string QueryId;

        public PolyServerQueryEvent(NetEntity entity, string queryId)
        {
            Entity = entity;
            QueryId = queryId;
        }
    }

    [Serializable, NetSerializable]
    public sealed partial class PolyClientResponseEvent : EntityEventArgs
    {
        public readonly string? Stream;
        public readonly string QueryId;

        public PolyClientResponseEvent(string? png_stream, string queryId)
        {
            Stream = png_stream;
            QueryId = queryId;
        }
    }
}
