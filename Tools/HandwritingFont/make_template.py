"""Draws the sheets to write the handwriting font on: Tools/HandwritingFont/handwriting_template_<n>.png.
Run: python Tools/HandwritingFont/make_template.py"""
import os
from PIL import Image, ImageDraw, ImageFont
from layout import *

HERE = os.path.dirname(os.path.abspath(__file__))
LINE = {"asc": (110, 170, 110), "x": (90, 140, 220), "base": (220, 80, 80), "desc": (170, 170, 170)}


def font(size):
    for name in ("arial.ttf", "DejaVuSans.ttf", "seguisym.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def dashed(d, x0, x1, y, color, dash=14, gap=8, width=3):
    x = x0
    while x < x1:
        d.line([(x, y), (min(x + dash, x1), y)], fill=color, width=width)
        x += dash + gap


def sheet(n, chars):
    img = Image.new("RGB", (SHEET_W, SHEET_H), (255, 255, 255))
    d = ImageDraw.Draw(img)
    d.text((MARGIN_X, 22), f"Handwriting font, sheet {n} of {len(SHEETS)}", fill=(60, 60, 60), font=font(44))
    d.text((MARGIN_X, 78), "Write each character on a new layer above this one. Sit letters on the red line; small letters reach "
           "the blue line, capitals and tall letters the green one; tails drop to the grey one.",
           fill=(110, 110, 110), font=font(22))
    labels, symbols = font(26), font(30)
    for i, ch in enumerate(chars):
        x, y = cell_origin(i)
        d.rectangle([x, y, x + CELL_W, y + CELL_H], outline=(200, 200, 200), width=2)
        dashed(d, x + 8, x + CELL_W - 8, y + ASCENDER, LINE["asc"])
        dashed(d, x + 8, x + CELL_W - 8, y + X_HEIGHT, LINE["x"])
        d.line([(x + 8, y + BASELINE), (x + CELL_W - 8, y + BASELINE)], fill=LINE["base"], width=3)
        dashed(d, x + 8, x + CELL_W - 8, y + DESCENDER, LINE["desc"])
        d.text((x + 10, y + 6), label(ch), fill=(150, 150, 150), font=symbols if ch not in PRIVATE and not ch.isascii() else labels)
    for i in range(len(chars), COLS * ROWS):   # unused boxes
        x, y = cell_origin(i)
        d.rectangle([x, y, x + CELL_W, y + CELL_H], fill=(246, 246, 246))
    path = os.path.join(HERE, f"handwriting_template_{n}.png")
    img.save(path)
    print(path)


if __name__ == "__main__":
    for n, chars in enumerate(SHEETS, 1):
        sheet(n, chars)
