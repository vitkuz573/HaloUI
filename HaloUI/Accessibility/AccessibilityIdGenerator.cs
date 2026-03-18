namespace HaloUI.Accessibility;

public static class AccessibilityIdGenerator
{
    public static string Create(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        
        return $"{prefix}-{Guid.NewGuid():N}";
    }
}