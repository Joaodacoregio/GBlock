# Gera src/GBlock.App/Assets/gblock.ico (um chinelo verde e preto) em varios tamanhos.
# Uso: powershell -ExecutionPolicy Bypass -File tools/GerarIcone.ps1

Add-Type -AssemblyName System.Drawing

$destino = Join-Path $PSScriptRoot '..\src\GBlock.App\Assets\gblock.ico'
New-Item -ItemType Directory -Force (Split-Path $destino) | Out-Null

function Desenhar([int]$tamanho) {
    $bmp = New-Object System.Drawing.Bitmap $tamanho, $tamanho, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    # Desenho feito numa grade de 256 e inclinado, para lembrar um chinelo "jogado".
    $escala = $tamanho / 256.0
    $g.ScaleTransform($escala, $escala)
    $g.TranslateTransform(128, 128)
    $g.RotateTransform(28)
    $g.TranslateTransform(-128, -129)

    # Sola: bico largo, cintura fina, calcanhar arredondado.
    $sola = New-Object System.Drawing.Drawing2D.GraphicsPath
    $sola.AddBezier(128, 16, 164, 16, 186, 44, 186, 82)
    $sola.AddBezier(186, 82, 186, 114, 168, 130, 170, 152)
    $sola.AddBezier(170, 152, 172, 170, 182, 184, 180, 204)
    $sola.AddBezier(180, 204, 178, 228, 156, 242, 128, 242)
    $sola.AddBezier(128, 242, 100, 242, 78, 228, 76, 204)
    $sola.AddBezier(76, 204, 74, 184, 84, 170, 86, 152)
    $sola.AddBezier(86, 152, 88, 130, 70, 114, 70, 82)
    $sola.AddBezier(70, 82, 70, 44, 92, 16, 128, 16)
    $sola.CloseFigure()

    $gradiente = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.PointF 70, 16),
        (New-Object System.Drawing.PointF 186, 242),
        [System.Drawing.ColorTranslator]::FromHtml('#4ADE80'),
        [System.Drawing.ColorTranslator]::FromHtml('#15803D'))
    $g.FillPath($gradiente, $sola)

    $contorno = New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#052E16')), 10
    $contorno.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPath($contorno, $sola)

    # Tiras em V saindo do pino entre os dedos.
    # Em tamanhos pequenos a tira engrossa e o brilho some, senao vira borrao.
    $pequeno = $tamanho -le 32
    $larguraTira = if ($pequeno) { 36 } else { 24 }
    $tira = New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#0A0A0A')), $larguraTira
    $tira.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $tira.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawBezier($tira, 128, 66, 148, 80, 170, 104, 178, 142)
    $g.DrawBezier($tira, 128, 66, 108, 80, 86, 104, 78, 142)

    $brilho = New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#22C55E')), 6
    $brilho.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $brilho.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    if (-not $pequeno) {
        $g.DrawBezier($brilho, 132, 70, 150, 84, 168, 106, 174, 136)
        $g.DrawBezier($brilho, 124, 70, 106, 84, 88, 106, 82, 136)
    }

    $pino = New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml('#0A0A0A'))
    $g.FillEllipse($pino, 112, 50, 32, 32)

    $g.Dispose()
    return $bmp
}

# Entradas pequenas em DIB (compatibilidade maxima); 256 em PNG.
function EmDib([System.Drawing.Bitmap]$bmp) {
    $t = $bmp.Width
    $ms = New-Object System.IO.MemoryStream
    $w = New-Object System.IO.BinaryWriter $ms
    $w.Write([int]40); $w.Write([int]$t); $w.Write([int]($t * 2))
    $w.Write([int16]1); $w.Write([int16]32); $w.Write([int]0)
    $w.Write([int]0); $w.Write([int]0); $w.Write([int]0); $w.Write([int]0); $w.Write([int]0)
    for ($y = $t - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $t; $x++) {
            $c = $bmp.GetPixel($x, $y)
            $w.Write([byte]$c.B); $w.Write([byte]$c.G); $w.Write([byte]$c.R); $w.Write([byte]$c.A)
        }
    }
    $linhaMascara = [int]([math]::Ceiling($t / 32.0) * 4)
    $mascara = New-Object byte[] ($linhaMascara * $t)
    $w.Write($mascara, 0, $mascara.Length)
    $w.Flush()
    return ,$ms.ToArray()
}

$tamanhos = 16, 20, 24, 32, 40, 48, 64, 256
$imagens = foreach ($t in $tamanhos) {
    $bmp = Desenhar $t
    if ($t -eq 256) {
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        , $ms.ToArray()
    } else {
        , (EmDib $bmp)
    }
    $bmp.Dispose()
}

$saida = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $saida
$w.Write([int16]0); $w.Write([int16]1); $w.Write([int16]$tamanhos.Count)

$offset = 6 + 16 * $tamanhos.Count
for ($i = 0; $i -lt $tamanhos.Count; $i++) {
    $t = $tamanhos[$i]
    $lado = if ($t -ge 256) { 0 } else { $t }
    $w.Write([byte]$lado); $w.Write([byte]$lado); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([int16]1); $w.Write([int16]32)
    $w.Write([int]$imagens[$i].Length); $w.Write([int]$offset)
    $offset += $imagens[$i].Length
}
foreach ($img in $imagens) { $w.Write([byte[]]$img, 0, $img.Length) }
$w.Flush()

[System.IO.File]::WriteAllBytes((Resolve-Path (Split-Path $destino)).Path + '\gblock.ico', $saida.ToArray())
Write-Host "Icone gerado em $destino"
