# Thimble & Thread: project context for Claude Code

Read this first. It is the full handoff from earlier planning chats.

## What we're building
A cozy, Coffee Talk-style game in Unity. Instead of serving coffee, the player runs a craft shop
and makes and sells finished crafts AND patterns (sold independently of each other):
cross stitch, knitting, embroidery, sewing, crochet.
Items include sweaters, stuffed animals, tablecloths, scarves, tote bags and more.

- Each craft has a minigame that mimics the real action. A separate Pattern mode
  fills in grid pixels to design a pattern.
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
- The owner makes ALL final illustrations in Procreate (raster), exported as high-res PNG.
  Stay with PNG (no SVG). Art should be drawn at or above its target size since raster can't scale up cleanly.
  Do NOT generate final art. Use placeholder sprites only (see Art workflow).

## Tech decisions
- Unity 6.6 (a Supported release, LTS-level support until 6.7 LTS), 2D (URP), C#. Fast Enter Play Mode is the default in 6.6 new projects, so static fields persist between Play sessions; reset any static state with [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)].
- Dev machine is Windows; shell commands are run in Git Bash (use bash syntax and /c/Users/... paths, not PowerShell).
- Platform order: Android first (sideloadable APK, built by GitHub Actions), then PC, iOS later.
  Reason: Apple developer costs. Switch is a possible later port.
- Touch-first. Keep all input in one wrapper so PC mouse support comes cheaply.
- Dialogue: plan to use Ink or Yarn Spinner.
- Backup and sync via GitHub with Git LFS for art and audio.

## Repo layout
- Assets/_Project/{Art,Audio,Data,Dialogue,Prefabs,Scenes,Scripts,Settings}
- Assets/_Project/Scripts/{Core,Crafts,Data,Dialogue,Economy,UI,Utility,Editor}
- SourceArt/ holds the artist's masters (.procreate etc.). It is OUTSIDE Assets on purpose.
- Docs/ holds ART_SIZES.md, ART_SWAP.md, STORYLINE.md.
- .github/workflows/build-android.yml builds an APK (needs secrets UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD).

## Code that already exists
- Data/CraftData.cs: CraftType enum, StitchCell, Pattern,
  SaveData (coins, day, ownedSupplies, unlockedItems, relationships), CustomerRelationship.
- Data/CraftItemDef.cs: CraftItemDef (ScriptableObject). A ScriptableObject class must live in a
  file with the same name, or its assets lose their script link.
- Data/CustomerDef.cs: Expression enum, CustomerDef (portraits, requestable items by relationship level).
- Data/SupplyDef.cs: supply with craft, price, tier.
- Data/PatternDef.cs: book pattern (grid drawn as text). Data/ContentLibrary.cs: all content in one asset,
  kept in sync automatically by Editor/ContentLibraryRefresher.cs. Runtime code reads content from it.
- Docs/ADDING_CONTENT.md: how the owner adds supplies, items, customers, portraits and patterns.
- Crafts/CrossStitchCanvas.cs: stitch mode and pattern-fill mode, Accuracy(target) method.
- Core/GameManager.cs: singleton with ScreenId state, coins, relationships, day, events.
- Core/SaveSystem.cs: JSON save to persistentDataPath.
- Economy/PricingCalculator.cs, OrderSystem.cs (Order, OrderGenerator), ShopManager.cs.
- UI/ScreenRouter.cs: shows or hides screen panels for the current ScreenId.
- Editor/PlaceholderGenerator.cs: menu Tools > Thimble & Thread > Generate Placeholder Art And Data.
  Makes placeholder PNGs at real target sizes plus sample data. Never overwrites existing files.

IMPORTANT: this code was written without being compiled. First job in any new session:
open the project, check the console, and fix any compile errors.

## Screen flow
Title > Save select > Shop (day hub).
- Day: Greet a customer > Dialogue > buy a pattern / accept an order or commission / Not today > Shop.
  Order book > a finished order > Hand over. Supply Market. "Close the shop" > End-of-day summary > Workroom.
- Night (Workroom hub): Order book > pick an order > Workbench > Minigame > "Done for now" (keeps the work)
  or Finish > Result (set aside for hand-over tomorrow, or keep working). Pattern Studio. "Go to bed" > next day.
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
- The artist replaces placeholders by overwriting a PNG with the SAME filename and SAME aspect ratio.
  Keep the .meta files. Never rename or move assets outside Unity.
- Do not delete or regenerate existing art files.

## Conventions and gotchas
- Data-driven: new items, customers and supplies are ScriptableObjects, not hard-coded.
- New Unity projects may default to the new Input System. CrossStitchCanvas uses classic Input,
  so Active Input Handling must be set to Both (or Input Manager Old).
- Always commit .meta files. Never commit Library/, keystores, or build output.
- Track PNG, PSD, .procreate and audio files with Git LFS (see .gitattributes).
- Avoid anything that raises modal pop-ups in the Unity editor. The owner often works remotely
  from a phone and cannot click dialogs. Editor/AutoSaveBeforeReload.cs saves scenes before reloads.
- Edit scenes through Unity MCP tools, not by editing .unity files on disk while the scene is open.

## Working style
- Work on a git branch and commit after each meaningful change, with a clear message.
- Keep changes small and easy to review. The owner may be reviewing on a phone.
- Explain what changed in a few plain sentences.
- Ask before big refactors or deleting files.

## Remote setup (already planned)
- Claude Code Remote Control: run `claude remote-control` in the project folder, keep the terminal open.
- Unity MCP bridge: MCP for Unity by CoplayDev. Register with
  `claude mcp add --transport http UnityMCP http://localhost:8080/mcp`
  Unity must be open with the project loaded and the MCP server started.
- Headless alternative: Unity batch mode (-batchmode -nographics -quit -projectPath <folder>)
  for compile, tests and builds. It cannot run while the editor has the same project open.

## Next tasks (suggested order)
1. Verify everything compiles; run the placeholder generator; build a test scene with ScreenRouter.
2. Workbench flow: Order book, save slots with autosave, resumable work, Result screen
   (pricing, stars, relationship points). Then, in stages: day/night mode; pattern shelf and
   pattern orders; supplies used per project; commissions.
3. ICraftMinigame interface so every craft plugs in the same way; refactor CrossStitchCanvas to implement it.
4. Dialogue system (Ink or Yarn Spinner) with portrait and expression hooks, including indirect
   requests: the player picks which item fits, scored by how well it matches.
5. Knitting and crochet minigames, then embroidery and sewing.
6. Pattern Studio and pattern marketplace (create, name, price, sell patterns).
   Include friend share codes: export a pattern or item as a code, import a friend's code.
7. Supply market UI, Journal (relationships), End-of-day summary.
8. Audio manager, touch input wrapper, settings (volume, text speed).
