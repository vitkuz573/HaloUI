using Microsoft.AspNetCore.Components;

namespace HaloUI.Components;

public partial class HaloSelectOption<TValue>
{
    [CascadingParameter]
    internal HaloSelect<TValue>? Owner { get; set; }

    [Parameter]
    public TValue? Value { get; set; }
    
    [Parameter]
    public string? Text { get; set; }
    
    [Parameter]
    public bool Disabled { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool _isRegistered;
    private bool _pendingRegister;

    protected override void OnParametersSet()
    {
        if (Owner is null)
        {
            _pendingRegister = true;

            return;
        }

        EnsureRegistered();

        Owner.UpdateOption(this);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!_pendingRegister || Owner is null || _isRegistered)
        {
            return;
        }

        EnsureRegistered();

        Owner.UpdateOption(this);

        _pendingRegister = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(disposing);
            return;
        }

        if (_isRegistered && Owner is not null)
        {
            Owner.UnregisterOption(this);

            _isRegistered = false;
        }

        base.Dispose(disposing);
    }

    private void EnsureRegistered()
    {
        if (_isRegistered || Owner is null)
        {
            return;
        }

        Owner.RegisterOption(this);

        _isRegistered = true;
        _pendingRegister = false;
    }
}
