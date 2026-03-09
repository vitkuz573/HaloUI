// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using HaloUI.Abstractions;
using HaloUI.Accessibility.Aria;

namespace HaloUI.Accessibility;

/// <summary>
/// Helps build a consistent set of accessibility attributes for component markup.
/// </summary>
public sealed class AccessibilityAttributesBuilder
{
    private static readonly StringComparer AttributeComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly HashSet<string> TokenizedAttributes = new(
        [
            "role",
            AriaAttributes.Controls.Name,
            AriaAttributes.DescribedBy.Name,
            AriaAttributes.FlowTo.Name,
            AriaAttributes.LabelledBy.Name,
            AriaAttributes.Owns.Name,
            AriaAttributes.Relevant.Name,
            AriaAttributes.DropEffect.Name
        ], AttributeComparer);
    
    private static readonly IReadOnlyDictionary<string, string> EmptyTags = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    private const string AccessibleNameIdentifier = "accessible name";
    private const string AccessibleNameRecommendation = "aria-label, aria-labelledby, or title";

    private readonly Dictionary<string, object> _attributes = new(AttributeComparer);
    private readonly Dictionary<string, TokenAccumulator> _tokenAttributes = new(AttributeComparer);
    private AriaRole? _enforcedRole;
    private AriaRoleDefinition? _roleDefinition;
    private AriaRoleCompliance _roleCompliance = AriaRoleCompliance.None;
    private InspectionMetadataBuilder? _inspectionMetadata;
    private bool _emitSuccessEvents;
    private bool _enforceCompliance;
    private bool _accessibleNameFromContent;
    private bool _accessibleNameProvidedExternally;

