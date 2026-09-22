using System.Windows;
using GBlock.App.ViewModels;

namespace GBlock.App.Views;

public partial class JanelaRegra : Window
{
    private readonly RegraEditorViewModel _viewModel;

    public JanelaRegra(RegraEditorViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.Validar())
            return;

        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
