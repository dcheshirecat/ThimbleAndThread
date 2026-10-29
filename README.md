# Thimble & Thread
A cozy, Coffee Talk-style craft shop game made in Unity. Make and sell finished crafts and patterns:
cross stitch, knitting, embroidery, sewing, crochet.

## What's in this repo
- Core game scripts: game manager, saving, pricing, orders, shop, screen router (Assets/_Project/Scripts)
- Cross stitch minigame and pattern-fill mode
- Data types for items, customers, supplies, patterns
- A placeholder-art generator (Tools > Thimble & Thread > Generate Placeholder Art And Data)
- Docs: art sizes, art swap guide, storyline
- CLAUDE.md: full project context for Claude Code
- build-android workflow: builds a sideloadable Android APK (add it manually; see below)

## First-time setup (Unity 6.6, Git Bash on Windows)
1. Install Git (includes Git Bash and Git LFS), Unity Hub, and Unity 6.6 with Android Build Support.
2. In Git Bash: `git lfs install`, then `git clone <this repo's HTTPS URL>`.
3. In Unity Hub create a throwaway Universal 2D project, then copy its `ProjectSettings` and `Packages`
   folders into your clone (merge `Assets` too). Open the clone folder with Add > Add project from disk.
4. Set Edit > Project Settings > Player > Active Input Handling to Both.
5. Run Tools > Thimble & Thread > Generate Placeholder Art And Data.
6. Commit .meta files. Never commit Library/, keystores or builds.

## Android builds
Copy build-android.yml into .github/workflows/ on GitHub and add repo secrets
UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD (see game.ci/docs/github/activation).

## Note
The code was written without being compiled. If Unity shows console errors on first open, fix those first.
