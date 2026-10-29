#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// Tools > Thimble & Thread > Generate Placeholder Art And Data
/// Creates colored placeholder PNGs (at the REAL target sizes) plus the ScriptableObject data
/// that points at them. It never overwrites a file that already exists, so your real art is safe.
/// To swap in real art: overwrite the PNG with the same filename. The .meta file keeps every reference.
public static class PlaceholderGenerator
{
    const string Root = "Assets/_Project";

    [MenuItem("Tools/Thimble & Thread/Generate Placeholder Art And Data")]
    public static void Generate()
    {
        string[] dirs = { "Art/Items", "Art/Supplies", "Art/Backgrounds", "Art/Characters/Customers", "Data/Items", "Data/Customers", "Data/Supplies" };
        foreach (var d in dirs) Directory.CreateDirectory($"{Root}/{d}");

        MakeSprite($"{Root}/Art/Backgrounds/bg_shop_day.png", 2732, 2048, new Color(0.95f, 0.88f, 0.78f));
        MakeSprite($"{Root}/Art/Backgrounds/bg_workbench.png", 2732, 2048, new Color(0.80f, 0.66f, 0.50f));
        MakeSprite($"{Root}/Art/Backgrounds/bg_workroom_night.png", 2732, 2048, new Color(0.22f, 0.24f, 0.40f));

        // ---- Items ----
        var sweater   = Item("sweater",   "Sweater",   CraftType.Knitting,    60, 15, new Color(0.85f, 0.35f, 0.35f));
        var scarf     = Item("scarf",     "Scarf",     CraftType.Knitting,    30, 10, new Color(0.35f, 0.55f, 0.85f));
        var plushie   = Item("plushie",   "Plushie",   CraftType.Crochet,     45, 12, new Color(0.90f, 0.70f, 0.85f));
        var tablecloth = Item("tablecloth", "Tablecloth", CraftType.CrossStitch, 50, 14, new Color(0.95f, 0.95f, 0.80f));
        var sampler   = Item("sampler",   "Sampler",   CraftType.CrossStitch, 25, 8,  new Color(0.60f, 0.85f, 0.60f));
        var tote      = Item("tote",      "Tote Bag",  CraftType.Sewing,      35, 10, new Color(0.80f, 0.60f, 0.35f));

        // ---- Supplies ----
        // Fabric: Aida comes in pieces of 400 squares; a project uses its pattern's width x height.
        Supply("aida_14ct",   "White Aida 14ct", SupplyKind.Fabric, CraftType.CrossStitch, 10, 1, new Color(0.95f, 0.95f, 0.90f), 400, "squares", 1);
        Supply("aida_18ct",   "Fine Aida 18ct",  SupplyKind.Fabric, CraftType.CrossStitch, 25, 2, new Color(0.90f, 0.90f, 0.95f), 400, "squares");
        // Yarn: balls of 300 stitches.
        Supply("wool_basic",  "Basic Wool",      SupplyKind.Yarn, CraftType.Knitting, 8,  1, new Color(0.75f, 0.55f, 0.45f), 300, "stitches");
        Supply("wool_merino", "Merino Wool",     SupplyKind.Yarn, CraftType.Knitting, 20, 3, new Color(0.55f, 0.45f, 0.75f), 300, "stitches");
        Supply("cotton_yarn", "Cotton Yarn",     SupplyKind.Yarn, CraftType.Crochet,  6,  1, new Color(0.60f, 0.80f, 0.70f), 300, "stitches");
        // Floss: a skein covers 30 squares. These are also the Pattern Studio's colours.
        var red      = Supply("floss_red",      "Red Floss",      SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.85f, 0.25f, 0.30f), 30, "squares", 1);
        var pink     = Supply("floss_pink",     "Pink Floss",     SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.95f, 0.70f, 0.80f), 30, "squares");
        Supply("floss_gold",     "Gold Floss",     SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.95f, 0.65f, 0.15f), 30, "squares");
        Supply("floss_green",    "Green Floss",    SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.30f, 0.65f, 0.40f), 30, "squares");
        var sky      = Supply("floss_sky",      "Sky Blue Floss", SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.55f, 0.75f, 0.95f), 30, "squares");
        var navy     = Supply("floss_navy",     "Navy Floss",     SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.20f, 0.30f, 0.55f), 30, "squares", 1);
        Supply("floss_purple",   "Purple Floss",   SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.55f, 0.40f, 0.70f), 30, "squares");
        var charcoal = Supply("floss_charcoal", "Charcoal Floss", SupplyKind.Thread, CraftType.CrossStitch, 3, 1, new Color(0.25f, 0.20f, 0.25f), 30, "squares", 1);
        // Notions for plushies (used once item recipes arrive).
        Supply("stuffing",           "Polyester Stuffing", SupplyKind.Stuffing, CraftType.Sewing, 6, 1, new Color(0.97f, 0.97f, 0.97f), 3, "plushies");
        Supply("safety_eyes_black",  "Black Safety Eyes",  SupplyKind.Eyes,     CraftType.Sewing, 4, 1, new Color(0.10f, 0.10f, 0.10f), 4, "pairs");

        // ---- Customers ----
        Customer("mara",   "Mara",        "A nervous new parent who loves anything soft.",
                 new Color(0.95f, 0.75f, 0.60f), new[] { (plushie, 0), (scarf, 1) });
        Customer("tobias", "Old Tobias",  "A retired fisherman searching for his late wife's tablecloth pattern.",
                 new Color(0.55f, 0.65f, 0.80f), new[] { (tablecloth, 0), (sweater, 2) });
        Customer("juno",   "Juno",        "A teen who wants knitwear that doesn't look grandma-made.",
                 new Color(0.70f, 0.90f, 0.60f), new[] { (sampler, 0), (tote, 1), (sweater, 3) });

        // ---- The great-aunt's pattern book (starter shelf) ----
        BookPattern("seaside_heart", "Seaside Heart", sampler, 8, new[] { red },
            "........\n..1..1..\n.111111.\n.111111.\n..1111..\n...11...\n........\n........");
        BookPattern("anchor_border", "Anchor", tablecloth, 12, new[] { navy },
            "...1....\n..111...\n...1....\n...1....\n1..1..1.\n.1.1.1..\n..111...\n........");
        BookPattern("little_bunny", "Little Bunny", plushie, 10, new[] { pink, charcoal },
            ".1...1..\n.1...1..\n.11111..\n.12121..\n.11111..\n..111...\n.11111..\n........");
        BookPattern("wave_stripes", "Wave Stripes", scarf, 9, new[] { sky, navy },
            "........\n1...1...\n.1.1.1.1\n..1...1.\n........\n2...2...\n.2.2.2.2\n..2...2.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ContentLibraryRefresher.Refresh();
        Debug.Log("Placeholder art and data generated.");
    }

    /// Creates the book pattern, or gives an older one its threads if it has none yet.
    static PatternDef BookPattern(string id, string name, CraftItemDef item, int price, SupplyDef[] threads, string grid)
    {
        Directory.CreateDirectory($"{Root}/Data/Patterns");
        string assetPath = $"{Root}/Data/Patterns/pattern_{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PatternDef>(assetPath);
        if (existing != null)
        {
            if (existing.threads == null || existing.threads.Length == 0)
            {
                existing.threads = threads;
                EditorUtility.SetDirty(existing);
            }
            return existing;
        }
        var def = ScriptableObject.CreateInstance<PatternDef>();
        def.patternId = id; def.displayName = name; def.item = item; def.price = price;
        def.threads = threads; def.grid = grid; def.startsOnShelf = true;
        AssetDatabase.CreateAsset(def, assetPath);
        return def;
    }

    // ---------- helpers ----------

    static Sprite MakeSprite(string path, int w, int h, Color c)
    {
        if (!File.Exists(path))
        {
            var pixels = new Color32[w * h];
            Color32 fill = c, border = Color.Lerp(c, Color.black, 0.35f);
            int b = Mathf.Max(6, Mathf.Min(w, h) / 64);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[y * w + x] = (x < b || y < b || x >= w - b || y >= h - b) ? border : fill;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.maxTextureSize = 4096;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static CraftItemDef Item(string id, string name, CraftType craft, int price, int patternPrice, Color c)
    {
        string assetPath = $"{Root}/Data/Items/item_{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<CraftItemDef>(assetPath);
        if (existing != null) return existing;
        var def = ScriptableObject.CreateInstance<CraftItemDef>();
        def.itemId = id; def.itemName = name; def.craft = craft; def.basePrice = price; def.patternPrice = patternPrice;
        def.icon = MakeSprite($"{Root}/Art/Items/item_{id}_icon.png", 512, 512, c);
        def.requiredSupplyIds = new string[0];
        AssetDatabase.CreateAsset(def, assetPath);
        return def;
    }

    /// Creates the supply, or fills in the pack details on an older supply asset that doesn't have them
    /// yet (perPack still 1, unit still "uses"). Never touches its art.
    static SupplyDef Supply(string id, string name, SupplyKind kind, CraftType craft, int price, int tier, Color c,
                            int perPack, string unit, int starterPacks = 0)
    {
        string assetPath = $"{Root}/Data/Supplies/supply_{id}.asset";
        var def = AssetDatabase.LoadAssetAtPath<SupplyDef>(assetPath);
        bool isNew = def == null;
        if (isNew)
        {
            def = ScriptableObject.CreateInstance<SupplyDef>();
            def.supplyId = id; def.displayName = name; def.craft = craft; def.tier = tier;
            def.icon = MakeSprite($"{Root}/Art/Supplies/supply_{id}.png", 256, 256, c);
        }
        if (isNew || (def.perPack <= 1 && def.unit == "uses"))
        {
            def.kind = kind; def.color = c; def.price = price; def.perPack = perPack; def.unit = unit;
            def.starterPacks = starterPacks;
            EditorUtility.SetDirty(def);
        }
        if (isNew) AssetDatabase.CreateAsset(def, assetPath);
        return def;
    }

    static CustomerDef Customer(string id, string name, string bio, Color c, (CraftItemDef item, int minLevel)[] wants)
    {
        string assetPath = $"{Root}/Data/Customers/customer_{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<CustomerDef>(assetPath);
        if (existing != null) return existing;
        var def = ScriptableObject.CreateInstance<CustomerDef>();
        def.customerId = id; def.displayName = name; def.bio = bio;
        def.fullBody = MakeSprite($"{Root}/Art/Characters/Customers/customer_{id}_body.png", 1500, 2000, c);

        var exprs = (Expression[])System.Enum.GetValues(typeof(Expression));
        def.portraits = new ExpressionSprite[exprs.Length];
        for (int i = 0; i < exprs.Length; i++)
        {
            var shade = Color.Lerp(c, Color.white, 0.12f * i);
            def.portraits[i] = new ExpressionSprite
            {
                expression = exprs[i],
                sprite = MakeSprite($"{Root}/Art/Characters/Customers/customer_{id}_{exprs[i].ToString().ToLower()}.png", 1024, 1024, shade)
            };
        }

        def.requestable = new OrderOption[wants.Length];
        for (int i = 0; i < wants.Length; i++)
            def.requestable[i] = new OrderOption { item = wants[i].item, minRelationshipLevel = wants[i].minLevel };

        AssetDatabase.CreateAsset(def, assetPath);
        return def;
    }
}
#endif
