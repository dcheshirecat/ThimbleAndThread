"""Turns the written sheets into a font.

Input: handwriting_<style>_<n>.png (style = regular or bold, n = 1..3), exported from Procreate at the template's size
with ONLY the writing layer visible (transparent background), or on plain white.
Output: ThimbleHand-<Style>.ttf.

Run: python Tools/HandwritingFont/build_font.py <folder with the sheets> <output folder> [regular|bold]
Each box is traced as it was written: the ink is smoothed, its outline becomes the letter, and the letter keeps the
height it was written at against the guide lines (so a tall 'l' stays tall and a 'g' keeps its tail).
"""
import os, sys
import cv2
import numpy as np
from PIL import Image
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen
from layout import *

UPSCALE = 4            # trace at 4x for smooth outlines
SIDE_BEARING = 45      # space left and right of each letter, font units
SPACE_WIDTH = 260
MIN_INK_AREA = 12      # specks smaller than this (px at 1x) are dust, not ink


def ink_mask(sheet):
    """1 where there is writing: alpha if the sheet is transparent, else dark pixels."""
    rgba = np.array(sheet.convert("RGBA")).astype(np.float32)
    alpha = rgba[..., 3] / 255
    if alpha.min() > 0.99:   # exported on white: use darkness (guide lines are light and coloured, writing is not)
        gray = rgba[..., :3].mean(axis=2) / 255
        sat = rgba[..., :3].max(axis=2) - rgba[..., :3].min(axis=2)
        return np.clip((0.75 - gray) / 0.5, 0, 1) * (sat < 90)
    return alpha


def trace(mask, top):
    """Outlines of the ink in one box (mask starts "top" px below the box's top), as (outer, [(x, y)...]) in font
    units: y up, 0 = the baseline."""
    big = cv2.resize(mask, None, fx=UPSCALE, fy=UPSCALE, interpolation=cv2.INTER_CUBIC)
    big = cv2.GaussianBlur(big, (0, 0), UPSCALE * 0.6)
    binary = (big > 0.5).astype(np.uint8)
    contours, hierarchy = cv2.findContours(binary, cv2.RETR_CCOMP, cv2.CHAIN_APPROX_NONE)
    shapes = []
    for c, h in zip(contours, hierarchy[0] if hierarchy is not None else []):
        if cv2.contourArea(c) < MIN_INK_AREA * UPSCALE * UPSCALE:
            continue
        pts = c[:, 0, :].astype(np.float64)
        # smooth along the outline, then keep a point every ~1.5 px: the points become the control points of a
        # quadratic B-spline (TrueType's implied on-curve midpoints), which gives smooth curves.
        k = 5
        padded = np.concatenate([pts[-k:], pts, pts[:k]])
        kernel = np.ones(2 * k + 1) / (2 * k + 1)
        smooth = np.stack([np.convolve(padded[:, i], kernel, mode="valid") for i in range(2)], axis=1)
        simple = cv2.approxPolyDP(smooth.astype(np.float32).reshape(-1, 1, 2), UPSCALE * 0.35, True)[:, 0, :]
        if len(simple) < 3:
            continue
        units = [((x / UPSCALE) * SCALE, (BASELINE - top - y / UPSCALE) * SCALE) for x, y in simple]
        shapes.append((h[3] < 0, units))   # RETR_CCOMP: no parent = an outer outline, else a hole
    return shapes


def area(points):
    return 0.5 * sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in zip(points, points[1:] + points[:1]))


def glyph_from(shapes):
    xs = [x for _, s in shapes for x, _ in s]
    shift = SIDE_BEARING - min(xs)
    pen = TTGlyphPen(None)
    for outer, s in shapes:
        pts = [(round(x + shift), round(y)) for x, y in s]
        # TrueType: outer outlines clockwise (negative area with y up), holes anticlockwise.
        if (area(pts) > 0) == outer:
            pts.reverse()
        pen.qCurveTo(*pts, None)   # all off-curve points: a closed smooth curve
        pen.closePath()
    width = round(max(xs) - min(xs) + 2 * SIDE_BEARING)
    return pen.glyph(), width


def build(folder, out_folder, style):
    names, glyphs, widths, cmap, missing = [".notdef", "space"], {}, {}, {0x20: "space"}, []
    pen = TTGlyphPen(None)
    pen.moveTo((50, 0)); pen.lineTo((50, 700)); pen.lineTo((450, 700)); pen.lineTo((450, 0)); pen.closePath()
    glyphs[".notdef"], widths[".notdef"] = pen.glyph(), 500
    glyphs["space"], widths["space"] = TTGlyphPen(None).glyph(), SPACE_WIDTH
    for n, chars in enumerate(SHEETS, 1):
        path = os.path.join(folder, f"handwriting_{style}_{n}.png")
        if not os.path.exists(path):
            missing.append(f"sheet {n} ({len(chars)} characters)")
            continue
        sheet = Image.open(path)
        if sheet.size != (SHEET_W, SHEET_H):
            sheet = sheet.resize((SHEET_W, SHEET_H), Image.LANCZOS)
        mask = ink_mask(sheet)
        for i, ch in enumerate(chars):
            x, y = cell_origin(i)
            cell = mask[y + LABEL_H: y + CELL_H - 4, x + 4: x + CELL_W - 4]
            shapes = trace(cell, LABEL_H)
            if not shapes:
                missing.append(repr(ch))
                continue
            name = f"uni{codepoint(ch):04X}"
            glyphs[name], widths[name] = glyph_from(shapes)
            names.append(name)
            cmap[codepoint(ch)] = name
    if "uni00A0" not in glyphs:   # no-break space = space
        cmap[0xA0] = "space"

    family = "Thimble Hand"
    fb = FontBuilder(UNITS_PER_EM, isTTF=True)
    fb.setupGlyphOrder(names)
    fb.setupCharacterMap(cmap)
    fb.setupGlyf(glyphs)
    fb.setupHorizontalMetrics({g: (widths[g], 50 if g == ".notdef" else 0 if g == "space" else SIDE_BEARING)
                               for g in names})
    asc = round((BASELINE - LABEL_H) * SCALE)
    desc = -round((CELL_H - BASELINE) * SCALE)
    fb.setupHorizontalHeader(ascent=asc, descent=desc)
    fb.setupNameTable({"familyName": family, "styleName": style.capitalize()})
    bold = style == "bold"
    fb.setupOS2(sTypoAscender=asc, sTypoDescender=desc, usWinAscent=asc, usWinDescent=-desc,
                sxHeight=round((BASELINE - X_HEIGHT) * SCALE), sCapHeight=round((BASELINE - ASCENDER) * SCALE),
                usWeightClass=700 if bold else 400, fsSelection=0x20 if bold else 0x40)
    fb.setupPost()
    fb.font["head"].macStyle = 1 if bold else 0
    os.makedirs(out_folder, exist_ok=True)
    out = os.path.join(out_folder, f"ThimbleHand-{style.capitalize()}.ttf")
    fb.save(out)
    print(out, f"{len(names) - 2} characters")
    if missing:
        print("Not found (left out of the font):", ", ".join(missing))


if __name__ == "__main__":
    build(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else "regular")
