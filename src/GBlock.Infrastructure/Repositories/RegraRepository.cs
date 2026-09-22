using GBlock.Domain.Interfaces;
using GBlock.Domain.Models;

namespace GBlock.Infrastructure.Repositories;

public class RegraRepository : IRegraRepository
{
    private readonly ArquivoJson<List<RegraProcesso>> _arquivo = new("regras.json");

    public IReadOnlyList<RegraProcesso> Listar()
        => _arquivo.Carregar().OrderBy(r => r.Nome).ToList();

    public RegraProcesso? Obter(Guid id)
        => _arquivo.Carregar().FirstOrDefault(r => r.Id == id);

    public void Salvar(RegraProcesso regra)
    {
        var regras = _arquivo.Carregar();
        var indice = regras.FindIndex(r => r.Id == regra.Id);

        if (indice >= 0)
            regras[indice] = regra;
        else
            regras.Add(regra);

        _arquivo.Gravar(regras);
    }

    public void Remover(Guid id)
    {
        var regras = _arquivo.Carregar();
        regras.RemoveAll(r => r.Id == id);
        _arquivo.Gravar(regras);
    }
}
