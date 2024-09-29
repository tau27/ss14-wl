using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.Poly.Events;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Random.Helpers;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._WL.Poly
{
    public sealed partial class PolySystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IConfigurationManager _configMan = default!;
        [Dependency] private readonly IPlayerManager _playMan = default!;

        private List<MessageEntry> _messages = default!;

        private TimeSpan _time = TimeSpan.Zero;
        private bool _readyToPick = false;
        private TimeSpan _chooseInterval = default!;
        private bool _neededCleanup = false;

        private Dictionary<string, ChatMessage> _queriedEntities = default!;

        private const int MAX_QUERIES_PER_PLAYER = 20;

        public override void Initialize()
        {
            base.Initialize();

            _messages = new();
            _queriedEntities = new();

            _readyToPick = _configMan.GetCVar(WLCVars.PolyNeededRoundEndCleanup);
            _chooseInterval = TimeSpan.FromSeconds(_configMan.GetCVar(WLCVars.PolyMessageChooseCooldown));

            Subs.CVar(_configMan, WLCVars.PolyMessageChooseCooldown, (new_value) => _chooseInterval = TimeSpan.FromSeconds(new_value), true);
            Subs.CVar(_configMan, WLCVars.PolyNeededRoundEndCleanup, (needed) => _neededCleanup = needed);

            UpdatesOutsidePrediction = false;

            SubscribeLocalEvent<RoundRestartCleanupEvent>((_) =>
            {
                if (!_neededCleanup)
                    return;

                Clean();
            });

            SubscribeNetworkEvent<PolyClientResponseEvent>(OnClientPolyResponse);

            _chatManager.OnAfterChatMessage += (msg) =>
            {
                if (!_readyToPick)
                    return;

                var sender = msg.SenderEntity;

                DebugTools.AssertNotNull(sender);

                if (!sender.Valid)
                    return;

                if (!ShouldMessageBeChosen(msg))
                    return;

                _readyToPick = false;
                QueryAddMessage(msg);
            };

            _playMan.PlayerStatusChanged += (sender, args) =>
            {
                if (_playMan.Sessions.Length - 1 != 0)
                    return;

                if (args.NewStatus != SessionStatus.Connected)
                    return;

                HandleQueries();
            };
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!_timing.IsFirstTimePredicted)
                return;

            _time += _timing.TickPeriod;

            if (_time >= _chooseInterval)
            {
                _time = TimeSpan.Zero;
                _readyToPick = true;
            }
        }

        private void QueryAddMessage(ChatMessage msg)
        {
            var id = Guid.NewGuid().ToString();

            _queriedEntities.Add(id, msg);

            HandleQueries();
        }

        private void OnClientPolyResponse(PolyClientResponseEvent args, EntitySessionEventArgs sessionArgs)
        {
            var queried = args.QueryId;

            if (!_queriedEntities.TryGetValue(queried, out var msg)) //Если событие было выслано всем клиентам, то каждый клиент отправит ответ,
                return; //И тогда от каждого клиента добавится сообщение, эта проверка нужна, чтоб избежать этого

            _queriedEntities.Remove(queried);

            var entry = MessageToEntry(msg, args.Stream);

            _messages.Add(entry);
        }

        private void HandleQueries()
        {
            var sessions = _playMan.Sessions.ToDictionary(k => k, v => 0);

            if (sessions.Count == 0)
                return;

            var session_pair = PickSession();

            if (session_pair == null)
                return;

            foreach (var item in _queriedEntities)
            {
                var session = session_pair.Value.Key;
                var queries = session_pair.Value.Value;

                if (queries > MAX_QUERIES_PER_PLAYER)
                {
                    session_pair = PickSession();

                    if (session_pair == null)
                        break;

                    session = session_pair.Value.Key;
                    queries = session_pair.Value.Value;
                }

                var sender = item.Value.SenderEntity;
                var id = item.Key;

                var ev = new PolyServerQueryEvent(sender, id);

                RaiseNetworkEvent(ev, session);

                queries++;
            }

            KeyValuePair<ICommonSession, int>? PickSession()
            {
                if (sessions.Count == 0)
                    return null;

                return _random.Pick(sessions);
            }
        }

        #region Public
        public void Clean()
        {
            _messages.Clear();
            _queriedEntities.Clear();
        }

        public bool IsReadyToPick()
        {
            return _readyToPick;
        }

        public MessageEntry MessageToEntry(ChatMessage msg, string? pngBase64 = null)
        {
            var sender_ent = GetEntity(msg.SenderEntity);

            return new()
            {
                ColorHex = (msg.MessageColorOverride ?? msg.Channel.TextColor()).ToHex(),
                Content = msg.Message,
                SenderEntityName = Name(sender_ent),
                RoundId = _ticker.RoundId,
                IsRoundFlow = _ticker.RunLevel == GameRunLevel.InRound,
                PngBase64 = pngBase64
            };
        }

        public MessageEntry? Pick()
        {
            if (_messages.Count == 0)
                return null;

            return _random.PickAndTake(_messages);
        }

        public static bool ShouldMessageBeChosen(ChatMessage msg)
        {
            if (msg.HideChat)
                return false;

            if (string.IsNullOrEmpty(msg.Message))
                return false;

            var unnecessary_flags =
                ChatChannel.None |
                ChatChannel.Damage |
                ChatChannel.Visual |
                ChatChannel.Notifications |
                ChatChannel.AdminRelated |
                ChatChannel.Unspecified;

            if (unnecessary_flags.HasFlag(msg.Channel))
                return false;

            return true;
        }

        public TimeSpan HowLongBeforeReady()
        {
            return _chooseInterval - _time;
        }
        #endregion
    }

    public readonly record struct MessageEntry(
        string Content,
        string ColorHex,
        string SenderEntityName,
        int RoundId,
        bool IsRoundFlow,
        string? PngBase64 = null);
}
