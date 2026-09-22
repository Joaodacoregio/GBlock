using GBlock.Domain.Interfaces;
using GBlock.Domain.Models;

namespace GBlock.Infrastructure.Repositories;

public class UsoRepository : IUsoRepository
{
    private const int DiasMantidos = 90;

    private readonly ArquivoJson<Dictionary<string, UsoDiario>> _arquivo = new("uso.json");

    public UsoDiario ObterDia(DateOnly data)
    {
        var historico = _arquivo.Carregar();
        return historico.TryGetValue(Chave(data), out var uso)
            ? uso
            : new UsoDiario { Data = data };
    }

    public void Salvar(UsoDiario uso)
    {
        var historico = _arquivo.Carregar();
        historico[Chave(uso.Data)] = uso;

        var limite = DateOnly.FromDateTime(DateTime.Today).AddDays(-DiasMantidos);
        foreach (var chave in historico.Keys.ToList())
        {
            if (DateOnly.TryParse(chave, out var data) && data < limite)
                historico.Remove(chave);
        }

        _arquivo.Gravar(historico);
    }

    private static string Chave(DateOnly data) => data.ToString("yyyy-MM-dd");
}
