using Content.Shared._WL.CCVars;
using Content.Shared._WL.DiscordAuth;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._WL.DiscordAuth
{
    [UsedImplicitly]
    public sealed partial class ClientDiscordAuthSystem : SharedDiscordAuthSystem
    {
        [Dependency] private readonly IPlayerManager _playMan = default!;
        [Dependency] private readonly IConfigurationManager _confMan = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        private TimeSpan _expirationTime = TimeSpan.FromSeconds(WLCVars.DiscordAuthTokensExpirationTime.DefaultValue);

        private TimeSpan _expirationAccum = TimeSpan.Zero;

        private event Action<string>? OnTokenChanged;

        private string? _token;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<DiscordAuthTokenChangedEvent>(OnTokenChangedEvent);

            OnTokenChanged = (_) => { };

            _expirationTime = TimeSpan.FromSeconds(_confMan.GetCVar(WLCVars.DiscordAuthTokensExpirationTime));
            _confMan.OnValueChanged(WLCVars.DiscordAuthTokensExpirationTime, (value) => _expirationTime = TimeSpan.FromSeconds(value), true);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            _expirationAccum += _timing.TickPeriod;

            if (_expirationAccum >= _expirationTime)
            {
                _expirationAccum = TimeSpan.Zero;

                QueryCode();
            }
        }

        private void OnTokenChangedEvent(DiscordAuthTokenChangedEvent args)
        {
            if (args.Session.UserId != _playMan.LocalSession?.UserId)
                return;

            _token = args.NewToken;
            OnTokenChanged?.Invoke(args.NewToken);
        }

        public void SubscribeOnTokenChanged(Action<string> action)
        {
            OnTokenChanged += action;
        }

        public TimeSpan GetExpirationTimeRemains()
        {
            return _expirationTime - _expirationAccum;
        }

        public float GetExpirationTimeFraction(bool revert = true)
        {
            var fraction = _expirationAccum / _expirationTime;

            return (float)(revert
                ? 1d - fraction
                : fraction);
        }

        public void QueryCode()
        {
            if (_playMan?.LocalSession is not ICommonSession session)
                return;

            RaiseNetworkEvent(new DiscordAuthTokenQueryEvent(), session);
        }

        public string? GetUserCode()
        {
            return _token;
        }
    }
}
