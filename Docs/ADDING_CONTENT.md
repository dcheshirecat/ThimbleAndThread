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
3. **Replacing art:** overwrite the PNG with the same filename and the same size. See [ADDING_ART.md](ADDING_ART.md),
   [ART_SWAP.md](ART_SWAP.md). Sizes are in [ART_SIZES.md](ART_SIZES.md).

## Importing a PNG as a sprite

Drop the PNG into the right `Art/` folder, then click it in Unity and set
**Texture Type: Sprite (2D and UI)**, **Generate Mip Maps: off**, **Max Size: 4096**, and press Apply.
Filenames: `category_name_variant.png`, lowercase, e.g. `supply_yarn_alpaca_cream.png`.

## Yarns, threads, fabrics (supplies)

1. Art: a 512 x 512 icon in `Art/Supplies/`. It's the picture on the Supply Market shelf.
2. In `Data/Supplies/`: right-click > **Create > Craft > Supply**. Name the file `supply_<id>`.
3. Fill in:
   - **Supply Id**: e.g. `alpaca_yarn_cream`
   - **Display Name**: what players see, e.g. "100% Alpaca Yarn (Cream)"
   - **Kind**: Fabric, Thread (embroidery floss), Yarn, Stuffing, Eyes, Tool, Notion (buttons,
     ribbon, zips...) or Other. It also decides which market shelf it sits on.
   - **Craft**: which craft uses it (Knitting, Crochet, Cross Stitch...)
   - **Icon**: drag in your sprite
   - **Color**: the thread/yarn/fabric colour. Floss colours appear in the Pattern Studio and on the
     stitching screen, so pick it carefully.
   - **Price**: coins for one pack
   - **Tier**: 1 basic, 2 nicer, 3 premium. Higher tiers raise sale prices.
   - **Per Pack** and **Unit**: how much one pack gives, e.g. floss 30 "squares", Aida 400
     "squares", yarn 300 "stitches", stuffing 3 "plushies", eyes 4 "pairs".
   - **Starter Packs**: how many packs a new game begins with (0 for most things).
   - **Market**: when the Supply Market sells it. *Always* (basics, every day), *Rotating* (the
     selection that changes every 4 days, about 14 of these at a time), *Rare* (now and then one
     special item turns up) or *Not Sold*.
   - **Tool Type** and **Size** (tools only): e.g. hoops are type `hoop` with Size = the largest
     pattern side they fit (4" = 10, 6" = 12, 8" = 16). For yarn, Size is the yarn weight
     (3 = DK, 4 = worsted), which hooks and needles will need to match.

Supplies are used up: a cross stitch project uses one square of Aida per grid square and one square
of floss per stitched square of that colour. Tools are bought once and kept: stitching needs a hoop
big enough for the pattern and a tapestry needle. Every Thread you add becomes a new colour in the
Pattern Studio automatically. On the market, each supply shows its icon on its shelf (Fabric,
Embroidery Floss, Yarn, Notions, Tools).

## Things you can make (items)

1. Art: a 512 x 512 icon in `Art/Items/` (and later 1024 x 1024 finished-item art).
2. In `Data/Items/`: **Create > Craft > Item Definition**, file `item_<id>`.
3. Fill in **Item Id**, **Item Name**, **Craft**, **Icon**, **Base Price** (finished piece) and
   **Pattern Price**.
4. **Steps**: how it's made, in order. Press **+** for each step and pick its **Kind** (the minigame)
   and a **Label** the player sees, e.g. the tablecloth is *Cross Stitch: "Stitch the motif"* then
   *Hem: "Hem the edges"*. **Tools** lists anything extra the step needs (Tool Type and Min Size, as
   on the supply, plus a Label like "Fabric scissors"). **Materials** lists supplies the step uses up
   (Supply Id and Amount, e.g. `stuffing` 1). **Fabric Pieces** is how many pieces of the fabric chosen
   in a Choose Fabric step this step uses (cutting a plushie uses 2: front and back), or stitches of the
   yarn chosen in a Choose Yarn step. Tick **Per Pattern Square** to count it per square of the pattern
   (knitting a scarf uses 2 stitches of yarn per square). For knitting needles and crochet hooks, tick
   **Match Yarn** on the tool: the player then needs the size that matches the chosen yarn's weight.
   Leave Steps empty for a single cross stitch step.
   Kinds so far: Cross Stitch, Hem, Choose Fabric, Cut Pieces, Pin, Sew Seam, Turn Right Side Out,
   Stuff, Attach Eyes, Close Opening, Sew Sides, Hem Top, Attach Handles,
   Choose Yarn, Cast On, Knit Rows, Cast Off, Magic Ring, Crochet Rounds.

The player can stop between steps or partway through one; every step's work is saved. A step's
tools and materials are checked when the player starts it, and its materials are used up then.
The piece's quality is the average over its steps (choosing a fabric isn't judged).

**Sewing patterns** (e.g. the Sewn Plushie's "Felt Bunny") are drawn on the same grid as cross
stitch patterns: the filled squares are the shape of the piece (the game smooths the outline), and
squares in the second colour ('2' in a book pattern's grid) mark where the eyes go. So the Pattern
Studio designs plushies too. Bags (e.g. "Market Tote") work the same way: the filled squares are the
panel, the top edge stays open and is hemmed, and the handle is sewn on just below it.
**Knitting patterns** use the grid as texture: each square is one stitch, filled squares are purl
stitches and empty ones knit (so "Wave Stripes" knits a wavy texture).

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
3. **Threads**: drag in the floss supplies it uses, in order. They set the colours, and they're
   what the player must own to stitch it. (Palette is only a fallback for patterns without threads.)
4. **Grid**: draw it as text, one line per row, top row first. `.` is empty, `1` is the first
   thread, `2` the second, and so on:

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

- **Tools in the minigames.** Tools already exist as supplies (above). Later each one gets an
  in-minigame sprite too, e.g. the hoop frame, 2048 x 2048 with a transparent middle.
- **Character customization.** Plan: layered sprites. Draw each layer (body, skin tone, hair, outfit,
  accessories) on the **same canvas size** with a transparent background, lined up exactly, so the
  layers stack. Name them `player_<layer>_<name>_<colour>.png`, e.g. `player_hair_bob_brown.png`.
  Before building it, decide what can be customized (the player only, or customers too?) and
  whether colours are separate drawings or tinted from one greyscale drawing.
