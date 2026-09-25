using System.Windows;
using GBlock.App.ViewModels;
using GBlock.App.Views;
using GBlock.Domain.Enums;
using GBlock.Domain.Interfaces;
using GBlock.Domain.Models;
using GBlock.Infrastructure.Providers;
using GBlock.Infrastructure.Repositories;
using GBlock.Service.Monitoramento;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace GBlock.App;

public partial class App : Application
{
    private const string NomeMutex = "GBlock.InstanciaUnica";

    private Mutex? _instanciaUnica;
    private Forms.NotifyIcon? _bandeja;
    private MonitorService? _monitor;
    private PrincipalViewModel? _principalViewModel;
    private JanelaPrincipal? _janela;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _instanciaUnica = new Mutex(initiallyOwned: true, NomeMutex, out var novaInstancia);
        if (!novaInstancia)
        {
            MessageBox.Show("O GBlock ja esta em execucao (veja a bandeja do sistema).",
                "GBlock", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // Composicao manual: as camadas sao poucas e o grafo nao justifica um container.
        IRegraRepository regras = new RegraRepository();
        IUsoRepository usos = new UsoRepository();
        IProcessoProvider processos = new ProcessoProvider();
        IInicializacaoProvider inicializacao = new InicializacaoProvider();

        _monitor = new MonitorService(regras, usos, processos);
        _monitor.AvisoDeEncerramento += (_, estado) => NoDispatcher(() => Notificar(estado, TipoAviso.Aviso));
        _monitor.ProcessoEncerrado += (_, estado) => NoDispatcher(() => Notificar(estado, TipoAviso.Encerrado));
        _monitor.AberturaBloqueada += (_, estado) => NoDispatcher(() => Notificar(estado, TipoAviso.Bloqueado));
        _monitor.Iniciar();

        _principalViewModel = new PrincipalViewModel(regras, usos, processos, _monitor, inicializacao);

        MontarBandeja();

        var iniciarOculto = e.Args.Any(a => a.Equals("--tray", StringComparison.OrdinalIgnoreCase));
        if (!iniciarOculto)
            AbrirJanela();
    }

    private void MontarBandeja()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Abrir GBlock", null, (_, _) => NoDispatcher(AbrirJanela));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Sair", null, (_, _) => NoDispatcher(Encerrar));

        _bandeja = new Forms.NotifyIcon
        {
            Icon = IconeDaBandeja(),
            Text = "GBlock — controle de tempo",
            Visible = true,
            ContextMenuStrip = menu
        };

        _bandeja.DoubleClick += (_, _) => NoDispatcher(AbrirJanela);
    }

    private void AbrirJanela()
    {
        if (_principalViewModel is null)
            return;

        if (_janela is null)
        {
            _janela = new JanelaPrincipal(_principalViewModel);
            _janela.Closed += (_, _) => _janela = null;
        }

        _janela.Show();

        if (_janela.WindowState == WindowState.Minimized)
            _janela.WindowState = WindowState.Normal;

        _janela.Activate();
    }

    private void Notificar(EstadoRegra estado, TipoAviso tipo)
    {
        JanelaAviso.Mostrar(estado, tipo);

        _bandeja?.ShowBalloonTip(
            5000,
            "GBlock",
            tipo switch
            {
                _ when estado.Status == StatusRegra.ForaDoHorario && tipo != TipoAviso.Aviso =>
                    $"{estado.Regra.Nome}: passou do horario limite ({estado.Regra.HorarioLimite:HH:mm}).",
                TipoAviso.Aviso => $"{estado.Regra.Nome}: restam {RegraItemViewModel.Formatar(estado.Restante)}.",
                TipoAviso.Encerrado => $"{estado.Regra.Nome} foi encerrado — tempo do dia esgotado.",
                _ => $"{estado.Regra.Nome} esta bloqueado hoje."
            },
            Forms.ToolTipIcon.Warning);
    }

    private void Encerrar()
    {
        _monitor?.Parar();
        Shutdown();
    }

    private static void NoDispatcher(Action acao)
        => Current?.Dispatcher.Invoke(acao);

    /// <summary>O mesmo chinelo do executavel (Assets/gblock.ico), no tamanho pequeno da bandeja.</summary>
    private static Drawing.Icon IconeDaBandeja()
    {
        var recurso = GetResourceStream(new Uri("pack://application:,,,/Assets/gblock.ico"));
        using var fluxo = recurso!.Stream;
        return new Drawing.Icon(fluxo, Forms.SystemInformation.SmallIconSize);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _monitor?.Dispose();

        if (_bandeja is not null)
        {
            _bandeja.Visible = false;
            _bandeja.Dispose();
        }

        _instanciaUnica?.Dispose();
        base.OnExit(e);
    }
}
