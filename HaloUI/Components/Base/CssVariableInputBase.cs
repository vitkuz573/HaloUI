using System.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace HaloUI.Components.Base;

/// <summary>
/// Input base that exposes helpers for building CSS custom property styles.
/// </summary>
public abstract class CssVariableInputBase<TValue> : InputBase<TValue>
{
    protected static void AppendCssVariable(StringBuilder builder, string name, string? value)
        => CssVariableBuilder.Append(builder, name, value);
}