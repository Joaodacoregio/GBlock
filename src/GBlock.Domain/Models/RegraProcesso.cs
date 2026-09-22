namespace GBlock.Domain.Models;

/// <summary>
/// Regra de uso de um processo: quanto tempo ele pode ficar aberto em cada dia da semana.
/// </summary>
public class RegraProcesso
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nome amigavel exibido na interface (ex.: "League of Legends").</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Nome do executavel, com ou sem ".exe" (ex.: "LeagueClient" ou "LeagueClient.exe").</summary>
    public string NomeProcesso { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    /// <summary>Minutos liberados por dia da semana. Dia ausente = zero minutos = bloqueado.</summary>
    public Dictionary<DayOfWeek, int> LimitesPorDia { get; set; } = new();

    /// <summary>Minutos restantes em que o aviso de encerramento deve aparecer.</summary>
    public int MinutosAviso { get; set; } = 5;

    public int LimiteMinutos(DayOfWeek dia)
        => LimitesPorDia.TryGetValue(dia, out var minutos) ? Math.Max(0, minutos) : 0;

    public void DefinirLimite(DayOfWeek dia, int minutos)
        => LimitesPorDia[dia] = Math.Max(0, minutos);

    /// <summary>Compara o nome do processo ignorando caixa e extensao.</summary>
    public bool Corresponde(string nomeProcesso)
        => string.Equals(Normalizar(NomeProcesso), Normalizar(nomeProcesso), StringComparison.OrdinalIgnoreCase);

    public static string Normalizar(string nome)
    {
        var limpo = (nome ?? string.Empty).Trim();
        return limpo.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? limpo[..^4]
            : limpo;
    }
}