    public AccessibilityAttributesBuilder WithRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return WithAttribute("role", role);
    }

    public AccessibilityAttributesBuilder WithRole(AriaRole role)
    {
        var compliance = _roleCompliance == AriaRoleCompliance.None ? AriaRoleCompliance.EnsureRequiredAttributes : _roleCompliance;

        return WithRole(role, compliance);
    }

    public AccessibilityAttributesBuilder WithRole(AriaRole role, AriaRoleCompliance compliance)
    {
        SetRoleMetadata(role, compliance);

        EnsureTokenAccumulator("role").ReplaceWith(role.ToAttributeValue());

        return this;
    }

    public AccessibilityAttributesBuilder WithRoles(params AriaRole[] roles)
    {
        if (roles.Length == 0)
        {
            return WithAttribute("role", null);
        }

        ClearRoleMetadata();

        return WithTokenAttribute("role", roles.Select(static value => value.ToAttributeValue()));
    }

    public AccessibilityAttributesBuilder WithRoleCompliance(AriaRoleCompliance compliance)
    {
        _roleCompliance = compliance;

        if (_enforcedRole is { } role && _roleDefinition is null && AriaRoleRegistry.TryGetDefinition(role, out var definition))
        {
            _roleDefinition = definition;
        }

        return this;
    }

    public AccessibilityAttributesBuilder WithoutRoleCompliance()
    {
        _roleCompliance = AriaRoleCompliance.None;

        return this;
    }

    public AccessibilityAttributesBuilder ForComponent(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        return WithInspectorSource(componentType.FullName ?? componentType.Name);
    }

    public AccessibilityAttributesBuilder ForComponent(object component)
    {
        return ForComponent(component.GetType());
    }

    private AccessibilityAttributesBuilder WithInspectorSource(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            EnsureMetadata().Source = null;
        }
        else
        {
            EnsureMetadata().Source = source.Trim();
        }

        return this;
    }

    public AccessibilityAttributesBuilder WithInspectorElementId(string? elementId)
    {
        if (string.IsNullOrWhiteSpace(elementId))
        {
            EnsureMetadata().ElementId = null;
        }
        else
        {
            EnsureMetadata().ElementId = elementId.Trim();
        }

        return this;
    }

    public AccessibilityAttributesBuilder WithInspectorTag(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        EnsureMetadata().SetTag(key, value);

        return this;
    }

    public AccessibilityAttributesBuilder WithAccessibleNameFromContent(bool provided = true)
    {
        _accessibleNameFromContent = provided;

        return this;
    }

    public AccessibilityAttributesBuilder WithAccessibleNameProvided(bool provided = true)
    {
        _accessibleNameProvidedExternally = provided;

        return this;
    }

    public AccessibilityAttributesBuilder WithAccessibleNameFromAdditionalAttributes(IReadOnlyDictionary<string, object>? attributes)
    {
        if (attributes is null)
        {
            return this;
        }

        if (ContainsAccessibleName(attributes))
        {
            _accessibleNameProvidedExternally = true;
        }

        return this;
    }

    private InspectionMetadataBuilder EnsureMetadata()
    {
        return _inspectionMetadata ??= new InspectionMetadataBuilder();
    }

    public AccessibilityAttributesBuilder CaptureSuccessfulInspections(bool capture = true)
    {
        _emitSuccessEvents = capture;
        
        return this;
    }

    public AccessibilityAttributesBuilder RequireCompliance(bool enforce = true)
    {
        _enforceCompliance = enforce;
        
        return this;
    }

    public AccessibilityAttributesBuilder WithAttribute(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (string.Equals(name, "role", StringComparison.OrdinalIgnoreCase))
        {
            return WithRoleAttribute(value);
        }

        if (value is null || (value is string stringValue && string.IsNullOrWhiteSpace(stringValue)))
        {
            _attributes.Remove(name);
            _tokenAttributes.Remove(name);

            return this;
        }

        if (IsTokenizedAttribute(name))
        {
            EnsureTokenAccumulator(name).ReplaceWith(value.ToString());
        }
        else
        {
            _attributes[name] = value;
        }

        return this;
    }

    public AccessibilityAttributesBuilder WithAttribute(AriaBooleanAttribute attribute, bool? value)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (value is null || (!attribute.RenderOnFalse && !value.Value))
        {
            return WithAttribute(attribute.Name, null);
        }

        return WithAttribute(attribute.Name, value.Value ? "true" : "false");
    }

    public AccessibilityAttributesBuilder WithAttribute<TEnum>(AriaEnumAttribute<TEnum> attribute, TEnum? value) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (value is null)
        {
            return WithAttribute(attribute.Name, null);
        }

        return WithAttribute(attribute.Name, AriaEnumValueCache<TEnum>.ToAttributeValue(value.Value));
    }

    public AccessibilityAttributesBuilder WithAttribute<TEnum>(AriaTokenEnumAttribute<TEnum> attribute, params TEnum[] values) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (values.Length == 0)
        {
            return WithAttribute(attribute.Name, null);
        }

        return WithTokenAttribute(attribute.Name, values.Select(static value => AriaEnumValueCache<TEnum>.ToAttributeValue(value)));
    }

    public AccessibilityAttributesBuilder WithAttribute(AriaTokenListAttribute attribute, params string?[] tokens)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        return WithTokenAttribute(attribute.Name, tokens);
    }

    public AccessibilityAttributesBuilder WithAttribute(AriaIdReferenceAttribute attribute, params string?[] ids)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (attribute.AllowMultiple)
        {
            return WithTokenAttribute(attribute.Name, ids);
        }

        if (ids.Length == 0)
        {
            return WithAttribute(attribute.Name, null);
        }

        foreach (var id in ids)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                return WithAttribute(attribute.Name, id.Trim());
            }
        }

        return WithAttribute(attribute.Name, null);
    }

    public AccessibilityAttributesBuilder WithAttribute(AriaStringAttribute attribute, string? value)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        return WithAttribute(attribute.Name, value);
    }

    public AccessibilityAttributesBuilder WithAttribute(AriaIntegerAttribute attribute, int? value)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (value is null)
        {
            return WithAttribute(attribute.Name, null);
        }

        return WithAttribute(attribute.Name, value.Value.ToString(CultureInfo.InvariantCulture));
    }

    public AccessibilityAttributesBuilder WithAttribute(AriaNumberAttribute attribute, double? value)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (value is null)
        {
            return WithAttribute(attribute.Name, null);
        }

        return WithAttribute(attribute.Name, value.Value.ToString(CultureInfo.InvariantCulture));
    }

    public AccessibilityAttributesBuilder WithAttribute(AriaNumberAttribute attribute, decimal? value)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (value is null)
        {
            return WithAttribute(attribute.Name, null);
        }

        return WithAttribute(attribute.Name, value.Value.ToString(CultureInfo.InvariantCulture));
    }

    public AccessibilityAttributesBuilder WithAria(string name, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var key = $"aria-{name}";

        return WithAttribute(key, value);
    }

    public AccessibilityAttributesBuilder WithAria(string name, bool? value, bool emitFalse = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (value is null || (value == false && !emitFalse))
        {
            return WithAria(name, null);
        }

        return WithAria(name, value.Value ? "true" : "false");
    }

    public AccessibilityAttributesBuilder WithDisabled(bool disabled)
    {
        return WithAttribute(AriaAttributes.Disabled, disabled);
    }

    public AccessibilityAttributesBuilder WithBusy(bool busy)
    {
        return WithAttribute(AriaAttributes.Busy, busy);
    }

    public AccessibilityAttributesBuilder WithInvalid(bool invalid)
    {
        return WithAttribute(AriaAttributes.Invalid, invalid ? AriaInvalidState.True : AriaInvalidState.False);
    }

    public AccessibilityAttributesBuilder WithRequired(bool required)
    {
        return WithAttribute(AriaAttributes.Required, required);
    }

    public AccessibilityAttributesBuilder WithPressed(bool pressed)
    {
        return WithAttribute(AriaAttributes.Pressed, pressed ? AriaPressedState.True : AriaPressedState.False);
    }

    public AccessibilityAttributesBuilder WithLabelledBy(params string?[] elementIds)
    {
        return WithAttribute(AriaAttributes.LabelledBy, elementIds);
    }

    public AccessibilityAttributesBuilder WithDescribedBy(params string?[] elementIds)
    {
        return WithAttribute(AriaAttributes.DescribedBy, elementIds);
    }

    public AccessibilityAttributesBuilder WithTokenAttribute(string attributeName, params string?[] tokens)
    {
        return WithTokenAttribute(attributeName, (IEnumerable<string?>)tokens);
    }

    public AccessibilityAttributesBuilder WithTokenAttribute(string attributeName, IEnumerable<string?> tokens)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeName);

        if (!IsTokenizedAttribute(attributeName))
        {
            throw new ArgumentException($"Only tokenized attributes are supported via {nameof(WithTokenAttribute)}. Provided attribute: '{attributeName}'", nameof(attributeName));
        }

        var sanitized = tokens
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(static token => token!.Trim())
            .ToArray();

        if (sanitized.Length == 0)
        {
            _tokenAttributes.Remove(attributeName);
            _attributes.Remove(attributeName);

            return this;
        }

        var accumulator = EnsureTokenAccumulator(attributeName);

        foreach (var token in sanitized)
        {
            accumulator.Add(token);
        }

        return this;
    }

    public IReadOnlyDictionary<string, object> Build()
    {
        return Build(null);
    }

    public IReadOnlyDictionary<string, object> Build(IAriaDiagnosticsHub? diagnosticsHub)
    {
        var result = new Dictionary<string, object>(_attributes, AttributeComparer);

        foreach (var (attribute, accumulator) in _tokenAttributes)
        {
            if (accumulator.HasTokens)
            {
                result[attribute] = accumulator.ToString();
            }
            else
            {
                result.Remove(attribute);
            }
        }

        var compliance = EvaluateRoleCompliance(result);
        
        PublishInspection(diagnosticsHub, result, compliance);

        if (!_enforceCompliance || !compliance.HasFailures)
        {
            return result;
        }

        var roleDescriptor = ResolveRoleDescriptor(result);
        var message = BuildComplianceMessage(roleDescriptor, compliance.MissingRequired, compliance.Disallowed, compliance.Recommendations);
        
        throw new InvalidOperationException(message);
    }

    public static Dictionary<string, object> Merge(IReadOnlyDictionary<string, object>? additionalAttributes, IReadOnlyDictionary<string, object> accessibilityAttributes, params string[] excludedAttributes)
    {
        var result = new Dictionary<string, object>(AttributeComparer);

        HashSet<string>? excluded = null;

        if (excludedAttributes is { Length: > 0 })
        {
            excluded = new HashSet<string>(excludedAttributes, AttributeComparer);
        }

        if (additionalAttributes is not null)
        {
            foreach (var attribute in additionalAttributes)
            {
                if (excluded?.Contains(attribute.Key) == true)
                {
                    continue;
                }

                result[attribute.Key] = attribute.Value;
            }
        }

        foreach (var attribute in accessibilityAttributes)
        {
            if (result.TryGetValue(attribute.Key, out var existing) && IsTokenizedAttribute(attribute.Key))
            {
                result[attribute.Key] = CombineTokenValues(existing.ToString(), attribute.Value.ToString());
            }
            else
            {
                result[attribute.Key] = attribute.Value;
            }
        }

        return result;
    }

    private static bool IsTokenizedAttribute(string attributeName)
    {
        return TokenizedAttributes.Contains(attributeName);
    }

    private static bool ContainsAccessibleName(IReadOnlyDictionary<string, object> attributes)
    {
        return attributes.Any(attribute => IsAccessibleNameAttribute(attribute.Key, attribute.Value));
    }

    private static bool HasAccessibleNameAttribute(IReadOnlyDictionary<string, object> attributes)
    {
        return ContainsAccessibleName(attributes);
    }

    private static bool IsAccessibleNameAttribute(string attributeName, object? value)
    {
        if (string.Equals(attributeName, AriaAttributes.Label.Name, StringComparison.OrdinalIgnoreCase) || string.Equals(attributeName, AriaAttributes.LabelledBy.Name, StringComparison.OrdinalIgnoreCase) || string.Equals(attributeName, "title", StringComparison.OrdinalIgnoreCase) || string.Equals(attributeName, "alt", StringComparison.OrdinalIgnoreCase))
        {
            return HasNonEmptyValue(value);
        }

        return false;
    }

    private static bool HasNonEmptyValue(object? value)
    {
        return value switch
        {
            null => false,
            string stringValue => !string.IsNullOrWhiteSpace(stringValue),
            IEnumerable<string> tokens => tokens.Any(static token => !string.IsNullOrWhiteSpace(token)),
            _ => true
        };
    }

    private static void AddIfMissing(List<string> target, string value)
    {
        if (!target.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)))
        {
            target.Add(value);
        }
    }

    private static string CombineTokenValues(string? existing, string? additional)
    {
        var accumulator = new TokenAccumulator();

        accumulator.AddFromDelimited(existing);
        accumulator.AddFromDelimited(additional);

        return accumulator.HasTokens ? accumulator.ToString() : string.Empty;
    }

    private ComplianceEvaluation EvaluateRoleCompliance(Dictionary<string, object> attributes)
    {
        if (_roleDefinition is null || _roleCompliance == AriaRoleCompliance.None)
        {
            return ComplianceEvaluation.Success;
        }

        var missingRequired = _roleDefinition.GetMissingRequired(attributes).ToList();
        var disallowed = _roleCompliance == AriaRoleCompliance.Strict
            ? _roleDefinition.GetDisallowedAttributes(attributes.Keys).ToList()
            : new List<string>();

        var recommended = _roleDefinition.GetMissingRecommended(attributes).ToList();

        var accessibleName = EvaluateAccessibleName(attributes);

        if (accessibleName.MissingAccessibleName)
        {
            AddIfMissing(missingRequired, AccessibleNameIdentifier);
        }

        if (accessibleName.AccessibleNameProhibited)
        {
            AddIfMissing(disallowed, AccessibleNameIdentifier);
        }

        if (accessibleName.UnsupportedContentSource)
        {
            AddIfMissing(recommended, AccessibleNameRecommendation);
        }

        return new ComplianceEvaluation(
            missingRequired.ToArray(),
            disallowed.ToArray(),
            recommended.ToArray());
    }

    private AccessibleNameEvaluation EvaluateAccessibleName(IReadOnlyDictionary<string, object> attributes)
    {
        if (_roleDefinition is null || _roleCompliance == AriaRoleCompliance.None)
        {
            return AccessibleNameEvaluation.NotApplicable;
        }

        var hasNameFromAttributes = HasAccessibleNameAttribute(attributes);
        var hasNameFromContent = _accessibleNameFromContent && _roleDefinition.SupportsNameFromContent;
        var hasNameFromExternal = _accessibleNameProvidedExternally;

        var missing = false;
        var disallowed = false;
        var unsupportedContent = _accessibleNameFromContent && !_roleDefinition.SupportsNameFromContent;

        switch (_roleDefinition.AccessibleNameRequirement)
        {
            case AriaAccessibleNameRequirement.Required:
                missing = !(hasNameFromAttributes || hasNameFromContent || hasNameFromExternal);
                break;
            case AriaAccessibleNameRequirement.Prohibited:
                if (hasNameFromAttributes || hasNameFromExternal || _accessibleNameFromContent)
                {
                    disallowed = true;
                }

                break;
        }

        if (_roleDefinition.AccessibleNameRequirement == AriaAccessibleNameRequirement.Required &&
            !hasNameFromAttributes &&
            !hasNameFromExternal &&
            _accessibleNameFromContent &&
            !_roleDefinition.SupportsNameFromContent)
        {
            missing = true;
        }

        return new AccessibleNameEvaluation(missing, disallowed, unsupportedContent);
    }

    private string ResolveRoleDescriptor(Dictionary<string, object> attributes)
    {
        if (_enforcedRole is { } role)
        {
            return role.ToAttributeValue();
        }

        if (attributes.TryGetValue("role", out var value))
        {
            return value.ToString() ?? "unknown";
        }

        return "unknown";
    }

    private static string BuildComplianceMessage(string role, IReadOnlyCollection<string> missingRequired, IReadOnlyCollection<string> disallowed, IReadOnlyCollection<string> recommended)
    {
        var builder = new StringBuilder();
        
        builder.Append("ARIA role '");
        builder.Append(role);
        builder.Append("' compliance failed.");

        if (missingRequired.Count > 0)
        {
            builder.Append(' ');
            builder.Append("Missing required attributes: ");
            builder.Append(string.Join(", ", missingRequired));
            builder.Append('.');
        }

        if (disallowed.Count > 0)
        {
            builder.Append(' ');
            builder.Append("Unsupported attributes present: ");
            builder.Append(string.Join(", ", disallowed));
            builder.Append('.');
        }

        if (recommended.Count <= 0)
        {
            return builder.ToString();
        }

        builder.Append(' ');
        builder.Append("Consider adding: ");
        builder.Append(string.Join(", ", recommended));
        builder.Append('.');

        return builder.ToString();
    }

    private void PublishInspection(IAriaDiagnosticsHub? diagnosticsHub, IReadOnlyDictionary<string, object> attributes, ComplianceEvaluation evaluation)
    {
        if (diagnosticsHub is null)
        {
            return;
        }

        var metadata = _inspectionMetadata?.Build() ?? new AriaInspectionMetadata(null, null, EmptyTags);
        var role = _enforcedRole ?? TryResolveRole(attributes);
        var severity = evaluation.HasFailures
            ? AriaDiagnosticsSeverity.Error
            : evaluation.HasWarnings && _roleCompliance != AriaRoleCompliance.None
                ? AriaDiagnosticsSeverity.Warning
                : AriaDiagnosticsSeverity.Success;

        if (severity == AriaDiagnosticsSeverity.Success && !_emitSuccessEvents)
        {
            return;
        }

        var snapshot = new Dictionary<string, string>(AttributeComparer);

        foreach (var attribute in attributes)
        {
            snapshot[attribute.Key] = attribute.Value.ToString() ?? string.Empty;
        }

        var @event = new AriaDiagnosticsEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, role, _roleCompliance, severity, snapshot, evaluation.MissingRequired, evaluation.Disallowed, evaluation.Recommendations, metadata);

        diagnosticsHub.Publish(@event);
    }

    private static AriaRole? TryResolveRole(IReadOnlyDictionary<string, object> attributes)
    {
        if (!attributes.TryGetValue("role", out var value))
        {
            return null;
        }

        var roleToken = value.ToString();

        if (AriaRoleExtensions.TryParse(roleToken, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private AccessibilityAttributesBuilder WithRoleAttribute(object? value)
    {
        if (value is null)
        {
            _tokenAttributes.Remove("role");
            _attributes.Remove("role");
            
            ClearRoleMetadata();

            return this;
        }

        var stringValue = value.ToString();

        if (string.IsNullOrWhiteSpace(stringValue))
        {
            _tokenAttributes.Remove("role");
            _attributes.Remove("role");
            
            ClearRoleMetadata();

            return this;
        }

        var trimmed = stringValue.Trim();

        if (AriaRoleExtensions.TryParse(trimmed, out var parsed))
        {
            if (_roleCompliance == AriaRoleCompliance.None)
            {
                _roleCompliance = AriaRoleCompliance.EnsureRequiredAttributes;
            }

            SetRoleMetadata(parsed, _roleCompliance);
        }
        else
        {
            ClearRoleMetadata();
        }

        EnsureTokenAccumulator("role").ReplaceWith(trimmed);

        return this;
    }

    private void SetRoleMetadata(AriaRole role, AriaRoleCompliance compliance)
    {
        _enforcedRole = role;
        _roleCompliance = compliance;
        _roleDefinition = AriaRoleRegistry.TryGetDefinition(role, out var definition) ? definition : null;
    }

    private void ClearRoleMetadata()
    {
        _enforcedRole = null;
        _roleDefinition = null;
        _roleCompliance = AriaRoleCompliance.None;
    }

    private TokenAccumulator EnsureTokenAccumulator(string attributeName)
    {
        if (_tokenAttributes.TryGetValue(attributeName, out var accumulator))
        {
            return accumulator;
        }

        accumulator = new TokenAccumulator();
        
        _tokenAttributes[attributeName] = accumulator;

        if (!_attributes.TryGetValue(attributeName, out var existing))
        {
            return accumulator;
        }

        accumulator.ReplaceWith(existing.ToString());
        
        _attributes.Remove(attributeName);

        return accumulator;
    }

    private sealed class TokenAccumulator
    {
        private readonly List<string> _ordered = [];
        private readonly HashSet<string> _unique = new(StringComparer.Ordinal);

        public bool HasTokens => _ordered.Count > 0;

        public void ReplaceWith(string? value)
        {
            _ordered.Clear();
            _unique.Clear();

            AddFromDelimited(value);
        }

        public void Add(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            if (_unique.Add(token))
            {
                _ordered.Add(token);
            }
        }

        public void AddFromDelimited(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                Add(token);
            }
        }

        public override string ToString()
        {
            return string.Join(' ', _ordered);
        }
    }

    private sealed record AccessibleNameEvaluation(bool MissingAccessibleName, bool AccessibleNameProhibited, bool UnsupportedContentSource)
    {
        public static AccessibleNameEvaluation NotApplicable { get; } = new(false, false, false);
    }

    private sealed record ComplianceEvaluation(IReadOnlyList<string> MissingRequired, IReadOnlyList<string> Disallowed, IReadOnlyList<string> Recommendations)
    {
        public bool HasFailures => MissingRequired.Count > 0 || Disallowed.Count > 0;

        public bool HasWarnings => Recommendations.Count > 0;

        public static ComplianceEvaluation Success { get; } = new([], [], []);
    }

    private sealed class InspectionMetadataBuilder
    {
        private Dictionary<string, string>? _tags;

        public string? Source { get; set; }

        public string? ElementId { get; set; }

        public void SetTag(string key, string value)
        {
            _tags ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _tags[key] = value.Trim();
        }

        public AriaInspectionMetadata Build()
        {
            IReadOnlyDictionary<string, string> tags;

            if (_tags is null || _tags.Count == 0)
            {
                tags = EmptyTags;
            }
            else
            {
                tags = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(_tags, StringComparer.OrdinalIgnoreCase));
            }

            return new AriaInspectionMetadata(Source, ElementId, tags);
        }
    }
}
