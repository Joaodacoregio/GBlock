using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GBlock.App.Base;

/// <summary>
/// true =&gt; Visible. Passe "Invertido" como ConverterParameter para trocar o sentido.
/// </summary>
public class BoolParaVisibilidadeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var valor = value is true;

        if (string.Equals(parameter as string, "Invertido", StringComparison.OrdinalIgnoreCase))
            valor = !valor;

        return valor ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
