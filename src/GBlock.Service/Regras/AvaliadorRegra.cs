using GBlock.Domain.Enums;
using GBlock.Domain.Models;

namespace GBlock.Service.Regras;

/// <summary>Regras puras de decisao — sem acesso a processo, arquivo ou tela.</summary>
public static class AvaliadorRegra
{
    public static EstadoRegra Avaliar(RegraProcesso regra, DayOfWeek dia, int segundosConsumidos, bool emExecucao)
    {
        var limiteMinutos = regra.LimiteMinutos(dia);
        var limite = TimeSpan.FromMinutes(limiteMinutos);
        var consumido = TimeSpan.FromSeconds(segundosConsumidos);

        var status = Status(limiteMinutos, consumido, limite, regra.MinutosAviso);

        return new EstadoRegra
        {
            Regra = regra,
            Status = status,
            Consumido = consumido,
            Limite = limite,
            EmExecucao = emExecucao
        };
    }

    private static StatusRegra Status(int limiteMinutos, TimeSpan consumido, TimeSpan limite, int minutosAviso)
    {
        if (limiteMinutos <= 0)
            return StatusRegra.DiaBloqueado;

        if (consumido >= limite)
            return StatusRegra.Bloqueado;

        var restante = limite - consumido;
        return restante <= TimeSpan.FromMinutes(Math.Max(0, minutosAviso))
            ? StatusRegra.Aviso
            : StatusRegra.Liberado;
    }

    /// <summary>Marcos de aviso (em minutos restantes) que devem gerar notificacao uma unica vez.</summary>
    public static IEnumerable<int> MarcosDeAviso(RegraProcesso regra)
    {
        var marcos = new List<int>();

        if (regra.MinutosAviso > 0)
            marcos.Add(regra.MinutosAviso);

        foreach (var marco in new[] { 5, 1 })
        {
            if (marco < regra.MinutosAviso && !marcos.Contains(marco))
                marcos.Add(marco);
        }

        return marcos.OrderByDescending(m => m);
    }
}
