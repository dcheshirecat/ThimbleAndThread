# Art list: every screen to design and every picture to draw

A checklist of all the art the game needs. Everything in the game today is a placeholder (coloured shapes
or flat drawings made by code). Tick things off as you go.

- Sizes and the sRGB colour profile: [ART_SIZES.md](ART_SIZES.md). How to get files in: [ADDING_ART.md](ADDING_ART.md).
- **Replacing a placeholder:** use exactly the filename given here, at the size given (or bigger with the
  same shape).
- **"Code-drawn"** means there is no PNG yet: the game draws a shape. Make a new PNG and Claude wires it in.
- Drop finished PNGs in the shared Google Drive folder **"Game Art - Thimble and Thread"** (any subfolder is
  fine, as long as the filename is right) and tell Claude.
- The repo is public for now, so anything committed can be seen and copied.

Counts in **bold** are the number of drawings.

---

## Part 1: UI screens to design

These are layouts: how each screen looks (frames, panels, buttons, decoration). One mock-up per screen is
enough. Claude then cuts it into pieces (9-slice panels, buttons, icons) and rebuilds the screen.
Portrait, 2048 x 2732 safe area (2048 x 4440 including the extra top and bottom).

### Shared pieces (design these first; every screen uses them)
- [ ] **1** panel / card frame (9-slice, 512 x 512)
- [ ] **3** button states: normal, pressed, disabled (`ui_button_normal/pressed/disabled.png`, ~512 x 192)
- [ ] **1** small round "knob" button (for the pencil, pins, close buttons; 256 x 256)
- [ ] **1** pop-up / notice card (the explainers, "Goal reached", the doorbell notice)
- [ ] **1** dialogue box with a nameplate
- [ ] **1** top bar: weekday badge, day + chapter, clock, coins, notices bell, settings gear
- [ ] **1** personal area (round portrait + "Name · Level N" + XP bar)
- [ ] **1** progress bar (XP, friendship meter, step progress)
- [ ] **1** tab (the Journal tabs, Customize tabs, Trade tabs)
- [ ] **1** list row (order rows, shop list rows)
- [ ] **1** shop tile (market / boutique / workshop tiles; plus a gold "rare" version)
- [ ] **1** font (or two: headings and body). Check the licence allows a game you sell.

### Start of the game
- [ ] **1** Title screen (logo, Play, How to Play)
- [ ] **1** Save select (3 slots: day, coins, last played; New / Continue / Delete; the confirm pop-up)
- [ ] **1** How to Play / tutorial pages (frame for a picture + text)
- [ ] **1** Customize: Your Look (name, pronouns, skin, hair, top, Your Makes) and Your Shop tab

