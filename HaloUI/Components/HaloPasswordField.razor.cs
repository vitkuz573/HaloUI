using Microsoft.AspNetCore.Components;

namespace HaloUI.Components;

public partial class HaloPasswordField
{
    [Parameter] public string? Label { get; set; }

    private bool _showPassword;

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }
}
