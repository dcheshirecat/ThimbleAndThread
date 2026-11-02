# Art sizes (PNG, transparent, 2x oversampled where possible)
The game is portrait. UI reference canvas: 2048 x 2732 (width is fixed; taller phones show more height).

## Colour profile: sRGB (every file)
- Make every canvas in **sRGB** (Procreate: new canvas > Colour Profile > RGB > **sRGB IEC61966-2.1**).
- **Not Display P3** (Procreate's default on newer iPads). Unity treats every PNG as sRGB, so P3 art comes out
  duller or with shifted colours in the game, and phones show it differently from the iPad.
- Procreate can't convert a canvas afterwards: choose sRGB when you create it. To check an existing one:
  Actions > Canvas > Canvas information > Colour Profile.
- Export as PNG (Share > PNG), never JPEG.

The full list of what to draw: [ART_LIST.md](ART_LIST.md).

| Asset | px |
|---|---|
| Full backgrounds | 2048 x 4440 (keep important things in the middle 2048 x 2732) |
| Pannable backgrounds | 4096 x 3072 |
| Customer full-body | 1500 x 2000 |
| Dialogue portraits | 1024 x 1024 |
| Finished-item art | 1024 x 1024 |
| Item / pattern icons | 512 x 512 |
| Supply icons (Supply Market shelves) | 512 x 512 |
| Tileable textures | 1024 x 1024 |
| Stitch / loop / needle sprites | 256 x 256 |
| UI buttons | ~512 x 192 |
| UI panels (9-slice) | 512 x 512 |
| Small UI icons | 128 x 128 |
| Title logo | 2048 wide |
| App icon | 1024 x 1024 |

Naming: category_name_variant.png, lowercase, e.g. customer_mara_happy.png, item_sweater_icon.png, supply_thread_red.png
Masters (.procreate etc.) live in SourceArt/, NOT Assets/.
