using System.Windows;
using GBlock.App.Base;
using GBlock.App.ViewModels;

namespace GBlock.App.Views;

public partial class JanelaRegra : Window
{
    private readonly RegraEditorViewModel _viewModel;

    public JanelaRegra(RegraEditorViewModel viewModel)
    {
        InitializeComponent();
        TemaJanela.AplicarEscuro(this);

        // Em telas baixas a janela nao passa da area util; o conteudo rola.
        MaxHeight = SystemParameters.WorkArea.Height;

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
