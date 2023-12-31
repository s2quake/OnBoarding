using System.Reflection;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;

namespace OnBoarding.ConsoleHost.Actions;

abstract class ActionBase : IAction
{
    private Dictionary? _values;

    protected ActionBase()
    {
        TypeId = GetType().GetCustomAttribute<ActionTypeAttribute>() is ActionTypeAttribute attribute
            ? attribute.TypeIdentifier
            : throw new NullReferenceException(
                $"Given type {this.GetType()} is missing {nameof(ActionTypeAttribute)}.");
    }

    public IValue PlainValue
    {
        get
        {
            if (_values is null)
            {
                _values = Dictionary.Empty;
                _values = _values.Add("type_id", TypeId);
                _values = OnInitialize(_values);
            }
            return _values;
        }
    }

    public IValue TypeId { get; }

    protected virtual Dictionary OnInitialize(Dictionary values) => values;

    protected virtual void OnLoadPlainValue(Dictionary values)
    {
    }

    protected virtual IWorld OnExecute(IActionContext context) => context.PreviousState;

    #region IAction

    void IAction.LoadPlainValue(IValue plainValue)
    {
        if (plainValue is Dictionary values)
        {
            OnLoadPlainValue(values);
        }
    }

    IWorld IAction.Execute(IActionContext context) => OnExecute(context);

    #endregion
}
