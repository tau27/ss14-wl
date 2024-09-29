using Content.Client.Sprite;
using Content.Shared._WL.Poly.Events;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using System.IO;

namespace Content.Client._WL.Poly
{
    [UsedImplicitly]
    public sealed partial class ClientPolySystem : EntitySystem
    {
        [Dependency] private readonly ContentSpriteSystem _contentSpriteSystem = default!;
        [Dependency] private readonly ILogManager _log = default!;

        private ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();

            _sawmill = _log.GetSawmill("poly");

            SubscribeNetworkEvent<PolyServerQueryEvent>(OnServerQuery);
        }

        private void OnServerQuery(PolyServerQueryEvent args)
        {
            var ent = GetEntity(args.Entity);

#pragma warning disable CS4014
            _contentSpriteSystem.Export(ent, Direction.South, (queue, image) =>
            {
                try
                {
                    using var stream = new MemoryStream();

                    image.SaveAsPng(stream);

                    using var reader = new StreamReader(stream);

                    stream.Position = 0;
                    var str = reader.ReadToEnd();

                    var ev = new PolyClientResponseEvent(str, args.QueryId);

                    RaiseNetworkEvent(ev);
                }
                catch (Exception)
                {
                    _sawmill.Error("Неизвестная ошибка при рендере фотографии для Поли!");
                }
            });
#pragma warning restore CS4014
        }
    }
}
