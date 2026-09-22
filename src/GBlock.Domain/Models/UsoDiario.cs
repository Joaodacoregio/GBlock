namespace GBlock.Domain.Models;

/// <summary>Consumo acumulado de todas as regras em um dia.</summary>
public class UsoDiario
{
    public DateOnly Data { get; set; }

    /// <summary>Segundos ja consumidos por regra.</summary>
    public Dictionary<Guid, int> SegundosPorRegra { get; set; } = new();

    public int Segundos(Guid regraId)
        => SegundosPorRegra.TryGetValue(regraId, out var s) ? s : 0;

    public void Acumular(Guid regraId, int segundos)
        => SegundosPorRegra[regraId] = Segundos(regraId) + segundos;

    public void Zerar(Guid regraId)
        => SegundosPorRegra[regraId] = 0;
}
