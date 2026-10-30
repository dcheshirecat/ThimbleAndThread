# Thimble & Thread: project context for Claude Code

Read this first. It is the full handoff from earlier planning chats and sessions.

## What we're building
A cozy, Coffee Talk-style game in Unity. Instead of serving coffee, the player runs a craft shop
and makes and sells finished crafts AND patterns (sold independently of each other):
cross stitch, knitting, embroidery, sewing, crochet.
Items include sweaters, stuffed animals, tablecloths, scarves, tote bags and more.

- Each craft is its own thing, whatever you're making: a sewn plushie and a crocheted plushie are
  different items with different steps (both will exist; build the sewn one first).
- Making an item mimics how it's really made, step by step. Each item has its own sequence of steps
  (a "recipe"), and each step is a small minigame of the real action. Items never reuse another
  craft's minigame as a stand-in once their own steps exist. Examples:
  - Sewn plushie: choose fabric, cut the pieces along the pattern lines, pin, sew the seams, turn it
    right side out, stuff it, attach the eyes, close the opening.
  - Crocheted plushie (amigurumi): magic ring, crochet the rounds, stuff, add eyes, sew the parts together.
  - Tote bag: cut the panels, sew the sides, hem the top, attach the handles.
  - Tablecloth: cross stitch the motif, then hem the edges.
  The player can stop between (or during) steps and pick up later; progress is saved per step.
  The basic actions per craft:
  - Cross stitch: drag corner-to-corner to make a half stitch, drag the opposite diagonal to finish the X.
  - Knitting: alternate two-needle swipes, one per stitch; rhythm affects quality. Knit and purl rows
    make the texture.
  - Crochet: hook and pull. Tap to hook a loop, flick to pull it through. Rounds spiral outward (good for plushies).
  - Embroidery: trace a printed outline with the needle; accuracy along the line sets quality. Stitch types
    (backstitch, satin fill, French knot) use different gestures.
  - Sewing: cut pattern pieces, then guide fabric under the machine needle along the seam line,
    pressing to run the machine and controlling speed.
