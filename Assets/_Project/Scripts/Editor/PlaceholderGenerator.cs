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

        // ---- Items ----
        var sweater   = Item("sweater",   "Sweater",   CraftType.Knitting,    60, 15, new Color(0.85f, 0.35f, 0.35f));
        var scarf     = Item("scarf",     "Scarf",     CraftType.Knitting,    30, 10, new Color(0.35f, 0.55f, 0.85f));
        var plushie   = Item("plushie",   "Plushie",   CraftType.Crochet,     45, 12, new Color(0.90f, 0.70f, 0.85f));
        var tablecloth = Item("tablecloth", "Tablecloth", CraftType.CrossStitch, 50, 14, new Color(0.95f, 0.95f, 0.80f));
        var sampler   = Item("sampler",   "Sampler",   CraftType.CrossStitch, 25, 8,  new Color(0.60f, 0.85f, 0.60f));
        var tote      = Item("tote",      "Tote Bag",  CraftType.Sewing,      35, 10, new Color(0.80f, 0.60f, 0.35f));

        // ---- Supplies ----
        Supply("aida_14ct",   "Aida 14ct",     CraftType.CrossStitch, 0,  1, new Color(0.95f, 0.95f, 0.90f));
        Supply("aida_18ct",   "Aida 18ct",     CraftType.CrossStitch, 40, 2, new Color(0.90f, 0.90f, 0.95f));
        Supply("wool_basic",  "Basic Wool",    CraftType.Knitting,    0,  1, new Color(0.75f, 0.55f, 0.45f));
        Supply("wool_merino", "Merino Wool",   CraftType.Knitting,    60, 3, new Color(0.55f, 0.45f, 0.75f));
        Supply("cotton_yarn", "Cotton Yarn",   CraftType.Crochet,     0,  1, new Color(0.60f, 0.80f, 0.70f));

        // ---- Customers ----
        Customer("mara",   "Mara",        "A nervous new parent who loves anything soft.",
                 new Color(0.95f, 0.75f, 0.60f), new[] { (plushie, 0), (scarf, 1) });
        Customer("tobias", "Old Tobias",  "A retired fisherman searching for his late wife's tablecloth pattern.",
                 new Color(0.55f, 0.65f, 0.80f), new[] { (tablecloth, 0), (sweater, 2) });
        Customer("juno",   "Juno",        "A teen who wants knitwear that doesn't look grandma-made.",
                 new Color(0.70f, 0.90f, 0.60f), new[] { (sampler, 0), (tote, 1), (sweater, 3) });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Placeholder art and data generated.");
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

    static SupplyDef Supply(string id, string name, CraftType craft, int price, int tier, Color c)
    {
        string assetPath = $"{Root}/Data/Supplies/supply_{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SupplyDef>(assetPath);
        if (existing != null) return existing;
        var def = ScriptableObject.CreateInstance<SupplyDef>();
        def.supplyId = id; def.displayName = name; def.craft = craft; def.price = price; def.tier = tier;
        def.icon = MakeSprite($"{Root}/Art/Supplies/supply_{id}.png", 256, 256, c);
        AssetDatabase.CreateAsset(def, assetPath);
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
