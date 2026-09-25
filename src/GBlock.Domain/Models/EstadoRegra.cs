using GBlock.Domain.Enums;

namespace GBlock.Domain.Models;

/// <summary>Fotografia do estado de uma regra no instante da verificacao.</summary>
public class EstadoRegra
{
    public required RegraProcesso Regra { get; init; }
    public StatusRegra Status { get; init; }
    public TimeSpan Consumido { get; init; }
    public TimeSpan Limite { get; init; }
    public bool EmExecucao { get; init; }

    /// <summary>Quanto falta para a trava de horario; null quando a regra permite jogar depois do horario.</summary>
    public TimeSpan? AteHorarioLimite { get; init; }

    /// <summary>Saldo efetivo: o menor entre o tempo do dia e o tempo ate o horario limite.</summary>
    public TimeSpan Restante
    {
        get
        {
            var saldo = Limite > Consumido ? Limite - Consumido : TimeSpan.Zero;
            return AteHorarioLimite is { } ate && ate < saldo ? ate : saldo;
        }
    }

    public double PercentualUsado => Limite.TotalSeconds <= 0
        ? 100d
        : Math.Min(100d, Consumido.TotalSeconds / Limite.TotalSeconds * 100d);
}
