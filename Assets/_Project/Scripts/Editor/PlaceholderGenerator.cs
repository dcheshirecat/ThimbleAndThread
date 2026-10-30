#if UNITY_EDITOR
using System.Collections.Generic;
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
        var plushie   = Item("plushie",   "Crocheted Plushie", CraftType.Crochet, 45, 12, new Color(0.90f, 0.70f, 0.85f));
        if (plushie.itemName == "Plushie") { plushie.itemName = "Crocheted Plushie"; EditorUtility.SetDirty(plushie); }   // now that there's a sewn one
        var sewnPlushie = Item("plushie_sewn", "Sewn Plushie", CraftType.Sewing, 45, 12, new Color(0.95f, 0.75f, 0.80f));
        var tablecloth = Item("tablecloth", "Tablecloth", CraftType.CrossStitch, 50, 14, new Color(0.95f, 0.95f, 0.80f));
        var sampler   = Item("sampler",   "Sampler",   CraftType.CrossStitch, 25, 8,  new Color(0.60f, 0.85f, 0.60f));
        var tote      = Item("tote",      "Tote Bag",  CraftType.Sewing,      35, 10, new Color(0.80f, 0.60f, 0.35f));

        // ---- Recipes (items without one are a single cross stitch step for now) ----
        Recipe(tablecloth, Step(StepKind.CrossStitch, "Stitch the motif"), Step(StepKind.Hem, "Hem the edges"));
        Recipe(sewnPlushie,
            Step(StepKind.ChooseFabric, "Choose the fabric"),
            Step(StepKind.CutPieces, "Cut the pieces", pieces: 2, tools: Tool("scissors", "Fabric scissors")),
            Step(StepKind.Pin, "Pin them together", tools: Tool("pins", "Pins")),
            Step(StepKind.SewSeam, "Sew the seam", tools: Tool("sewing_machine", "Sewing machine")),
            Step(StepKind.TurnRightSideOut, "Turn it right side out"),
            Step(StepKind.Stuff, "Stuff it", materials: Need("stuffing", 1)),
            Step(StepKind.AttachEyes, "Attach the eyes", materials: Need("safety_eyes_black", 1)),
            Step(StepKind.CloseOpening, "Close the opening", tools: Tool("sewing_needle", "Hand sewing needle")));
        Recipe(tote,
            Step(StepKind.ChooseFabric, "Choose the fabric"),
            Step(StepKind.CutPieces, "Cut the panels", pieces: 2, tools: Tool("scissors", "Fabric scissors")),
            Step(StepKind.SewSides, "Sew the sides", tools: Tool("sewing_machine", "Sewing machine")),
            Step(StepKind.HemTop, "Hem the top", tools: Tool("sewing_machine", "Sewing machine")),
            Step(StepKind.AttachHandles, "Attach the handle", materials: Need("webbing", 1), tools: Tool("sewing_needle", "Hand sewing needle")));

        // ---- Supplies ----
        // Market slots: Always = basics always for sale; Rotating = the selection that changes every 4 days;
        // Rare = now and then one special item. The last number is starter packs (the new-game kit).
        const MarketSlot A = MarketSlot.Always, R = MarketSlot.Rotating, X = MarketSlot.Rare;
        var cs = CraftType.CrossStitch;

        // Cross stitch fabric: pieces of 400 squares; a project uses its pattern's width x height.
        Supply("aida_14ct",  "White Aida 14ct",        SupplyKind.Fabric, cs, 10, 1, new Color(0.95f, 0.95f, 0.90f), 400, "squares", A, 1);
        Supply("aida_cream", "Cream Aida 14ct",        SupplyKind.Fabric, cs, 12, 1, new Color(0.96f, 0.91f, 0.78f), 400, "squares", R);
        Supply("aida_18ct",  "Fine Aida 18ct",         SupplyKind.Fabric, cs, 25, 2, new Color(0.90f, 0.90f, 0.95f), 400, "squares", R);
        Supply("linen_28ct", "Linen Evenweave 28ct",   SupplyKind.Fabric, cs, 45, 3, new Color(0.88f, 0.82f, 0.70f), 400, "squares", X);
        // Sewing fabric, in pieces (recipes will say how many pieces an item needs).
        Supply("felt_cream",      "Cream Felt",          SupplyKind.Fabric, CraftType.Sewing, 5,  1, new Color(0.97f, 0.93f, 0.82f), 4, "pieces", A);
        Supply("felt_brown",      "Brown Felt",          SupplyKind.Fabric, CraftType.Sewing, 5,  1, new Color(0.55f, 0.38f, 0.25f), 4, "pieces", R);
        Supply("quilting_floral", "Floral Quilting Cotton", SupplyKind.Fabric, CraftType.Sewing, 9, 1, new Color(0.95f, 0.75f, 0.70f), 2, "pieces", R);
        Supply("fleece_blue",     "Blue Fleece",         SupplyKind.Fabric, CraftType.Sewing, 12, 2, new Color(0.55f, 0.70f, 0.90f), 2, "pieces", R);
        Supply("canvas_natural",  "Natural Canvas",      SupplyKind.Fabric, CraftType.Sewing, 14, 1, new Color(0.85f, 0.78f, 0.62f), 2, "pieces", R);
        Supply("minky_pink",      "Pink Minky",          SupplyKind.Fabric, CraftType.Sewing, 30, 3, new Color(0.98f, 0.78f, 0.85f), 2, "pieces", X);

        // Yarn: balls of 300 stitches. size = yarn weight (3 = DK, 4 = worsted); hooks and needles must match.
        Supply("acrylic_yarn", "Acrylic Yarn",     SupplyKind.Yarn, CraftType.Knitting, 5,  1, new Color(0.85f, 0.45f, 0.40f), 300, "stitches", R, 0, "", 4);
        Supply("wool_basic",   "Basic Wool",       SupplyKind.Yarn, CraftType.Knitting, 8,  1, new Color(0.75f, 0.55f, 0.45f), 300, "stitches", R, 0, "", 4);
        Supply("wool_merino",  "Merino Wool",      SupplyKind.Yarn, CraftType.Knitting, 20, 3, new Color(0.55f, 0.45f, 0.75f), 300, "stitches", R, 0, "", 4);
        Supply("cotton_yarn",  "Cotton Yarn",      SupplyKind.Yarn, CraftType.Crochet,  6,  1, new Color(0.60f, 0.80f, 0.70f), 300, "stitches", R, 0, "", 4);
        Supply("alpaca_yarn",  "100% Alpaca Yarn", SupplyKind.Yarn, CraftType.Knitting, 35, 3, new Color(0.90f, 0.86f, 0.80f), 300, "stitches", X, 0, "", 3);
        Supply("mohair_yarn",  "Mohair Yarn",      SupplyKind.Yarn, CraftType.Knitting, 30, 3, new Color(0.80f, 0.70f, 0.85f), 300, "stitches", X, 0, "", 3);

        // Floss: a skein covers 30 squares. These are also the Pattern Studio's colours.
        var red      = Supply("floss_red",      "Red Floss",      SupplyKind.Thread, cs, 3, 1, new Color(0.85f, 0.25f, 0.30f), 30, "squares", A, 1);
        var pink     = Supply("floss_pink",     "Pink Floss",     SupplyKind.Thread, cs, 3, 1, new Color(0.95f, 0.70f, 0.80f), 30, "squares", R);
        Supply("floss_gold",     "Gold Floss",     SupplyKind.Thread, cs, 3, 1, new Color(0.95f, 0.65f, 0.15f), 30, "squares", R);
        Supply("floss_green",    "Green Floss",    SupplyKind.Thread, cs, 3, 1, new Color(0.30f, 0.65f, 0.40f), 30, "squares", R);
        var sky      = Supply("floss_sky",      "Sky Blue Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.55f, 0.75f, 0.95f), 30, "squares", R);
        var navy     = Supply("floss_navy",     "Navy Floss",     SupplyKind.Thread, cs, 3, 1, new Color(0.20f, 0.30f, 0.55f), 30, "squares", A, 1);
        Supply("floss_purple",   "Purple Floss",   SupplyKind.Thread, cs, 3, 1, new Color(0.55f, 0.40f, 0.70f), 30, "squares", R);
        var charcoal = Supply("floss_charcoal", "Charcoal Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.25f, 0.20f, 0.25f), 30, "squares", A, 1);
        Supply("floss_white",    "White Floss",    SupplyKind.Thread, cs, 3, 1, new Color(0.98f, 0.98f, 0.96f), 30, "squares", A);
        Supply("floss_brown",    "Brown Floss",    SupplyKind.Thread, cs, 3, 1, new Color(0.50f, 0.32f, 0.20f), 30, "squares", R);
        Supply("floss_orange",   "Orange Floss",   SupplyKind.Thread, cs, 3, 1, new Color(0.95f, 0.50f, 0.20f), 30, "squares", R);
        Supply("floss_teal",     "Teal Floss",     SupplyKind.Thread, cs, 3, 1, new Color(0.15f, 0.55f, 0.55f), 30, "squares", R);
        Supply("floss_lavender", "Lavender Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.78f, 0.70f, 0.90f), 30, "squares", R);
        Supply("floss_metallic_gold", "Metallic Gold Floss", SupplyKind.Thread, cs, 8, 3, new Color(0.85f, 0.72f, 0.35f), 30, "squares", X);

        // Tools: bought once and kept. Hoop size = the largest pattern side it fits.
        Supply("hoop_4in", "4\" Embroidery Hoop", SupplyKind.Tool, cs, 6,  1, new Color(0.80f, 0.65f, 0.45f), 1, "hoop", R, 0, "hoop", 10);
        Supply("hoop_6in", "6\" Embroidery Hoop", SupplyKind.Tool, cs, 8,  1, new Color(0.78f, 0.62f, 0.42f), 1, "hoop", R, 1, "hoop", 12);
        Supply("hoop_8in", "8\" Embroidery Hoop", SupplyKind.Tool, cs, 12, 1, new Color(0.75f, 0.60f, 0.40f), 1, "hoop", R, 0, "hoop", 16);
        Supply("tapestry_needle",   "Tapestry Needle",   SupplyKind.Tool, cs, 2, 1, new Color(0.75f, 0.75f, 0.80f), 1, "needle", A, 1, "tapestry_needle");
        Supply("embroidery_needle", "Embroidery Needle", SupplyKind.Tool, CraftType.Embroidery, 2, 1, new Color(0.70f, 0.70f, 0.78f), 1, "needle", R, 0, "embroidery_needle");
        Supply("crochet_hook_5mm",  "5mm Crochet Hook",  SupplyKind.Tool, CraftType.Crochet, 6, 1, new Color(0.55f, 0.75f, 0.85f), 1, "hook", R, 0, "crochet_hook", 4);
        Supply("knitting_needles_4mm", "4mm Knitting Needles", SupplyKind.Tool, CraftType.Knitting, 8, 1, new Color(0.70f, 0.55f, 0.40f), 1, "pair", R, 0, "knitting_needles", 3);
        Supply("knitting_needles_5mm", "5mm Knitting Needles", SupplyKind.Tool, CraftType.Knitting, 8, 1, new Color(0.65f, 0.50f, 0.35f), 1, "pair", R, 0, "knitting_needles", 4);
        Supply("fabric_scissors", "Fabric Scissors",    SupplyKind.Tool, CraftType.Sewing, 15, 1, new Color(0.60f, 0.60f, 0.65f), 1, "pair", A, 0, "scissors");
        Supply("pins",            "Box of Pins",        SupplyKind.Tool, CraftType.Sewing, 3,  1, new Color(0.90f, 0.40f, 0.45f), 1, "box", A, 0, "pins");
        Supply("sewing_needle",   "Hand Sewing Needles", SupplyKind.Tool, CraftType.Sewing, 2, 1, new Color(0.78f, 0.78f, 0.82f), 1, "pack", A, 0, "sewing_needle");
        // The great-aunt's old machine comes with the shop: in every new game, never sold.
        Supply("sewing_machine",  "Great-Aunt's Sewing Machine", SupplyKind.Tool, CraftType.Sewing, 0, 1, new Color(0.25f, 0.30f, 0.30f), 1, "machine", MarketSlot.NotSold, 1, "sewing_machine");
        Supply("marking_pen",     "Fabric Marking Pen", SupplyKind.Tool, CraftType.Sewing, 4,  1, new Color(0.35f, 0.55f, 0.85f), 1, "pen", R, 0, "marking_pen");

        // Notions: used up by sewn and crocheted items (once item recipes arrive).
        Supply("stuffing",          "Polyester Stuffing", SupplyKind.Stuffing, CraftType.Sewing, 6, 1, new Color(0.97f, 0.97f, 0.97f), 3, "plushies", A);
        Supply("safety_eyes_black", "Black Safety Eyes",  SupplyKind.Eyes,     CraftType.Sewing, 4, 1, new Color(0.10f, 0.10f, 0.10f), 4, "pairs", A);
        Supply("buttons_wood",      "Wooden Buttons",     SupplyKind.Notion,   CraftType.Sewing, 4, 1, new Color(0.65f, 0.48f, 0.30f), 8, "buttons", R);
        Supply("ribbon_satin",      "Satin Ribbon",       SupplyKind.Notion,   CraftType.Sewing, 3, 1, new Color(0.90f, 0.35f, 0.50f), 2, "lengths", R);
        Supply("zip_20cm",          "20cm Zip",           SupplyKind.Notion,   CraftType.Sewing, 3, 1, new Color(0.30f, 0.30f, 0.35f), 1, "zips", R);
        Supply("interfacing",       "Interfacing",        SupplyKind.Notion,   CraftType.Sewing, 5, 1, new Color(0.92f, 0.92f, 0.92f), 2, "pieces", R);
        Supply("webbing",           "Cotton Webbing",     SupplyKind.Notion,   CraftType.Sewing, 3, 1, new Color(0.55f, 0.42f, 0.28f), 2, "handles", A);

        // ---- Customers ----
        var mara = Customer("mara",   "Mara",        "A nervous new parent who loves anything soft.",
                 new Color(0.95f, 0.75f, 0.60f), new[] { (plushie, 0), (scarf, 1) });
        Wants(mara, sewnPlushie, 0);
        Customer("tobias", "Old Tobias",  "A retired fisherman searching for his late wife's tablecloth pattern.",
                 new Color(0.55f, 0.65f, 0.80f), new[] { (tablecloth, 0), (sweater, 2) });
        var juno = Customer("juno",   "Juno",        "A teen who wants knitwear that doesn't look grandma-made.",
                 new Color(0.70f, 0.90f, 0.60f), new[] { (sampler, 0), (tote, 0), (sweater, 3) });
        var junoTote = System.Array.Find(juno.requestable, o => o.item == tote);
        if (junoTote != null && junoTote.minRelationshipLevel == 1) { junoTote.minRelationshipLevel = 0; EditorUtility.SetDirty(juno); }   // totes are playable now

        // ---- The great-aunt's pattern book (starter shelf) ----
        BookPattern("seaside_heart", "Seaside Heart", sampler, 8, new[] { red },
            "........\n..1..1..\n.111111.\n.111111.\n..1111..\n...11...\n........\n........");
        BookPattern("anchor_border", "Anchor", tablecloth, 12, new[] { navy },
            "...1....\n..111...\n...1....\n...1....\n1..1..1.\n.1.1.1..\n..111...\n........");
        BookPattern("little_bunny", "Little Bunny", plushie, 10, new[] { pink, charcoal },
            ".1...1..\n.1...1..\n.11111..\n.12121..\n.11111..\n..111...\n.11111..\n........");
        BookPattern("wave_stripes", "Wave Stripes", scarf, 9, new[] { sky, navy },
            "........\n1...1...\n.1.1.1.1\n..1...1.\n........\n2...2...\n.2.2.2.2\n..2...2.");
        // A sewing pattern: the filled squares are the piece's shape, '2' marks the eyes.
        BookPattern("felt_bunny", "Felt Bunny", sewnPlushie, 12, new[] { pink, charcoal },
            ".11....11.\n.11....11.\n.11....11.\n.11111111.\n1111111111\n1121111211\n1111111111\n1111111111\n.11111111.\n..111111..");
        // A bag pattern: the filled squares are the panel (cut twice: front and back).
        BookPattern("market_tote", "Market Tote", tote, 10, new[] { navy },
            "111111111\n111111111\n111111111\n111111111\n111111111\n111111111\n111111111");

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

    // Bump when the catalogue's details change, so supplies made by an older generator get updated.
    const int SupplyGeneratorVersion = 3;

    /// Gives an item its steps, unless it already has some (so steps set by hand are kept).
    static void Recipe(CraftItemDef item, params RecipeStep[] steps)
    {
        if (item.steps != null && item.steps.Length > 0) return;
        item.steps = steps;
        EditorUtility.SetDirty(item);
    }

    static RecipeStep Step(StepKind kind, string label, int pieces = 0, ToolNeed tools = null, MaterialNeed materials = null) =>
        new RecipeStep
        {
            kind = kind, label = label, fabricPieces = pieces,
            tools = tools != null ? new[] { tools } : new ToolNeed[0],
            materials = materials != null ? new[] { materials } : new MaterialNeed[0],
        };

    static ToolNeed Tool(string type, string label) => new ToolNeed { toolType = type, label = label };
    static MaterialNeed Need(string supplyId, int amount) => new MaterialNeed { supplyId = supplyId, amount = amount };

    /// Adds an item to what a customer asks for, if it isn't there yet.
    static void Wants(CustomerDef customer, CraftItemDef item, int minLevel)
    {
        if (System.Array.Exists(customer.requestable, o => o.item == item)) return;
        var list = new List<OrderOption>(customer.requestable) { new OrderOption { item = item, minRelationshipLevel = minLevel } };
        customer.requestable = list.ToArray();
        EditorUtility.SetDirty(customer);
    }

    /// Creates the supply, or updates the details of one an older generator made. Supplies the owner
    /// edits by hand keep their changes once they're at this version. Never touches its art.
    static SupplyDef Supply(string id, string name, SupplyKind kind, CraftType craft, int price, int tier, Color c,
                            int perPack, string unit, MarketSlot market, int starterPacks = 0, string toolType = "", int size = 0)
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
        if (isNew || def.generatorVersion < SupplyGeneratorVersion)
        {
            def.kind = kind; def.color = c; def.price = price; def.perPack = perPack; def.unit = unit;
            def.starterPacks = starterPacks; def.market = market; def.toolType = toolType; def.size = size;
            def.generatorVersion = SupplyGeneratorVersion;
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
