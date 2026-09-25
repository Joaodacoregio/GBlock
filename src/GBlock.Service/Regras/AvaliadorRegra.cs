using GBlock.Domain.Enums;
using GBlock.Domain.Models;

namespace GBlock.Service.Regras;

/// <summary>Regras puras de decisao — sem acesso a processo, arquivo ou tela.</summary>
public static class AvaliadorRegra
{
    public static EstadoRegra Avaliar(RegraProcesso regra, DateTime agora, int segundosConsumidos, bool emExecucao)
    {
        var limiteMinutos = regra.LimiteMinutos(agora.DayOfWeek);
        var horario = TimeOnly.FromDateTime(agora);
        var limite = TimeSpan.FromMinutes(limiteMinutos);
        var consumido = TimeSpan.FromSeconds(segundosConsumidos);

        return new EstadoRegra
        {
            Regra = regra,
            Status = Status(regra, limiteMinutos, horario, consumido, limite),
            Consumido = consumido,
            Limite = limite,
            EmExecucao = emExecucao,
            AteHorarioLimite = regra.TempoAteHorarioLimite(horario)
        };
    }

    private static StatusRegra Status(RegraProcesso regra, int limiteMinutos, TimeOnly horario, TimeSpan consumido, TimeSpan limite)
    {
        if (limiteMinutos <= 0)
            return StatusRegra.DiaBloqueado;

        if (consumido >= limite)
            return StatusRegra.Bloqueado;

        if (regra.ForaDoHorario(horario))
            return StatusRegra.ForaDoHorario;

        var restante = limite - consumido;
        if (regra.TempoAteHorarioLimite(horario) is { } ateHorario && ateHorario < restante)
            restante = ateHorario;

        return restante <= TimeSpan.FromMinutes(Math.Max(0, regra.MinutosAviso))
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
