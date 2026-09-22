using System.Text.Json;
using System.Text.Json.Serialization;

namespace GBlock.Infrastructure.Repositories;

/// <summary>
/// Persistencia simples em JSON dentro de %AppData%\GBlock. Serializada por um lock
/// porque o monitor (background) e a interface gravam no mesmo arquivo.
/// </summary>
public class ArquivoJson<T> where T : new()
{
    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _caminho;
    private readonly object _trava = new();

    public ArquivoJson(string nomeArquivo)
    {
        var pasta = PastaDados;
        Directory.CreateDirectory(pasta);
        _caminho = Path.Combine(pasta, nomeArquivo);
    }

    public static string PastaDados => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GBlock");

    public T Carregar()
    {
        lock (_trava)
        {
            if (!File.Exists(_caminho))
                return new T();

            try
            {
                var json = File.ReadAllText(_caminho);
                return string.IsNullOrWhiteSpace(json)
                    ? new T()
                    : JsonSerializer.Deserialize<T>(json, Opcoes) ?? new T();
            }
            catch (JsonException)
            {
                // Arquivo corrompido: preserva para diagnostico e recomeca limpo.
                File.Move(_caminho, _caminho + ".bak", overwrite: true);
                return new T();
            }
        }
    }

    public void Gravar(T dados)
    {
        lock (_trava)
        {
            var temporario = _caminho + ".tmp";
            File.WriteAllText(temporario, JsonSerializer.Serialize(dados, Opcoes));
            File.Move(temporario, _caminho, overwrite: true);
        }
    }
}
