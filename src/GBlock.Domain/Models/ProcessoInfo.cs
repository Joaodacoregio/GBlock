namespace GBlock.Domain.Models;

/// <summary>Processo em execucao, usado na tela de selecao.</summary>
public record ProcessoInfo(string Nome, string Titulo, int Instancias)
{
    public string Exibicao => string.IsNullOrWhiteSpace(Titulo) ? Nome : $"{Nome}  —  {Titulo}";
}
