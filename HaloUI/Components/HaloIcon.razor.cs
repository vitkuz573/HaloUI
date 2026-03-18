using Microsoft.AspNetCore.Components;
using HaloUI.Iconography;

namespace HaloUI.Components;

public partial class HaloIcon
{
    private const string DefaultSvgViewBox = "0 0 24 24";
    private static readonly IHaloIconResolver DefaultResolver = new PassthroughHaloIconResolver();

    private HaloIconDefinition? _resolvedIcon;

    [Parameter]
    public IHaloIconReference? Name { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool Decorative { get; set; } = true;

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public RenderFragment? Fallback { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject]
    public IEnumerable<IHaloIconResolver> IconResolvers { get; set; } = [];

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _resolvedIcon = ResolveIcon();
    }

    private HaloIconDefinition? ResolveIcon()
    {
        if (Name is not { } iconReference)
        {
            return null;
        }

        var iconToken = iconReference.ToIconToken();

        if (iconToken.IsEmpty)
        {
            return null;
        }

        var resolver = IconResolvers.FirstOrDefault() ?? DefaultResolver;

        if (resolver.TryResolve(iconToken, out var icon))
        {
            return icon;
        }

        return null;
    }

    private Dictionary<string, object> BuildAttributes(HaloIconDefinition icon, bool includeValueAsClass)
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (AdditionalAttributes is not null)
        {
            foreach (var attribute in AdditionalAttributes)
            {
                attributes[attribute.Key] = attribute.Value;
            }
        }

        attributes["class"] = BuildCssClass(icon, includeValueAsClass, attributes);

        if (Decorative)
        {
            attributes["aria-hidden"] = "true";
            attributes.Remove("role");
            attributes.Remove("aria-label");
        }
        else
        {
            attributes["role"] = "img";

            var resolvedAriaLabel = !string.IsNullOrWhiteSpace(AriaLabel) ? AriaLabel : icon.Name.Value;
            attributes["aria-label"] = resolvedAriaLabel;
            attributes.Remove("aria-hidden");
        }

        if (!string.IsNullOrWhiteSpace(Title))
        {
            attributes["title"] = Title!;
        }

        return attributes;
    }

    private string BuildCssClass(HaloIconDefinition icon, bool includeValueAsClass, IReadOnlyDictionary<string, object> attributes)
    {
        var classes = new List<string>
        {
            "halo-icon"
        };

        if (!string.IsNullOrWhiteSpace(icon.ProviderClass))
        {
            classes.Add(icon.ProviderClass!);
        }

        if (includeValueAsClass && !string.IsNullOrWhiteSpace(icon.Value))
        {
            classes.Add(icon.Value);
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        if (attributes.TryGetValue("class", out var existingClass) &&
            existingClass is not null &&
            !string.IsNullOrWhiteSpace(existingClass.ToString()))
        {
            classes.Add(existingClass.ToString()!);
        }

        return string.Join(' ', classes);
    }

    private static string ResolveViewBox(HaloIconDefinition icon)
    {
        return string.IsNullOrWhiteSpace(icon.ViewBox)
            ? DefaultSvgViewBox
            : icon.ViewBox!;
    }
}
