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

## Better tools

A tool supply (kind Tool) can have **Assist** (0 to 1): how much better than the basic one it is. The best
owned tool that fits a step helps with it, and the step says so: scissors and needles widen the line you
must follow, a sewing machine takes corners faster, knitting needles forgive an uneven rhythm, a hook catches
loops more easily. Same **Tool Type** (and size, for needles and hooks) as the basic tool it improves.

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
   **Pattern Thread** and **Thread Amount**: floss in one of the pattern's own colours (0 = first,
   1 = second) that the step uses, in squares like cross stitch (embroidery steps use these).
   Leave Steps empty for a single cross stitch step.
   Kinds so far: Cross Stitch, Hem, Choose Fabric, Cut Pieces, Pin, Sew Seam, Turn Right Side Out,
   Stuff, Attach Eyes, Close Opening, Sew Sides, Hem Top, Attach Handles,
   Choose Yarn, Cast On, Knit Rows, Cast Off, Magic Ring, Crochet Rounds,
   Hoop Fabric, Backstitch, Satin Fill, French Knots, Press, Weave Ends, Sew Up (a sweater's side seams).

The player can stop between steps or partway through one; every step's work is saved. A step's
tools and materials are checked when the player starts it, and its materials are used up then.
The piece's quality is the average over its steps (choosing a fabric isn't judged).

