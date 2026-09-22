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

    public TimeSpan Restante => Limite > Consumido ? Limite - Consumido : TimeSpan.Zero;

    public double PercentualUsado => Limite.TotalSeconds <= 0
        ? 100d
        : Math.Min(100d, Consumido.TotalSeconds / Limite.TotalSeconds * 100d);
}
