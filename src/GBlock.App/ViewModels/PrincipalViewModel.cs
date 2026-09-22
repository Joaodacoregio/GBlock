using System.Collections.ObjectModel;
using System.Windows;
using GBlock.App.Base;
using GBlock.Domain.Interfaces;
using GBlock.Domain.Models;

namespace GBlock.App.ViewModels;

public class PrincipalViewModel : ObservableObject
{
    private readonly IRegraRepository _regras;
    private readonly IUsoRepository _usos;
    private readonly IProcessoProvider _processos;
    private readonly IMonitorService _monitor;
    private readonly IInicializacaoProvider _inicializacao;

    private RegraItemViewModel? _selecionado;
    private bool _iniciarComWindows;

    public PrincipalViewModel(
        IRegraRepository regras,
        IUsoRepository usos,
        IProcessoProvider processos,
        IMonitorService monitor,
        IInicializacaoProvider inicializacao)
    {
        _regras = regras;
        _usos = usos;
        _processos = processos;
        _monitor = monitor;
        _inicializacao = inicializacao;
        _iniciarComWindows = inicializacao.EstaHabilitado();

        NovaRegraCommand = new RelayCommand(NovaRegra);
        EditarRegraCommand = new RelayCommand(EditarRegra, () => Selecionado is not null);
        RemoverRegraCommand = new RelayCommand(RemoverRegra, () => Selecionado is not null);
        ZerarUsoCommand = new RelayCommand(ZerarUso, () => Selecionado is not null);

        _monitor.EstadoAtualizado += (_, estados) => Application.Current?.Dispatcher.Invoke(() => Sincronizar(estados));
        Sincronizar(_monitor.EstadoAtual());
    }

    /// <summary>Preenchido pela janela: abre o editor e devolve true quando o usuario confirma.</summary>
    public Func<RegraEditorViewModel, bool>? AbrirEditor { get; set; }

    /// <summary>Preenchido pela janela: confirmacao simples (sim/nao).</summary>
    public Func<string, bool>? Confirmar { get; set; }

    public ObservableCollection<RegraItemViewModel> Itens { get; } = [];

    public RelayCommand NovaRegraCommand { get; }
    public RelayCommand EditarRegraCommand { get; }
    public RelayCommand RemoverRegraCommand { get; }
    public RelayCommand ZerarUsoCommand { get; }

    public RegraItemViewModel? Selecionado
    {
        get => _selecionado;
        set => Definir(ref _selecionado, value);
    }

    public bool IniciarComWindows
    {
        get => _iniciarComWindows;
        set
        {
            if (!Definir(ref _iniciarComWindows, value))
                return;

            if (value)
                _inicializacao.Habilitar();
            else
                _inicializacao.Desabilitar();
        }
    }

    public bool ListaVazia => Itens.Count == 0;

    private void NovaRegra()
    {
        var editor = new RegraEditorViewModel(new RegraProcesso(), _processos);
        if (AbrirEditor?.Invoke(editor) != true)
            return;

        _regras.Salvar(editor.Aplicar());
        _monitor.Verificar();
    }

    private void EditarRegra()
    {
        if (Selecionado is null || _regras.Obter(Selecionado.Id) is not { } regra)
            return;

        var editor = new RegraEditorViewModel(regra, _processos);
        if (AbrirEditor?.Invoke(editor) != true)
            return;

        _regras.Salvar(editor.Aplicar());
        _monitor.Verificar();
    }

    private void RemoverRegra()
    {
        if (Selecionado is null)
            return;

        if (Confirmar?.Invoke($"Remover a regra \"{Selecionado.Nome}\"?") != true)
            return;

        _regras.Remover(Selecionado.Id);
        Itens.Remove(Selecionado);
        Selecionado = null;
        OnPropertyChanged(nameof(ListaVazia));
        _monitor.Verificar();
    }

    /// <summary>Devolve o tempo do dia para a regra selecionada (libera de novo o processo).</summary>
    private void ZerarUso()
    {
        if (Selecionado is null)
            return;

        if (Confirmar?.Invoke($"Zerar o tempo ja usado hoje por \"{Selecionado.Nome}\"?") != true)
            return;

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var uso = _usos.ObterDia(hoje);
        uso.Zerar(Selecionado.Id);
        _usos.Salvar(uso);

        _monitor.Parar();
        _monitor.Iniciar();
    }

    private void Sincronizar(IReadOnlyList<EstadoRegra> estados)
    {
        foreach (var estado in estados)
        {
            var item = Itens.FirstOrDefault(i => i.Id == estado.Regra.Id);

            if (item is null)
                Itens.Add(new RegraItemViewModel(estado));
            else
                item.Atualizar(estado);
        }

        var ids = estados.Select(e => e.Regra.Id).ToHashSet();
        foreach (var removido in Itens.Where(i => !ids.Contains(i.Id)).ToList())
            Itens.Remove(removido);

        OnPropertyChanged(nameof(ListaVazia));
    }
}
