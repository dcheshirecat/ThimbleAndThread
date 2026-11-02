using System.Collections.Generic;
using UnityEngine;

/// Saturdays out in town (owner, 2026-11-01): a map of the town opens, with the places people work, their homes, a
/// coffee shop and every shop. Three outings fill the day (morning, afternoon, evening); going into a shop is free.
/// Everyone you've met is somewhere: most often at their work (CustomerDef.place), otherwise at home. On someone's
/// birthday you can go and find them.
public partial class ShopDay
{
    public struct TownPlace
    {
        public string id, name, blurb;
        public ScreenId shop;          // a shop's screen (Title = not a shop)
        public string activity;        // what you can do there, or null
        public TownPlace(string id, string name, string blurb, string activity = null, ScreenId shop = ScreenId.Title)
        { this.id = id; this.name = name; this.blurb = blurb; this.activity = activity; this.shop = shop; }
    }

    public static readonly TownPlace[] Places =
    {
        new TownPlace("harbour", "The Harbour", "Boats, nets and gulls. Someone's always mending something.", "Walk along the harbour wall"),
        new TownPlace("lighthouse", "The Lighthouse", "White and red on the headland, the light turning even by day."),
        new TownPlace("beach", "The Beach", "Wet sand, rock pools and the lifeguard hut.", "Paddle in the shallows"),
        new TownPlace("cafe", "The Driftwood Café", "Coffee, cinnamon buns and a window full of sea.", "Have a coffee by the window"),
        new TownPlace("tearoom", "Dot's Tea Room", "Doilies, scones, and the Saturday craft circle.", "Join the craft circle"),
        new TownPlace("library", "The Library", "Quiet, warm, and smelling of old paper.", "Read a craft book for an hour"),
        new TownPlace("bookshop", "Ropewalk Books", "Piles of books on every step of the stairs."),
        new TownPlace("church", "St. Brigid's", "The church on the hill, and its hall below.", "Rummage at the jumble sale"),
        new TownPlace("florist", "Petals", "Buckets of flowers out on the pavement."),
        new TownPlace("studio", "Kit's Studio", "A painter's studio above the chandlery, door propped open."),
        new TownPlace("hospital", "The Cottage Hospital", "Small and white, at the top of the town."),
        new TownPlace("school", "The School", "Quiet on a Saturday, except for the music room."),
        new TownPlace("park", "The Park", "Swings, a skate ramp and a bandstand.", "Sit on a bench in the sun"),
        new TownPlace("homes", "Fore Street Homes", "Little houses up the hill, front doors painted every colour."),
        new TownPlace("knot", "Knot & Needle", "The crochet shop across the square."),
        new TownPlace("market", "The Supply Market", "Fabric, thread, yarn and tools.", null, ScreenId.Market),
        new TownPlace("boutique", "Moreau's Boutique", "Clothes, hair and accessories.", null, ScreenId.Boutique),
        new TownPlace("patterns", "Finch's Pattern Parlour", "Patterns, and magazines full of them.", null, ScreenId.PatternShop),
        new TownPlace("workshop", "Marsh's Workshop", "Furniture and decorations for the shop.", null, ScreenId.Furniture),
        new TownPlace("curios", "Quill's Curiosities", "Antiques and oddments in the old customs house.", null, ScreenId.Curios),
        new TownPlace("tech", "Byte & Bobbin", "Repairs and gadgets, up in the chandlery loft.", null, ScreenId.TechShop),
    };

