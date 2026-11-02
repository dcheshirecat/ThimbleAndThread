using System.Collections.Generic;
using UnityEngine;

/// Weekends (owner, 2026-10-31): the shop keeps a different routine at the end of the week.
///  - Saturday: the shop stays closed. Visit a friend you're close to (friendship level 2+), or go on a date
///    with someone you're seeing, or stay in and craft. Then it's night, as usual.
///  - Sunday: a stall at the market on the square, 10 to 4. Shoppers come by every few minutes, each after a
///    handmade piece from the stall (made with "Make One for the Stall" in the Pattern Studio) or a copy of one
///    of your patterns. Sell, ask for more (they may walk off), or keep it. Unlocked and explained the first Sunday.
public partial class ShopDay
{
    public const string StallId = "stall";                 // Order.customerId for pieces made to sell at the stall
    public const string StallFlag = "stall:open";
    public const int StallOpens = 10 * 60, StallCloses = 16 * 60;
    public const int VisitLevel = 2;                       // friendship needed for a Saturday visit

    public bool IsSaturday => Save != null && Week.IsSaturday(Save.day);
    public bool IsSunday => Save != null && Week.IsSunday(Save.day);
    string SaturdayDoneFlag => $"saturday:{Save.day}";

    /// A new day starts: weekends begin their own way.
    void StartWeekendDay()
    {
        if (IsSaturday) { Save.visitorsToday.Clear(); StartSaturday(); }
        else if (IsSunday) StartSunday();
    }

    // ---- Saturday ----

    /// Friends you can visit (romance first, then the closest); at most four.
    public List<CustomerDef> SaturdayFriends()
    {
        var list = new List<CustomerDef>();
        foreach (var c in content.customers)
            if (c != null && Save.met.Contains(c.customerId) && !string.IsNullOrEmpty(c.inkName)
                && (GM.GetRelationship(c.customerId).level >= VisitLevel || CanDate(c)))
                list.Add(c);
        list.Sort((a, b) =>
        {
            var ra = GM.GetRelationship(a.customerId); var rb = GM.GetRelationship(b.customerId);
            int r = CanDate(b).CompareTo(CanDate(a));
            return r != 0 ? r : rb.level.CompareTo(ra.level);
        });
        return list.GetRange(0, Mathf.Min(4, list.Count));
    }

    /// Saturday: the town map opens (ShopDayTown.cs).
    public void StartSaturday()
    {
        if (Save.flags.Contains(SaturdayDoneFlag)) return;
        if (OutingsDone == 0) Save.clock = OpensAt;
        GM.ChangeScreen(ScreenId.TownMap);
    }

    /// A Saturday with a friend: their ink knot name_saturday* (name_date* when you're seeing each other), else a
    /// few plain lines. Spending the day together counts for more than a chat.
    void Visit(CustomerDef c, bool date)
    {
        Met(c.customerId);
        string knot = Fresh(Variants(date ? $"{c.inkName}_date" : $"{c.inkName}_saturday"));
        System.Action done = () =>
        {
            GM.AddRelationshipPoints(c.customerId, date ? 12 : 8);
            if (date) AddHearts(c, 15);
            Progress.AddNews(date ? $"A Saturday date with {c.displayName}." : $"Spent Saturday with {c.displayName}.");
            Stamps.Earn(Save, date ? "weekend:date" : "weekend:visit", date ? "A Saturday date" : "A day with a friend", c.displayName,
                        date ? StampTheme.Romance : StampTheme.Friendship);
            OutingDone();
        };
        if (knot != null) { Play(c, new List<string> { knot }, done); return; }
        Say(c, date ? Pick("I've been looking forward to this all week.", "Just us. No shop, no customers. Perfect.")
                    : Pick("Come in, come in! I'll put the kettle on.", "A whole Saturday? Brilliant. I know just the place."),
            ("Continue", () => Narrate(c, date ? "The hours go by far too quickly." : "You talk and talk; the time flies.", ("Continue", done))));
    }

