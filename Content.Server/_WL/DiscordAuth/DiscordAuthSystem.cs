using Content.Shared._WL.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Text;
using Robust.Shared.Player;
using Content.Shared._WL.DiscordAuth;

namespace Content.Server._WL.DiscordAuth
{
    public sealed partial class DiscordAuthSystem : SharedDiscordAuthSystem
    {
        [Dependency] private readonly ISharedPlayerManager _playMan = default!;
        [Dependency] private readonly IConfigurationManager _confMan = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        private Dictionary<NetUserId, string> _playersTokensKeys = default!;

        private const string AllowedCodeSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_.,@#!?1234567890";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<DiscordAuthTokenQueryEvent>((args, session) =>
            {
                var user = session.SenderSession.UserId;
                if (!_playersTokensKeys.TryGetValue(user, out var code))
                    code = EnsureUser(user);

                RaiseNetworkEvent(new DiscordAuthTokenChangedEvent(code, user), session.SenderSession);
            });

            _playersTokensKeys = new();

            _playMan.PlayerStatusChanged += (sender, args) =>
            {
                var session = args.Session;
                var status = args.NewStatus;

                switch (status)
                {
                    case Robust.Shared.Enums.SessionStatus.Connected:
                        EnsureUser(session.UserId);
                        break;
                    case Robust.Shared.Enums.SessionStatus.Disconnected:
                        UnensureUser(session.UserId);
                        break;
                    default:
                        break;
                }
            };
        }

        private string EnsureUser(NetUserId userId)
        {
            var code = GenerateUCode();
            _playersTokensKeys[userId] = code;

            return code;
        }

        private bool UnensureUser(NetUserId userId)
        {
            return _playersTokensKeys.Remove(userId);
        }

        public string GenerateUCode(int length = 32)
        {
            var stb = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                var @char = _random.Pick(AllowedCodeSymbols.ToCharArray());
                stb.Append(@char);
            }

            return stb.ToString();
        }

        public string? GetUserCode(NetUserId? id)
        {
            if (id == null)
                return null;

            if (!_playersTokensKeys.TryGetValue(id.Value, out var code))
                code = EnsureUser(id.Value);

            return code;
        }
    }
}
