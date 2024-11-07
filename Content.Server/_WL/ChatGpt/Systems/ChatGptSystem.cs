using Content.Server._WL.ChatGpt.Elements.OpenAi;
using Content.Server._WL.ChatGpt.Elements.OpenAi.Request;
using Content.Server._WL.ChatGpt.Elements.OpenAi.Response;
using Content.Server._WL.ChatGpt.Managers;
using Content.Server.GameTicking;
using Content.Shared.Dataset;
using Content.Shared.GameTicking;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using JetBrains.Annotations;
using Prometheus;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._WL.ChatGpt.Systems
{
    public sealed partial class ChatGptSystem : EntitySystem
    {
        [GeneratedRegex(@"(\{\s*\$\s*(\S+)\s*\})")]
        private static partial Regex SearchRegex();

        [Dependency] private readonly IChatGptManager _gpt = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ILogManager _logMan = default!;

        private ISawmill _sawmill = default!;

        private const string SawmillId = "chat.gpt.sys";

        private static readonly Gauge _inputTokens = Metrics.CreateGauge(
            "gpt_input_tokens_count",
            "Количество входящих токенов за весь раунд.");

        private static readonly Gauge _outputTokens = Metrics.CreateGauge(
            "gpt_output_tokens_count",
            "Количество выходящих токенов за весь раунд.");

        private decimal _spentRubles = 0;

        private Dictionary<ProtoId<AIChatPrototype>, List<GptChatMessage>> _dialogues = default!;

        private static readonly TimeSpan QueryTimeout = TimeSpan.FromMilliseconds(3000); //УБЕРИТЕ((((((((((((( пиздец

        public override void Initialize()
        {
            base.Initialize();

            _dialogues = new();
            _sawmill = _logMan.GetSawmill(SawmillId);

            SubscribeLocalEvent<RoundRestartCleanupEvent>((_) => ClearMemory());
            SubscribeLocalEvent<RoundStartedEvent>(async (_) =>
            {
                try
                {
                    _spentRubles = await _gpt.GetBalanceAsync();
                }
                catch (Exception ex)
                {
                    _sawmill.Error(ex.ToStringBetter());
                }
            });

            SubscribeLocalEvent<RoundEndTextAppendEvent>((args) =>
            {
                try
                {
                    Task.Run(async () =>
                    {
                        var now = await _gpt.GetBalanceAsync();

                        args.AddLine(Loc.GetString("gpt-model-round-end-balance", ("spent", _spentRubles - now)));

                        _spentRubles = now;
                    }).Wait(QueryTimeout); //Я шатал асинхронный код
                }
                catch (Exception ex)
                {
                    _sawmill.Error(ex.ToStringBetter());
                }
            });

            SetupMemory();
        }

        public async Task<GptChatResponse> SendWithMemory(
            ProtoId<AIChatPrototype> ai,
            GptChatRequest req,
            IEnumerable<ToolFunctionModel>? methods = null,
            string? senderName = null,
            CancellationToken cancel = default)
        {
            SetupMemory();

            if (!_gpt.IsEnabled(out var reason))
                throw new NullReferenceException(reason);

            var proto = _protoMan.Index(ai);
            var messages = _dialogues[ai];

            if (proto.UseMemory)
            {
                foreach (var message in req.GetMessages())
                {
                    messages.Add(message);
                }

                req.Messages = messages.ToArray();
            }

            var resp = await _gpt.SendChatQueryAsync(req, methods, cancel);

            _inputTokens.Inc(resp.Usage.InputTokens);
            _outputTokens.Inc(resp.Usage.OutputTokens);

            if (proto.UseMemory)
            {
                if (resp.Choices.Length == 0)
                    return resp;

                var a_msg = _random.Pick(resp.Choices).Message.ToChatMessage(senderName);

                messages.Add(a_msg);
            }

            return resp;
        }

        private void SetupMemory()
        {
            var protos = _protoMan.EnumeratePrototypes<AIChatPrototype>();

            foreach (var proto in protos)
            {
                var msg = new GptChatMessage.System(Format(proto));
                _dialogues.TryAdd(proto, [msg]);
            }
        }

        /// <summary>
        /// Очищает память всех диалогов с моделью.
        /// Помимо базового промта.
        /// </summary>
        [PublicAPI]
        public void ClearMemory()
        {
            foreach (var item in _dialogues)
            {
                ClearMemory(item.Key);
            }

            _inputTokens.Set(0);
            _outputTokens.Set(0);
        }

        /// <summary>
        /// Очищает память конкретного диалога с моделью.
        /// Помимо базового промта.
        /// </summary>
        /// <param name="proto">Прототип</param>
        [PublicAPI]
        public void ClearMemory(ProtoId<AIChatPrototype> proto)
        {
            var proto_ = _protoMan.Index(proto);
            var list = _dialogues[proto];
            var msg = new GptChatMessage.System(Format(proto_));
            list.Clear();
            list.Add(msg);
        }

        [PublicAPI]
        public string Format(
            AIChatPrototype proto_in,
            params (string, object)[] arguments)
        {
            var searchRegex = SearchRegex();
            var str = Loc.GetString(proto_in.BasePrompt);

            var matches = searchRegex.Matches(str);

            foreach (var match in matches.ToList())
            {
                var toReplace = match.Groups[1].Value;
                var id = match.Groups[2].Value;

                _protoMan.TryIndex<WeightedRandomPrototype>(id, out var proto);
                _protoMan.TryIndex<DatasetPrototype>(id, out var dataset);

                var @string = string.Empty;

                if (proto == null && dataset == null)
                {
                    var pair = arguments.ToList().FirstOrDefault(a => a.Item1.Equals(id));

                    @string = pair.Item2.ToString();
                }
                else
                {
                    if (proto != null)
                        @string = proto.Pick(_random);
                    else if (dataset != null)
                        @string = _random.Pick(dataset);
                }

                if (string.IsNullOrEmpty(@string))
                    continue;

                var index = str.IndexOf(toReplace);
                if (index < 0)
                {
                    continue;
                }

                str = string.Concat(str.AsSpan()[..index], @string, str.AsSpan(index + toReplace.Length));
            }

            return str;
        }
    }
}
