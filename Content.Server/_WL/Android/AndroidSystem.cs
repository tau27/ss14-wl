using Content.Server.PowerCell;
using Content.Shared.PowerCell;

namespace Content.Server._WL.Android
{
    public sealed partial class AndroidSystem : EntitySystem
    {
        [Dependency] private readonly PowerCellSystem _powerCell = default!;

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<AndroidComponent, PowerCellDrawComponent>();
            while (query.MoveNext(out var uid, out _, out var powerCellDrawComp))
            {
                if (!powerCellDrawComp.CanDraw || powerCellDrawComp.Drawing)
                    continue;

                if (!_powerCell.HasDrawCharge(uid, powerCellDrawComp))
                    continue;

                _powerCell.SetPowerCellDrawEnabled(uid, true, powerCellDrawComp);
            }
        }
    }
}
