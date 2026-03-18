using System.Threading;
using System.Threading.Tasks;

namespace HaloUI.Abstractions;

public interface IDialogContextProvider
{
    ValueTask<DialogContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default);
}