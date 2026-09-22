using GBlock.Domain.Models;

namespace GBlock.Domain.Interfaces;

public interface IUsoRepository
{
    /// <summary>Carrega o uso do dia informado, criando um registro vazio quando nao existir.</summary>
    UsoDiario ObterDia(DateOnly data);

    void Salvar(UsoDiario uso);
}
