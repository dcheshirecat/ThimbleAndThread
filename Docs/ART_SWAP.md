# Swapping placeholder art for real art
1. In Unity run: Tools > Thimble & Thread > Generate Placeholder Art And Data (once).
2. Placeholders are created at the real target sizes (see ART_SIZES.md) with the final filenames.
3. To replace one: overwrite the PNG with your real art, SAME filename, SAME pixel size/aspect ratio.
   Keep its .meta file next to it. Do not rename or move it outside Unity - references would break.
4. The generator never overwrites an existing PNG, so re-running it is safe.
5. Adding a brand-new item/customer: copy the pattern in PlaceholderGenerator.cs, or use
   Assets > Create > Craft > (Item Definition | Customer | Supply) and drag in sprites.
