using System.Linq.Expressions;

namespace HaloUI.Abstractions;

public class DialogParameters : Dictionary<string, object?>
{
}

public class DialogParameters<TComponent> : DialogParameters
{
    public void Add(Expression<Func<TComponent, object?>> selector, object? value)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var name = selector.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression member } => member.Member.Name,
            _ => throw new ArgumentException("Selector must be a member expression.", nameof(selector))
        };

        this[name] = value;
    }
}