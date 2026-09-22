namespace GBlock.Domain.Interfaces;

/// <summary>Controla o auto-start do GBlock junto com o Windows.</summary>
public interface IInicializacaoProvider
{
    bool EstaHabilitado();
    void Habilitar();
    void Desabilitar();
}