    // ---- More to do on a Saturday (owner, 2026-10-31: keep working on the weekends) ----

    public const int CircleXp = 25, CircleFriendship = 3;

    /// The craft circle at the tea room: an afternoon of stitching and gossip. Experience in your main craft, and a little
    /// friendship with the crafty friends who are there.
    void CraftCircle()
    {
        var craft = Progress.MostPractised();
        string level = Progress.AddXp(craft, CircleXp);
        var there = new List<CustomerDef>();
        foreach (var c in content.customers)
            if (c != null && !c.shopOwner && Save.met.Contains(c.customerId) && c.makes != null && c.makes.Length > 0) there.Add(c);
        for (int i = there.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (there[i], there[j]) = (there[j], there[i]); }
        if (there.Count > 3) there.RemoveRange(3, there.Count - 3);
        foreach (var c in there) GM.AddRelationshipPoints(c.customerId, CircleFriendship);
        string who = there.Count == 0 ? "A circle of strangers with knitting bags makes room for you. By the end of the afternoon they're not strangers."
            : there.Count == 1 ? $"{there[0].displayName} waves you over to a seat by the window."
            : $"{string.Join(", ", there.ConvertAll(c => c.displayName).GetRange(0, there.Count - 1))} and {there[there.Count - 1].displayName} are already there, needles going.";
        Stamps.Earn(Save, "weekend:circle", "The craft circle", "Saturdays at the tea room.", StampTheme.Friendship);
        Narrate(null, $"{who}\n\nTea, cake, and a lot of {Progression.CraftName(craft).ToLower()}. (+{CircleXp} experience)" + (level != null ? $"\n{level}" : ""),
            ("Continue", OutingDone));
    }

    /// The jumble sale: three odd lots of supplies, cheap. Buy one, or just enjoy the rummage.
    void JumbleSale()
    {
        var pool = new List<SupplyDef>();
        foreach (var s in content.supplies)
            if (s != null && s.market != MarketSlot.NotSold && s.kind != SupplyKind.Book && Progress.SupplyShown(s)
                && !(s.kind == SupplyKind.Tool && shop.Owns(s.supplyId))) pool.Add(s);
        var options = new List<(string, System.Action)>();
        for (int n = 0; n < 3 && pool.Count > 0; n++)
        {
            var s = pool[Random.Range(0, pool.Count)];
            pool.Remove(s);
            int price = Mathf.Max(1, s.price / 2);
            options.Add(($"{s.displayName} ({price} coin{(price == 1 ? "" : "s")})", () =>
            {
                if (!GM.TrySpend(price)) { Narrate(null, "You count your coins. Not quite enough. Never mind.", ("Continue", OutingDone)); return; }
                shop.Add(s.supplyId, Mathf.Max(1, s.perPack));
                GM.Persist();
                Narrate(null, Pick($"A bargain! The {s.displayName.ToLower()} goes in your bag.", $"The lady at the till wraps the {s.displayName.ToLower()} in yesterday's paper."),
                    ("Continue", OutingDone));
            }));
        }
        options.Add(("Just enjoy the rummage", () => Narrate(null,
            Pick("You leave with nothing but a slice of lemon drizzle and a very good mood.", "A hand-knitted tea cosy shaped like a hen. You don't buy it. You'll think about it all week."),
            ("Continue", OutingDone))));
        Narrate(null, "Trestle tables, bric-a-brac and a tea urn. Under a pile of curtains you find a box of craft things, all half price:", options.ToArray());
    }

    void EndSaturday()
    {
        if (!Save.flags.Contains(SaturdayDoneFlag)) Save.flags.Add(SaturdayDoneFlag);
        LastMessage = "";
        CloseShop();   // on to the evening, as after any day
    }

    // ---- Sunday: the market stall ----

    public bool StallOpen => Save != null && Save.flags.Contains(StallFlag);
    /// The stall needs a shop that's made a name for itself (owner, 2026-11-01): level 4.
    public const int StallLevel = 4;
    public bool CanHaveStall => Progress.PlayerLevel >= StallLevel;

