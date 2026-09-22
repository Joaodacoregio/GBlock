using System.Collections.ObjectModel;
using GBlock.App.Base;
using GBlock.Domain.Interfaces;
using GBlock.Domain.Models;

namespace GBlock.App.ViewModels;

public class RegraEditorViewModel : ObservableObject
{
    private static readonly DayOfWeek[] OrdemSemana =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    private readonly IProcessoProvider _processos;
    private readonly RegraProcesso _regra;

    private string _nome;
    private string _nomeProcesso;
    private int _minutosAviso;
    private bool _ativa;
    private ProcessoInfo? _processoSelecionado;
    private string _erro = string.Empty;

    public RegraEditorViewModel(RegraProcesso regra, IProcessoProvider processos)
    {
        _regra = regra;
        _processos = processos;

        _nome = regra.Nome;
        _nomeProcesso = regra.NomeProcesso;
        _minutosAviso = regra.MinutosAviso;
        _ativa = regra.Ativa;

        Dias = new ObservableCollection<DiaLimiteViewModel>(
            OrdemSemana.Select(d => new DiaLimiteViewModel(d, regra.LimiteMinutos(d))));

        AtualizarProcessosCommand = new RelayCommand(CarregarProcessos);
        AplicarATodosCommand = new RelayCommand(AplicarATodos);

        CarregarProcessos();
    }

    public ObservableCollection<DiaLimiteViewModel> Dias { get; }
    public ObservableCollection<ProcessoInfo> ProcessosDisponiveis { get; } = [];

    public RelayCommand AtualizarProcessosCommand { get; }
    public RelayCommand AplicarATodosCommand { get; }

    public string Titulo => string.IsNullOrWhiteSpace(_regra.Nome) ? "Nova regra" : $"Editar — {_regra.Nome}";

    public string Nome { get => _nome; set => Definir(ref _nome, value); }
    public string NomeProcesso { get => _nomeProcesso; set => Definir(ref _nomeProcesso, value); }
    public bool Ativa { get => _ativa; set => Definir(ref _ativa, value); }
    public string Erro { get => _erro; private set => Definir(ref _erro, value); }

    public int MinutosAviso
    {
        get => _minutosAviso;
        set => Definir(ref _minutosAviso, Math.Clamp(value, 0, 120));
    }

    /// <summary>Escolher um processo da lista preenche o executavel (e o nome, se estiver vazio).</summary>
    public ProcessoInfo? ProcessoSelecionado
    {
        get => _processoSelecionado;
        set
        {
            if (!Definir(ref _processoSelecionado, value) || value is null)
                return;

            NomeProcesso = value.Nome;

            if (string.IsNullOrWhiteSpace(Nome))
                Nome = string.IsNullOrWhiteSpace(value.Titulo) ? value.Nome : value.Titulo;
        }
    }

    public bool Validar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            Erro = "Informe um nome para a regra.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NomeProcesso))
        {
            Erro = "Escolha um processo da lista ou digite o nome do executavel.";
            return false;
        }

        if (Dias.All(d => d.TotalMinutos == 0))
        {
            Erro = "Libere tempo em pelo menos um dia da semana.";
            return false;
        }

        Erro = string.Empty;
        return true;
    }

    public RegraProcesso Aplicar()
    {
        _regra.Nome = Nome.Trim();
        _regra.NomeProcesso = RegraProcesso.Normalizar(NomeProcesso);
        _regra.MinutosAviso = MinutosAviso;
        _regra.Ativa = Ativa;

        foreach (var dia in Dias)
            _regra.DefinirLimite(dia.Dia, dia.TotalMinutos);

        return _regra;
    }

    private void CarregarProcessos()
    {
        var selecionado = NomeProcesso;

        ProcessosDisponiveis.Clear();
        foreach (var processo in _processos.ListarEmExecucao())
            ProcessosDisponiveis.Add(processo);

        _processoSelecionado = ProcessosDisponiveis
            .FirstOrDefault(p => string.Equals(p.Nome, RegraProcesso.Normalizar(selecionado), StringComparison.OrdinalIgnoreCase));
        OnPropertyChanged(nameof(ProcessoSelecionado));
    }

    private void AplicarATodos()
    {
        var referencia = Dias.FirstOrDefault(d => d.TotalMinutos > 0);
        if (referencia is null)
            return;

        foreach (var dia in Dias)
            dia.Aplicar(referencia.TotalMinutos);
    }
}
