using GBlock.Domain.Models;

namespace GBlock.Domain.Interfaces;

public interface IRegraRepository
{
    IReadOnlyList<RegraProcesso> Listar();
    RegraProcesso? Obter(Guid id);
    void Salvar(RegraProcesso regra);
    void Remover(Guid id);
}
