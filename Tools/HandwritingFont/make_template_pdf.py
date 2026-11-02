"""The three template sheets as one small all-text PDF (handwriting_templates.pdf), same layout as the PNGs:
one page per sheet, 2048 x 2732 points, so Procreate imports each page at 2048 x 2732.
Run: python Tools/HandwritingFont/make_template_pdf.py"""
import os
from layout import *

HERE = os.path.dirname(os.path.abspath(__file__))


def esc(s):
    out = ""
    for ch in s:
        if ch in "()\\":
            out += "\\" + ch
        elif ord(ch) < 128:
            out += ch
        else:
            out += "\\%03o" % ch.encode("cp1252")[0]
    return out


def cell_form():
    """One box with its guide lines, drawn at the origin (PDF y goes up, so lines are measured from the box top)."""
    def y(px):
        return CELL_H - px
    ops = ["0.78 G 2 w 0 0 %d %d re S" % (CELL_W, CELL_H)]
    for px, rgb, dash in ((ASCENDER, "0.43 0.67 0.43", True), (X_HEIGHT, "0.35 0.55 0.86", True),
                          (BASELINE, "0.86 0.31 0.31", False), (DESCENDER, "0.67 0.67 0.67", True)):
        ops.append("%s RG 3 w %s 8 %d m %d %d l S" % (rgb, "[14 8] 0 d" if dash else "[] 0 d", y(px), CELL_W - 8, y(px)))
    return "\n".join(ops)


def page(n, chars):
    ops = ["BT /F1 44 Tf 0.24 g %d %d Td (%s) Tj ET" % (MARGIN_X, SHEET_H - 66, esc(f"Handwriting font, sheet {n} of {len(SHEETS)}")),
           "BT /F1 22 Tf 0.43 g %d %d Td (%s) Tj ET" % (MARGIN_X, SHEET_H - 100, esc(
               "Write each character on a new layer above this one. Sit letters on the red line; small letters reach the "
               "blue line, capitals and tall letters the green one; tails drop to the grey one."))]
    for i in range(COLS * ROWS):
        x, top = cell_origin(i)
        bottom = SHEET_H - top - CELL_H
        if i < len(chars):
            ops.append("q 1 0 0 1 %d %d cm /Cell Do Q" % (x, bottom))
            ops.append("BT /F1 26 Tf 0.59 g %d %d Td (%s) Tj ET" % (x + 10, SHEET_H - top - 30, esc(label(chars[i]))))
        else:
            ops.append("0.965 g %d %d %d %d re f" % (x, bottom, CELL_W, CELL_H))
    return "\n".join(ops)


def build():
    objs = {}
    form = cell_form()
    objs[3] = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"
    objs[4] = "<< /Type /XObject /Subtype /Form /BBox [0 0 %d %d] /Length %d >>\nstream\n%s\nendstream" % (
        CELL_W, CELL_H, len(form), form)
    kids = []
    for n, chars in enumerate(SHEETS, 1):
        content = page(n, chars)
        pid, cid = 3 + 2 * n, 4 + 2 * n
        objs[cid] = "<< /Length %d >>\nstream\n%s\nendstream" % (len(content.encode("latin-1")), content)
        objs[pid] = ("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 %d %d] /Contents %d 0 R "
                     "/Resources << /Font << /F1 3 0 R >> /XObject << /Cell 4 0 R >> >> >>" % (SHEET_W, SHEET_H, cid))
        kids.append("%d 0 R" % pid)
    objs[1] = "<< /Type /Catalog /Pages 2 0 R >>"
    objs[2] = "<< /Type /Pages /Kids [%s] /Count %d >>" % (" ".join(kids), len(kids))
    out, offsets = "%PDF-1.4\n", {}
    for k in sorted(objs):
        offsets[k] = len(out.encode("latin-1"))
        out += "%d 0 obj\n%s\nendobj\n" % (k, objs[k])
    xref = len(out.encode("latin-1"))
    out += "xref\n0 %d\n0000000000 65535 f \n" % (len(objs) + 1)
    for k in sorted(objs):
        out += "%010d 00000 n \n" % offsets[k]
    out += "trailer\n<< /Size %d /Root 1 0 R >>\nstartxref\n%d\n%%%%EOF\n" % (len(objs) + 1, xref)
    path = os.path.join(HERE, "handwriting_templates.pdf")
    open(path, "w", encoding="latin-1", newline="\n").write(out)
    print(path, len(out), "bytes")


if __name__ == "__main__":
    build()
