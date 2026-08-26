param([Parameter(Mandatory = $true)][string]$SpiritRoot)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$drawingAssembly = [System.Drawing.Bitmap].Assembly.Location
Add-Type -ReferencedAssemblies $drawingAssembly -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public static class SpiritAuraRecolor
{
    public static void Process(string inputPath, string outputPath, string targetHex)
    {
        using (var source = new Bitmap(inputPath))
        using (var output = source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format32bppArgb))
        {
            var rect = new Rectangle(0, 0, output.Width, output.Height);
            var data = output.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var bytes = Math.Abs(data.Stride) * output.Height;
                var pixels = new byte[bytes];
                Marshal.Copy(data.Scan0, pixels, 0, bytes);
                var target = ColorTranslator.FromHtml(targetHex);
                double targetHue, targetSaturation, targetValue;
                RgbToHsv(target.R, target.G, target.B, out targetHue, out targetSaturation, out targetValue);

                for (var y = 0; y < output.Height; y++)
                {
                    var row = y * data.Stride;
                    for (var x = 0; x < output.Width; x++)
                    {
                        var i = row + x * 4;
                        var b = pixels[i];
                        var g = pixels[i + 1];
                        var r = pixels[i + 2];
                        var a = pixels[i + 3];
                        double hue, saturation, value;
                        RgbToHsv(r, g, b, out hue, out saturation, out value);
                        if (!IsAura(hue, saturation, a)) continue;

                        byte outR, outG, outB;
                        var outSaturation = saturation * targetSaturation;
                        var outValue = value * (1.0 - saturation * (1.0 - targetValue));
                        HsvToRgb(targetHue, outSaturation, outValue, out outR, out outG, out outB);
                        pixels[i] = outB;
                        pixels[i + 1] = outG;
                        pixels[i + 2] = outR;
                    }
                }

                Marshal.Copy(pixels, 0, data.Scan0, bytes);
            }
            finally { output.UnlockBits(data); }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            output.Save(outputPath, ImageFormat.Png);
        }
    }

    public static long Validate(string inputPath, string outputPath)
    {
        using (var source = new Bitmap(inputPath))
        using (var output = new Bitmap(outputPath))
        {
            if (source.Width != output.Width || source.Height != output.Height)
                throw new InvalidDataException("Canvas size changed: " + outputPath);

            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var outputData = output.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var bytes = Math.Abs(sourceData.Stride) * source.Height;
                var sourcePixels = new byte[bytes];
                var outputPixels = new byte[bytes];
                Marshal.Copy(sourceData.Scan0, sourcePixels, 0, bytes);
                Marshal.Copy(outputData.Scan0, outputPixels, 0, bytes);
                long changed = 0;

                for (var i = 0; i < bytes; i += 4)
                {
                    var sb = sourcePixels[i]; var sg = sourcePixels[i + 1];
                    var sr = sourcePixels[i + 2]; var sa = sourcePixels[i + 3];
                    var ob = outputPixels[i]; var og = outputPixels[i + 1];
                    var orr = outputPixels[i + 2]; var oa = outputPixels[i + 3];
                    if (sa != oa) throw new InvalidDataException("Alpha changed: " + outputPath);
                    if (sr == orr && sg == og && sb == ob) continue;

                    double hue, saturation, value;
                    RgbToHsv(sr, sg, sb, out hue, out saturation, out value);
                    if (!IsAura(hue, saturation, sa))
                        throw new InvalidDataException("Non-aura pixel changed: " + outputPath);
                    changed++;
                }

                if (changed == 0) throw new InvalidDataException("No aura pixels changed: " + outputPath);
                return changed;
            }
            finally
            {
                source.UnlockBits(sourceData);
                output.UnlockBits(outputData);
            }
        }
    }

    private static bool IsAura(double hue, double saturation, byte alpha)
    {
        return alpha != 0 && saturation >= 0.003 && hue >= 160.0 && hue <= 300.0;
    }

    private static void RgbToHsv(byte r, byte g, byte b, out double hue, out double saturation, out double value)
    {
        var rd = r / 255.0; var gd = g / 255.0; var bd = b / 255.0;
        var max = Math.Max(rd, Math.Max(gd, bd));
        var min = Math.Min(rd, Math.Min(gd, bd));
        var delta = max - min;
        value = max;
        saturation = max == 0.0 ? 0.0 : delta / max;
        if (delta == 0.0) { hue = 0.0; return; }
        if (max == rd) hue = 60.0 * (((gd - bd) / delta) % 6.0);
        else if (max == gd) hue = 60.0 * (((bd - rd) / delta) + 2.0);
        else hue = 60.0 * (((rd - gd) / delta) + 4.0);
        if (hue < 0.0) hue += 360.0;
    }

    private static void HsvToRgb(double hue, double saturation, double value, out byte r, out byte g, out byte b)
    {
        var c = value * saturation;
        var x = c * (1.0 - Math.Abs((hue / 60.0) % 2.0 - 1.0));
        var m = value - c;
        double rd, gd, bd;
        if (hue < 60.0) { rd = c; gd = x; bd = 0.0; }
        else if (hue < 120.0) { rd = x; gd = c; bd = 0.0; }
        else if (hue < 180.0) { rd = 0.0; gd = c; bd = x; }
        else if (hue < 240.0) { rd = 0.0; gd = x; bd = c; }
        else if (hue < 300.0) { rd = x; gd = 0.0; bd = c; }
        else { rd = c; gd = 0.0; bd = x; }
        r = (byte)Math.Round((rd + m) * 255.0);
        g = (byte)Math.Round((gd + m) * 255.0);
        b = (byte)Math.Round((bd + m) * 255.0);
    }
}
'@

$sourceDirectory = Join-Path $SpiritRoot 'A_Origon'
$palettes = [ordered]@{
    Red = '#FF5252'
    Blue = '#03A9F4'
    Green = '#AFB42B'
    Orange = '#E64A19'
    Black = '#212121'
    Violet = '#9C27B0'
    Yellow = '#FFC107'
}
$sources = @(Get-ChildItem -LiteralPath $sourceDirectory -File -Filter '*.png' | Sort-Object Name)
if ($sources.Count -ne 10) { throw "Expected 10 source PNGs, found $($sources.Count)." }

$changedPixels = [long]0
$validated = 0
foreach ($palette in $palettes.GetEnumerator()) {
    $matches = @(Get-ChildItem -LiteralPath $SpiritRoot -Directory | Where-Object Name -Like "$($palette.Key)_*")
    if ($matches.Count -ne 1) { throw "Expected one $($palette.Key)_* folder, found $($matches.Count)." }
    $outputDirectory = $matches[0].FullName
    foreach ($source in $sources) {
        $outputPath = Join-Path $outputDirectory $source.Name
        [SpiritAuraRecolor]::Process($source.FullName, $outputPath, $palette.Value)
        $changedPixels += [SpiritAuraRecolor]::Validate($source.FullName, $outputPath)
        $validated++
    }
}

Write-Output "Generated=$validated; Validated=$validated; Palettes=$($palettes.Count); Frames=$($sources.Count); ChangedAuraPixels=$changedPixels"