    /// A Sunday before the stall: a quiet day off.
    void QuietSunday()
    {
        Save.visitorsToday.Clear();
        Narrate(null, "Sunday. The shop's closed, and the square fills with market stalls. Maybe one day you'll have one of your own.",
            ("Wander round the market", () => Narrate(null,
                Pick("Jars of honey, a man selling lobster pots, a stall of old buttons in tins. You buy a tin of buttons for nothing.",
                     "Stallholders call out prices. You note which ones draw a crowd."),
                ("Head home", EndSaturday))),
            ("Stay in and craft", EndSaturday));
    }

    public void StartSunday()
    {
        if (!StallOpen && !CanHaveStall) { QuietSunday(); return; }
        if (!StallOpen)
        {
            Save.flags.Add(StallFlag);
            Stamps.Earn(Save, StallFlag, "A market stall", "Every Sunday, a stall at the market on the square.", StampTheme.Shop);
            Explainers.Explain(Save, StallFlag, true,
                "Sunday market\nOn Sundays the shop stays shut and you take a stall at the market on the square, from 10 till 4. " +
                "Lots of people come by: sell them your handmade pieces and copies of your patterns, as many as you can.",
                "Stocking the stall\nAt night, open a pattern in the Pattern Studio and tap Make One for the Stall. Finished pieces " +
                "wait for Sunday. Shoppers name a price: take it, ask for more (they might walk off), or keep the piece.");
        }
        Save.visitorsToday.Clear();
        Save.clock = StallOpens;
        stallSoldToday.Clear();
        shopper = null;
        nextShopper = StallOpens + Random.Range(3, 8);
        GM.ChangeScreen(ScreenId.Stall);
    }

    /// Someone at the stall: what they're after and what they'll pay.
    public class Shopper
    {
        public string line;          // what they say
        public StallItem piece;      // a handmade piece from the stall, or
        public OwnedPattern pattern; // a copy of a pattern
        public int offer;
        public bool haggled;
        public int leavesAt;         // they wander off if no one serves them
        public CustomerDef friend;   // someone you know, come to see your stall
        public string name;          // who they are ("A fisherman in oilskins", or a friend's name)
        public Color face;           // a stranger's placeholder portrait colour
        public bool chatted;
    }

    static readonly string[] Strangers =
    {
        "A fisherman in oilskins", "A woman with a pram", "A boy with a kite", "A tourist with a map", "An old man with a terrier",
        "A girl in a yellow raincoat", "A couple holding hands", "A man with a paper bag of chips", "A woman in a sunhat", "A cyclist",
    };

    // ---- The stall's patterns (owner, 2026-11-01): choose which are on show; a copy sold is a copy gone (keep your own) ----
    public const int StallPatternSpots = 4;
    public List<OwnedPattern> StallShown() => Save.patterns.FindAll(p => Save.stallPatterns.Contains(p.id));
    public static int CopiesToSell(OwnedPattern p) => Mathf.Max(0, p.copies - 1);

    public void ToggleStallPattern(string id)
    {
        if (Save.stallPatterns.Remove(id)) { GM.Persist(); return; }
        var p = Save.patterns.Find(x => x.id == id);
        if (p == null || IsMystery(p) || Save.stallPatterns.Count >= StallPatternSpots) return;
        Save.stallPatterns.Add(id);
        GM.Persist();
    }

    /// A word with whoever's at the stall, buying or not (owner, 2026-11-01: anyone, not just friends).
    public void StallChat()
    {
        var s = shopper;
        if (s == null || s.chatted) return;
        s.chatted = true;
        Progress.AddBonusXp(2);
        if (s.friend != null) GM.AddRelationshipPoints(s.friend.customerId, 3);
        s.line = s.friend != null
            ? Pick("Isn't it a lovely day for it? I'm so glad you've got a stall.", "I've told everyone at work about your shop, you know.", "You look like you were born behind a stall!")
            : Pick("Lovely morning for it. Is all this your own work? Amazing.", "My gran used to knit like this. You've brought it all back.",
                   "We're just visiting, but we'll come and find your shop.", "How long does something like that take you? Goodness.");
    }

