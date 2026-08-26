# Aura Color Variant Guide

## Purpose

Create color variants from the ten source PNG frames in this folder without changing their artwork, canvas, or animation sequence.

## Required Rules

1. Change only the original blue/cyan aura tint.
2. Do not redraw, crop, scale, rotate, move, blur, or reshape any part of an image.
3. Keep every output at exactly `1024 x 1536` pixels.
4. Preserve all pixel coordinates and alpha values.
5. Keep the central white energy, white particles, and non-aura areas unchanged.
6. Keep the original filename for the matching frame inside each color folder.
7. Use deterministic pixel recoloring, not generative image reconstruction.

## Palette

| Output folder | Aura color |
| --- | --- |
| `Red` | `#D32F2F` |
| `Brown` | `#5D4037` |
| `Green` | `#388E3C` |
| `Blue` | `#1976D2` |
| `Violet` | `#7B1FA2` |
| `Yellow` | `#FBC02D` |
| `Grad1` | Top `#D32F2F` to bottom `#FFEB3B` |
| `Grad2` | `#616161` |

## Recoloring Method

- Select only blue/cyan aura pixels from the source image.
- Remap their hue, saturation, and tint toward the assigned palette color.
- Preserve the original glow falloff and alpha channel.
- For `Grad1`, interpolate vertically from red at the top to yellow at the bottom.
- Treat `Grad2` as a solid neutral-gray tint because one color was specified.

## Validation Checklist

- Each output folder contains ten PNG frames.
- Every PNG is `1024 x 1536`.
- Output alpha values match the source frame exactly.
- Pixels outside the aura selection match the source exactly.
- No object, particle, edge, position, or frame order has changed.
- The central white energy remains white.
