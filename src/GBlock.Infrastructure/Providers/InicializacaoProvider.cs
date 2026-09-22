using System.Diagnostics;
using Microsoft.Win32;
using GBlock.Domain.Interfaces;

namespace GBlock.Infrastructure.Providers;

/// <summary>
/// Auto-start via chave Run do usuario atual. Nao exige privilegio de administrador
/// e mantem o processo na sessao interativa, que e o que permite abrir janelas e bandeja.
/// </summary>
public class InicializacaoProvider : IInicializacaoProvider
{
    private const string Chave = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string Valor = "GBlock";

    public bool EstaHabilitado()
    {
        using var chave = Registry.CurrentUser.OpenSubKey(Chave);
        return chave?.GetValue(Valor) is not null;
    }

    public void Habilitar()
    {
        using var chave = Registry.CurrentUser.OpenSubKey(Chave, writable: true)
                          ?? Registry.CurrentUser.CreateSubKey(Chave);
        chave.SetValue(Valor, $"\"{CaminhoExecutavel()}\" --tray");
    }

    public void Desabilitar()
    {
        using var chave = Registry.CurrentUser.OpenSubKey(Chave, writable: true);
        chave?.DeleteValue(Valor, throwOnMissingValue: false);
    }

    private static string CaminhoExecutavel()
        => Process.GetCurrentProcess().MainModule?.FileName
           ?? Environment.ProcessPath
           ?? throw new InvalidOperationException("Nao foi possivel localizar o executavel do GBlock.");
}
