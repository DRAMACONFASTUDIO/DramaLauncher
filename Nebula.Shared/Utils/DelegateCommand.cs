using System.Windows.Input;

namespace Nebula.Shared.Utils;

public class DelegateCommand<T> : ICommand
{
    private readonly Action<T> _func;
    public readonly Ref<T> TRef = new();

    public DelegateCommand(Action<T> func)
    {
        _func = func;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _func(TRef.Value);
    }

    public event EventHandler? CanExecuteChanged;
}