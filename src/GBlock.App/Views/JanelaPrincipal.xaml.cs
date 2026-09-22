using System.ComponentModel;
using System.Windows;
using GBlock.App.ViewModels;

namespace GBlock.App.Views;

public partial class JanelaPrincipal : Window
{
    private readonly PrincipalViewModel _viewModel;

    public JanelaPrincipal(PrincipalViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.AbrirEditor = AbrirEditor;
        _viewModel.Confirmar = Confirmar;

        DataContext = _viewModel;
    }

    private bool AbrirEditor(RegraEditorViewModel editor)
    {
        var janela = new JanelaRegra(editor) { Owner = this };
        return janela.ShowDialog() == true;
    }

    private bool Confirmar(string pergunta)
        => MessageBox.Show(this, pergunta, "GBlock", MessageBoxButton.YesNo, MessageBoxImage.Question)
           == MessageBoxResult.Yes;

    /// <summary>Fechar apenas esconde: o monitor precisa continuar rodando na bandeja.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
