namespace GBlock.App.Views;

public enum TipoAviso
{
    /// <summary>O tempo esta acabando.</summary>
    Aviso,

    /// <summary>O tempo acabou e o processo foi fechado.</summary>
    Encerrado,

    /// <summary>Tentativa de abrir com o tempo do dia ja esgotado.</summary>
    Bloqueado
}