- Coffee Talk-style requests: customers often describe what they want indirectly ("something warm,
  like my mother used to make") and the player interprets it. Several items can fit; a better match
  earns more relationship points.
- Relaxed pacing, not a timed minigame gauntlet. Work is a set of tasks the player picks up or puts
  down at any time:
  - Order book: accepted requests go into a list. Several can be open at once. No deadlines,
    nothing expires, no late penalty.
  - Pause and resume: leaving a minigame keeps the work (e.g. every stitch), even across app restarts.
  - Unfinished orders simply carry over to the next day.
  - Story quests (e.g. find Tobias's tablecloth pattern, a quilt patch) live in the same list as orders.
- Day and night. Neither is timed; each lasts as long as the player likes.
  - Day (shop open, day background): customers visit; sell patterns from the shelf; take new orders;
    hand over finished orders (coins + friendship); buy supplies. No crafting during the day.
  - "Close the shop" switches to night (one way, ends the day's trading) with an end-of-day summary.
  - Night (shop closed, workroom background): work on orders in the minigames (pause/resume as above);
    design patterns in the Pattern Studio. "Go to bed" starts the next day.
- What customers want (each visitor does one of these):
  - Buy a pattern off the shelf: instant sale.
  - Pattern order: pick a pattern from the shelf and have the finished item made.
  - Commission: a rarer special quest. It needs a brand-new pattern designed for it (not judged or
    scored), and may need supplies or a technique the player doesn't have yet, or rarer materials
    (e.g. 100% alpaca yarn instead of regular). After delivery its pattern can go on the shelf.
- Pattern shelf: starts with a few patterns from the great-aunt's book (recovered pages add more);
  patterns designed at night can be shelved at a price the player sets.
- Techniques: story chapters unlock each craft; smaller techniques (French knots, cables...) come
  from technique books bought at the market or from recovered pattern-book pages.
- Supplies are used up per project, so the player restocks: cheap basics, pricier premium materials.
  Realistic amounts (owner's choice, for depth): supplies are counted, e.g. a skein of floss covers a
  fixed number of squares, a ball of yarn a number of stitches, fabric is cut by size; designs show
  how much of each material they need. New games start with a small kit (white Aida, a few basic
  thread colours) so the first order can be made right away; everything else is bought.
- Supply Market (owner's decisions):
  - Stock changes every 4 days (days 1-4, 5-8, ...). Three tiers: a few basics are ALWAYS for sale
    (white Aida, basic floss colours, stuffing, a basic needle), a rotating selection (~10 items) changes
    each period, and now and then a RARE special item appears (e.g. 100% alpaca, linen evenweave,
    metallic floss), useful for commissions. The selection is seeded per save, so saves differ. The
    market shows how many days until new stock.
  - Looks like a store, not a list (owner's choice): shelves per kind (Fabric, Embroidery Floss, Yarn,
    Notions, Tools), each a grid of tiles with the supply's icon (SupplyDef.icon, 512 x 512), name,
    price and how many you have. Rare items get a gold tile.
  - Real variety: fabrics (Aida counts, evenweave linen, felt, quilting cotton, fleece/minky, canvas),
    threads (many floss colours, metallic), yarns (acrylic, wool, merino, cotton, 100% alpaca, mohair,
    by weight and colour), tools (embroidery hoops by size, tapestry/embroidery needles, crochet hooks and
    knitting needles by size, fabric scissors, pins, marking pen) and notions (stuffing, safety eyes,
    buttons, ribbon, zips, interfacing).
- Tools are bought once and kept, UNLESS a project uses one up as part of the product (e.g. a customer
  wants framed embroidery in a special hoop: that hoop is a material of that order and is consumed).
- Tools gate work: each step lists the tools it needs (e.g. a cross stitch pattern bigger than the
  hoop needs a bigger hoop: 4" hoop up to 10x10, 6" up to 12x12, 8" up to 16x16; knitting needles and
  crochet hooks must match the yarn weight). The Workbench lists missing tools like missing materials.
- Materials: the player can only craft with materials they own (bought). Minigames offer only owned
  threads, yarns, fabrics and notions (eyes, stuffing...). Designing is free: the Pattern Studio shows
  every colour/material the market sells, owned or not, and a design may use any of them; to make it,
  the player must first buy what's missing (the game lists it and points to the Supply Market).
- Save files: 3 slots on a Save select screen (day, coins, last played; New Game / Continue / Delete,
  with an in-game confirm, never an editor or OS pop-up). Autosave on every meaningful change and when
  the app is backgrounded; no manual save button. The save holds everything: order book, in-progress
  work, quests, relationships, supplies, coins, designed patterns. Saves carry a format version and
  are upgraded on load; writes go to a temp file first so a crash can't corrupt a slot.
- Economy: sales earn coins. Coins buy supplies (cloth, yarn, thread). Higher material tier
  gives higher sale prices.
- Customers have relationship levels. Higher levels unlock new requests, dialogue, and story.
- Players can sell patterns and items to friends via share codes (a short code, QR or small file
  the friend imports). No backend, accounts or networking packages. An online friends marketplace
  may come later; real-time multiplayer is not planned.
  Eventually (owner, 2026-10-30): players trade SUPPLIES with each other too, and buy each other's
  patterns. Keep this possible: supplies and patterns are plain data keyed by permanent ids
  (supplyId + amount; a Pattern's grid + thread supplyIds), so a share code can carry them. A gift or
  trade code must remove the items from the sender's save when it's made, so nothing gets duplicated.
- The owner makes ALL final illustrations in Procreate (raster), exported as high-res PNG.
  Stay with PNG (no SVG). Art should be drawn at or above its target size since raster can't scale up cleanly.
  Do NOT generate final art. Use placeholder sprites only (see Art workflow).

## Tech decisions
- Unity 6.6 (a Supported release, LTS-level support until 6.7 LTS), 2D (URP), C#. Fast Enter Play Mode is the default in 6.6 new projects, so static fields persist between Play sessions; reset any static state with [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)].
- Dev machine is Windows; shell commands are run in Git Bash (use bash syntax and /c/Users/... paths, not PowerShell).
- Platform order: Android first (sideloadable APK, built by GitHub Actions), then PC, iOS later.
  Reason: Apple developer costs. Switch is a possible later port.
- Portrait only, phone first (owner's choice: landscape is too small to read).
- Touch-first. Keep all input in one wrapper so PC mouse support comes cheaply.
- Dialogue: plan to use Ink or Yarn Spinner.
- UI is Unity UI (uGUI) with the legacy Text component for now (TextMeshPro's essentials importer pops
  up a dialog). Will be restyled once the owner's UI art exists.
- Backup and sync via GitHub with Git LFS for art and audio.

## Repo layout
- Assets/_Project/{Art,Audio,Data,Dialogue,Prefabs,Scenes,Scripts,Settings}
- Assets/_Project/Scripts/{Core,Crafts,Data,Dialogue,Economy,UI,Utility,Editor}
- SourceArt/ holds the artist's masters (.procreate etc.). It is OUTSIDE Assets on purpose.
- Docs/: ART_SIZES.md, ART_SWAP.md, ADDING_ART.md, ADDING_CONTENT.md, STORYLINE.md.
- .github/workflows/build-android.yml builds an APK.

## Code that already exists (all compiles and is smoke-tested; Assets/_Project/Scripts)
- Data/CraftData.cs: CraftType, StitchCell, Pattern (grid + palette), StitchMark (x, y, mask, color),
  Pattern.threads (supplyId per palette colour), SupplyStock, MaterialNeed, OwnedPattern,
  SaveData (version 5: coins, day, night, stock, orders, visitorsToday, patterns, today counters,
  relationships; ownedSupplies is legacy, converted into stock on load), CustomerRelationship.
- Data/CraftItemDef.cs, CustomerDef.cs, SupplyDef.cs, PatternDef.cs (book pattern; grid drawn as text),
  ContentLibrary.cs (every content asset; Editor/ContentLibraryRefresher.cs keeps Data/ContentLibrary.asset
  in sync on any change under Data/). A ScriptableObject class must live in a file with the same name,
  or its assets lose their script link (m_Script fileID 0); this bit CraftItemDef once.
- Core/GameManager.cs: singleton (DefaultExecutionOrder -100), ScreenId enum (append new values at the
  end), save slot, HubScreen (Shop by day / Workroom by night), Persist() = autosave.
- Core/SaveSystem.cs: 3 slots, temp-file writes, version upgrade, never overwrites an unreadable slot.
- Core/ShopDay.cs: the game loop: visitors, shelf sales and pattern orders, day/night, order book,
  crafting, set aside / hand over, Pattern Studio actions (price, shelf, new/edit/save/delete design).
- Crafts/CrossStitchCanvas.cs: stitch and paint modes, thread colour per stitch, undo per drag,
  Score() counts right-colour squares. Crafts/MinigameHost.cs opens the order's current recipe step
  (ICraftStep) in the screen band 0.40-0.86 of a portrait screen, and also opens the board as the
  designer canvas. Steps: CrossStitchStep; PieceStep (base: touch in 0..1 of the area, piece shape,
  drawing helpers) with TracingStep (Hem, CutPieces, SewSeam, CloseOpening) and ChooseFabric, Pin,
  TurnRightSideOut, Stuff, CastOn, KnitRows (KnitLayout), CrochetRounds, ChooseMaterial; PlacementStep (tap where things go: AttachEyes, AttachHandles); bag steps
  SewSides and HemTop, CastOff, MagicRing, HoopFabric, Backstitch (TracingStep), SatinFill, FrenchKnots
  (PlacementStep). New step kinds: append to StepKind (Data/Recipe.cs), implement
  ICraftStep (usually via PieceStep), add to MinigameHost.Create and a hint in OrderFlowUI.StepHint.
  Crafts/PlaceholderPatterns.cs.
- Economy/OrderSystem.cs (Order with saved work, materialsUsed, fabricId; OrderGenerator), PricingCalculator.cs,
  ShopManager.cs (counted stock, Buy packs, starter kit, Missing/Use), Materials.cs (what a pattern needs:
  Aida = width x height squares, floss = squares per colour; EnsureThreads maps old colours to floss).
- SupplyDef has kind, color, price per pack, perPack, unit, starterPacks. Floss supplies (kind Thread)
  are the Pattern Studio's colours. Materials are used when work first starts (fabric cut, floss set aside);
  starting is refused until everything is owned; the Workbench lists what to buy.
- UI/ScreenRouter.cs, OrderFlowUI.cs (shop/workroom/order/pattern screens), SaveSlotsUI.cs,
  PatternDesignerUI.cs, ThreadSwatches.cs.
- Editor/TestSceneBuilder.cs generates Scenes/TestRouter.unity (Tools > Thimble & Thread > Rebuild Test
  Scene). The scene is fully generated: change the builder and rebuild; don't hand-edit the scene.
- Editor/PlaceholderGenerator.cs: placeholder PNGs + sample data + 4 book patterns; never overwrites.
- Editor/AutoSaveBeforeReload.cs: saves only scenes that have a path (no Save As pop-up).
- Docs/ADDING_CONTENT.md and Docs/ADDING_ART.md are the owner's guides. Keep them current.

## Screen flow
Title > Save select > Shop (day hub).
- Day: Greet a customer > Dialogue > buy a pattern / accept an order or commission / Not today > Shop.
  Order book > a finished order > Hand over. Supply Market. "Close the shop" > End-of-day summary > Workroom.
- Night (Workroom hub): Order book > pick an order > Workbench > Minigame > "Done for now" (keeps the work)
  or Finish > Result (set aside for hand-over tomorrow, or keep working). Pattern Studio
  (list > pattern page > designer). "Go to bed" > next day.
Side screens from either hub: Journal (relationships and unlocks), Settings.

## Storyline: "Thimble & Thread"
The player inherits their great-aunt's haberdashery in a seaside town. It opens at dusk.
Chapters unlock one craft each: 1 Cross stitch, 2 Knitting, 3 Embroidery, 4 Crochet (a rival shop opens),
5 Sewing (the festival Community Quilt needs one patch from each befriended customer;
the ending depends on relationship levels). Running mystery: the great-aunt's pattern book is
missing pages; maxing a customer's relationship earns a page.
Starter customers: Mara (new parent, soft items), Old Tobias (fisherman, wants his late wife's tablecloth
pattern), Juno (teen, wants knitwear that doesn't look grandma-made).

## Art workflow (important)
- The game is PORTRAIT only (phones first). UI reference canvas 2048 x 2732, scaled by width, so taller
  phones get extra height; lay screens out with anchors as fractions of the screen.
- Backgrounds are 2048 x 4440 (tall phone shape) and fill the screen by cropping, never stretching;
  important content stays in the middle 2048 x 2732, which shows on every device. Sizes: Docs/ART_SIZES.md.
- Procreate canvases should be sRGB (Display P3 shifts colours in Unity). See Docs/ADDING_ART.md.
- The artist replaces placeholders by overwriting a PNG with the SAME filename and SAME aspect ratio.
  Keep the .meta files. Never rename or move assets outside Unity.
- Do not delete or regenerate existing art files.

## Conventions and gotchas
- Data-driven: new items, customers, supplies and patterns are ScriptableObjects, not hard-coded.
- IDs (itemId, supplyId, customerId, patternId) are permanent once saves exist: saves store ids.
- Bump SaveData.CurrentVersion and add an upgrade step in SaveSystem.Upgrade when adding save fields.
- New Unity projects may default to the new Input System. CrossStitchCanvas uses classic Input,
  so Active Input Handling is set to Both.
- Always commit .meta files. Never commit Library/, keystores, or build output.
- Track PNG, PSD, .procreate and audio files with Git LFS (see .gitattributes).
- Avoid anything that raises modal pop-ups in the Unity editor. The owner often works remotely
  from a phone and cannot click dialogs.
- Edit scenes through Unity MCP tools, not by editing .unity files on disk while the scene is open.
- Bash heredocs containing Python with quotes have broken several times; for multi-line patches prefer
  the Edit/Write tools or a script file written with Write.

## Working style
- Work on a git branch and commit after each meaningful change, with a clear message.
- Keep changes small and easy to review. The owner may be reviewing on a phone.
- Explain what changed in a few plain sentences.
- Ask before big refactors or deleting files.
- Write important decisions into this file as they are made (context gets compressed).

## Environment
- Repo: C:\Users\julie\ThimbleAndThread (GitHub dcheshirecat/ThimbleAndThread, private). Unity 6.6 editor:
  D:\6000.6.0f1\Editor\Unity.exe (Unity Hub installs to D:\). Android Build Support is NOT installed
  locally yet (only Windows standalone), so APKs come from GitHub Actions.
- App: product "Thimble & Thread", company "cheshirecat", id com.cheshirecat.thimbleandthread, portrait
  only. Saves: %USERPROFILE%/AppData/LocalLow/cheshirecat/Thimble & Thread/save_<slot>.json.
- D:\temp-unity was the throwaway project used to create settings; no longer needed.
- Claude Code Remote Control: `claude remote-control` in the project folder.
- Unity MCP bridge (not installed yet; the owner will set it up): Package Manager > Add package from git
  URL `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`; its wizard configures Claude
  Code (HTTP http://localhost:8080/mcp), then restart Claude. Once the editor has the project open, batch
  mode can't run on it; work through the bridge instead. Check the package is editor-only before committing.

## Build and release
- Pushing to main runs .github/workflows/build-android.yml (GameCI, about an hour; it frees runner disk
  space first, which is required). APK: Actions > Build Android > run > Artifacts. Workflows can also be
  run on a branch via Run workflow. Secrets UNITY_LICENSE/EMAIL/PASSWORD are set (Personal license).
- Merging: the owner approves merges to main. Fast-forward main from the feature branch
  (git push origin origin/<branch>:refs/heads/main); never force-push. The owner wants a new build only
  when there is something to try, so hold merges until a feature is complete.
- The package-added define APP_UI_EDITOR_ONLY is committed in ProjectSettings on purpose (without it the
  first Play triggers a recompile).

## How Claude tests (headless, while the editor is closed)
- Compile: Unity.exe -batchmode -nographics -quit -projectPath <repo> -logFile <file>; grep "error CS".
- Rebuild scene / generate data: add -executeMethod TestSceneBuilder.Rebuild or PlaceholderGenerator.Generate.
- Play-mode smoke tests: a temporary Editor/TempSmokeTest.cs (never committed; delete it and its .meta
  after). Run WITHOUT -nographics and WITHOUT -quit, with -executeMethod TempSmokeTest.Run. Required
  tricks: batch mode doesn't advance frames, so on each EditorApplication.update set isPaused = true and
  call EditorApplication.Step(); a domain reload can happen right after entering Play, so keep a
  SessionState "running" flag and an [InitializeOnLoad] static ctor that exits and re-enters Play mode
  and restarts the steps. Click buttons with GameObject.Find("Button_<LabelWithoutSpaces>") and
  onClick.Invoke(); drive the board via reflection (OnPress/OnRelease). Screenshots: Canvas to
  ScreenSpaceCamera, camera targetTexture = RenderTexture at phone size (1080x2400), ReadPixels, save
  PNG, then view it. Use `timeout` so a hung editor can't block.
- Move the owner's real save files aside before a test and restore them after.

## Open questions for the owner
- Replace the three landscape placeholder backgrounds with portrait-sized placeholders? (Ask first:
  "do not regenerate art files".)
- Player character customization: player only, or customers too? Separate colour drawings or tints?

## Status
Done and on main: project setup, save slots + autosave, order book with resumable work, day/night,
pattern shelf (sales + pattern orders), book patterns, content library, portrait layout, thread colours,
undo, Pattern Studio designer, owner guides, materials (counted supplies, starter kit), rotating Supply
Market with shelves, tools that gate work, item recipes (tablecloth: stitch + hem; Sewn Plushie:
8 sewing steps; tote bag: 5 steps; scarf and sweater: knitting; crocheted plushie: crochet; Embroidered Hoop: embroidery), commissions. Everything is placeholder art and Unity default UI.
Still stand-ins: items without their own recipe (sampler) are
made with the cross stitch board; Journal and Settings screens are empty.
CI: main failed with exit code 137 (out of memory) and once with "no space left on device" while pulling
the Unity image. The workflow deletes preinstalled runner tools, adds 12 GB swap on /mnt, and (since
2026-10-30, commit 764543c) sets unity-builder's dockerMemoryLimit: 18g: GameCI caps the build container
at 95% of RAM by default, so the swap was out of reach and 137 kept happening. Not yet confirmed green.

Tutorial (branch tutorial, merged): Data/Tutorial.asset (TutorialDef: pages with title, text, optional art,
TutorialAnim placeholder scene). UI/TutorialUI.cs shows it on first launch (PlayerPrefs "tutorial_seen") and
from the title's How to Play button (ScreenId.Tutorial); placeholder scenes are code-drawn UI shapes.

## Owner feedback, 2026-10-30 (after playing; drives the next tasks)
1. Stitches should look realistic (not flat lines/squares).
2. Some thread colours are so bright that finished stitches are hard to see.
3. Crafting steps should flow from one to the next, without going back to the Workbench between steps.
4. Dialogue with customers, and special scenes at friendship thresholds. Some characters romanceable.
5. More depth and flow: something that pulls you in; right now it feels flat. Crafting more complex and realistic.
6. Limited display space: only N patterns on show at a time; choose which based on what sells.
7. Skills unlockable (crafts and pattern creation), not all from the start. Journal pages: friendship
   progress, unlocked crafts, inventory, etc.
8. Supply store: items marked by rarity; once bought, an item is out of stock for the next two days.
Owner's answers (2026-10-30):
- Romance: NEW adult characters written as romance options (2-3; e.g. the rival shop owner of chapter 4).
  Mara, Juno and Tobias stay friendship-only. Juno is a teen: never romanceable.
- Dialogue: Claude drafts in Ink (plain-text .ink files the owner can edit anywhere); the owner edits.
- Unlocking: all three combined: story chapters unlock crafts (1 cross stitch, 2 knitting, 3 embroidery,
  4 crochet, 5 sewing), crafting experience unlocks techniques and pattern design, books/lessons can be
  bought, and friends can teach things at friendship milestones.
- Display: 4 pattern spots at the start, more via shop upgrades; the shelf shows how each pattern sells.
DONE 2026-10-30 (feedback 1, 2, 3, 6, 8): cross stitch board looks like Aida (fabric, woven holes, faint
chart) and stitches are shaded floss (shadow, body, sheen) that stay visible in any colour; seams and hems
are dashed stitches (TracingStep.StitchLength; 0 = a continuous cut or yarn); knit/purl/crochet stitches
are shaded tapered loops; eyes, knots, pins and stuffing are round dots with a shine (PieceStep.Dot).
Finishing a step goes straight into the next (stops at the Workbench only if a tool/material is missing).
Display: SaveData.displaySpots (4); OwnedPattern.sold/ordered shown in the Pattern Studio; customers who
find nothing say what they hoped for. Market: rarity (Common = Always, Uncommon = Rotating, Rare), shelf
packs 6/3/1 (ShopManager.ShelfPacks); when the last is bought it's sold out for the rest of the day and the
next two (SaveData.marketBought). Owner asked for "out of stock after buying"; packs per item keep basics
like floss buyable more than once (explained to the owner). Save version 7.
DONE 2026-10-30 (feedback 4, 5, 7):
- Progression (Core/Progression.cs): SkillDef (crafts + techniques; learned via chapter, craft level, book,
  or friend), ChapterDef (5 chapters, goals), UpgradeDef (display spots, extra visitor, lamp), craft
  experience (60 per level). New games: chapter 1, cross stitch only; saves from before v8 learn everything.
  Customers only order what you can make but buy any displayed pattern. Commissions need Pattern Design.
- Journal (UI/JournalUI.cs): Story, Friends (with romance), Skills, Supplies, News.
- Dialogue: Ink files in Assets/_Project/Dialogue read by Dialogue/InkLite.cs (our own subset reader; the
  official ink-unity-integration package was blocked by the permission check as untrusted third-party code,
  so ask the owner before adding it; if added, delete Editor/InkTextImporter.cs). Visits: scene due ->
  greeting -> commission / indirect request (offer a displayed pattern; best fit = +50% friendship) /
  browse. Hand-overs: reaction + scene. Story scenes queue in SaveData.pendingScenes. UI/DialogueUI.cs.
- Characters: Isla Marlowe (lighthouse keeper, chapter 2) and Rowan Hale (they/them, Knot & Needle,
  chapter 4) are romanceable (scene 3 choice -> ~ romance(1); scenes 4-5 branch on {romance}).
- Depth: finishing steps (Press, WeaveEnds), sampler backstitch outline, chapter line on the shop screen,
  "Looking ahead" on the end-of-day summary. Save version 9.
Still open from the feedback: more complex crafting can keep growing (e.g. half stitches, cables, colour
changes); dialogue drafts await the owner's edits; character art for Isla and Rowan.

All eight feedback points have a first version (merged to main 2026-10-30).

## Next tasks (in this order)
1. DONE (on main): Materials, rotating Supply Market with shelves, tools that gate work.
2. DONE (on main): item recipes. CraftItemDef.steps (RecipeStep: StepKind, label,
   extra tools); Order.step + Order.steps (StepProgress: done, score, progress, traced points); ICraftStep
   (Begin/SaveWork/Undo) with CrossStitchStep and TracingStep (drag along a dashed guide; HemStep is the
   first). Finishing a step returns to the Workbench with the next one; quality = average of step scores.
   Tools are checked per step. The tablecloth is now "Stitch the motif" then "Hem the edges".
3. DONE (on main): the Sewn Plushie (item plushie_sewn, CraftType Sewing; the
   old "plushie" item is now named Crocheted Plushie, same id). 8 steps: Choose fabric, Cut, Pin, Sew seam,
   Turn right side out, Stuff, Attach eyes, Close opening. Book pattern "Felt Bunny"; Mara asks for it.
   Sewing patterns use the grid: filled squares = the piece's shape (PieceShape smooths the outline,
   inset seam line with a gap at the bottom), colour index 1 = eye marks. Each RecipeStep lists its tools,
   materials and fabricPieces (of the fabric chosen in Choose Fabric); a step's materials are used when
   it's started. The great-aunt's sewing machine is a NotSold starter tool (older saves get it on load);
   scissors, pins, hand sewing needles, cream felt, stuffing and safety eyes are always on sale.
   Starter book patterns added in an update now appear in older saves.
   Tote bag DONE: Choose fabric, Cut the panels, Sew the sides, Hem the top, Attach the handle (uses Cotton
   Webbing, always on sale). Book pattern "Market Tote"; Juno asks for totes from level 0. PieceShape also
   gives bags a Mouth (top edge), Sides and HandleEnds; corner smoothing cuts at most half a grid square.
   Still to do here: machine speed control on Sew Seam (press to run).
4. DONE (on main): knitting and crochet. Scarf and sweater: Choose yarn, Cast on (tap per
   stitch), Knit the rows (one swipe per stitch: down = knit, up = purl; pattern's filled squares are
   purls; a steady rhythm = even stitches; a wrong swipe is a mistake), Cast off (trace the top edge).
   Crocheted plushie: Choose yarn, Magic ring (trace a circle), Crochet the rounds (tap to hook, flick to
   pull through; 3 rounds of 6/12/18), Stuff, Attach eyes, Close it up. ChooseMaterialStep handles fabric
   and yarn; RecipeStep.fabricPieces is the amount of the chosen material (perPatternSquare multiplies by
   the pattern's squares); ToolNeed.matchYarn/exactSize make needles and hooks match the yarn weight.
   Acrylic yarn, 5mm needles and the 5mm hook are always on sale.
5. DONE (on main): embroidery. Item hoop_art "Embroidered Hoop" (Juno, level 0), book pattern "Little Whale".
   Steps: Hoop the fabric (trace the ring; uses White Embroidery Cotton), Backstitch the outline (trace),
   Fill with satin stitch (drag back and forth; coverage, minus stray stitches), Add French knots (twirl
   twice round the mark, then let go; PlacementStep). Floss comes from the pattern's own threads via
   RecipeStep.patternThread/threadAmount. Embroidery needles and embroidery cotton are always on sale.
   Mesh colours go through PieceShape.VertexColor (linear colour space), or filled pieces look too light.
6. DONE (on main): commissions. CommissionDef assets (Data/Commissions): customer, item, min level,
   request text (indirect), optional requiredSupply, bonus coins/friendship. A visitor with an eligible
   commission (level reached, not done, not already in the book) offers it 30% of the time instead of
   browsing. The order starts with needsDesign: Start opens the Pattern Studio designer (item locked,
   empty designs refused); the design becomes order.target, then the item's recipe as usual. Required
   supply restricts the Choose step (or the Aida for cross stitch). Hand-over adds the bonus, records it in
   SaveData.commissionsDone (save version 6) and puts the pattern in the book, off the shelf.
   Commissions so far: Juno's skate-park scarf (level 0), Mara's moon bunny (sewn plushie, pink minky,
   level 1), Tobias's Sunday cloth (tablecloth, linen evenweave, level 1).
7. DONE: dialogue, Journal, unlocks, chapters, upgrades (see "Owner feedback" above).
8. Next: friend share codes (patterns, items, supplies); audio; Settings screen; touch input wrapper;
   sewing machine speed control; more complex crafting (half stitches, cables, colour changes).
