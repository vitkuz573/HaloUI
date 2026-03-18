namespace HaloUI.Accessibility.Aria;

/// <summary>
/// Describes the ARIA attributes that are required or permitted for a specific role.
/// </summary>
public sealed class AriaRoleDefinition
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private readonly HashSet<string> _required;
    private readonly HashSet<string> _allowed;
    private readonly HashSet<string> _recommended;

    public AriaRoleDefinition(
        AriaRole role,
        IEnumerable<AriaAttributeDefinition>? required = null,
        IEnumerable<AriaAttributeDefinition>? allowed = null,
        IEnumerable<AriaAttributeDefinition>? recommended = null,
        string? description = null,
        AriaAccessibleNameRequirement accessibleNameRequirement = AriaAccessibleNameRequirement.Unspecified,
        bool supportsNameFromContent = false)
    {
        Role = role;
        Description = description;
        AccessibleNameRequirement = accessibleNameRequirement;
        SupportsNameFromContent = supportsNameFromContent;

        _required = new HashSet<string>(required?.Select(a => a.Name) ?? [], Comparer);
        _allowed = new HashSet<string>(allowed?.Select(a => a.Name) ?? [], Comparer);
        _recommended = new HashSet<string>(recommended?.Select(a => a.Name) ?? [], Comparer);

        foreach (var attribute in _required)
        {
            _allowed.Add(attribute);
        }

        foreach (var attribute in _recommended)
        {
            _allowed.Add(attribute);
        }
    }

    public AriaRole Role { get; }

    public string? Description { get; }

    public AriaAccessibleNameRequirement AccessibleNameRequirement { get; }

    public bool SupportsNameFromContent { get; }

    public IReadOnlyCollection<string> RequiredAttributeNames => _required;

    public IReadOnlyCollection<string> AllowedAttributeNames => _allowed;

    public IReadOnlyCollection<string> RecommendedAttributeNames => _recommended;

    internal IEnumerable<string> GetMissingRequired(IReadOnlyDictionary<string, object> attributes)
    {
        foreach (var attribute in _required)
        {
            if (!attributes.TryGetValue(attribute, out var value) || IsMissing(value))
            {
                yield return attribute;
            }
        }
    }

    internal IEnumerable<string> GetMissingRecommended(IReadOnlyDictionary<string, object> attributes)
    {
        return _recommended.Where(attribute => !attributes.ContainsKey(attribute));
    }

    internal IEnumerable<string> GetDisallowedAttributes(IEnumerable<string> attributes)
    {
        return attributes.Where(attribute => !IsAllowed(attribute));
    }

    private bool IsAllowed(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            return false;
        }

        if (!attributeName.StartsWith("aria-", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (_allowed.Contains(attributeName))
        {
            return true;
        }

        return GlobalAttributeNames.Contains(attributeName);
    }

    private static bool IsMissing(object? value)
    {
        return value switch
        {
            null => true,
            string stringValue => string.IsNullOrWhiteSpace(stringValue),
            _ => false
        };
    }

    private static readonly HashSet<string> GlobalAttributeNames = new(
        [
            AriaAttributes.Atomic.Name,
            AriaAttributes.Busy.Name,
            AriaAttributes.Controls.Name,
            AriaAttributes.Current.Name,
            AriaAttributes.DescribedBy.Name,
            AriaAttributes.Description.Name,
            AriaAttributes.Details.Name,
            AriaAttributes.Disabled.Name,
            AriaAttributes.DropEffect.Name,
            AriaAttributes.ErrorMessage.Name,
            AriaAttributes.Expanded.Name,
            AriaAttributes.FlowTo.Name,
            AriaAttributes.Grabbed.Name,
            AriaAttributes.HasPopup.Name,
            AriaAttributes.Hidden.Name,
            AriaAttributes.Invalid.Name,
            AriaAttributes.KeyShortcuts.Name,
            AriaAttributes.Label.Name,
            AriaAttributes.LabelledBy.Name,
            AriaAttributes.Live.Name,
            AriaAttributes.Owns.Name,
            AriaAttributes.Relevant.Name,
            AriaAttributes.RoleDescription.Name,
            AriaAttributes.BrailleLabel.Name,
            AriaAttributes.BrailleRoleDescription.Name
        ], Comparer);
}