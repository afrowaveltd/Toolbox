# From canonical Assets/icon.png to multi-resolution Windows .ico.
# Runs only during a Windows-hosted build and writes under obj/.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Source,
    [Parameter(Mandatory = $true)][string] $Destination
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$sourcePath = [System.IO.Path]::GetFullPath($Source)
$destinationPath = [System.IO.Path]::GetFullPath($Destination)
if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "Canonical PNG icon missing: $sourcePath"
}
[System.IO.Directory]::CreateDirectory(
    [System.IO.Path]::GetDirectoryName($destinationPath)) | Out-Null

$image = $null
$frames = New-Object 'System.Collections.Generic.List[object]'
try {
    $image = [System.Drawing.Image]::FromFile($sourcePath)
    foreach ($size in @(16, 24, 32, 48, 64, 128, 256)) {
        $bitmap = $null
        $graphics = $null
        $buffer = $null
        try {
            $bitmap = [System.Drawing.Bitmap]::new(
                $size, $size,
                [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode =
                [System.Drawing.Drawing2D.CompositingMode]::SourceOver
            $graphics.CompositingQuality =
                [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode =
                [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode =
                [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.DrawImage(
                $image, [System.Drawing.Rectangle]::new(0, 0, $size, $size))
            $buffer = [System.IO.MemoryStream]::new()
            $bitmap.Save($buffer, [System.Drawing.Imaging.ImageFormat]::Png)
            $frames.Add([pscustomobject]@{
                Size = $size
                Bytes = $buffer.ToArray()
            })
        }
        finally {
            if ($null -ne $buffer) { $buffer.Dispose() }
            if ($null -ne $graphics) { $graphics.Dispose() }
            if ($null -ne $bitmap) { $bitmap.Dispose() }
        }
    }
}
finally {
    if ($null -ne $image) { $image.Dispose() }
}
# ICO header + one 16-byte directory entry per embedded PNG frame.
# In ICO, zero represents 256 pixels in a one-byte size field.
$stream = [System.IO.File]::Create($destinationPath)
$writer = [System.IO.BinaryWriter]::new($stream)
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$frames.Count)
    $offset = [uint32](6 + 16 * $frames.Count)
    foreach ($frame in $frames) {
        $dimension = [byte]($frame.Size % 256)
        $writer.Write($dimension)
        $writer.Write($dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$frame.Bytes.Length)
        $writer.Write([uint32]$offset)
        $offset += [uint32]$frame.Bytes.Length
    }
    foreach ($frame in $frames) {
        $writer.Write([byte[]]$frame.Bytes)
    }
}
finally {
    $writer.Dispose()
}
Write-Host "Generated Windows icon with $($frames.Count) sizes: $destinationPath"