    public Shopper CurrentShopper => shopper;
    Shopper shopper;
    int nextShopper;
    readonly Dictionary<string, int> stallSoldToday = new Dictionary<string, int>();
    public int StallSales { get; private set; }

    /// Called by the stall screen every frame: the clock moves and shoppers come and go.
    public void StallTick()
    {
        if (PauseMenu.Paused) return;
        if (Save == null || !IsSunday || Save.night) return;
        if (Save.clock < StallCloses)
        {
            clockCarry += Time.unscaledDeltaTime * MinutesPerSecond;
            int whole = (int)clockCarry;
            if (whole > 0) { clockCarry -= whole; Save.clock = Mathf.Min(StallCloses, Save.clock + whole); }
        }
        if (shopper != null && Save.clock >= shopper.leavesAt) { LastMessage = "They wandered off to the next stall."; shopper = null; }
        if (shopper == null && Save.clock >= nextShopper && Save.clock < StallCloses)
        {
            shopper = NewShopper();
            nextShopper = Save.clock + Random.Range(6, 15);
        }
    }

    Shopper NewShopper()
    {
        var wants = new List<Shopper>();
        foreach (var piece in Save.stall)
        {
            var item = content.Item(piece.itemId);
            if (item == null) continue;
            int price = Mathf.Max(1, Mathf.RoundToInt(item.basePrice * Mathf.Lerp(0.6f, 1.15f, piece.quality)));
            wants.Add(new Shopper { piece = piece, offer = price,
                line = Pick($"Oh, look at this {piece.name}! Would you take {price} coins?", $"Is the {piece.name} handmade? I'll give you {price} for it.",
                            $"My daughter would love the {piece.name}. {price} coins?") });
            wants.Add(wants[wants.Count - 1]);   // finished pieces draw the eye: twice as likely
        }
        foreach (var p in StallShown())
        {
            if (IsMystery(p) || CopiesToSell(p) <= 0) continue;   // only copies you can spare
            wants.Add(new Shopper { pattern = p, offer = p.price,
                line = Pick($"I make these myself. Could I buy a copy of your {p.name} pattern? {p.price} coins?",
                            $"That {p.name} chart is lovely. {p.price} for a copy?") });
        }
        int leaves = Save.clock + 20;
        string stranger = Strangers[Random.Range(0, Strangers.Length)];
        var face = Color.HSVToRGB(Random.value, 0.35f, 0.85f);
        if (wants.Count == 0 || Random.value < 0.15f)
            return new Shopper { line = Pick("Just looking, thanks! Lovely stall.", "Gorgeous work. I'll come back when I've been paid.", "Ooh. Hmm. Maybe next week!"),
                                 leavesAt = Save.clock + 8, name = stranger, face = face };
        var s = wants[Random.Range(0, wants.Count)];
        var picked = new Shopper { line = s.line, piece = s.piece, pattern = s.pattern, offer = s.offer, leavesAt = leaves, name = stranger, face = face };
        // Now and then it's someone you know (owner, 2026-10-31: more to the weekends): a sale to a friend counts for friendship.
        var friends = new List<CustomerDef>();
        foreach (var c in content.customers)
            if (c != null && !c.shopOwner && Save.met.Contains(c.customerId) && !Save.visitorsToday.Contains(c.customerId)) friends.Add(c);
        if (friends.Count > 0 && Random.value < 0.25f)
        {
            picked.friend = friends[Random.Range(0, friends.Count)];
            Save.visitorsToday.Add(picked.friend.customerId);   // once each per Sunday
            picked.name = picked.friend.displayName;
            picked.line = Pick("Ooh, your stall! ", "There you are! ", "I came to see what you've made. ") + picked.line;
        }
        return picked;
    }

