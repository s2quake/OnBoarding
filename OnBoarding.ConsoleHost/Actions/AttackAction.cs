using Bencodex.Types;
using Libplanet.Action;

namespace OnBoarding.ConsoleHost.Actions;

[ActionType("attack")]
sealed class AttackAction : ActionBase
{
    protected override void OnInitialize(Dictionary values)
    {
        base.OnInitialize(values);
    }
}