### Day (the shop)
- [ ] **1** Shop scene, middle page: shopkeeper behind the counter, the door, decor spots
- [ ] **1** Shop scene, left page: the pickup corner (finished orders, shelving)
- [ ] **1** Shop scene, right page: the pattern display (4 spots, more with upgrades)
- [ ] **1** OPEN / CLOSED sign
- [ ] **1** Shops menu (drops down under the coin counter; shop hours, shut shops greyed)
- [ ] **1** Decorating tray (the pencil's drawer of pieces along the bottom; glowing spot outlines)
- [ ] **1** Online Orders panel (the computer on the counter, level 7)
- [ ] **1** Dialogue screen (customer in place of the shopkeeper, choices)
- [ ] **1** Day summary ("Close the shop": takings, friends, looking ahead)

### Animations / transitions
- [ ] **1** Day to night (shop closing: light changes, sign flips, the workroom lamps come on)
- [ ] **1** Night to morning (going to bed, the next day)
- [ ] **1** Doorbell + door swinging open, customer stepping in
- [ ] **1** Journal opening and page turning
- [ ] **1** Stamp being pressed (achievements)
- [ ] **1** Level up / new craft learned
These can be a few frames each (a short flipbook of PNGs), or a "before" and "after" picture that Claude
cross-fades. Say which you'd prefer.

### Night (the workroom)
- [ ] **1** Workroom hub (order book, Pattern Studio, Go to Bed)
- [ ] **1** Order book (list of orders, "Missing: ..." rows)
- [ ] **1** Workbench (the order's steps, tools and materials)
- [ ] **1** Gather (cupboard tiles + the ticked needs list)
- [ ] **1** Minigame frame (the work in the middle, hint text, Undo / Done for now / Finish, colour swatches)
- [ ] **1** Result (quality, extras to add, Set Aside)
- [ ] **1** Pattern Studio list and pattern page
- [ ] **1** Pattern designer (the grid, colour swatches, Full / Half / 3/4, Cloth, naming pop-up)

### Shops in town
- [ ] **1** The Supply Market (shelves per kind, tiles, shopping list, Special Order)
- [ ] **1** Moreau's Boutique (rail, mirror / try-on)
- [ ] **1** Finch's Pattern Parlour (patterns, magazines)
- [ ] **1** Marsh's Workshop (furniture tiles, attic and basement expansions)
- [ ] **1** Quill's Curiosities
- [ ] **1** Byte & Bobbin
(These can share one shop layout with a different background each.)

### Weekends
- [ ] **1** Saturday town map (see the map in Part 2) + the place card that pops up from a pin
- [ ] **1** Sunday market stall (the stall, shoppers, Sell / Ask for More / Keep It, Pack Up)

### Overlays
- [ ] **1** Journal: storybook with tabs (Orders, Story, Friends, Supplies, Stamps, Goals)
- [ ] **1** Friend card (round portrait, name, friendship meter, notes, likes / dislikes)
- [ ] **1** Stamps page (a stamp album)
- [ ] **1** Skills report (opened from the XP bar)
- [ ] **1** Notices list ("What's new")
- [ ] **1** Calendar (4 seasons of 28 days, birthdays)
- [ ] **1** Settings (volumes, haptics, How to Play, Save and Quit) and the pause menu
- [ ] **1** Trade (Give / Sell / Surprise / Patterns / Open / My Codes)

**About 50 screen layouts** (fewer if shops and pop-ups share one design).

---

## Part 2: Backgrounds

| | File | Size | Now |
|---|---|---|---|
| [ ] Shop by day | `Backgrounds/bg_shop_day.png` | 2048 x 4440 | placeholder |
| [ ] Shop at night / the workroom | `Backgrounds/bg_workroom_night.png` | 2048 x 4440 | placeholder |
| [ ] Workbench close-up | `Backgrounds/bg_workbench.png` | 2048 x 4440 | placeholder |
| [ ] Town map (Saturdays) | `Backgrounds/town_map.png` | 2048 x 3600 | placeholder |
| [ ] Title screen | `Backgrounds/bg_title.png` | 2048 x 4440 | code-drawn |
| [ ] Pickup corner (shop left page) | `Backgrounds/bg_shop_pickups.png` | 2048 x 4440 | code-drawn |
| [ ] Pattern display (shop right page) | `Backgrounds/bg_shop_display.png` | 2048 x 4440 | code-drawn |
| [ ] Supply Market | `Backgrounds/bg_market.png` | 2048 x 4440 | code-drawn |
| [ ] Moreau's Boutique | `Backgrounds/bg_boutique.png` | 2048 x 4440 | code-drawn |
| [ ] Finch's Pattern Parlour | `Backgrounds/bg_pattern_shop.png` | 2048 x 4440 | code-drawn |
| [ ] Marsh's Workshop | `Backgrounds/bg_workshop.png` | 2048 x 4440 | code-drawn |
| [ ] Quill's Curiosities | `Backgrounds/bg_curios.png` | 2048 x 4440 | code-drawn |
| [ ] Byte & Bobbin | `Backgrounds/bg_tech_shop.png` | 2048 x 4440 | code-drawn |
| [ ] Sunday market stall | `Backgrounds/bg_stall.png` | 2048 x 4440 | code-drawn |
| [ ] Attic (shop expansion) | `Backgrounds/bg_attic.png` | 2048 x 4440 | code-drawn |
| [ ] Basement (shop expansion) | `Backgrounds/bg_basement.png` | 2048 x 4440 | code-drawn |
| [ ] Journal (open book) | `UI/journal_book.png` | 2048 x 2732 | code-drawn |

**17 backgrounds.** Saturday places (harbour, lighthouse, café...) are text scenes for now; a background per
place would add up to **15** more (harbour, lighthouse, beach, café, tea room, library, bookshop, church,
florist, Kit's studio, hospital, school, park, Fore Street homes, Knot & Needle). Optional, later.

---

## Part 3: Characters

### 25 characters. Each needs:
- **1** full body (`customer_<id>_body.png`, 1500 x 2000)
- **5** talking heads, one per expression (`customer_<id>_neutral / happy / sad / surprised / thoughtful.png`,
  1024 x 1024, head in the same place on each)

= **6 drawings each, 150 in all.** All are placeholders now.

| Character | id | Who | Full body | Talking heads (5) |
|---|---|---|---|---|
| Mara | `mara` | new parent | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Old Tobias | `tobias` | fisherman, widower | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Juno | `juno` | teen, knitwear | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Priya Nair | `priya` | teacher | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Hal Penrose | `hal` | retired postman, pigeons | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Isla Marlowe | `isla` | lighthouse keeper (romance) | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Eli Brooks | `eli` | Driftwood Café (romance) | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Nadia Kowalski | `nadia` | night-shift midwife | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Sol Reyes | `sol` | lifeguard | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Gwen Tallow | `gwen` | harbourmaster | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Midge Lowe | `midge` | Ropewalk Books | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Theo Lindqvist | `theo` | Tobias's grandson | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Felix Adeyemi | `felix` | Petals the florist | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Dot Pascoe | `dot` | tea room, Aunt Win's friend | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Benji Marsh | `benji` | carpenter, Marsh's Workshop | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Sam | `sam` | music teacher, Benji's fiancé | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Owen Price | `owen` | vicar, beekeeper | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Wenna Trelawney | `wenna` | quilting bee | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Kit Calloway | `kit` | painter | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Rowan Hale | `rowan` | Knot & Needle, rival (romance) | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Bram Okafor | `bram` | Supply Market (romance) | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Celeste Moreau | `celeste` | Moreau's Boutique (romance) | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Ada Finch | `ada` | Pattern Parlour | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Jasper Quill | `jasper` | Quill's Curiosities (romance) | [ ] | [ ] [ ] [ ] [ ] [ ] |
| Ravi Sen | `ravi` | Byte & Bobbin (romance) | [ ] | [ ] [ ] [ ] [ ] [ ] |

Files go in `Art/Characters/Customers/`. Optional extras, later: a blush expression for romance scenes,
date outfits, a seasonal outfit.

### The player character (the shopkeeper)
Code-drawn today. Final art = separate drawings for every option and colour (your choice: no tinting).
All on the **same canvas** so the layers stack (suggest 1500 x 2000, like the full bodies), plus a matching
talking-head crop for the round portrait in the top bar.

| Layer | Options | Drawings |
|---|---|---|
| [ ] Body / skin | 6 skin tones | **6** |
| [ ] Hair | 5 styles (Short, Bob, Long, Bun/Top Knot, Curly/Big Curls) x 8 colours | **40** |
| [ ] Tops | 3 (Jumper, Apron, Cosy Cardigan) x 8 colours | **24** |
| [ ] Accessories | Daisy Brooch, Round Glasses, Felt Hat, Sailor Neckerchief | **4** |
| [ ] Things you made, worn | sweater, scarf, cable scarf (drawn white or light grey, for the yarn colour) | **3** |
| [ ] Face | one neutral, one happy | **2** |

About **79 drawings.** Ways to make it fewer: fewer colours, or draw hair and tops in light grey and let the
game tint them (say if you change your mind on tinting).
Boutique tiles for the cosmetics: `Cosmetics/cosmetic_<id>.png` (512 x 512): **7** (Daisy Brooch, Round
Glasses, Felt Hat, Sailor Neckerchief, Top Knot, Big Curls, Cosy Cardigan).

---

## Part 4: Items and patterns

### Finished items (12)
Each item needs an **icon** (`Items/item_<id>_icon.png`, 512 x 512, placeholder now) and a **finished
picture** (`Items/item_<id>_finished.png`, 1024 x 1024, code-drawn now): **24 drawings.**

| Item | id | Icon | Finished |
|---|---|---|---|
| Sampler | `sampler` | [ ] | [ ] |
| Tablecloth | `tablecloth` | [ ] | [ ] |
| Scarf | `scarf` | [ ] | [ ] |
| Cable-Knit Scarf | `cable_scarf` | [ ] | [ ] |
| Sweater | `sweater` | [ ] | [ ] |
| Embroidered Hoop | `hoop_art` | [ ] | [ ] |
| Crocheted Plushie | `plushie` | [ ] | [ ] |
| Sewn Plushie | `plushie_sewn` | [ ] | [ ] |
| Tote Bag | `tote` | [ ] | [ ] |
| Needlepoint Cushion | `cushion` | [ ] | [ ] |
| Patchwork Quilt | `quilt` | [ ] | [ ] |
| Woven Wall Hanging | `wall_hanging` | [ ] | [ ] |

Tip: draw items plain (white/grey) where the colours come from the player's pattern; the game lays the
pattern on top.

### Patterns
The 26 book patterns (Anchor, Bear Cub, Daisy Hoop, Felt Bunny...) are charts the game draws from the grid,
so they need **no art**. Optional: a printed "pattern leaflet" cover frame (**1**) and a magazine cover (**1**).

---

## Part 5: Crafting (the minigames)

All code-drawn today. These make the crafting feel real; draw at the size given, transparent.

### Tools in the hand (256 x 256, `Crafts/Tools/`)
- [ ] tapestry needle, embroidery needle, sewing needle (**3**)
- [ ] crochet hook, pair of knitting needles, seam ripper, iron (**4**)
- [ ] sewing machine foot + needle (**1**), shuttle and beater (weaving, **2**), rotary cutter, scissors, pin (**3**)

### Stitches (256 x 256, `Crafts/<Craft>/`; drawn white so the game can colour them per thread)
- [ ] Cross stitch: half "/", half "\", full X (2 versions: either leg on top), 3/4 stitch, tent stitch (**6**)
- [ ] Backstitch / running stitch segment, satin stitch strand, stem stitch, French knot (**4**)
- [ ] Knit V, purl bump, cable cross (left and right), dropped-stitch ladder (**5**)
- [ ] Crochet single crochet, increase, decrease, magic ring (**4**)
- [ ] Machine stitch dash, hand stitch dash, ladder stitch (**3**)
- [ ] Weaving: weft over/under, warp thread (**2**)

### Surfaces (tileable textures, 1024 x 1024, `Textures/`)
- [ ] Aida white, Aida cream, linen evenweave, needlepoint canvas (**4**)
- [ ] felt, quilting cotton, minky, fleece, canvas (tote) (**5**)
- [ ] knitted wool, crocheted fabric, woven cloth (**3**)
- [ ] pattern paper (sewing designs), wooden workbench (**2**)

### Frames and holders (`Crafts/Frames/`)
- [ ] embroidery hoop (wooden ring + screw), Q-Snap frame, stretcher bars with tacks, frame loom with pegs (**4**)
- [ ] ironing board + heat dial, sewing machine (side view) (**2**)
- [ ] stuffing tuft, safety eye, button, pin head, safety pin (**5**)

**About 53 crafting pieces.**

---

## Part 6: Supplies (112 shop tiles)
`Supplies/supply_<id>.png`, 512 x 512, all placeholders now. Several can be one drawing in different colours
(e.g. draw one skein of floss and recolour it per colour in Procreate).

- [ ] **Floss (34):** black, brown, burgundy, charcoal, coral, cream, forest, gold, green, lavender, lemon, magenta,
  mint, navy, olive, orange, peach, periwinkle, pink, plum, purple, red, rose, royal, rust, silver grey, sky, tan,
  teal, turquoise, white, yellow, embroidery cotton, warp cotton
- [ ] **Yarn (19):** acrylic (cream + black, brown, green, grey, navy, orange, pink, purple, red, sky, white, yellow),
  basic wool, merino, alpaca, mohair, cotton, weaving yarn
- [ ] **Fabric (15):** Aida 14ct, Aida 18ct, Aida cream, linen 28ct, mono canvas, natural canvas, felt cream, felt brown,
  fleece blue, minky pink, quilting cotton, quilting floral, velvet backing, wadding, interfacing
- [ ] **Notions (11):** buttons white, buttons wood, lace trim, satin ribbon, cotton webbing, zip 20cm, stuffing, cushion
  pad, safety eyes, masking tape, dowel
- [ ] **Tools (28):** hoops 4", 6", 8", Q-Snap frame, stretcher bars, frame loom, crochet hooks (5mm, ergonomic 5mm),
  knitting needles (4mm, 5mm, bamboo 5mm), needles (tapestry, bent tapestry, embroidery, gold embroidery, sewing,
  sharps, quilting), fabric scissors, sharp scissors, rotary cutter, pins, safety pins, marking pen, sewing machine,
  quilter's machine
- [ ] **Books (5):** Cables and Arans, Knots, Needlepoint for Cross Stitchers, Pattern Design, Weaving on a Frame Loom

The exact list of files is the `Art/Supplies/` folder.

---

## Part 7: Shop decoration and upgrades

### Decor (25 pieces, `Decor/decor_<id>.png`; 512 x 512 for the shop tile, drawn at the size it stands in the shop)
Brass Lamp, Jar of Buttons, Brass Counter Bell, Cuckoo Clock, Dress Form, Tall Fern, Grandfather Clock,
Harbour Painting, Knitted Pouffe, Lighthouse Painting, Wicker Parcel Basket, Patchwork Rug, Pickup Bunting,
Rag Rug, Green Reading Lamp, Ribbon Reel Stand, Framed Sampler, Victorian Sewing Box, Seashell Mobile,
Ship's Bell, Big Spool of Parcel String, Plant in a Teacup, Teapot Collection, Old Tide Chart, Umbrella Stand.
**25 drawings** (one picture is used both in the shop and on the tile).

### Upgrades (4, `Upgrades/upgrade_<id>.png`, 512 x 512)
Display Case, Display Shelf, Shop Sign, Work Lamp. **4 drawings.**

### Shop furniture (code-drawn now)
- [ ] counter, door (closed + open), pattern display spots, pickup shelving, the counter computer (**6**)

---

## Part 8: Icons and small UI (128-256 px, transparent, `UI/`)
- [ ] Weekday symbols x 7 (`UI/Weekdays/weekday_1_monday.png` ... `weekday_7_sunday.png`, 256 x 256, white)
- [ ] Settings gear (`UI/icon_gear.png`), decorating pencil (`UI/icon_pencil.png`) (**2**)
- [ ] coin, XP star, heart (friendship), romance heart, notices bell, clock, calendar, journal, shops bag,
  close (x), back arrow, undo, stitch picker, lock, tick, birthday cake (**16**)
- [ ] Map pins: place, shop, your home (**3**), plus a name plaque (**1**)
- [ ] Stamp themes: Craft, Skill, Friendship, Romance, Story, Shop (**6** stamp frames)
- [ ] Logo (`Logo/logo_title.png`, 2048 wide) and app icon (1024 x 1024, no transparency) (**2**)

**About 37 icons.**

---

## Totals (roughly)

| Part | Drawings |
|---|---|
| UI screen layouts | ~50 |
| Backgrounds | 17 (+15 optional) |
| Characters | 150 |
| Player character | ~79 + 7 tiles |
| Items | 24 |
| Crafting | ~53 |
| Supplies | 112 |
| Decor, upgrades, furniture | 35 |
| Icons | ~37 |

Suggested order: shared UI pieces + the shop background + Mara, Tobias and Juno first. The first
day then looks finished, and the rest can follow.
