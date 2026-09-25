using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GBlock.App.Base;

/// <summary>Deixa a barra de titulo nativa escura, combinando com o tema preto + verde.</summary>
public static class TemaJanela
{
    private const int DwmUsarModoEscuro = 20;
    private const int DwmUsarModoEscuroAntigo = 19; // Windows 10 anterior ao 20H1

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr janela, int atributo, ref int valor, int tamanho);

    public static void AplicarEscuro(Window janela)
        => janela.SourceInitialized += (_, _) =>
        {
            var handle = new WindowInteropHelper(janela).Handle;
            var ligado = 1;

            if (DwmSetWindowAttribute(handle, DwmUsarModoEscuro, ref ligado, sizeof(int)) != 0)
                DwmSetWindowAttribute(handle, DwmUsarModoEscuroAntigo, ref ligado, sizeof(int));
        };
}
