using Content.Server.Paper;
using Content.Server.Station.Systems;

namespace Content.Server._WL.Documents
{
    public sealed partial class PrintedDocumentFormatSystem : EntitySystem
    {
        [Dependency] private readonly PaperSystem _paper = default!;
        [Dependency] private readonly StationSystem _station = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PrintedDocumentFormatComponent, MapInitEvent>(OnMapInit);
        }

        private void OnMapInit(EntityUid document, PrintedDocumentFormatComponent comp, MapInitEvent args)
        {
            var paperComp = EnsureComp<PaperComponent>(document);

            var station = _station.GetOwningStation(document);
            var stationName = station != null
                ? Name(station.Value)
                : null;

            _paper.SetContent(document, FormatString(Loc.GetString(paperComp.Content), stationName), paperComp);
        }

        public static string FormatString(string content, string? station = null)
        {
            return content
                .Replace(":DATE:", DateTime.Now.AddYears(1000).ToString())
                .Replace(":STATION:", station ?? "Station XX-000");
        }
    }
}