    public void StallSell()
    {
        var s = shopper;
        if (s == null || (s.piece == null && s.pattern == null)) { shopper = null; return; }
        GM.AddCoins(s.offer);
        Save.coinsToday += s.offer;
        if (s.friend != null) GM.AddRelationshipPoints(s.friend.customerId, 4);
        Progress.AddBonusXp(s.friend != null ? 5 : 3);   // a good sale is experience too (owner, 2026-11-01)
        AudioManager.Play(Sfx.Coin);
        if (s.piece != null) { Save.stall.Remove(s.piece); LastMessage = $"Sold the {s.piece.name} for {s.offer} coins."; }
        else
        {
            s.pattern.copies--;
            s.pattern.sold++;
            Save.patternsSoldToday++;
            Save.totalPatternsSold++;
            LastMessage = $"Sold a copy of {s.pattern.name} for {s.offer} coins.";
        }
        StallSales++;
        shopper = null;
        GM.Persist();
    }

    /// Ask for a bit more: half the time they agree (+20%), otherwise they walk off.
    public void StallHaggle()
    {
        var s = shopper;
        if (s == null || s.haggled || (s.piece == null && s.pattern == null)) return;
        s.haggled = true;
        if (Random.value < 0.5f)
        {
            s.offer = Mathf.CeilToInt(s.offer * 1.2f);
            Progress.AddBonusXp(4);
            s.line = Pick($"Fair enough, it's worth it. {s.offer} it is.", $"You drive a hard bargain! {s.offer}, then.");
        }
        else { LastMessage = "\"Hmm, a bit much for me.\" They moved on."; shopper = null; }
    }

    public void StallDecline()
    {
        if (shopper == null) return;
        LastMessage = shopper.piece != null || shopper.pattern != null ? "You kept it. They smiled and moved on." : "";
        shopper = null;
    }

    /// Pack up the stall: the day's done (the evening in the workroom, as usual).
    public void PackUpStall()
    {
        shopper = null;
        LastMessage = StallSales > 0 ? $"A good Sunday at the market: {StallSales} sold." : "A quiet Sunday at the market.";
        StallSales = 0;
        CloseShop();
    }

    /// Pattern Studio: make the selected pattern to sell on the stall.
    public void MakeForStall() => MakeForStall(SelectedPattern, ScreenId.PatternEdit);

    /// The workroom's "Craft for the Stall": any pattern in your book you can make (owner, 2026-11-01).
    public void MakeForStall(string patternId) => MakeForStall(Save.patterns.Find(x => x.id == patternId), ScreenId.Workroom);

    /// Patterns in your book you could make for the stall.
    public List<OwnedPattern> StallPatterns() => Save.patterns.FindAll(p => PatternShown(p) && Progress.CanMake(ItemFor(p.itemId)) && (!p.designed || p.design.cells.Count > 0));

    void MakeForStall(OwnedPattern p, ScreenId back)
    {
        var item = p != null ? content.Item(p.itemId) : null;
        if (item == null || !(StallOpen || CanHaveStall)) return;
        if (!Progress.CanMake(item)) { LastMessage = $"You haven't learned {Progression.CraftName(item.craft)} yet."; GM.ChangeScreen(back); return; }
        Save.orders.Add(new Order
        {
            id = System.Guid.NewGuid().ToString("N"), customerId = StallId, itemId = item.itemId, item = item,
            dayPlaced = Save.day, target = DesignFor(p),
        });
        LastMessage = $"A {item.itemName.ToLower()} for your market stall is in the order book. Work on it at night.";
        GM.Persist();
        GM.ChangeScreen(back);
    }

    /// A stall piece is finished: it waits for Sunday.
    void SetAsideForStall()
    {
        Save.stall.Add(new StallItem
        {
            id = Selected.id, itemId = Selected.itemId, quality = Selected.accuracy,
            name = $"{Selected.target.displayName} {Selected.item.itemName.ToLower()}".Trim(),
        });
        Save.orders.Remove(Selected);
        GM.Persist();
        LastMessage = $"The {Selected.item.itemName.ToLower()} is packed for Sunday's market stall ({Save.stall.Count} ready).";
        Selected = null;
        GM.ChangeScreen(ScreenId.OrderBook);
    }
}
