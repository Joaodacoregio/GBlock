namespace GBlock.Domain.Enums;

public enum StatusRegra
{
    /// <summary>Ainda existe tempo disponivel para hoje.</summary>
    Liberado,

    /// <summary>O tempo restante entrou na faixa de aviso.</summary>
    Aviso,

    /// <summary>O tempo do dia acabou, o processo deve ser encerrado/bloqueado.</summary>
    Bloqueado,

    /// <summary>O dia atual nao esta configurado (limite zero).</summary>
    DiaBloqueado
}
