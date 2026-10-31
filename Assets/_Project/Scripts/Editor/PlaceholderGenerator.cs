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
        string[] dirs = { "Art/Items", "Art/Supplies", "Art/Backgrounds", "Art/Characters/Customers", "Data/Items", "Data/Customers", "Data/Supplies", "Art/Upgrades", "Art/Cosmetics", "Data/Cosmetics" };
        foreach (var d in dirs) Directory.CreateDirectory($"{Root}/{d}");

        MakeSprite($"{Root}/Art/Backgrounds/bg_shop_day.png", 2732, 2048, new Color(0.95f, 0.88f, 0.78f));
        MakeSprite($"{Root}/Art/Backgrounds/bg_workbench.png", 2732, 2048, new Color(0.80f, 0.66f, 0.50f));
        MakeSprite($"{Root}/Art/Backgrounds/bg_workroom_night.png", 2732, 2048, new Color(0.22f, 0.24f, 0.40f));
        Symbols();

        // ---- Items ----
        var sweater   = Item("sweater",   "Sweater",   CraftType.Knitting,    60, 15, new Color(0.85f, 0.35f, 0.35f));
        var scarf     = Item("scarf",     "Scarf",     CraftType.Knitting,    30, 10, new Color(0.35f, 0.55f, 0.85f));
        var plushie   = Item("plushie",   "Crocheted Plushie", CraftType.Crochet, 45, 12, new Color(0.90f, 0.70f, 0.85f));
        if (plushie.itemName == "Plushie") { plushie.itemName = "Crocheted Plushie"; EditorUtility.SetDirty(plushie); }   // now that there's a sewn one
        var hoopArt = Item("hoop_art", "Embroidered Hoop", CraftType.Embroidery, 40, 12, new Color(0.60f, 0.75f, 0.90f));
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
        var needles = Tool("knitting_needles", "Knitting needles", matchYarn: true);
        var hook = Tool("crochet_hook", "Crochet hook", matchYarn: true);
        Recipe(scarf,
            Step(StepKind.ChooseYarn, "Choose the yarn"),
            Step(StepKind.CastOn, "Cast on", tools: needles),
            Step(StepKind.KnitRows, "Knit the rows", pieces: 2, perSquare: true, tools: needles),
            Step(StepKind.CastOff, "Cast off", tools: needles));
        Recipe(sweater,
            Step(StepKind.ChooseYarn, "Choose the yarn"),
            Step(StepKind.CastOn, "Cast on", tools: needles),
            Step(StepKind.KnitRows, "Knit the front and back", pieces: 5, perSquare: true, tools: needles),
            Step(StepKind.CastOff, "Cast off", tools: needles));
        Recipe(plushie,
            Step(StepKind.ChooseYarn, "Choose the yarn"),
            Step(StepKind.MagicRing, "Make a magic ring", tools: hook),
            Step(StepKind.CrochetRounds, "Crochet the rounds", pieces: 150, tools: hook),
            Step(StepKind.Stuff, "Stuff it", materials: Need("stuffing", 1)),
            Step(StepKind.AttachEyes, "Attach the eyes", materials: Need("safety_eyes_black", 1)),
            Step(StepKind.CloseOpening, "Close it up", tools: Tool("tapestry_needle", "Tapestry needle")));
        var embroideryNeedle = Tool("embroidery_needle", "Embroidery needle");
        Recipe(hoopArt,
            Step(StepKind.HoopFabric, "Hoop the fabric", materials: Need("embroidery_cotton", 1), tools: Tool("hoop", "Embroidery hoop")),
            Step(StepKind.Backstitch, "Backstitch the outline", thread: 0, threadAmount: 10, tools: embroideryNeedle),
            Step(StepKind.SatinFill, "Fill with satin stitch", thread: 0, threadAmount: 15, tools: embroideryNeedle),
            Step(StepKind.FrenchKnots, "Add French knots", thread: 1, threadAmount: 2, tools: embroideryNeedle));
        Recipe(sampler,
            Step(StepKind.CrossStitch, "Stitch the design"),
            Step(StepKind.Backstitch, "Backstitch the outline", thread: 0, threadAmount: 4, tools: Tool("tapestry_needle", "Tapestry needle")),
            Step(StepKind.Press, "Press it"));
        // Finishing touches for items that already had recipes (added once).
        AddStep(tablecloth, Step(StepKind.Press, "Press it"));
        // Longer and richer (owner, 2026-10-31): backstitched details and mitred corners before the hem.
        InsertBefore(tablecloth, StepKind.Hem, Step(StepKind.Backstitch, "Backstitch the details", thread: 0, threadAmount: 6, tools: Tool("tapestry_needle", "Tapestry needle")));
        InsertBefore(tablecloth, StepKind.Hem, Step(StepKind.MitreCorners, "Mitre the corners", tools: Tool("pins", "Pins")));
        AddStep(scarf, Step(StepKind.WeaveEnds, "Weave in the ends", tools: Tool("tapestry_needle", "Tapestry needle")));
        AddStep(scarf, Step(StepKind.Press, "Block it"));
        AddStep(scarf, Step(StepKind.Fringe, "Add the fringe", pieces: 30, tools: hook));   // knotted on last, as knitters do
        AddStep(sweater, Step(StepKind.WeaveEnds, "Weave in the ends", tools: Tool("tapestry_needle", "Tapestry needle")));
        AddStep(sweater, Step(StepKind.Press, "Block it"));
        // Knitted like a real sweater (owner, 2026-10-31): back, front with the pattern as colourwork, two
        // sleeves, then sewn up. Replaces the old one-square recipe once (hand edits after that stay).
        Garments(scarf, Garment.Scarf);
        if (Garments(sweater, Garment.Sweater))
        {
            var tapestry = Tool("tapestry_needle", "Tapestry needle");
            sweater.steps = new[]
            {
                Step(StepKind.ChooseYarn, "Choose the yarn"),
                Piece(Step(StepKind.CastOn, "Cast on the back", tools: needles), "back"),
                Piece(Step(StepKind.KnitRows, "Knit the back", pieces: 2, perSquare: true, tools: needles), "back"),
                Piece(Step(StepKind.CastOn, "Cast on the front", tools: needles), "front"),
                Piece(Step(StepKind.KnitRows, "Knit the front, with the pattern", pieces: 2, perSquare: true, tools: needles), "front"),
                Piece(Step(StepKind.CastOn, "Cast on the sleeves", tools: needles), "sleeves"),
                Piece(Step(StepKind.KnitRows, "Knit the sleeves", pieces: 1, perSquare: true, tools: needles), "sleeves"),
                Step(StepKind.SewUp, "Sew it together", tools: tapestry),
                Step(StepKind.WeaveEnds, "Weave in the ends", tools: tapestry),
                Step(StepKind.Press, "Block it"),
            };
            EditorUtility.SetDirty(sweater);
        }
        Recipe(tote,
            Step(StepKind.ChooseFabric, "Choose the fabric"),
            Step(StepKind.CutPieces, "Cut the panels", pieces: 2, tools: Tool("scissors", "Fabric scissors")),
            Step(StepKind.SewSides, "Sew the sides", tools: Tool("sewing_machine", "Sewing machine")),
            Step(StepKind.HemTop, "Hem the top", tools: Tool("sewing_machine", "Sewing machine")),
            Step(StepKind.AttachHandles, "Attach the handle", materials: Need("webbing", 1), tools: Tool("sewing_needle", "Hand sewing needle")));
        AddStep(tote, Step(StepKind.Press, "Press it"));
        // Amigurumi the way it's really made (owner, 2026-10-31): a written pattern per part, eyes in before
        // stuffing, the head closed with decreases, a striped body, two ears, then sewn together.
        if (!System.Array.Exists(plushie.steps, st => st.piece == "head_close"))
        {
            var hook5 = Tool("crochet_hook", "Crochet hook", matchYarn: true);
            var tapestry = Tool("tapestry_needle", "Tapestry needle");
            plushie.steps = new[]
            {
                Step(StepKind.ChooseYarn, "Choose the yarn"),
                Step(StepKind.MagicRing, "Make a magic ring", tools: hook5),
                Piece(Step(StepKind.CrochetRounds, "Crochet the head", pieces: 80, tools: hook5), "head"),
                Step(StepKind.AttachEyes, "Fit the safety eyes", materials: Need("safety_eyes_black", 1)),
                Piece(Step(StepKind.Stuff, "Stuff the head", materials: Need("stuffing", 1)), "head"),
                Piece(Step(StepKind.CrochetRounds, "Close the head", pieces: 20, tools: hook5), "head_close"),
                Piece(Step(StepKind.CrochetRounds, "Crochet the body", pieces: 70, tools: hook5), "body"),
                Piece(Step(StepKind.Stuff, "Stuff the body", materials: Need("stuffing", 1)), "body"),
                Piece(Step(StepKind.CrochetRounds, "Crochet two ears", pieces: 30, tools: hook5), "ears"),
                Step(StepKind.SewParts, "Sew the parts together", tools: tapestry),
            };
            EditorUtility.SetDirty(plushie);
        }
        // The embroidered hoop the way an embroiderer does it (owner, 2026-10-31): transfer, hoop, tighten,
        // outline, satin at a slant, stem-stitched waves, knots, gather the back.
        InsertBefore(hoopArt, StepKind.HoopFabric, Step(StepKind.TransferDesign, "Trace the design onto the fabric", tools: Tool("marking_pen", "Fabric marking pen")));
        InsertBefore(hoopArt, StepKind.Backstitch, Step(StepKind.TightenHoop, "Pull it drum-tight", tools: Tool("hoop", "Embroidery hoop")));
        InsertBefore(hoopArt, StepKind.FrenchKnots, Step(StepKind.StemStitch, "Stem-stitch the waves", thread: 0, threadAmount: 6, tools: embroideryNeedle));
        AddStep(hoopArt, Step(StepKind.FinishBack, "Gather the back", tools: embroideryNeedle));
        // Sewing the way a sewist does (owner, 2026-10-31): pin the pattern, clip curves, box corners, ladder stitch.
        var pinsTool = Tool("pins", "Pins");
        InsertBefore(sewnPlushie, StepKind.CutPieces, Step(StepKind.PinPattern, "Pin the pattern to the fabric", tools: pinsTool));
        InsertBefore(sewnPlushie, StepKind.TurnRightSideOut, Step(StepKind.ClipCurves, "Clip the curves", tools: Tool("scissors", "Fabric scissors")));
        InsertBefore(tote, StepKind.CutPieces, Step(StepKind.PinPattern, "Pin the pattern to the fabric", tools: pinsTool));
        InsertBefore(tote, StepKind.HemTop, Step(StepKind.BoxCorners, "Box the corners", tools: Tool("sewing_machine", "Sewing machine")));
        foreach (var st in sewnPlushie.steps)
            if (st.kind == StepKind.CloseOpening && st.label == "Close the opening") { st.label = "Ladder-stitch it closed"; EditorUtility.SetDirty(sewnPlushie); }

        // ---- Supplies ----
        // Market slots: Always = basics always for sale; Rotating = the selection that changes every 4 days;
        // Rare = now and then one special item. The last number is starter packs (the new-game kit).
        const MarketSlot A = MarketSlot.Always, R = MarketSlot.Rotating, X = MarketSlot.Rare;
        var cs = CraftType.CrossStitch;

        // Cross stitch fabric: pieces of 400 squares; a project uses its pattern's width x height.
        // Cut pieces, one per project (a piece holds a design up to 16 x 16 stitches: Materials.FabricPieces).
        Supply("aida_14ct",  "White Aida 14ct",        SupplyKind.Fabric, cs, 4,  1, new Color(0.95f, 0.95f, 0.90f), 1, "pieces", A, 2);
        Supply("aida_cream", "Cream Aida 14ct",        SupplyKind.Fabric, cs, 5,  1, new Color(0.96f, 0.91f, 0.78f), 1, "pieces", R);
        Supply("aida_18ct",  "Fine Aida 18ct",         SupplyKind.Fabric, cs, 10, 2, new Color(0.90f, 0.90f, 0.95f), 1, "pieces", R);
        Supply("linen_28ct", "Linen Evenweave 28ct",   SupplyKind.Fabric, cs, 18, 3, new Color(0.88f, 0.82f, 0.70f), 1, "pieces", X);
        // Sewing fabric, in pieces (recipes will say how many pieces an item needs).
        Supply("felt_cream",      "Cream Felt",          SupplyKind.Fabric, CraftType.Sewing, 5,  1, new Color(0.97f, 0.93f, 0.82f), 4, "pieces", A);
        Supply("felt_brown",      "Brown Felt",          SupplyKind.Fabric, CraftType.Sewing, 5,  1, new Color(0.55f, 0.38f, 0.25f), 4, "pieces", R);
        Supply("quilting_floral", "Floral Quilting Cotton", SupplyKind.Fabric, CraftType.Sewing, 9, 1, new Color(0.95f, 0.75f, 0.70f), 2, "pieces", R);
        Supply("fleece_blue",     "Blue Fleece",         SupplyKind.Fabric, CraftType.Sewing, 12, 2, new Color(0.55f, 0.70f, 0.90f), 2, "pieces", R);
        Supply("canvas_natural",  "Natural Canvas",      SupplyKind.Fabric, CraftType.Sewing, 14, 1, new Color(0.85f, 0.78f, 0.62f), 2, "pieces", R);
        Supply("embroidery_cotton", "White Embroidery Cotton", SupplyKind.Fabric, CraftType.Embroidery, 6, 1, new Color(0.97f, 0.96f, 0.93f), 2, "pieces", A);
        Supply("minky_pink",      "Pink Minky",          SupplyKind.Fabric, CraftType.Sewing, 30, 3, new Color(0.98f, 0.78f, 0.85f), 2, "pieces", X);

        // Yarn: balls of 300 stitches. size = yarn weight (3 = DK, 4 = worsted); hooks and needles must match.
        // Acrylic yarn in colours, for colourwork (each of a sweater pattern's colours is knitted in the closest one).
        foreach (var (yid, yname, ycol, slot) in new[]
        {
            ("acrylic_yarn_white", "White", new Color(0.95f, 0.95f, 0.93f), A), ("acrylic_yarn_black", "Black", new Color(0.12f, 0.12f, 0.14f), A),
            ("acrylic_yarn_red", "Red", new Color(0.80f, 0.15f, 0.20f), A), ("acrylic_yarn_navy", "Navy", new Color(0.15f, 0.20f, 0.45f), A),
            ("acrylic_yarn_green", "Green", new Color(0.25f, 0.55f, 0.30f), R), ("acrylic_yarn_yellow", "Yellow", new Color(0.95f, 0.80f, 0.25f), R),
            ("acrylic_yarn_pink", "Pink", new Color(0.95f, 0.60f, 0.70f), R), ("acrylic_yarn_sky", "Sky Blue", new Color(0.55f, 0.75f, 0.92f), R),
            ("acrylic_yarn_brown", "Brown", new Color(0.45f, 0.30f, 0.20f), R), ("acrylic_yarn_grey", "Grey", new Color(0.55f, 0.55f, 0.58f), R),
            ("acrylic_yarn_purple", "Purple", new Color(0.50f, 0.30f, 0.65f), R), ("acrylic_yarn_orange", "Orange", new Color(0.92f, 0.50f, 0.20f), R),
        })
            Supply(yid, $"{yname} Acrylic Yarn", SupplyKind.Yarn, CraftType.Knitting, 5, 1, ycol, 300, "stitches", slot, 0, "", 4);
        Supply("acrylic_yarn", "Acrylic Yarn",     SupplyKind.Yarn, CraftType.Knitting, 5,  1, new Color(0.85f, 0.45f, 0.40f), 300, "stitches", A, 0, "", 4);
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
        var white    = Supply("floss_white",    "White Floss",    SupplyKind.Thread, cs, 3, 1, new Color(0.98f, 0.98f, 0.96f), 30, "squares", A);
        Supply("floss_brown",    "Brown Floss",    SupplyKind.Thread, cs, 3, 1, new Color(0.50f, 0.32f, 0.20f), 30, "squares", R);
        Supply("floss_orange",   "Orange Floss",   SupplyKind.Thread, cs, 3, 1, new Color(0.95f, 0.50f, 0.20f), 30, "squares", R);
        Supply("floss_teal",     "Teal Floss",     SupplyKind.Thread, cs, 3, 1, new Color(0.15f, 0.55f, 0.55f), 30, "squares", R);
        Supply("floss_lavender", "Lavender Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.78f, 0.70f, 0.90f), 30, "squares", R);
        // More colours (owner, 2026-10-30): most rotate through the market; black is always stocked.
        Supply("floss_black", "Black Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.10f, 0.09f, 0.11f), 30, "squares", A);
        Supply("floss_cream", "Cream Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.96f, 0.92f, 0.80f), 30, "squares", R);
        Supply("floss_silver_grey", "Silver Grey Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.70f, 0.71f, 0.74f), 30, "squares", R);
        Supply("floss_yellow", "Sunny Yellow Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.98f, 0.85f, 0.25f), 30, "squares", R);
        Supply("floss_lemon", "Lemon Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.98f, 0.95f, 0.55f), 30, "squares", R);
        Supply("floss_coral", "Coral Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.97f, 0.50f, 0.45f), 30, "squares", R);
        Supply("floss_rose", "Dusty Rose Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.80f, 0.55f, 0.60f), 30, "squares", R);
        Supply("floss_burgundy", "Burgundy Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.50f, 0.12f, 0.20f), 30, "squares", R);
        Supply("floss_peach", "Peach Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.98f, 0.78f, 0.62f), 30, "squares", R);
        Supply("floss_mint", "Mint Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.65f, 0.88f, 0.75f), 30, "squares", R);
        Supply("floss_olive", "Olive Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.50f, 0.52f, 0.25f), 30, "squares", R);
        Supply("floss_forest", "Forest Green Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.12f, 0.38f, 0.22f), 30, "squares", R);
        Supply("floss_turquoise", "Turquoise Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.20f, 0.75f, 0.75f), 30, "squares", R);
        Supply("floss_royal", "Royal Blue Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.18f, 0.32f, 0.80f), 30, "squares", R);
        Supply("floss_periwinkle", "Periwinkle Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.60f, 0.65f, 0.92f), 30, "squares", R);
        Supply("floss_plum", "Plum Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.45f, 0.20f, 0.40f), 30, "squares", R);
        Supply("floss_magenta", "Magenta Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.85f, 0.20f, 0.60f), 30, "squares", R);
        Supply("floss_tan", "Tan Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.80f, 0.66f, 0.48f), 30, "squares", R);
        Supply("floss_rust", "Rust Floss", SupplyKind.Thread, cs, 3, 1, new Color(0.70f, 0.30f, 0.15f), 30, "squares", R);
        Supply("floss_metallic_silver", "Metallic Silver Floss", SupplyKind.Thread, cs, 8, 3, new Color(0.80f, 0.82f, 0.86f), 30, "squares", X);
        Supply("floss_metallic_gold", "Metallic Gold Floss", SupplyKind.Thread, cs, 8, 3, new Color(0.85f, 0.72f, 0.35f), 30, "squares", X);

        // Tools: bought once and kept. Hoop size = the largest pattern side it fits.
        Supply("hoop_4in", "4\" Embroidery Hoop", SupplyKind.Tool, cs, 6,  1, new Color(0.80f, 0.65f, 0.45f), 1, "hoop", R, 0, "hoop", 10);
        Supply("hoop_6in", "6\" Embroidery Hoop", SupplyKind.Tool, cs, 8,  1, new Color(0.78f, 0.62f, 0.42f), 1, "hoop", R, 1, "hoop", 12);
        Supply("hoop_8in", "8\" Embroidery Hoop", SupplyKind.Tool, cs, 12, 1, new Color(0.75f, 0.60f, 0.40f), 1, "hoop", R, 0, "hoop", 16);
        Supply("tapestry_needle",   "Tapestry Needle",   SupplyKind.Tool, cs, 2, 1, new Color(0.75f, 0.75f, 0.80f), 1, "needle", A, 1, "tapestry_needle");
        Supply("embroidery_needle", "Embroidery Needle", SupplyKind.Tool, CraftType.Embroidery, 2, 1, new Color(0.70f, 0.70f, 0.78f), 1, "needle", A, 0, "embroidery_needle");
        Supply("crochet_hook_5mm",  "5mm Crochet Hook",  SupplyKind.Tool, CraftType.Crochet, 6, 1, new Color(0.55f, 0.75f, 0.85f), 1, "hook", A, 0, "crochet_hook", 4);
        Supply("knitting_needles_4mm", "4mm Knitting Needles", SupplyKind.Tool, CraftType.Knitting, 8, 1, new Color(0.70f, 0.55f, 0.40f), 1, "pair", R, 0, "knitting_needles", 3);
        Supply("knitting_needles_5mm", "5mm Knitting Needles", SupplyKind.Tool, CraftType.Knitting, 8, 1, new Color(0.65f, 0.50f, 0.35f), 1, "pair", A, 0, "knitting_needles", 4);
        Supply("fabric_scissors", "Fabric Scissors",    SupplyKind.Tool, CraftType.Sewing, 15, 1, new Color(0.60f, 0.60f, 0.65f), 1, "pair", A, 0, "scissors");
        Supply("pins",            "Box of Pins",        SupplyKind.Tool, CraftType.Sewing, 3,  1, new Color(0.90f, 0.40f, 0.45f), 1, "box", A, 0, "pins");
        Supply("sewing_needle",   "Hand Sewing Needles", SupplyKind.Tool, CraftType.Sewing, 2, 1, new Color(0.78f, 0.78f, 0.82f), 1, "pack", A, 0, "sewing_needle");
        // The great-aunt's old machine comes with the shop: in every new game, never sold.
        // Books: bought once and kept; each teaches a skill (see the skills below).
        var knotsBook = Supply("book_knots", "A Little Book of Knots", SupplyKind.Book, CraftType.Embroidery, 15, 1, new Color(0.55f, 0.35f, 0.45f), 1, "book", R);
        var designBook = Supply("book_pattern_design", "The Pattern Maker's Notebook", SupplyKind.Book, cs, 25, 1, new Color(0.30f, 0.40f, 0.55f), 1, "book", R);
        // Better tools (owner, 2026-10-30: craft upgrades): each helps a little (SupplyDef.assist).
        Assist(Supply("scissors_sharp", "Sharp Dressmaker's Shears", SupplyKind.Tool, CraftType.Sewing, 32, 2, new Color(0.45f, 0.50f, 0.60f), 1, "pair", R, 0, "scissors"), 0.6f);
        Assist(Supply("sewing_needle_sharps", "Fine Sharps Needles", SupplyKind.Tool, CraftType.Sewing, 9, 2, new Color(0.85f, 0.85f, 0.90f), 1, "pack", R, 0, "sewing_needle"), 0.5f);
        Assist(Supply("embroidery_needle_gold", "Gold-Eye Embroidery Needles", SupplyKind.Tool, CraftType.Embroidery, 12, 2, new Color(0.90f, 0.78f, 0.35f), 1, "pack", R, 0, "embroidery_needle"), 0.5f);
        Assist(Supply("qsnap_frame", "Q-Snap Frame (holds 16 x 16 stitches)", SupplyKind.Tool, cs, 18, 2, new Color(0.85f, 0.85f, 0.88f), 1, "frame", R, 0, "hoop", 16), 0.4f);
        Assist(Supply("tapestry_needle_bent", "Bent-Tip Tapestry Needle", SupplyKind.Tool, cs, 8, 2, new Color(0.65f, 0.70f, 0.80f), 1, "needle", R, 0, "tapestry_needle"), 0.5f);
        Assist(Supply("knitting_needles_bamboo_5mm", "5mm Bamboo Knitting Needles", SupplyKind.Tool, CraftType.Knitting, 22, 2, new Color(0.85f, 0.75f, 0.50f), 1, "pair", R, 0, "knitting_needles", 4), 0.6f);
        Assist(Supply("crochet_hook_ergo_5mm", "5mm Soft-Grip Crochet Hook", SupplyKind.Tool, CraftType.Crochet, 16, 2, new Color(0.60f, 0.45f, 0.75f), 1, "hook", R, 0, "crochet_hook", 4), 0.6f);
        Assist(Supply("sewing_machine_quilter", "Quilter's Sewing Machine", SupplyKind.Tool, CraftType.Sewing, 140, 3, new Color(0.35f, 0.55f, 0.65f), 1, "machine", X, 0, "sewing_machine"), 0.7f);
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
        var tobias = Customer("tobias", "Old Tobias",  "A retired fisherman searching for his late wife's tablecloth pattern.",
                 new Color(0.55f, 0.65f, 0.80f), new[] { (tablecloth, 0), (sweater, 2) });
        var juno = Customer("juno",   "Juno",        "A teen who wants knitwear that doesn't look grandma-made.",
                 new Color(0.70f, 0.90f, 0.60f), new[] { (sampler, 0), (tote, 0), (sweater, 3) });
        var junoTote = System.Array.Find(juno.requestable, o => o.item == tote);
        Wants(juno, hoopArt, 0);
        if (junoTote != null && junoTote.minRelationshipLevel == 1) { junoTote.minRelationshipLevel = 0; EditorUtility.SetDirty(juno); }   // totes are playable now
        // Romanceable adults, arriving with the story.
        var isla = Customer("isla", "Isla Marlowe", "Keeper of the old lighthouse. Dry, gruff, and secretly hopeless at knitting.",
                 new Color(0.45f, 0.55f, 0.60f), new[] { (sweater, 0), (scarf, 0), (hoopArt, 1) });
        var rowan = Customer("rowan", "Rowan Hale", "Owns Knot & Needle across the square. Competitive, charming, and they learned to crochet here.",
                 new Color(0.75f, 0.50f, 0.55f), new[] { (plushie, 0), (tote, 0), (sampler, 1) });
        Character(mara, "mara", 1, false);
        Character(tobias, "tobias", 1, false);
        Character(juno, "juno", 1, false);   // a teen: never romanceable
        Character(isla, "isla", 2, true);
        Character(rowan, "rowan", 4, true);
        // More characters (owner, 2026-10-30): a teacher from chapter 1, and a third romance option.
        var priya = Customer("priya", "Priya Nair", "Teaches the little ones at St. Brigid's. Cheerful, organised, always covered in glitter.",
                 new Color(0.95f, 0.65f, 0.45f), new[] { (sampler, 0), (tablecloth, 0), (hoopArt, 1), (plushie, 2) });
        var eli = Customer("eli", "Eli Brooks", "Runs the Driftwood, the café on the harbour (he/him). Warm, scattered, always has cold hands.",
                 new Color(0.60f, 0.75f, 0.70f), new[] { (tablecloth, 0), (scarf, 0), (hoopArt, 1), (sweater, 2) });
        Character(priya, "priya", 1, false);
        Character(eli, "eli", 2, true);
        // How much each of them likes a surprise extra on their order.
        Likes(mara, 0.8f); Likes(tobias, 0.35f); Likes(juno, 0.6f); Likes(isla, 0.45f); Likes(rowan, 0.55f);
        Likes(priya, 0.9f); Likes(eli, 0.85f);

        // ---- Special touches: extras that can be added to finished pieces as a surprise ----
        var lace = Supply("lace_trim", "Cotton Lace Trim", SupplyKind.Notion, CraftType.Sewing, 4, 2, new Color(0.97f, 0.95f, 0.90f), 2, "lengths", R);
        Decorates(lace, 0.3f, tablecloth, sampler, tote, hoopArt);
        Decorates(content_Supply("buttons_wood"), 0.2f, sweater, sewnPlushie, plushie, tote);
        Decorates(content_Supply("ribbon_satin"), 0.2f, hoopArt, sewnPlushie, plushie, sampler);
        var whiteButtons = Supply("buttons_white", "White Buttons", SupplyKind.Notion, CraftType.Sewing, 3, 1, new Color(0.96f, 0.96f, 0.94f), 8, "buttons", R);
        Decorates(whiteButtons, 0.15f, sweater, sewnPlushie, plushie, tote);

        // ---- The people who run the other shops (owner, 2026-10-30): friends, and romance options ----
        var bram = Customer("bram", "Bram Okafor", "Runs the Supply Market (he/him). Knows every thread count by heart and laughs like a foghorn.",
                 new Color(0.55f, 0.40f, 0.30f), new (CraftItemDef, int)[0]);
        var celeste = Customer("celeste", "Celeste Moreau", "Owns Moreau's, the boutique on the square (she/her): hair, clothes, accessories. Blunt, stylish, secretly sentimental.",
                 new Color(0.70f, 0.45f, 0.60f), new (CraftItemDef, int)[0]);
        Character(bram, "bram", 1, true);
        Character(celeste, "celeste", 1, true);
        ShopOwner(bram); ShopOwner(celeste);

        // ---- What people like and dislike: never said in dialogue; the player finds out by adding a special touch
        //      to their order (the reaction notes it in the Journal). Each is about a decorating supply. ----
        var wood = content_Supply("buttons_wood");
        var ribbon = content_Supply("ribbon_satin");
        // Crafts people do themselves (they buy those patterns rather than ordering the piece).
        Makes(isla, CraftType.Knitting);
        Makes(rowan, CraftType.Crochet, CraftType.Knitting);
        Makes(priya, CraftType.CrossStitch, CraftType.Embroidery);
        Makes(mara, CraftType.Knitting);
        Makes(juno, CraftType.Embroidery, CraftType.Sewing);
        Prefs(tobias, ("wooden_buttons", "wooden buttons", true, wood), ("white_buttons", "white buttons", false, whiteButtons), ("lace", "lace", false, lace));
        Prefs(mara, ("ribbon", "satin ribbon", true, ribbon), ("lace", "lace trim", true, lace));
        Prefs(juno, ("lace", "lace", false, lace), ("wooden_buttons", "wooden buttons", true, wood));
        Prefs(isla, ("ribbon", "ribbon", false, ribbon), ("white_buttons", "plain white buttons", true, whiteButtons));
        Prefs(rowan, ("lace", "lace trim", true, lace), ("white_buttons", "white buttons", false, whiteButtons));
        Prefs(priya, ("ribbon", "ribbon", true, ribbon), ("wooden_buttons", "wooden buttons", true, wood));
        Prefs(eli, ("wooden_buttons", "wooden buttons", true, wood), ("lace", "lace", false, lace));

        // ---- The boutique's stock: hairstyles, tops and accessories for the shopkeeper ----
        Cosmetic("hair_bun", "Top Knot", CosmeticSlot.Hair, 3, 25, new Color(0.40f, 0.26f, 0.18f));
        Cosmetic("hair_curly", "Big Curls", CosmeticSlot.Hair, 4, 30, new Color(0.40f, 0.26f, 0.18f));
        Cosmetic("top_cardigan", "Cosy Cardigan", CosmeticSlot.Top, 2, 35, new Color(0.60f, 0.45f, 0.70f));
        Cosmetic("acc_glasses", "Round Glasses", CosmeticSlot.Accessory, 0, 20, new Color(0.20f, 0.18f, 0.20f));
        Cosmetic("acc_hat", "Felt Hat", CosmeticSlot.Accessory, 1, 30, new Color(0.45f, 0.30f, 0.25f));
        Cosmetic("acc_brooch", "Daisy Brooch", CosmeticSlot.Accessory, 2, 15, new Color(0.98f, 0.85f, 0.30f));
        Cosmetic("acc_neckerchief", "Sailor Neckerchief", CosmeticSlot.Accessory, 3, 18, new Color(0.25f, 0.40f, 0.75f));

        // ---- Commissions: special requests, each needing a brand-new pattern ----
        Commission("juno_skate_scarf", juno, scarf, 0, null,
            "The wind at the skate park goes straight through me. I need something round my neck that my nan " +
            "would never knit in a million years. Make it look like it has opinions.");
        Commission("mara_moon_bunny", mara, sewnPlushie, 1, "minky_pink",
            "She only settles with something soft against her cheek. Softer than anything I own. Something to " +
            "watch over her at night, the way the moon does.");
        Commission("tobias_sunday_cloth", tobias, tablecloth, 1, "linen_28ct",
            "Elsa laid a cloth for Sunday dinner, every week. Linen, always linen. The sea was on it somewhere... " +
            "I can't remember how it went. Would you make one that feels like hers?");

        // ---- The great-aunt's pattern book (starter shelf) ----
        BookPattern("seaside_heart", "Seaside Heart", sampler, 8, new[] { red },
            "........\n..1..1..\n.111111.\n.111111.\n..1111..\n...11...\n........\n........");
        // Half stitches ('a' = colour 1 as a half stitch) give the sail its diagonal edge.
        BookPattern("little_sailboat", "Little Sailboat", sampler, 10, new[] { red, navy },
            "..........\n....1.....\n....1a....\n....11a...\n....111a..\n....1111a.\n....1.....\n.22222222.\n..222222..\n..........");
        BookPattern("anchor_border", "Anchor", tablecloth, 12, new[] { navy },
            "...1....\n..111...\n...1....\n...1....\n1..1..1.\n.1.1.1..\n..111...\n........");
        BookPattern("little_bunny", "Little Bunny", plushie, 10, new[] { pink, charcoal },
            ".1...1..\n.1...1..\n.11111..\n.12121..\n.11111..\n..111...\n.11111..\n........");
        BookPattern("wave_stripes", "Wave Stripes", scarf, 9, new[] { sky, navy },
            "........\n1...1...\n.1.1.1.1\n..1...1.\n........\n2...2...\n.2.2.2.2\n..2...2.");
        // A sewing pattern: the filled squares are the piece's shape, '2' marks the eyes.
        BookPattern("felt_bunny", "Felt Bunny", sewnPlushie, 12, new[] { pink, charcoal },
            ".11....11.\n.11....11.\n.11....11.\n.11111111.\n1111111111\n1121111211\n1111111111\n1111111111\n.11111111.\n..111111..");
        // An embroidery pattern: the filled squares are the shape (outlined, then satin-filled); '2' marks French knots.
        BookPattern("little_whale", "Little Whale", hoopArt, 12, new[] { navy, white },
            "........11\n........1.\n.111111.1.\n1111111111\n1211111111\n1111111111\n.11111111.\n..111111..");
        // A bag pattern: the filled squares are the panel (cut twice: front and back).
        BookPattern("market_tote", "Market Tote", tote, 10, new[] { navy },
            "111111111\n111111111\n111111111\n111111111\n111111111\n111111111\n111111111");

        // ---- Ada Finch's pattern shop (owner, 2026-10-31): ready-made patterns by complexity and rarity ----
        ShopPattern("lighthouse", "Lighthouse", sampler, 10, 25, MarketSlot.Always, new[] { content_Supply("floss_red"), content_Supply("floss_navy") },
            "...11...\n..1111..\n...22...\n...11...\n...22...\n...11...\n..2222..\n.222222.\n22222222\n........");
        ShopPattern("starfish", "Starfish", sampler, 8, 20, MarketSlot.Always, new[] { content_Supply("floss_coral") },
            "....1...\n...111..\n1111111.\n.11111..\n..111...\n.11.11..\n11...11.\n........");
        ShopPattern("shell_border", "Shell Border", tablecloth, 12, 30, MarketSlot.Always, new[] { content_Supply("floss_teal") },
            "........\n.1....1.\n111..111\n1.1..1.1\n111..111\n........\n11111111\n........");
        ShopPattern("simple_stripe", "Simple Stripe", scarf, 9, 22, MarketSlot.Always, new[] { content_Supply("floss_red") },
            "........\n11111111\n........\n........\n11111111\n........\n........\n11111111");
        ShopPattern("daisy_hoop", "Daisy Hoop", hoopArt, 11, 28, MarketSlot.Always, new[] { content_Supply("floss_yellow"), content_Supply("floss_white") },
            "........\n...11...\n..1111..\n.112211.\n.112211.\n..1111..\n...11...\n........");
        ShopPattern("harbour_cottage", "Harbour Cottage", sampler, 16, 55, MarketSlot.Rotating, new[] { content_Supply("floss_red"), content_Supply("floss_cream"), content_Supply("floss_navy") },
            "............\n.....a1.....\n....a111....\n...a11111...\n..a1111111..\n..22222222..\n..23322332..\n..23322332..\n..22233222..\n..22233222..\n333333333333\n............");
        ShopPattern("snowflake_jumper", "Snowflake Jumper", sweater, 20, 60, MarketSlot.Rotating, new[] { content_Supply("floss_white") },
            "..........\n....1.....\n..1.1.1...\n...111....\n.1111111..\n...111....\n..1.1.1...\n....1.....\n..........\n..........");
        ShopPattern("bear_cub", "Bear Cub", plushie, 14, 45, MarketSlot.Rotating, new[] { content_Supply("floss_brown"), content_Supply("floss_charcoal") },
            "11....11\n11111111\n.111111.\n.121121.\n.111111.\n..1221..\n.111111.\n........");
        ShopPattern("sailor_tote", "Sailor Tote", tote, 13, 40, MarketSlot.Rotating, new[] { content_Supply("floss_royal") },
            "111111111\n111111111\n111111111\n111111111\n111111111\n111111111\n111111111\n111111111");
        ShopPattern("rose_garden", "Rose Garden", hoopArt, 16, 55, MarketSlot.Rotating, new[] { content_Supply("floss_rose"), content_Supply("floss_forest") },
            "..........\n...1111...\n..111111..\n..112111..\n..111111..\n...1111...\n....22....\n..2222....\n....2222..\n....22....");
        ShopPattern("midnight_sea", "Midnight Sea", tablecloth, 26, 110, MarketSlot.Rare, new[] { content_Supply("floss_navy"), content_Supply("floss_metallic_silver"), content_Supply("floss_sky") },
            "..............\n...2......2...\n..222....222..\n...2......2...\n..............\n3a.3a.3a.3a.3a\n11111111111111\n1a1a1a1a1a1a1a\n11111111111111\n..............\n....2....2....\n...222..222...\n....2....2....\n..............");
        ShopPattern("golden_koi", "Golden Koi", sampler, 24, 100, MarketSlot.Rare, new[] { content_Supply("floss_orange"), content_Supply("floss_metallic_gold"), content_Supply("floss_white") },
            "............\n......11....\n....11111...\n...1121111..\n..11112111a.\n.a1111121111\n..1111111a..\n...11111....\n....a11.....\n...11..11...\n..3.3..3.3..\n............");
        ShopPattern("fox_in_ferns", "Fox in the Ferns", sweater, 28, 120, MarketSlot.Rare, new[] { content_Supply("floss_rust"), content_Supply("floss_forest"), content_Supply("floss_white") },
            "............\n..1......1..\n..11....11..\n..11111111..\n..13111131..\n..11111111..\n...113311...\n....1111....\n2....11....2\n22..2112..22\n.22222222222\n............");

        // Ada Finch runs the pattern shop: an old friend of the great-aunt, a retired pattern designer (she/her).
        var ada = Customer("ada", "Ada Finch", "Runs Finch's Pattern Parlour (she/her). A retired pattern designer who knew your great-aunt for fifty years.",
                 new Color(0.45f, 0.55f, 0.45f), new (CraftItemDef, int)[0]);
        Character(ada, "ada", 1, false);
        ShopOwner(ada);

        Tutorial();

        // ---- Skills: crafts open with the story; techniques come from levels, books and friends ----
        Skill("craft_cross_stitch", "Cross Stitch", "Stitch Xs on Aida to make samplers, tablecloths and more.", true, cs, chapter: 1);
        Skill("craft_knitting", "Knitting", "Two needles, one stitch at a time: scarves and sweaters.", true, CraftType.Knitting, chapter: 2);
        Skill("craft_embroidery", "Embroidery", "Backstitch, satin stitch and knots on fabric in a hoop.", true, CraftType.Embroidery, chapter: 3);
        Skill("craft_crochet", "Crochet", "A hook and a loop: amigurumi that spirals out from a magic ring.", true, CraftType.Crochet, chapter: 4);
        Skill("craft_sewing", "Sewing", "Cut, pin and sew: plushies and tote bags on the old machine.", true, CraftType.Sewing, chapter: 5);
        Skill(ShopDayPatternDesign, "Pattern Design", "Design your own patterns in the Pattern Studio, and take commissions.", false, cs,
              levelCraft: cs, craftLevel: 2, book: designBook);
        Skill("french_knots", "French Knots", "Wrap the thread twice and pull through: tiny raised dots, like an eye.", false, CraftType.Embroidery,
              book: knotsBook, friend: juno, friendLevel: 1);
        RequireSkill(hoopArt, StepKind.FrenchKnots, "french_knots");

        // ---- The story, in chapters: each opens a craft and sets goals for the next ----
        Chapter(1, "The Shop at the Edge of the Sea",
            "The key is heavier than you expected. Your great-aunt's haberdashery smells of lavender and old wool, and her " +
            "pattern book waits on the counter, a few pages missing. Put some patterns on display and see who comes in.",
            "Hand over 2 orders.", deliveries: 2);
        Chapter(2, "Casting On",
            "Juno lingers by the yarn baskets. \"You knit, right? Everyone says your great-aunt could knit anything.\" Under the " +
            "counter you find her needles, a half-finished row still on them. Knitting is open!",
            "Hand over 5 orders in all, and become friends with someone (friendship level 1).", deliveries: 5, friendLevel: 1);
        Chapter(3, "A Thread of Memory",
            "Old Tobias brings in a faded photograph: a tablecloth with the sea embroidered round its edge. \"Elsa made it. I'd " +
            "know it anywhere.\" Your great-aunt's book has an embroidery section, and a page torn out of it. Embroidery is open!",
            "Hand over 9 orders in all and sell 3 patterns.", deliveries: 9, patternsSold: 3);
        Chapter(4, "Knot & Needle",
            "A new shop opens across the square: Knot & Needle, all crisp paper bags and crochet in the window. Its owner, Rowan, " +
            "leans in your doorway. \"Healthy competition,\" they say, and leave a crochet hook on your counter. Crochet is open!",
            "Hand over 14 orders in all, and grow a friendship to level 2.", deliveries: 14, friendLevel: 2);
        Chapter(5, "The Community Quilt",
            "The festival committee knocks: this year's Community Quilt needs a patch from each of your friends, and someone has " +
            "to sew it all together. That someone is you. Sewing is open, and the old machine hums like it remembers.",
            "", 0);

        // ---- Shop upgrades: on the market's "For the Shop" shelf ----
        Upgrade("display_shelf", "Extra Display Shelf", "One more pattern fits on display.", 80, 1, UpgradeEffect.DisplaySpot, 1);
        Upgrade("work_lamp", "Brass Work Lamp", "Craft by better light: 25% more experience.", 100, 1, UpgradeEffect.CraftLamp, 25);
        // ---- Achievement quests (owner, 2026-10-31): tiers, each shown once the one before is claimed ----
        QuestTiers("deliver", QuestGoal.OrdersDelivered, "Hand over {0} orders", "Hand over your first order", new[] { 1, 5, 15, 30, 60 }, 30, 25);
        QuestTiers("sell", QuestGoal.PatternsSold, "Sell {0} patterns", "Sell your first pattern", new[] { 1, 5, 15, 30 }, 25, 20);
        QuestTiers("friend", QuestGoal.BestFriendship, "Grow a friendship to level {0}", null, new[] { 1, 2, 3, 4 }, 40, 30);
        QuestTiers("meet", QuestGoal.PeopleMet, "Meet {0} people in town", null, new[] { 3, 6, 10 }, 20, 15);
        QuestTiers("design", QuestGoal.PatternsDesigned, "Design {0} patterns", "Design your own pattern", new[] { 1, 3, 8 }, 30, 30);
        QuestTiers("commission", QuestGoal.Commissions, "Complete {0} commissions", "Complete a commission", new[] { 1, 3, 6 }, 60, 40);
        foreach (CraftType craft in System.Enum.GetValues(typeof(CraftType)))
        {
            string name = craft == CraftType.CrossStitch ? "cross stitch" : craft.ToString().ToLower();
            QuestTiers($"make_{craft.ToString().ToLower()}", QuestGoal.ItemsMade, $"Make {{0}} {name} pieces", $"Make your first {name} piece",
                       new[] { 1, 5, 12 }, 20, 30, craft);
        }

        Upgrade("shop_sign", "Painted Shop Sign", "A customer drops by a second time each day.", 120, 2, UpgradeEffect.ExtraVisitor, 1);
        Upgrade("display_case", "Glass Display Case", "Two more patterns fit on display.", 180, 3, UpgradeEffect.DisplaySpot, 2);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ContentLibraryRefresher.Refresh();
        Debug.Log("Placeholder art and data generated.");
    }

    /// Creates the book pattern, or gives an older one its threads if it has none yet.
    /// A pattern sold at the pattern shop (not in the book at the start).
    static void ShopPattern(string id, string name, CraftItemDef item, int price, int shopPrice, MarketSlot rarity, SupplyDef[] threads, string grid)
    {
        var def = BookPattern(id, name, item, price, threads, grid);
        if (def.forSale) return;   // already set up (hand edits stay)
        def.startsOnShelf = false;
        def.forSale = true;
        def.rarity = rarity;
        def.shopPrice = shopPrice;
        EditorUtility.SetDirty(def);
    }

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

    /// Placeholder icons for the top bar: a symbol per weekday (Monday a moon, Tuesday a flame (triangle), Wednesday a
    /// drop, Thursday a star, Friday a heart, Saturday a ring, Sunday a sun) and the settings gear. White on
    /// clear, 256 x 256: the badge behind them gives the colour.
    static void Symbols()
    {
        Directory.CreateDirectory($"{Root}/Art/UI/Weekdays");
        System.Func<float, float, bool>[] shapes =
        {
            (x, y) => x * x + y * y < 0.16f && (x - 0.17f) * (x - 0.17f) + (y - 0.1f) * (y - 0.1f) > 0.11f,                // moon
            (x, y) => y > -0.36f && Mathf.Abs(x) < 0.38f * (0.4f - y) / 0.76f,                                                // flame (a triangle)
            (x, y) => (y < 0 && x * x + y * y < 0.09f) || (y >= 0 && Mathf.Abs(x) < 0.3f * (1 - y / 0.42f)),               // drop
            (x, y) => Star(x, y),
            (x, y) => Heart(x * 2.6f, y * 2.6f + 0.15f),
            (x, y) => { float r = Mathf.Sqrt(x * x + y * y); return r > 0.24f && r < 0.4f; },                              // ring
            (x, y) => { float r = Mathf.Sqrt(x * x + y * y), a = Mathf.Atan2(y, x); return r < 0.22f || (r < 0.42f && r > 0.27f && Mathf.Cos(a * 8) > 0.55f); }, // sun
        };
        for (int i = 0; i < 7; i++) MakeIcon($"{Root}/Art/UI/Weekdays/{Week.SymbolFile(i)}", shapes[i]);
        MakeIcon($"{Root}/Art/UI/icon_gear.png", (x, y) =>
        {
            float r = Mathf.Sqrt(x * x + y * y), a = Mathf.Atan2(y, x);
            return r > 0.11f && (r < 0.3f || (r < 0.42f && Mathf.Cos(a * 8) > 0.3f));
        });
    }

    static bool Star(float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y), a = Mathf.Atan2(y, x) - Mathf.PI / 2;
        float t = Mathf.Abs(Mathf.Repeat(a, Mathf.PI * 2 / 5) - Mathf.PI / 5) / (Mathf.PI / 5);   // 1 at a point, 0 between
        return r < Mathf.Lerp(0.18f, 0.44f, t * t);
    }

    static bool Heart(float x, float y)
    {
        float a = x * x + y * y - 1;
        return a * a * a - x * x * y * y * y <= 0;
    }

    /// A white shape on a clear square (inside(x, y) with x and y from -0.5 to 0.5), softened at the edge.
    static void MakeIcon(string path, System.Func<float, float, bool> inside)
    {
        if (File.Exists(path)) return;
        const int size = 256, ss = 3;
        var pixels = new Color32[size * size];
        for (int py = 0; py < size; py++)
            for (int px = 0; px < size; px++)
            {
                int hits = 0;
                for (int sy = 0; sy < ss; sy++)
                    for (int sx = 0; sx < ss; sx++)
                        if (inside((px + (sx + 0.5f) / ss) / size - 0.5f, (py + (sy + 0.5f) / ss) / size - 0.5f)) hits++;
                pixels[py * size + px] = new Color32(255, 255, 255, (byte)(255 * hits / (ss * ss)));
            }
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();
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
    const int SupplyGeneratorVersion = 6;   // 6: cross stitch fabric in pieces

    /// Gives an item its steps, unless it already has some (so steps set by hand are kept).
    static void Recipe(CraftItemDef item, params RecipeStep[] steps)
    {
        if (item.steps != null && item.steps.Length > 0) return;
        item.steps = steps;
        EditorUtility.SetDirty(item);
    }

    /// Sets a knitted item's garment shape if it has none yet; true when it was just set.
    static bool Garments(CraftItemDef item, Garment garment)
    {
        if (item == null || item.garment != Garment.Square) return false;
        item.garment = garment;
        EditorUtility.SetDirty(item);
        return true;
    }

    static RecipeStep Piece(RecipeStep step, string piece) { step.piece = piece; return step; }

    /// Puts a step in front of the first step of another kind, unless the recipe already has one of its kind.
    static void InsertBefore(CraftItemDef item, StepKind before, RecipeStep step)
    {
        if (System.Array.Exists(item.steps, s => s.kind == step.kind)) return;
        var list = new List<RecipeStep>(item.steps);
        int at = list.FindIndex(s => s.kind == before);
        list.Insert(at < 0 ? list.Count : at, step);
        item.steps = list.ToArray();
        EditorUtility.SetDirty(item);
    }

    /// Adds a step to the end of an item's recipe, unless it already has a step of that kind.
    static void AddStep(CraftItemDef item, RecipeStep step)
    {
        if (System.Array.Exists(item.steps, s => s.kind == step.kind)) return;
        var list = new List<RecipeStep>(item.steps) { step };
        item.steps = list.ToArray();
        EditorUtility.SetDirty(item);
    }

    static RecipeStep Step(StepKind kind, string label, int pieces = 0, ToolNeed tools = null, MaterialNeed materials = null, bool perSquare = false,
                           int thread = 0, int threadAmount = 0) =>
        new RecipeStep
        {
            kind = kind, label = label, fabricPieces = pieces, perPatternSquare = perSquare,
            patternThread = thread, threadAmount = threadAmount,
            tools = tools != null ? new[] { tools } : new ToolNeed[0],
            materials = materials != null ? new[] { materials } : new MaterialNeed[0],
        };

    static ToolNeed Tool(string type, string label, bool matchYarn = false) => new ToolNeed { toolType = type, label = label, matchYarn = matchYarn };
    static MaterialNeed Need(string supplyId, int amount) => new MaterialNeed { supplyId = supplyId, amount = amount };

    const string ShopDayPatternDesign = ShopDay.PatternDesignSkill;

    /// Creates a skill, unless it exists (so edits made by hand are kept).
    static void Skill(string id, string name, string description, bool isCraft, CraftType craft, int chapter = 0,
                      CraftType levelCraft = CraftType.CrossStitch, int craftLevel = 0, SupplyDef book = null,
                      CustomerDef friend = null, int friendLevel = 0)
    {
        Directory.CreateDirectory($"{Root}/Data/Skills");
        string assetPath = $"{Root}/Data/Skills/skill_{id}.asset";
        if (AssetDatabase.LoadAssetAtPath<SkillDef>(assetPath) != null) return;
        var def = ScriptableObject.CreateInstance<SkillDef>();
        def.skillId = id; def.displayName = name; def.description = description; def.isCraft = isCraft; def.craft = craft;
        def.chapter = chapter; def.levelCraft = levelCraft; def.craftLevel = craftLevel; def.book = book;
        def.friend = friend; def.friendLevel = friendLevel;
        AssetDatabase.CreateAsset(def, assetPath);
    }

    /// Makes a recipe step need a skill, unless it already names one.
    static void RequireSkill(CraftItemDef item, StepKind kind, string skillId)
    {
        foreach (var s in item.steps)
            if (s.kind == kind && string.IsNullOrEmpty(s.requiredSkill)) { s.requiredSkill = skillId; EditorUtility.SetDirty(item); }
    }

    static void Chapter(int number, string title, string intro, string goalText, int deliveries, int patternsSold = 0, int friendLevel = 0)
    {
        Directory.CreateDirectory($"{Root}/Data/Chapters");
        string assetPath = $"{Root}/Data/Chapters/chapter_{number}.asset";
        if (AssetDatabase.LoadAssetAtPath<ChapterDef>(assetPath) != null) return;
        var def = ScriptableObject.CreateInstance<ChapterDef>();
        def.number = number; def.title = title; def.intro = intro; def.goalText = goalText;
        def.deliveries = deliveries; def.patternsSold = patternsSold; def.friendLevel = friendLevel;
        AssetDatabase.CreateAsset(def, assetPath);
    }

    /// A chain of quests, each shown once the one before is claimed; rewards grow with each tier. Never
    /// overwrites one that exists (so rewards edited by hand are kept).
    static void QuestTiers(string id, QuestGoal goal, string title, string firstTitle, int[] targets, int coins, int xp, CraftType? craft = null)
    {
        Directory.CreateDirectory($"{Root}/Data/Quests");
        QuestDef before = null;
        for (int i = 0; i < targets.Length; i++)
        {
            string questId = $"{id}_{targets[i]}";
            string assetPath = $"{Root}/Data/Quests/quest_{questId}.asset";
            var def = AssetDatabase.LoadAssetAtPath<QuestDef>(assetPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<QuestDef>();
                def.questId = questId;
                def.title = targets[i] == 1 && firstTitle != null ? firstTitle : string.Format(title, targets[i]);
                def.goal = goal; def.target = targets[i];
                def.oneCraft = craft.HasValue; def.craft = craft ?? CraftType.CrossStitch;
                def.after = before;
                def.coins = coins * (i + 1); def.xp = xp * (i + 1);
                AssetDatabase.CreateAsset(def, assetPath);
            }
            before = def;
        }
    }

    static void Upgrade(string id, string name, string description, int price, int chapter, UpgradeEffect effect, int amount)
    {
        Directory.CreateDirectory($"{Root}/Data/Upgrades");
        string assetPath = $"{Root}/Data/Upgrades/upgrade_{id}.asset";
        if (AssetDatabase.LoadAssetAtPath<UpgradeDef>(assetPath) != null) return;
        var def = ScriptableObject.CreateInstance<UpgradeDef>();
        def.upgradeId = id; def.displayName = name; def.description = description; def.price = price; def.chapter = chapter;
        def.effect = effect; def.amount = amount;
        def.icon = MakeSprite($"{Root}/Art/Upgrades/upgrade_{id}.png", 512, 512, new Color(0.60f, 0.50f, 0.75f));
        AssetDatabase.CreateAsset(def, assetPath);
    }

    /// Creates the tutorial's pages, unless the tutorial exists (so words edited by hand are kept).
    static void Tutorial()
    {
        string assetPath = $"{Root}/Data/Tutorial.asset";
        if (AssetDatabase.LoadAssetAtPath<TutorialDef>(assetPath) != null) return;
        var def = ScriptableObject.CreateInstance<TutorialDef>();
        TutorialPage Page(string title, TutorialAnim anim, string text) => new TutorialPage { title = title, text = text, animation = anim };
        def.pages = new[]
        {
            Page("Welcome to Thimble & Thread", TutorialAnim.Welcome,
                "Your great-aunt has left you her little haberdashery by the sea. It opens at dusk... and it's yours now."),
            Page("Day and Night", TutorialAnim.DayAndNight,
                "By day the shop is open: customers come in, buy patterns and order things made. Close up when you like, " +
                "and craft by lamplight in the workroom at night. Neither one has a timer."),
            Page("Your Customers", TutorialAnim.Customers,
                "People don't always say exactly what they want. Listen, choose something that fits, and they'll become " +
                "friends. Now and then someone asks for a commission: a brand-new pattern only you can design."),
            Page("Made by Hand", TutorialAnim.Stitching,
                "Everything is made step by step, the way it's really done. Cross stitch: drag from corner to corner, then " +
                "the other way to finish the X."),
            Page("Every Craft Its Own", TutorialAnim.Gestures,
                "Each craft you learn has its own moves, the way it's really done. The hint at the top always tells you what to do."),
            Page("No Rush", TutorialAnim.OrderBook,
                "Orders wait in your order book. Nothing has a deadline: stop whenever you like, your work is saved, and pick " +
                "it up another night."),
            Page("Supplies", TutorialAnim.Market,
                "You can only make things with what's in your cupboard. Buy supplies at the Supply Market by day; its stock " +
                "changes every four days. Tools are yours to keep."),
        };
        AssetDatabase.CreateAsset(def, assetPath);
    }

    /// Creates a commission, unless it exists (so wording edited by hand is kept).
    static void Commission(string id, CustomerDef customer, CraftItemDef item, int minLevel, string requiredSupplyId, string request)
    {
        Directory.CreateDirectory($"{Root}/Data/Commissions");
        string assetPath = $"{Root}/Data/Commissions/commission_{id}.asset";
        if (AssetDatabase.LoadAssetAtPath<CommissionDef>(assetPath) != null) return;
        var def = ScriptableObject.CreateInstance<CommissionDef>();
        def.commissionId = id; def.customer = customer; def.item = item; def.minRelationshipLevel = minLevel; def.request = request;
        if (requiredSupplyId != null) def.requiredSupply = AssetDatabase.LoadAssetAtPath<SupplyDef>($"{Root}/Data/Supplies/supply_{requiredSupplyId}.asset");
        AssetDatabase.CreateAsset(def, assetPath);
    }

    /// Links a customer to their dialogue (the knot names in Dialogue/*.ink) and sets when they arrive,
    /// unless a dialogue name is already set (so edits made by hand are kept).
    static SupplyDef content_Supply(string id) => AssetDatabase.LoadAssetAtPath<SupplyDef>($"{Root}/Data/Supplies/supply_{id}.asset");

    /// Makes a supply a special touch for these items (only if it isn't one yet, so hand edits stay).
    static void Decorates(SupplyDef s, float bonus, params CraftItemDef[] items)
    {
        if (s == null || (s.decorates != null && s.decorates.Length > 0)) return;
        s.decorates = items;
        s.decorateAmount = 1;
        s.decorateBonus = bonus;
        EditorUtility.SetDirty(s);
    }

    /// Sets a tool's quality (how much it helps), if not set yet.
    static void Assist(SupplyDef s, float assist)
    {
        if (s == null || s.assist > 0f) return;
        s.assist = assist;
        EditorUtility.SetDirty(s);
    }

    static void Makes(CustomerDef c, params CraftType[] crafts)
    {
        if (c == null || (c.makes != null && c.makes.Length > 0)) return;
        c.makes = crafts;
        EditorUtility.SetDirty(c);
    }

    static void ShopOwner(CustomerDef c)
    {
        if (c == null || c.shopOwner) return;
        c.shopOwner = true;
        EditorUtility.SetDirty(c);
    }

    /// Sets a customer's likes and dislikes, if they have none yet (hand edits stay).
    static void Prefs(CustomerDef c, params (string id, string text, bool likes, SupplyDef supply)[] prefs)
    {
        if (c == null || (c.preferences != null && c.preferences.Length > 0)) return;
        c.preferences = new Preference[prefs.Length];
        for (int i = 0; i < prefs.Length; i++)
            c.preferences[i] = new Preference { id = prefs[i].id, text = prefs[i].text, likes = prefs[i].likes, supply = prefs[i].supply };
        EditorUtility.SetDirty(c);
    }

    /// A boutique item with a placeholder swatch icon; never overwrites an existing one.
    static void Cosmetic(string id, string name, CosmeticSlot slot, int style, int price, Color c)
    {
        string assetPath = $"{Root}/Data/Cosmetics/cosmetic_{id}.asset";
        if (AssetDatabase.LoadAssetAtPath<CosmeticDef>(assetPath) != null) return;
        System.IO.Directory.CreateDirectory($"{Root}/Data/Cosmetics");
        var def = ScriptableObject.CreateInstance<CosmeticDef>();
        def.cosmeticId = id; def.displayName = name; def.slot = slot; def.styleIndex = style; def.price = price; def.color = c;
        def.icon = MakeSprite($"{Root}/Art/Cosmetics/cosmetic_{id}.png", 256, 256, c);
        AssetDatabase.CreateAsset(def, assetPath);
    }

    static void Likes(CustomerDef c, float chance)
    {
        if (c == null || !Mathf.Approximately(c.likesSurprises, 0.65f)) return;   // keep hand-set values
        c.likesSurprises = chance;
        EditorUtility.SetDirty(c);
    }

    static void Character(CustomerDef c, string inkName, int firstChapter, bool romanceable)
    {
        if (!string.IsNullOrEmpty(c.inkName)) return;
        c.inkName = inkName; c.firstChapter = firstChapter; c.romanceable = romanceable;
        EditorUtility.SetDirty(c);
    }

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
