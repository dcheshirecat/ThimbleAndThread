# Your handwriting font

The game's text (buttons that aren't drawn, headings, dialogue, the journal, lists) will be written in your
handwriting. You write each character once on three template sheets; Claude traces them into a real font
("Thimble Hand") and puts it in the game. You write the set twice: once normal, once bold.

## What to write
Three sheets, 115 characters in all. Each box says what goes in it.
1. Capitals A-Z, digits 0-9, `. , ! ? ' "`
2. Small letters a-z, `é`, `: ; - ( ) / & % + = < > @ # $`
3. `* [ ] \ _ ~ |`, curly quotes ‘ ’ “ ”, ellipsis …, middle dot ·, times ×, heart, filled and empty dots, star,
   flower, tick, four arrows, and seven little drawings: coin, needle, spool, button, thimble, yarn, scissors.

The templates: **`handwriting_templates.pdf` in the Drive folder's `Guidelines`** (one page per sheet), or the PNGs
`Tools/HandwritingFont/handwriting_template_1.png`, `_2.png` and `_3.png` on GitHub.

## In Procreate
1. **New canvas 2048 x 2732 px, sRGB.**
2. **Insert the template** as the bottom layer (Actions > Add > Insert a file and pick the PDF's page, or Insert a
   photo for a PNG), make it fill the canvas exactly, and lock it. Don't move or resize it after that.
3. **Add a new layer above it** and write in that layer. Use the brush you want the game's text to look like.
   - Sit every letter on the **red** line.
   - Small letters (a, c, e, m, n, o...) reach the **blue** line.
   - Capitals, digits and tall letters (b, d, f, h, k, l, t) reach the **green** line.
   - Tails (g, j, p, q, y) drop to the **grey** line.
   - Stay inside the box; leave a little room at the sides. The space between letters is added for you.
   - Write the way you'd like it to read: it's used at small sizes too, so keep loops open and letters clear.
   - Keep the little drawings (coin, spool...) about as tall as a capital and simple: they show at text size.
4. **Hide the template layer** and export the writing layer only: Share > PNG. A transparent background is best;
   plain white also works.
5. **Name them exactly:** `handwriting_regular_1.png`, `handwriting_regular_2.png`, `handwriting_regular_3.png`.
6. **Bold set:** the same three sheets again with a thicker brush, named `handwriting_bold_1.png` ... `_3.png`.
7. Put them in the Drive folder (`Guidelines` is fine) and tell Claude.

You can send one sheet at a time and Claude will show you a sample, but the font only goes into the game once all
three regular sheets are done (a missing letter would show as a blank). Re-doing a letter later: write the sheet
again and send it; the font is rebuilt from the sheets each time.

## For Claude
- `Tools/HandwritingFont/layout.py`: boxes, guide lines, which character where. Never change it once sheets exist.
- `make_template.py` draws the templates. `build_font.py <sheets folder> <out folder> regular|bold` traces each box
  (4x upscale, smoothing, quadratic outlines with holes) and writes `ThimbleHand-Regular.ttf` / `-Bold.ttf`; it lists
  boxes left empty. Baseline-to-green-line = 700 units of 1000.
- Put the fonts in `Assets/_Project/Art/Fonts/`; TestSceneBuilder uses `ThimbleHand-Regular.ttf` when it exists (rebuild
  the scene). Still to do when the font arrives: the bold file (check Unity picks it up for FontStyle.Bold, or switch
  bold texts to it in the builder), `PieceStep` (it makes its own text with the built-in font), `{coin}`-style tags for
  the private-use drawings (U+E000 coin ... U+E006 scissors), spacing check at small sizes, kerning if needed.
- Convert Display P3 sheets to sRGB is not needed (only the shape is used), but check the sheet size is 2048 x 2732.
