# Adding your illustrations

How to get art from Procreate into the game. Sizes are in [ART_SIZES.md](ART_SIZES.md); data
assets (yarns, customers...) are in [ADDING_CONTENT.md](ADDING_CONTENT.md).

## Procreate settings (once per canvas)

- **Colour profile: sRGB** (e.g. "sRGB IEC61966-2.1") when you create the canvas. Unity assumes
  sRGB; Display P3 art looks duller or shifted in the game. Procreate can't convert an existing
  canvas, so pick sRGB at the start.
- **Size: the target size from ART_SIZES.md, or bigger with the same shape.** Raster art can be
  shrunk cleanly but not enlarged.
- **Transparent background** for everything except full backgrounds and tileable textures: hide
  the Background colour layer before exporting.
- **Export: Share > PNG.** Not JPEG (no transparency, blurry edges).
- Keep the `.procreate` master in `SourceArt/` (outside Assets, on purpose).

## Getting files onto the PC and into the repo

1. Move the PNG to the PC (AirDrop, iCloud Drive, a USB cable...).
2. Put it in the right `Assets/_Project/Art/` folder (table below) with the right filename.
   - **Replacing a placeholder:** overwrite the existing PNG, same filename, same size. Don't delete
     it first, or its `.meta` file (which holds every link to it) can be lost.
   - **Brand-new art:** copy it in, then open Unity so it creates the `.meta` file.
3. Commit and push (or ask Claude to). PNGs go through Git LFS automatically.

Or just send the PNGs to Claude with a note saying what each one is.

## Where things go

| Art | Folder | Filename | Size |
|---|---|---|---|
| Backgrounds | `Art/Backgrounds/` | `bg_shop_day.png`, `bg_workroom_night.png`, `bg_workbench.png` | 2732 x 2048 |
| Customer, full body | `Art/Characters/Customers/` | `customer_mara_body.png` | 1500 x 2000 |
| Customer portraits | `Art/Characters/Customers/` | `customer_mara_happy.png` (neutral, happy, sad, surprised, thoughtful) | 1024 x 1024 |
| Player character | `Art/Characters/Player/` | `player_<layer>_<name>.png` | same canvas for all layers |
| Item icons | `Art/Items/` | `item_scarf_icon.png` | 512 x 512 |
| Finished-item art | `Art/Items/` | `item_scarf_finished.png` | 1024 x 1024 |
| Supply icons (yarn balls, skeins, fabric) | `Art/Supplies/` | `supply_alpaca_yarn_cream.png` | 256 x 256 |
| Tileable textures (yarn, fabric, wood) | `Art/Textures/` | `tex_aida_white.png`, `tex_knit_wool.png` | 1024 x 1024 |
| Stitch, loop, needle sprites | `Art/Crafts/<Craft>/` | `stitch_cross_half.png` | 256 x 256 |
| UI | `Art/UI/` | see below | see below |
| Logo | `Art/Logo/` | `logo_title.png` | 2048 wide |

## Characters

- Draw every expression of a character on the **same canvas, with the head in the same place**, so
  switching expressions doesn't make them jump. The easiest way: one Procreate file, one layer
  group per expression, export each group on its own.
- Full-body art: leave a little space under the feet. The game lines characters up on the floor by
  the bottom edge.

## Yarn, thread and fabric textures (tileable)

A tileable texture repeats with no visible seams, so a small image can cover a whole scarf.

- Square, 1024 x 1024, no transparency.
- Check the seams: move the artwork by exactly half its width and half its height (with wrapping),
  paint over the seam that now runs through the middle, and move it back.
- Even lighting, no strong shadows in one corner (they show up as a repeating pattern).
- After importing, Claude sets **Wrap Mode: Repeat** on these.

The game doesn't draw yarn textures yet; they come with the knitting and crochet minigames.
Making them now is fine.

## UI

The current buttons and panels are Unity's plain defaults. When you have UI art, Claude will swap the
whole UI to it in one go.

- **Panels and buttons stretch**, so they use "9-slice": draw the panel at 512 x 512 with decorated
  corners and edges and a plain middle, and keep the border the same width on all sides. Tell Claude
  the border width in pixels (e.g. "48 px") and the middle stretches without warping the corners.
- **Buttons** (~512 x 192): one PNG per state, `ui_button_normal.png`, `ui_button_pressed.png`,
  `ui_button_disabled.png`.
- **Icons** (coins, stars, hearts...): 128 x 128, `ui_icon_coin.png`.
- **Fonts:** send the `.ttf`/`.otf` file, and check its licence allows use in a game you sell.
- **App icon:** 1024 x 1024, no transparency, important shapes away from the corners (Android crops
  icons into circles and rounded squares).

## Quick checklist

- [ ] sRGB canvas, right size or bigger, same aspect ratio
- [ ] Transparent background (unless it's a background or texture)
- [ ] Exported as PNG
- [ ] Right folder, lowercase filename, same name as the placeholder it replaces
- [ ] `.procreate` master saved in `SourceArt/`
