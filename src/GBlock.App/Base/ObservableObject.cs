using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GBlock.App.Base;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propriedade = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propriedade));

    protected bool Definir<T>(ref T campo, T valor, [CallerMemberName] string? propriedade = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor))
            return false;

        campo = valor;
        OnPropertyChanged(propriedade);
        return true;
    }
}
