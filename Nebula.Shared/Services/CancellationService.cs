namespace Nebula.Shared.Services;

[ServiceRegister]
public class CancellationService
{
    private CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken Token => _cancellationTokenSource.Token;

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }
}