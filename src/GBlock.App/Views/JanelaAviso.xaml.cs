using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GBlock.App.ViewModels;
using GBlock.Domain.Models;


namespace GBlock.App.Views;

/// <summary>
/// Aviso flutuante no canto inferior direito. Fica sempre no topo (inclusive sobre jogos
/// em janela) e se fecha sozinho depois de alguns segundos.
/// </summary>
public partial class JanelaAviso : Window
{
    private static JanelaAviso? _aberta;

    private readonly DispatcherTimer _relogio = new() { Interval = TimeSpan.FromSeconds(1) };
    private int _segundosRestantes;

    private JanelaAviso(EstadoRegra estado, TipoAviso tipo)
    {
        InitializeComponent();

        _segundosRestantes = tipo == TipoAviso.Aviso ? 15 : 10;

        TituloTexto.Text = Titulo(tipo);
        MensagemTexto.Text = Mensagem(estado, tipo);
        Marcador.Background = new SolidColorBrush(Cor(tipo));

        _relogio.Tick += (_, _) =>
        {
            if (--_segundosRestantes <= 0)
            {
                Close();
                return;
            }

            ContagemTexto.Text = $"fecha em {_segundosRestantes}s";
        };
    }

    public static void Mostrar(EstadoRegra estado, TipoAviso tipo)
    {
        _aberta?.Close();

        var janela = new JanelaAviso(estado, tipo);
        _aberta = janela;
        janela.Closed += (_, _) =>
        {
            if (ReferenceEquals(_aberta, janela))
                _aberta = null;
        };

        janela.Posicionar();
        janela.Show();
        janela._relogio.Start();
    }

    private void Posicionar()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - Width - 16;
        Top = area.Bottom - Height - 16;
    }

    private static string Titulo(TipoAviso tipo) => tipo switch
    {
        TipoAviso.Aviso => "O tempo esta acabando",
        TipoAviso.Encerrado => "Tempo esgotado",
        _ => "Bloqueado por hoje"
    };

    private static string Mensagem(EstadoRegra estado, TipoAviso tipo) => tipo switch
    {
        TipoAviso.Aviso =>
            $"{estado.Regra.Nome} vai ser encerrado em {RegraItemViewModel.Formatar(estado.Restante)}. Salve o que estiver fazendo.",
        TipoAviso.Encerrado =>
            $"{estado.Regra.Nome} usou os {RegraItemViewModel.Formatar(estado.Limite)} liberados para hoje e foi fechado.",
        _ =>
            $"{estado.Regra.Nome} nao pode ser aberto: o tempo de hoje ja acabou. Libere de novo amanha."
    };

    private static Color Cor(TipoAviso tipo) => tipo switch
    {
        TipoAviso.Aviso => Color.FromRgb(0xD9, 0x77, 0x06),
        _ => Color.FromRgb(0xDC, 0x26, 0x26)
    };

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        _relogio.Stop();
        base.OnClosed(e);
    }
}
