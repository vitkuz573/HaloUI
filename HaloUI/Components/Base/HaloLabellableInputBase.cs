// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using HaloUI.Abstractions;
using HaloUI.Accessibility;
using HaloUI.Theme;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components.Base;

/// <summary>
/// Base input component that provides common label plumbing and theming support.
/// </summary>
/// <typeparam name="TValue">The value type.</typeparam>
public abstract class HaloLabellableInputBase<TValue> : ThemeAwareInputBase<TValue>, ILabellableInput
{
    private readonly string _generatedId = AccessibilityIdGenerator.Create("halo-input");
    private readonly Expression<Func<TValue>> _standaloneExpression;
    private string _inputId = string.Empty;
    private string? _labelElementId;
    private TValue _standaloneValue = default!;
    private bool _usingStandaloneExpression;

    protected HaloLabellableInputBase()
    {
        _standaloneExpression = () => StandaloneValue;
    }

    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Required { get; set; }
    [Inject] protected IEnumerable<IAriaDiagnosticsHub> AriaDiagnosticsHubs { get; set; } = [];

    /// <inheritdoc />
    public string InputId => _inputId;

    protected InputDesignTokens InputTokens => ThemeContext?.Theme.Tokens.Component.Get<InputDesignTokens>() ?? new InputDesignTokens();
    protected IAriaDiagnosticsHub? AriaDiagnosticsHub => AriaDiagnosticsHubs.FirstOrDefault();

    protected string? LabelElementId => string.IsNullOrWhiteSpace(Label)
        ? null
        : _labelElementId ??= AccessibilityIdGenerator.Create("halo-label");

    public async override Task SetParametersAsync(ParameterView parameters)
    {
        var hasValueExpression = parameters.TryGetValue<Expression<Func<TValue>>>(nameof(ValueExpression), out var expression) && expression is not null;

        if (!hasValueExpression)
        {
            _usingStandaloneExpression = true;

            ValueExpression ??= _standaloneExpression;
        }
        else
        {
            _usingStandaloneExpression = false;
        }

        await base.SetParametersAsync(parameters);

        if (_usingStandaloneExpression)
        {
            if (EditContext is not null)
            {
                var componentName = GetType().Name;
                throw new InvalidOperationException($"{componentName} requires a ValueExpression when used inside an EditForm. Provide 'ValueExpression' or switch to a read-only display component.");
            }

            StandaloneValue = CurrentValue!;
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _inputId = ResolveInputId();
    }

    protected Dictionary<string, object> BuildBaseInputAttributes(Action<AccessibilityAttributesBuilder>? configure = null, params string[] excludedAttributes)
    {
        var builder = new AccessibilityAttributesBuilder()
            .ForComponent(GetType())
            .WithInspectorElementId(InputId)
            .WithAttribute("id", InputId);

        if (LabelElementId is not null)
        {
            builder.WithLabelledBy(LabelElementId);
        }

        if (IsRequired)
        {
            builder.WithRequired(true);
        }

        configure?.Invoke(builder);
        builder.WithAccessibleNameFromAdditionalAttributes(AdditionalAttributes);

        var accessibilityAttributes = builder.Build(AriaDiagnosticsHub);

        return AccessibilityAttributesBuilder.Merge(AdditionalAttributes, accessibilityAttributes, excludedAttributes);
    }

    protected bool IsRequired => Required || AdditionalAttributesDeclareRequired();

    private TValue StandaloneValue
    {
        get => _standaloneValue;
        set => _standaloneValue = value;
    }

    private bool AdditionalAttributesDeclareRequired()
    {
        if (AdditionalAttributes is null)
        {
            return false;
        }

        foreach (var attribute in AdditionalAttributes)
        {
            if (string.Equals(attribute.Key, "required", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(attribute.Key, "aria-required", StringComparison.OrdinalIgnoreCase))
            {
                if (IsTruthy(attribute.Value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsTruthy(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        var stringValue = value.ToString()?.Trim();

        if (string.IsNullOrEmpty(stringValue))
        {
            return true;
        }

        if (bool.TryParse(stringValue, out var parsedBool))
        {
            return parsedBool;
        }

        return !string.Equals(stringValue, "0", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveInputId()
    {
        if (AdditionalAttributes is null)
        {
            return _generatedId;
        }

        foreach (var attribute in AdditionalAttributes)
        {
            if (string.Equals(attribute.Key, "id", StringComparison.OrdinalIgnoreCase))
            {
                return attribute.Value?.ToString() ?? _generatedId;
            }
        }

        return _generatedId;
    }
}
