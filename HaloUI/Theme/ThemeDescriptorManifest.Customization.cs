using HaloUI.Theme;

namespace HaloUI.Theme.Sdk.Runtime;

public static partial class ThemeDescriptorManifest
{
    static partial void OnDescriptorCreated(ref ThemeDescriptor descriptor)
    {
        descriptor = descriptor.Kind switch
        {
            ThemeDescriptorKind.Base => ApplyBaseIcon(descriptor),
            _ => descriptor
        };
    }

    static partial void OnGroupCreated(ref ThemeGroupDescriptor group)
    {
        if (group.Kind == ThemeDescriptorKind.Brand && group.Icon == HaloThemeIcons.Palette)
        {
            group = group with { Icon = HaloThemeIcons.Palette };
        }
    }

    private static ThemeDescriptor ApplyBaseIcon(ThemeDescriptor descriptor)
    {
        var icon = descriptor.Key switch
        {
            "DarkGlass" => HaloThemeIcons.NightlightRound,
            "Light" => HaloThemeIcons.WbSunny,
            _ => descriptor.Icon
        };

        return descriptor with { Icon = icon };
    }
}
