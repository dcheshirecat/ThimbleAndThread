# Swapping placeholder art for real art

Every picture in the game is a placeholder until you replace it. The full list of what to draw, with the exact file
name for each, is [ART_LIST.md](ART_LIST.md). Sizes and the colour profile (sRGB) are in [ART_SIZES.md](ART_SIZES.md).

## The usual way: the shared Google Drive folder

1. **Draw** in Procreate on an **sRGB** canvas, at the size in ART_LIST.md or bigger with the same shape.
2. **Export as PNG** (Share > PNG) and **name it exactly** as in ART_LIST.md: lowercase, underscores, `.png`
   (e.g. `customer_mara_happy.png`). The name is how the game knows what it is.
3. **Put it in the Google Drive folder "Game Art - Thimble and Thread".** From the iPad: Share > Save to Files >
   Google Drive > Game Art - Thimble and Thread. Subfolders are fine (e.g. `Characters`, `Mockups`); only the
   file name matters.
   - Screen mock-ups (`mockup_....png`) go in a `Mockups` subfolder: they're for Claude to build screens from.
   - Want to keep a layered master? Put the `.procreate` file in a `Masters` subfolder. Claude copies it to
     `SourceArt/` (outside the game).
4. **Tell Claude** "new art is in the Drive folder" (or name the files). Claude then:
   - copies each PNG over the placeholder of the same name in `Assets/_Project/Art/` (keeping its `.meta` file, so
     nothing in the game loses its link),
   - checks the size, shape, colour profile and transparency, and tells you about anything that's off,
   - wires in files for things that are still code-drawn ("new" in ART_LIST.md),
   - commits it, and merges when you ask, so it shows up in the next build.
5. **Replacing a picture again** later: upload the new version with the same name. Claude swaps it in the same way.

How Claude reaches the folder: either the claude.ai Google Drive connector (claude.ai > Settings > Connectors >
Google Drive; it must be signed in for Claude's sessions) or Google Drive for desktop on the PC, which shows the folder
as a drive (usually `G:\My Drive\Game Art - Thimble and Thread`). Either works; the steps above are the same.

The repo is public for now: art that's committed can be seen and copied by anyone. Say if a picture should wait.

## Other ways
- **Send the PNGs to Claude directly** in a message, named as in ART_LIST.md.
- **By hand on the PC:** overwrite the placeholder in `Assets/_Project/Art/...` with your PNG (same name, same shape).
  Don't delete the old file first, or its `.meta` file (which holds every link to it) can be lost. Then commit and push.

## Rules that keep things working
- Same file name, same aspect ratio. Bigger is fine; a different shape gets squashed.
- Never rename or move art files outside Unity; the `.meta` file next to each one must stay with it.
- PNG only (no JPEG, no SVG). sRGB, not Display P3.
- Transparent background for everything except backgrounds and tileable textures.

## For Claude: placeholders
- Placeholders come from Tools > Thimble & Thread > Generate Placeholder Art And Data (`PlaceholderGenerator.Generate`),
  at the real target sizes with the final file names. It never overwrites an existing PNG, so re-running it is safe.
- New items, customers, supplies or patterns: see [ADDING_CONTENT.md](ADDING_CONTENT.md).
