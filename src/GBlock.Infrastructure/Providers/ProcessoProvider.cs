using System.Diagnostics;
using GBlock.Domain.Interfaces;
using GBlock.Domain.Models;

namespace GBlock.Infrastructure.Providers;

public class ProcessoProvider : IProcessoProvider
{
    public IReadOnlyList<ProcessoInfo> ListarEmExecucao()
    {
        return Process.GetProcesses()
            .Where(TemJanela)
            .GroupBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ProcessoInfo(
                Nome: g.Key,
                Titulo: g.Select(TituloSeguro).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? string.Empty,
                Instancias: g.Count()))
            .OrderBy(p => p.Nome, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public int ContarInstancias(string nomeProcesso)
        => BuscarPorNome(nomeProcesso).Length;

    public int Encerrar(string nomeProcesso)
    {
        var encerrados = 0;

        foreach (var processo in BuscarPorNome(nomeProcesso))
        {
            try
            {
                processo.Kill(entireProcessTree: true);
                encerrados++;
            }
            catch (Exception)
            {
                // Processo ja saiu, ou e de outro usuario/elevado: nada a fazer.
            }
            finally
            {
                processo.Dispose();
            }
        }

        return encerrados;
    }

    private static Process[] BuscarPorNome(string nomeProcesso)
    {
        try
        {
            return Process.GetProcessesByName(RegraProcesso.Normalizar(nomeProcesso));
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static bool TemJanela(Process processo)
    {
        try
        {
            return processo.MainWindowHandle != IntPtr.Zero;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string TituloSeguro(Process processo)
    {
        try
        {
            return processo.MainWindowTitle;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
