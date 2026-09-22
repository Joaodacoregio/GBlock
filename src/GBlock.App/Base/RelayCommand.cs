using System.Windows.Input;

namespace GBlock.App.Base;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _executar;
    private readonly Func<object?, bool>? _podeExecutar;

    public RelayCommand(Action<object?> executar, Func<object?, bool>? podeExecutar = null)
    {
        _executar = executar;
        _podeExecutar = podeExecutar;
    }

    public RelayCommand(Action executar, Func<bool>? podeExecutar = null)
        : this(_ => executar(), podeExecutar is null ? null : _ => podeExecutar())
    {
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _podeExecutar?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _executar(parameter);
}
