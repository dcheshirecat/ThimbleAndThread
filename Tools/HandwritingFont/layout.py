"""The handwriting font's sheets: which character goes in which box, and where the guide lines are.
make_template.py draws the sheets from this; build_font.py reads the written sheets with it.
Never change the layout once sheets have been written on (the boxes must stay where they were drawn)."""

SHEET_W, SHEET_H = 2048, 2732          # Procreate canvas, portrait (same as the game's UI reference)
COLS, ROWS = 6, 7
MARGIN_X, HEADER = 124, 140            # left/right margin, instructions band at the top
CELL_W, CELL_H = 300, 370

# Guide lines, in pixels from the top of each box. Everything is measured against the baseline.
LABEL_H = 36        # the printed character name, top left (outside the writing area)
ASCENDER = 80       # top of tall letters (b d f h k l t) and capitals
X_HEIGHT = 170      # top of small letters (a c e m n o r s u v w x z)
BASELINE = 280      # where letters sit
DESCENDER = 345     # bottom of g j p q y

# Font units: the baseline-to-ascender distance (200 px) becomes 700 units of a 1000-unit em.
UNITS_PER_EM = 1000
SCALE = 700 / (BASELINE - ASCENDER)

# Symbols with no standard character get private code points; the game writes them as {coin} etc.
PRIVATE = {"coin": 0xE000, "needle": 0xE001, "spool": 0xE002, "button": 0xE003, "thimble": 0xE004,
           "yarn": 0xE005, "scissors": 0xE006}

SHEETS = [
    list("ABCDEFGHIJKLMNOPQRSTUVWXYZ") + list("0123456789") + list(".,!?'\""),
    list("abcdefghijklmnopqrstuvwxyz") + ["é"] + list(":;-()/&%+=<>@#$"),
    list("*[]\\_~|") + ["‘", "’", "“", "”", "…", "·", "×",
                        "♥", "●", "○", "★", "✿", "✓",
                        "←", "→", "↑", "↓"] + list(PRIVATE),
]

NAMES = {  # what the label says when the character itself is hard to read
    ".": "period", ",": "comma", "'": "apostrophe", '"': "quote", "-": "hyphen", "_": "underscore", "|": "bar",
    "‘": "open single quote", "’": "close single quote", "“": "open quote", "”": "close quote",
    "…": "ellipsis ...", "·": "middle dot", "×": "times", "♥": "heart", "●": "dot (filled)",
    "○": "dot (empty)", "★": "star", "✿": "flower", "✓": "tick", "←": "arrow left",
    "→": "arrow right", "↑": "arrow up", "↓": "arrow down", "\\": "backslash", "~": "tilde",
}


def cell_origin(index):
    """Top-left pixel of box number index on its sheet."""
    col, row = index % COLS, index // COLS
    return MARGIN_X + col * CELL_W, HEADER + row * CELL_H


def codepoint(ch):
    return PRIVATE[ch] if ch in PRIVATE else ord(ch)


def label(ch):
    if ch in PRIVATE:
        return ch
    if ch in NAMES:   # symbols by name only (the template's font may not have them)
        return NAMES[ch] if not ch.isascii() or len(NAMES[ch]) >= 12 else f"{ch}  {NAMES[ch]}"
    if ch.isalpha() and ch.isupper():
        return f"{ch}  (capital)"
    return ch
