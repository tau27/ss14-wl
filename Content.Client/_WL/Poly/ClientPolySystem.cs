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

        private async void OnServerQuery(PolyServerQueryEvent args)
        {
            var ent = GetEntity(args.Entity);

            try
            {
                await _contentSpriteSystem.Export(ent, Direction.South, (queue, image) =>
                {
                    try
                    {
                        //TODO: проверить захватывает ли GC потоки, кхм
                        using var stream = new MemoryStream(1024);

                        image.SaveAsPng(stream);

                        stream.Position = 0;
                        var str = Convert.ToBase64String(stream.GetBuffer());

                        var ev = new PolyClientResponseEvent(str, args.QueryId);

                        _sawmill.Info($"Запрос от Поли успешно обработан! Сущность: {ToPrettyString(args.Entity)}");

                        RaiseNetworkEvent(ev);
                    }
                    catch (Exception ex)
                    {
                        _sawmill.Error($"Неизвестная ошибка при рендере фотографии для Поли! {ex.Message}");
                    }
                });
            }
            catch (Exception exc)
            {
                _sawmill.Error($"Error: {exc.Message}");
            }
        }
    }
}