    /// Where each place sits on the town map (owner, 2026-11-02: an interactive map of points, not a list), as fractions of the
    /// map picture (x from the left, y from the bottom; the sea is along the bottom). The placeholder map is drawn to match
    /// (PlaceholderGenerator.TownMap): if the owner's drawing puts a place somewhere else, change its spot here.
    public static Vector2 MapSpot(string placeId) => placeId switch
    {
        "lighthouse" => new Vector2(0.86f, 0.10f),
        "harbour" => new Vector2(0.55f, 0.12f),
        "beach" => new Vector2(0.17f, 0.12f),
        "cafe" => new Vector2(0.28f, 0.25f),
        "home" => new Vector2(0.50f, 0.27f),       // Thimble & Thread
        "curios" => new Vector2(0.74f, 0.22f),
        "tech" => new Vector2(0.88f, 0.34f),
        "studio" => new Vector2(0.66f, 0.38f),
        "boutique" => new Vector2(0.16f, 0.40f),
        "market" => new Vector2(0.42f, 0.42f),
        "knot" => new Vector2(0.27f, 0.53f),
        "patterns" => new Vector2(0.56f, 0.52f),
        "workshop" => new Vector2(0.84f, 0.50f),
        "tearoom" => new Vector2(0.12f, 0.63f),
        "florist" => new Vector2(0.70f, 0.62f),
        "bookshop" => new Vector2(0.42f, 0.64f),
        "library" => new Vector2(0.20f, 0.75f),
        "park" => new Vector2(0.56f, 0.76f),
        "homes" => new Vector2(0.84f, 0.72f),
        "church" => new Vector2(0.30f, 0.88f),
        "school" => new Vector2(0.60f, 0.90f),
        "hospital" => new Vector2(0.86f, 0.86f),
        _ => new Vector2(0.5f, 0.5f),
    };

    /// The streets between places on the map (the placeholder drawing's roads).
    public static readonly (string, string)[] MapRoads =
    {
        ("beach", "cafe"), ("cafe", "home"), ("home", "harbour"), ("harbour", "curios"), ("curios", "tech"), ("tech", "lighthouse"),
        ("home", "market"), ("market", "boutique"), ("boutique", "tearoom"), ("market", "knot"), ("market", "patterns"),
        ("curios", "studio"), ("studio", "patterns"), ("studio", "florist"), ("tech", "workshop"), ("florist", "workshop"),
        ("patterns", "bookshop"), ("florist", "bookshop"), ("knot", "tearoom"), ("bookshop", "library"), ("bookshop", "park"),
        ("park", "school"), ("park", "homes"), ("workshop", "homes"), ("homes", "hospital"), ("school", "hospital"),
        ("library", "church"), ("church", "school"), ("cafe", "boutique"),
    };

    public const int OutingsPerSaturday = 3;
    public const string TownFlag = "town:open";
    static readonly string[] TimesOfDay = { "morning", "afternoon", "evening" };

    public int OutingsDone => Save.flags.FindAll(f => f.StartsWith($"outing:{Save.day}:")).Count;
    public string SaturdayTime => TimesOfDay[Mathf.Clamp(OutingsDone, 0, TimesOfDay.Length - 1)];

    /// Where someone is today (a Saturday): shop owners in their shop; others at work or at home.
    public string PlaceOf(CustomerDef c)
    {
        if (c.shopOwner) { var s = ShopOf(c.customerId); return System.Array.Find(Places, p => p.shop == s).id ?? "homes"; }
        int seed = Save.day * 7919;
        foreach (char ch in c.customerId) seed = seed * 31 + ch;
        var rng = new System.Random(seed);
        return !string.IsNullOrEmpty(c.place) && rng.NextDouble() < 0.6 ? c.place : "homes";
    }

    /// The people you've met at a place today.
    public List<CustomerDef> PeopleAt(string placeId)
    {
        var list = new List<CustomerDef>();
        foreach (var c in content.customers)
            if (c != null && Save.met.Contains(c.customerId) && c.firstChapter <= Save.chapter && PlaceOf(c) == placeId) list.Add(c);
        return list;
    }

