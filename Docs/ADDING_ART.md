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
| Backgrounds | `Art/Backgrounds/` | `bg_shop_day.png`, `bg_workroom_night.png`, `bg_workbench.png` | 2048 x 4440, portrait (see below) |
| Customer, full body | `Art/Characters/Customers/` | `customer_mara_body.png` | 1500 x 2000 |
| Customer portraits | `Art/Characters/Customers/` | `customer_mara_happy.png` (neutral, happy, sad, surprised, thoughtful) | 1024 x 1024 |
| Player character | `Art/Characters/Player/` | `player_<layer>_<name>.png` | same canvas for all layers |
| Item icons | `Art/Items/` | `item_scarf_icon.png` | 512 x 512 |
| Finished-item art | `Art/Items/` | `item_scarf_finished.png` | 1024 x 1024 |
| Supply icons (yarn balls, skeins, fabric) | `Art/Supplies/` | `supply_alpaca_yarn_cream.png` | 512 x 512 (shown on the market shelves) |
| Tileable textures (yarn, fabric, wood) | `Art/Textures/` | `tex_aida_white.png`, `tex_knit_wool.png` | 1024 x 1024 |
| Stitch, loop, needle sprites | `Art/Crafts/<Craft>/` | `stitch_cross_half.png` | 256 x 256 |
| UI | `Art/UI/` | see below | see below |
| Logo | `Art/Logo/` | `logo_title.png` | 2048 wide |

## Backgrounds (portrait)

The game is portrait, and phones come in many heights. Backgrounds are drawn tall, **2048 x 4440**,
and the game fills the screen with them, trimming whatever doesn't fit; it never stretches them.

- Keep anything important (the counter, the shop sign, faces) inside the **middle 2048 x 2732**:
  that part shows on every phone and iPad.
- Let the top and bottom be "extra" (ceiling, floorboards, sky) that only tall phones show.
- Leave calm areas where the buttons sit (the bottom third) so text stays readable.

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
- **Weekday symbols** (top bar, 256 x 256, transparent background), already in the game as white
  placeholders you can paint over: `Art/UI/Weekdays/weekday_1_monday.png` ... `weekday_7_sunday.png`
  (moon, flame, drop, star, heart, ring, sun for now; draw any symbol you like). Draw them WHITE: the
  game tints the round badge behind each one with that day's colour. If you'd rather draw them in full
  colour, tell Claude and the tint gets switched off.
- **Settings gear:** `Art/UI/icon_gear.png` (256 x 256, transparent, white; it's shown in dark ink).
- **Fonts:** send the `.ttf`/`.otf` file, and check its licence allows use in a game you sell.
- **App icon:** 1024 x 1024, no transparency, important shapes away from the corners (Android crops
  icons into circles and rounded squares).

## Music and sound effects

The game already plays soft generated placeholder sounds; dropping in a real file replaces one.
Put files in `Assets/_Project/Audio/` with these names (`.ogg` is best; `.wav` and `.mp3` work too),
then ask Claude to rebuild the scene so it picks them up.

| Sound | File | Notes |
|---|---|---|
| Day music (shop, title) | `Music/music_shop_day.ogg` | loops; make the end flow into the start |
| Night music (workroom) | `Music/music_workroom_night.ogg` | loops |
| Button tap | `SFX/sfx_click.ogg` | very short and quiet (under 0.1 s) |
| Coins earned | `SFX/sfx_coin.ogg` | under 0.5 s |
| A stitch / needle action | `SFX/sfx_stitch.ogg` | very short; it plays a lot |
| Finished crafting | `SFX/sfx_step_done.ogg` | a small happy chime, about 1 s |
| Customer arrives | `SFX/sfx_shop_bell.ogg` | the shop door bell, about 1 s |

- 44.1 kHz, mono is fine for effects; music can be stereo.
- Leave a little silence trimmed off the start of effects, or they feel late.
- Check the licence of any music or sounds you didn't make yourself.
- Players set music and sound volumes in Settings.

## Quick checklist

- [ ] sRGB canvas, right size or bigger, same aspect ratio
- [ ] Transparent background (unless it's a background or texture)
- [ ] Exported as PNG
- [ ] Right folder, lowercase filename, same name as the placeholder it replaces
- [ ] `.procreate` master saved in `SourceArt/`
