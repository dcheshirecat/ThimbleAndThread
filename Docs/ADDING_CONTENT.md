# Adding your own content

Everything in the game (yarns, threads, items, customers, patterns) is a **data asset**: a small file
in `Assets/_Project/Data/` that you fill in like a form. Art is a PNG in `Assets/_Project/Art/`
that the data asset points at. There is no code to write.

New data assets are picked up automatically: the **Content Library**
(`Assets/_Project/Data/ContentLibrary.asset`) updates itself whenever anything in `Data/` is added,
changed or removed. If something ever seems missing, run
**Tools > Thimble & Thread > Refresh Content Library**.

## Two ways to work

- **In Unity (on your PC):** follow the steps below.
- **From your phone:** put the PNGs in the repo (or send them to Claude) and say what they are,
  e.g. "this is 100% alpaca yarn, knitting, premium, costs 80". Claude creates the data asset,
  hooks up the art, and tests it.

## Rules that keep saves safe

1. **IDs are permanent.** Every asset has an id (`supplyId`, `itemId`, `customerId`, `patternId`).
   Save files remember things by id, so once a build is out, never change or reuse an id.
   Lowercase with underscores: `alpaca_yarn_cream`.
2. **Never rename or move files outside Unity.** Do it inside Unity's Project window, so the
   `.meta` files (which hold the links between files) move with them.
3. **Replacing art:** overwrite the PNG with the same filename and the same size. See
   [ART_SWAP.md](ART_SWAP.md). Sizes are in [ART_SIZES.md](ART_SIZES.md).

## Importing a PNG as a sprite

Drop the PNG into the right `Art/` folder, then click it in Unity and set
**Texture Type: Sprite (2D and UI)**, **Generate Mip Maps: off**, **Max Size: 4096**, and press Apply.
Filenames: `category_name_variant.png`, lowercase, e.g. `supply_yarn_alpaca_cream.png`.

## Yarns, threads, fabrics (supplies)

1. Art: a 256 x 256 icon in `Art/Supplies/`.
2. In `Data/Supplies/`: right-click > **Create > Craft > Supply**. Name the file `supply_<id>`.
3. Fill in:
   - **Supply Id**: e.g. `alpaca_yarn_cream`
   - **Display Name**: what players see, e.g. "100% Alpaca Yarn (Cream)"
   - **Craft**: which craft uses it (Knitting, Crochet, Cross Stitch...)
   - **Icon**: drag in your sprite
   - **Price**: in coins
   - **Tier**: 1 basic, 2 nicer, 3 premium. Higher tiers raise sale prices.

Coming in the supplies stage: supplies get used up per project, and get more detail (fibre, weight,
colour) so commissions can ask for exactly "100% alpaca". Supplies you make now will carry over.

## Things you can make (items)

1. Art: a 512 x 512 icon in `Art/Items/` (and later 1024 x 1024 finished-item art).
2. In `Data/Items/`: **Create > Craft > Item Definition**, file `item_<id>`.
3. Fill in **Item Id**, **Item Name**, **Craft**, **Icon**, **Base Price** (finished piece) and
   **Pattern Price**.

## Customers and portraits

1. Art in `Art/Characters/Customers/`:
   - Full body, 1500 x 2000: `customer_<id>_body.png`
   - One 1024 x 1024 portrait per expression: `customer_<id>_neutral.png`, `_happy`, `_sad`,
     `_surprised`, `_thoughtful`. Keep the face in the same place in every expression.
2. In `Data/Customers/`: **Create > Craft > Customer**, file `customer_<id>`.
3. Fill in **Customer Id**, **Display Name**, **Bio**, **Full Body**, and under **Portraits** add
   one entry per expression with its sprite.
4. **Requestable**: the items they're interested in, each with the friendship level it unlocks at
   (0 = from the start). They only browse shelf patterns for these items.

New customers start visiting the shop on the next day.

## Patterns for the shelf (great-aunt's book)

Patterns you design in the game's Pattern Studio are saved with your game. To add one to the book:

1. In `Data/Patterns/`: **Create > Craft > Pattern**, file `pattern_<id>`.
2. Fill in **Pattern Id**, **Display Name**, **Item** (what it makes), **Price**, and tick
   **Starts On Shelf** if it's in the book from the beginning.
3. **Palette**: the colours, in order.
4. **Grid**: draw it as text, one line per row, top row first. `.` is empty, `1` is the first
   palette colour, `2` the second, and so on:

   ```
   .1...1..
   .1...1..
   .11111..
   .12121..
   .11111..
   ```

Keep the first patterns small (8 x 8 or so); each square is two drags to stitch.

## Not built yet

These need a bit of code first. You can start the art now:

- **Embroidery hoops and other tools** (hoops, needles, hooks). Plan: a "Tool" asset like supplies,
  but bought once and kept, with a 256 x 256 icon plus an in-minigame sprite (e.g. the hoop frame,
  2048 x 2048 with a transparent middle). Bigger or nicer hoops could allow bigger patterns.
- **Character customization.** Plan: layered sprites. Draw each layer (body, skin tone, hair, outfit,
  accessories) on the **same canvas size** with a transparent background, lined up exactly, so the
  layers stack. Name them `player_<layer>_<name>_<colour>.png`, e.g. `player_hair_bob_brown.png`.
  Before building it, decide what can be customized (the player only, or customers too?) and
  whether colours are separate drawings or tinted from one greyscale drawing.
