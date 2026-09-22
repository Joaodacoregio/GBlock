using GBlock.Domain.Models;

namespace GBlock.Domain.Interfaces;

public interface IMonitorService
{
    /// <summary>Disparado a cada verificacao com o estado atual de todas as regras ativas.</summary>
    event EventHandler<IReadOnlyList<EstadoRegra>>? EstadoAtualizado;

    /// <summary>O tempo restante entrou na faixa de aviso configurada.</summary>
    event EventHandler<EstadoRegra>? AvisoDeEncerramento;

    /// <summary>O tempo acabou e o processo foi encerrado.</summary>
    event EventHandler<EstadoRegra>? ProcessoEncerrado;

    /// <summary>O processo tentou abrir com o tempo do dia ja esgotado.</summary>
    event EventHandler<EstadoRegra>? AberturaBloqueada;

    void Iniciar();
    void Parar();

    /// <summary>Forca uma verificacao imediata (usado apos salvar uma regra).</summary>
    void Verificar();

    IReadOnlyList<EstadoRegra> EstadoAtual();
}
