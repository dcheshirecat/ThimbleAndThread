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
        new TownPlace("pets", "Wren's Pet Emporium", "Kittens in the window, a parrot by the door.", null, ScreenId.PetShop),
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
        "pets" => new Vector2(0.34f, 0.34f),
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

    /// The people you've met at a place today. At the homes, only the friends whose door you know (KnowsHome).
    public List<CustomerDef> PeopleAt(string placeId)
    {
        var list = new List<CustomerDef>();
        foreach (var c in content.customers)
            if (c != null && Save.met.Contains(c.customerId) && c.firstChapter <= Save.chapter && PlaceOf(c) == placeId
                && (placeId != HomesId || KnowsHome(c))) list.Add(c);
        return list;
    }

    // ---- Not everywhere is open to you (owner, 2026-11-02): a home only when they're in, presents only for close friends ----

    public const string HomesId = "homes";
    public const int HomeLevel = 1;    // friendship at which someone has told you where they live
    public const int GiftLevel = 3;    // friendship needed to give someone a present at home (or romance: hearts enough to date)
    /// Places you can't just wander into: only when someone you know is there.
    static readonly string[] PrivatePlaces = { "hospital", "school", "studio" };

    public bool KnowsHome(CustomerDef c) => c != null && !c.shopOwner && Save.met.Contains(c.customerId)
        && (GM.GetRelationship(c.customerId).level >= HomeLevel || CanDate(c));

    public bool CanGift(CustomerDef c) => GM.GetRelationship(c.customerId).level >= GiftLevel || CanDate(c);

    /// Why a place can't be visited right now, or null when it can.
    public string Shut(TownPlace p)
    {
        if (p.shop != ScreenId.Title) return ShopOpen(p.shop) ? null : "It isn't open to you yet.";
        if (p.id == HomesId)
        {
            if (!System.Array.Exists(content.customers, c => KnowsHome(c))) return "You don't know anyone well enough yet to call at their house.";
            return PeopleAt(HomesId).Count > 0 ? null : "None of your friends are in just now. Try again later.";
        }
        if (p.id == "library" && SaturdayTime == "evening") return "The library's shut for the evening.";
        if (System.Array.IndexOf(PrivatePlaces, p.id) >= 0 && PeopleAt(p.id).Count == 0)
            return "You can't just wander in. Nobody you know is in today.";
        return null;
    }

    /// Fore Street: a front door for each friend whose home you know. You can knock on the ones who are in.
    void Homes()
    {
        var place = System.Array.Find(Places, p => p.id == HomesId);
        var options = new List<(string, System.Action)>();
        var lines = new List<string>();
        foreach (var c in content.customers)
        {
            if (!KnowsHome(c) || c.firstChapter > Save.chapter) continue;
            var who = c;
            if (PlaceOf(c) == HomesId)
            {
                lines.Add($"{c.displayName}'s house: a light on, someone's in.");
                options.Add(($"Knock at {c.displayName}'s", () => AtHome(who)));
            }
            else lines.Add($"{c.displayName}'s house: no one's in.");
        }
        options.Add(("Back to the map", BackToMap));
        Narrate(null, $"{place.name}. {place.blurb}\n\n{string.Join("\n", lines)}", options.ToArray());
    }

    /// At a friend's front door: tea, a birthday, a day together, a present.
    void AtHome(CustomerDef c)
    {
        Met(c.customerId);
        var options = new List<(string, System.Action)>();
        if (CanWishBirthday(c)) options.Add(("Wish them a happy birthday", () => WishBirthday(c, () => AtHome(c))));
        if (CanDate(c)) options.Add(("Ask them out for the day", () => Visit(c, true)));
        else if (GM.GetRelationship(c.customerId).level >= VisitLevel) options.Add(("Spend the day together", () => Visit(c, false)));
        else if (!Save.flags.Contains($"hello:{c.customerId}:{Save.day}")) options.Add(("Stop for a cup of tea", () => SayHello(c, HomesId)));
        if (CanGift(c) && !Save.flags.Contains(GiftFlag(c))) options.Add(("Give them a present", () => ChooseGift(c)));
        options.Add(("Say goodbye", Homes));
        Say(c, Pick("Oh! Hello! Come in, mind the shoes.", "You found me! Come in, the kettle's just boiled.", "Well, this is a nice surprise."), options.ToArray());
    }

    string GiftFlag(CustomerDef c) => $"gift:{c.customerId}:{Save.day}";

    /// Presents are things you've made: the pieces waiting for the Sunday stall.
    void ChooseGift(CustomerDef c)
    {
        var options = new List<(string, System.Action)>();
        foreach (var piece in Save.stall)
        {
            if (options.Count >= 4) break;
            var p = piece;
            options.Add(($"The {p.name}", () => GiveGift(c, p)));
        }
        options.Add(("Not today", () => AtHome(c)));
        Narrate(null, Save.stall.Count == 0
            ? "You haven't anything made to give. (Pieces you make for the stall can be given as presents.)"
            : "Which of your makes would they like?", options.ToArray());
    }

    void GiveGift(CustomerDef c, StallItem piece)
    {
        Save.stall.Remove(piece);
        Save.flags.Add(GiftFlag(c));
        bool wanted = c.requestable != null && System.Array.Exists(c.requestable, o => o != null && o.item != null && o.item.itemId == piece.itemId);
        GM.AddRelationshipPoints(c.customerId, (wanted ? 14 : 8) + Mathf.RoundToInt(piece.quality * 4));
        if (ShowsHearts(c)) AddHearts(c, wanted ? 15 : 10);
        string like = $"{c.customerId}:like:your {piece.name}";   // they said so: the Journal lists it
        if (wanted && !Save.knownPrefs.Contains(like)) Save.knownPrefs.Add(like);
        Progress.AddNews($"You gave {c.displayName} your {piece.name}.");
        Stamps.Earn(Save, "town:gift", "A present", $"For {c.displayName}.", StampTheme.Friendship);
        GM.Persist();
        string knot = Fresh(Variants($"{c.inkName}_gift"));
        if (knot != null) { Play(c, new List<string> { knot }, () => AtHome(c)); return; }
        Say(c, wanted ? Pick($"For me? I've wanted one of these for ages. Oh, I love it.", "You made this? I'm going to cry. It's exactly right.")
                      : Pick("For me? That's so kind. I'll find just the place for it.", "You didn't have to! Thank you. Really."),
            ("Continue", () => AtHome(c)));
    }

    /// The map's place was tapped: what's there, who's there, and what to do.
    public void GoTo(string placeId)
    {
        var place = System.Array.Find(Places, p => p.id == placeId);
        if (place.id == null) return;
        var options = new List<(string, System.Action)>();
        var people = PeopleAt(placeId);
        string shut = Shut(place);
        if (shut != null) { Narrate(null, $"{place.name}. {place.blurb}\n\n{shut}", ("Back to the map", BackToMap)); return; }
        if (placeId == HomesId) { Homes(); return; }
        if (place.shop != ScreenId.Title)
        {
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
