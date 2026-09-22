using GBlock.Domain.Models;

namespace GBlock.Domain.Interfaces;

public interface IProcessoProvider
{
    /// <summary>Processos visiveis para o usuario, agrupados por nome de executavel.</summary>
    IReadOnlyList<ProcessoInfo> ListarEmExecucao();

    /// <summary>Quantidade de instancias em execucao do processo informado.</summary>
    int ContarInstancias(string nomeProcesso);

    /// <summary>Encerra todas as instancias do processo. Retorna quantas foram encerradas.</summary>
    int Encerrar(string nomeProcesso);
}
