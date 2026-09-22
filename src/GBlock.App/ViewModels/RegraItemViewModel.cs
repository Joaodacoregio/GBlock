using System.Globalization;
using GBlock.App.Base;
using GBlock.Domain.Enums;
using GBlock.Domain.Models;

namespace GBlock.App.ViewModels;

/// <summary>Linha da lista principal — reflete o ultimo estado publicado pelo monitor.</summary>
public class RegraItemViewModel : ObservableObject
{
    private string _nome = string.Empty;
    private string _nomeProcesso = string.Empty;
    private string _statusTexto = string.Empty;
    private string _corStatus = "#4B5563";
    private string _consumidoTexto = "00:00";
    private string _limiteTexto = "00:00";
    private string _restanteTexto = "00:00";
    private double _percentual;
    private bool _emExecucao;
    private bool _ativa = true;

    public RegraItemViewModel(EstadoRegra estado)
    {
        Id = estado.Regra.Id;
        Atualizar(estado);
    }

    public Guid Id { get; }

    public string Nome { get => _nome; private set => Definir(ref _nome, value); }
    public string NomeProcesso { get => _nomeProcesso; private set => Definir(ref _nomeProcesso, value); }
    public string StatusTexto { get => _statusTexto; private set => Definir(ref _statusTexto, value); }
    public string CorStatus { get => _corStatus; private set => Definir(ref _corStatus, value); }
    public string ConsumidoTexto { get => _consumidoTexto; private set => Definir(ref _consumidoTexto, value); }
    public string LimiteTexto { get => _limiteTexto; private set => Definir(ref _limiteTexto, value); }
    public string RestanteTexto { get => _restanteTexto; private set => Definir(ref _restanteTexto, value); }
    public double Percentual { get => _percentual; private set => Definir(ref _percentual, value); }
    public bool EmExecucao { get => _emExecucao; private set => Definir(ref _emExecucao, value); }
    public bool Ativa { get => _ativa; private set => Definir(ref _ativa, value); }

    public void Atualizar(EstadoRegra estado)
    {
        Nome = estado.Regra.Nome;
        NomeProcesso = estado.Regra.NomeProcesso;
        Ativa = estado.Regra.Ativa;
        EmExecucao = estado.EmExecucao;
        Consumido(estado);
        StatusTexto = Descrever(estado.Status, estado.EmExecucao);
        CorStatus = Cor(estado.Status);
    }

    private void Consumido(EstadoRegra estado)
    {
        ConsumidoTexto = Formatar(estado.Consumido);
        LimiteTexto = Formatar(estado.Limite);
        RestanteTexto = Formatar(estado.Restante);
        Percentual = estado.PercentualUsado;
    }

    public static string Formatar(TimeSpan tempo)
        => string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", (int)tempo.TotalHours, tempo.Minutes);

    private static string Descrever(StatusRegra status, bool emExecucao) => status switch
    {
        StatusRegra.DiaBloqueado => "Bloqueado hoje",
        StatusRegra.Bloqueado => "Tempo esgotado",
        StatusRegra.Aviso => emExecucao ? "Acabando" : "Pouco tempo",
        _ => emExecucao ? "Em execucao" : "Liberado"
    };

    private static string Cor(StatusRegra status) => status switch
    {
        StatusRegra.DiaBloqueado => "#DC2626",
        StatusRegra.Bloqueado => "#DC2626",
        StatusRegra.Aviso => "#D97706",
        _ => "#16A34A"
    };
}