    /// The map's place was tapped: what's there, who's there, and what to do.
    public void GoTo(string placeId)
    {
        var place = System.Array.Find(Places, p => p.id == placeId);
        if (place.id == null) return;
        var options = new List<(string, System.Action)>();
        var people = PeopleAt(placeId);
        if (place.shop != ScreenId.Title)
        {
            if (!ShopOpen(place.shop)) { Narrate(null, $"{place.name}. {place.blurb}\n\nIt isn't open to you yet.", ("Back to the map", BackToMap)); return; }
            options.Add(("Go inside", () => EnterShop(OwnerOf(place.shop), place.shop)));
        }
        foreach (var c in people)
        {
            var who = c;
            if (CanWishBirthday(c)) options.Add(($"Wish {c.displayName} a happy birthday", () => WishBirthday(who, () => GoTo(placeId))));
            if (CanDate(c)) options.Add(($"Go on a date with {c.displayName}", () => Visit(who, true)));
            else if (GM.GetRelationship(c.customerId).level >= VisitLevel) options.Add(($"Spend time with {c.displayName}", () => Visit(who, false)));
            else if (!Save.flags.Contains($"hello:{c.customerId}:{Save.day}"))
                options.Add(($"Say hello to {c.displayName}", () => SayHello(who, placeId)));
        }
        if (place.activity != null) options.Add((place.activity, () => Activity(placeId)));
        options.Add(("Back to the map", BackToMap));
        string here = people.Count == 0 ? "No one you know is here just now."
            : people.Count == 1 ? $"{people[0].displayName} is here." : $"Here: {string.Join(", ", people.ConvertAll(c => c.displayName))}.";
        foreach (var c in people) if (IsBirthday(c, Save.day)) here += $"\nIt's {c.displayName}'s birthday today!";
        Narrate(null, $"{place.name}. {place.blurb}\n\n{here}", options.ToArray());
    }

    void BackToMap() => GM.ChangeScreen(ScreenId.TownMap);

    /// A quick hello with someone you don't know well yet: a little friendship, and it doesn't take up the outing.
    void SayHello(CustomerDef c, string placeId)
    {
        Save.flags.Add($"hello:{c.customerId}:{Save.day}");
        GM.AddRelationshipPoints(c.customerId, 2);
        Say(c, Pick("Oh, hello! Fancy seeing you here.", "Day off? Good for you.", "You're out and about! The shop must be shut."),
            ("Continue", () => GoTo(placeId)));
    }

    void Activity(string placeId)
    {
        switch (placeId)
        {
            case "tearoom": CraftCircle(); return;
            case "church": JumbleSale(); return;
            case "library":
                Progress.AddBonusXp(10);
                Narrate(null, "You find a battered book on old stitches and lose an hour in it. (+10 experience)", ("Continue", OutingDone));
                return;
            case "cafe":
                Progress.AddBonusXp(5);
                Narrate(null, Pick("A flat white and a cinnamon bun, the sea doing its thing outside. You sketch a pattern on a napkin. (+5 experience)",
                                   "The café's busy. You listen in on three conversations at once and come away with an idea. (+5 experience)"),
                    ("Continue", OutingDone));
                return;
            case "harbour":
                Narrate(null, Pick("Gulls, salt wind, a fishing boat coming in. You buy chips and eat them on the harbour wall.",
                                   "The tide's out. You find a piece of sea glass the colour of mint floss and keep it."), ("Continue", OutingDone));
                return;
            case "beach":
                Narrate(null, Pick("The water's freezing. You last ten seconds and laugh all the way back up the sand.",
                                   "You find a perfect cockle shell, ridged like knitting."), ("Continue", OutingDone));
                return;
            default:
                Narrate(null, Pick("The sun comes out. You stay longer than you meant to.", "A quiet hour, just watching the town go by."),
                    ("Continue", OutingDone));
                return;
        }
    }

    /// An outing's over: the next part of the day on the map, or home in the evening.
    void OutingDone()
    {
        int n = OutingsDone;
        Save.flags.Add($"outing:{Save.day}:{n}");
        Save.clock = Mathf.Min(ClosesAt, OpensAt + (n + 1) * 180);
        GM.Persist();
        if (n + 1 >= OutingsPerSaturday)
        {
            Narrate(null, "The light's going gold over the harbour. Time to head home.", ("Head home", EndSaturday));
            return;
        }
        BackToMap();
    }

    /// The map's "Go Home" button: the rest of Saturday in the workroom.
    public void GoHomeSaturday() => EndSaturday();
}