**Sewing patterns** (e.g. the Sewn Plushie's "Felt Bunny") are drawn on the same grid as cross
stitch patterns: the filled squares are the shape of the piece (the game smooths the outline), and
squares in the second colour ('2' in a book pattern's grid) mark where the eyes go. So the Pattern
Studio designs plushies too. Bags (e.g. "Market Tote") work the same way: the filled squares are the
panel, the top edge stays open and is hemmed, and the handle is sewn on just below it.
**Half stitches** in a book pattern's grid: a lowercase letter, 'a' for colour 1, 'b' for colour 2 and so on (only the "/" leg is
stitched; good for diagonal edges).
**Knitting patterns** depend on the item's **Garment** (on the item asset):
- **Scarf**: a long strip with ribbed ends. The pattern is texture: filled squares are purl stitches,
  empty ones knit, repeated along the scarf (so "Wave Stripes" knits a wavy texture).
- **Sweater**: knitted in pieces like a real one. Each Cast On / Knit Rows step has a **Piece**: `back`
  (ribbed hem, plain body, neckline), `front` (the same, with the pattern knitted in as colourwork, each
  filled square in its colour, and a deeper neckline) or `sleeves` (two, ribbed cuffs widening to the
  shoulder). Then Sew Up joins them, then Weave Ends and Block.
- **Square** (any other knitted item): the pattern's grid as texture, as above.
Colourwork yarns aren't counted from the cupboard yet; only the main yarn is.
Sweaters and scarves can also be made for the player to wear (Pattern Studio > Make One for Me).
**Embroidery patterns** (e.g. "Little Whale") use the grid like sewing ones: the filled squares are the
shape that's backstitched round and satin-filled in the first colour, and squares in the second colour
are French knots in that colour.

## Commissions (special requests)

A commission is a special request from one customer: the player designs a brand-new pattern for
it (the design isn't judged), then makes the item. Each one is offered until it's delivered, once;
after delivery its pattern goes into the player's book, ready to put on the shelf.

1. In `Data/Commissions/`: **Create > Craft > Commission**, file `commission_<id>`.
2. Fill in:
   - **Commission Id**: e.g. `tobias_sunday_cloth`
   - **Customer** and **Item** (what gets made)
   - **Min Relationship Level**: offered once the friendship is this high (0 = from the start)
   - **Request**: what they say. Coffee Talk style works well: describe the feeling, not the item.
   - **Required Supply** (optional): the fabric or yarn it must be made from, e.g. 100% Alpaca Yarn or
     Linen Evenweave. Rare supplies only turn up at the market now and then, which makes these special.
   - **Bonus Coins** and **Bonus Friendship**: on top of the usual reward.

## Dialogue (what customers say)

All dialogue is in `Assets/_Project/Dialogue/`, one `.ink` file per character plus `story.ink`. They're
plain text: edit them in any text editor, or in inkle's free **Inky** editor to preview them. The game
reads a simple part of the Ink language:

```
=== mara_scene_1 ===
Mara: Can I sit for a minute? # thoughtful
* [Laugh] You laugh, and so does she.
    ~ friendship(5)
* [Put the kettle on]
    Mara: You're an angel. # happy
- Mara: This shop always felt safe.
-> END
```

- `=== name ===` starts a scene (a "knot"). `Name: text` is a line; text without a name is narration.
- `# happy` (or neutral, sad, surprised, thoughtful) sets the speaker's portrait for that line.
- `* [Choice]` is an answer button; lines indented under it happen when it's picked. `- ` gathers the
  choices back together. `* {romance} [...]` only shows when the condition holds (`{not romance}`,
  `{level >= 3}` or any flag name work too).
- `~ note(Keeps the lighthouse.)` writes something the character has just told you about themselves into the
  Journal's Friends page (put it right after the line where they say it; no brackets inside the note). The
  Journal only shows what they've told you, never a written bio.
- Commands: `~ friendship(5)` adds friendship, `~ romance(1)` starts a romance (romanceable characters
  only), `~ flag(name)` / `~ unflag(name)` remember things for later conditions.

The game looks for knots by name (the character's **Ink Name**, set on the customer asset):
- Greetings: any knot named `mara_visit` or starting with `mara_visit_`. Write as many as you like
  (`mara_visit_rain`, `mara_visit_list`...). A number straight after `_visit_` is the friendship level
  needed (`mara_visit_2`, `mara_visit_2_word`). Each visit picks one the player hasn't heard lately, so
  more greetings = less repetition.
- First meetings: `mara_intro` (or variants `mara_intro_...`) plays the first time someone visits (or you
  first walk into their shop) instead of the usual hello. Put `~ met()` straight after the line where they
  say their name: until then their name shows as ??? (in the dialogue and the Journal's Friends page).
- Shop owners (Bram, Celeste) also greet you at their door and see you off: `bram_greet...` when you
  walk in, `bram_bye...` when you leave without buying, `bram_thanks...` when you bought something.
  Same rules as greetings (a number after the kind is the friendship level needed, e.g. `bram_greet_2_friend`).
  Keep them short: the last line gets the buttons ("Have a look around", "Stop for a chat", "Goodbye!").
- `mara_request_<anything>`: an indirect request. Start it with `# fits: scarf, sweater` (item ids that
  would do) and `# best: scarf` (the perfect one: 50% more friendship). The player offers a pattern
  from the display.
- `mara_handover_good`, `_okay`, `_poor`: what they say when they come back to collect their order.
  Add variants the same way (`mara_handover_good_hug`); one not heard lately is picked.
- Requests are picked the same way, so a character with several `mara_request_...` knots varies too.
- `mara_scene_1`, `_scene_2`...: a scene played once, when friendship reaches that level.
- `mara_suggest_yes`, `_buy`, `_no` (plus variants like `mara_suggest_no_rain`): how they react when you
  suggest a pattern on display. Which one plays follows the character, not chance: _yes for things they'd
  want (their Requestable items), _buy for crafts they do themselves (**Makes** on the customer asset), else _no.
- `chapter_1`, `chapter_2`...: story scenes, played when a chapter begins.

**The player's name and pronouns.** The player picks a name and she/her, he/him or they/them when the
game starts (and can change them in Settings > Change Your Look). Write these tokens in any line or
choice and the game fills them in:

| Token | she/her | he/him | they/them |
|---|---|---|---|
| `{name}` | the player's name | | |
| `{they}` / `{They}` | she / She | he / He | they / They |
| `{them}` / `{Them}` | her | him | them |
| `{their}` / `{Their}` | her | his | their |
| `{theirs}`, `{themself}` | hers, herself | his, himself | theirs, themself |
| `{is}` / `{are}`, `{was}`/`{were}`, `{have}`, `{do}` | is, was, has, does | is, was, has, does | are, were, have, do |
| `{s}` (verb ending: "{they} knit{s}") | s | s | (nothing) |
| `{niece}` | niece | nephew | nibling |

Example: `Isla: You're the new one. The great-{niece}.` Never write "she" or "he" for the player directly.

**Likes and dislikes are never said in dialogue** (owner's choice): they're a surprise. The player
finds out by adding a special touch (lace, buttons, ribbon...) to someone's order; their reaction is the
clue, and the Journal's Friends page notes it from then on. They're set on the customer asset (below).

## New characters

Make a customer as below, then set **Ink Name** (e.g. `isla`), **First Chapter** (when they start
visiting) and **Romanceable** (adult characters only), and write their `.ink` file.

## Story chapters

In `Data/Chapters/`: one asset per chapter (**Create > Craft > Chapter**): its number, title, intro,
and goals to reach the next one (orders handed over, patterns sold, a friendship level, all totals).
The goals are never shown to the player (the story moving on is a surprise); **Goal Text** is just a
note for you. Its story scene is the `chapter_N` knot in story.ink.

## Goals (achievement quests)

In `Data/Quests/` (**Create > Craft > Quest**): open goals on the Journal's Goals page, like "Sell 5
patterns". When one is reached a pop-up says so, and the player taps it in the Journal to claim its
**Coins** and **Xp** (experience). **Goal** is what it counts (orders handed over, patterns sold, things
made, best friendship level, people met, patterns designed, commissions) and **Target** how many.
**One Craft** counts only things made in that **Craft**, hides the quest until the craft is learned, and
gives the experience to that craft (otherwise it goes to the craft practised most). **After** makes a
chain: the quest shows only once the one before it has been claimed. Design and commission goals stay
hidden until Pattern Design is learned. Keep **Quest Id** fixed once saves exist.

## Skills (crafts and techniques)

In `Data/Skills/` (**Create > Craft > Skill**). A skill is learned by ANY of its routes: reaching a
**Chapter**, a **Craft Level**, buying a **Book** (a supply of kind Book), or a **Friend** at a
friendship level. **Is Craft** makes it a whole craft (its items can be ordered). A recipe step can need
a technique: put the skill id in the step's **Required Skill** (e.g. French knots). **Grants Patterns**
adds patterns to the book when learned.

## Shop upgrades

In `Data/Upgrades/` (**Create > Craft > Upgrade**): name, price, the chapter it's offered from, and
its effect: Display Spot (more patterns on display), Extra Visitor (a customer drops by twice), or
Craft Lamp (Amount = % more experience). They're sold on the market's "For the Shop" shelf.

## The tutorial ("How to Play")

The tutorial shows the first time the app is opened, and from the title screen's How to Play button.
Its pages are in `Data/Tutorial.asset`: click it and edit each page's **Title** and **Text**. To use
your own illustration on a page, drag a sprite into its **Art** (any size; it keeps its shape and fits
the picture area, about 1700 x 1150). Pages without art show a simple animated placeholder scene.

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

5. **Preferences** (optional): things they like or dislike, each with an **Id** (permanent), **Text** (as the
   Journal shows it, e.g. "wooden buttons"), **Likes**, and the **Supply** it's about (a special touch,
   one with "Decorates" set). Adding that supply to their order then always pleases or disappoints them.
6. **Shop Owner**: tick it for people who run a shop the player visits (Bram at the market, Celeste at
   the boutique). They never come in as customers; the player chats with them in their own shop (once a
   day; their friendship scenes play there too). Buying from them also adds friendship.

New customers start visiting the shop on the next day.

## The boutique (hair, clothes, accessories)

In `Data/Cosmetics/` (**Create > Craft > Cosmetic**): id (permanent), name, price, **Slot** (Hair, Top,
Accessory), **Style Index** and a placeholder **Color**/**Icon**. Hair styles: 0 short, 1 bob, 2 long,
3 top knot, 4 big curls (0-2 are free). Tops: 0 jumper, 1 apron, 2 cardigan (0-1 free). Accessories:
0 glasses, 1 hat, 2 brooch, 3 neckerchief. The shopkeeper is drawn from simple shapes for now
(UI/ShopkeeperView.cs); your layered drawings per option and colour will replace them.

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


## Patterns for the pattern shop (Ada Finch's)

A pattern sold at the pattern shop is a normal Pattern asset (Assets/_Project/Data/Patterns) with **For Sale** ticked,
**Starts On Shelf** unticked, a **Rarity** (Always = common, Rotating = uncommon, Rare) and a **Shop Price** (what the
player pays). **Price** is what it then sells for on the display. Each 4-day period the shop stocks 3 common and
2 uncommon patterns, and sometimes 1 rare one, only for crafts the player has learned. Rarer patterns should be bigger,
use more colours and half stitches: the shop shows a complexity of 1 to 5 worked out from that.
