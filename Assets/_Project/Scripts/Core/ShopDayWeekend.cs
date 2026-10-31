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
                && (GM.GetRelationship(c.customerId).level >= VisitLevel || GM.GetRelationship(c.customerId).romance))
                list.Add(c);
        list.Sort((a, b) =>
        {
            var ra = GM.GetRelationship(a.customerId); var rb = GM.GetRelationship(b.customerId);
            int r = rb.romance.CompareTo(ra.romance);
            return r != 0 ? r : rb.level.CompareTo(ra.level);
        });
        return list.GetRange(0, Mathf.Min(4, list.Count));
    }

    public void StartSaturday()
    {
        if (Save.flags.Contains(SaturdayDoneFlag)) return;
        Explainers.Explain(Save, "saturday", true,
            "Saturdays\nThe shop is closed on Saturdays. Spend the day with a friend you've grown close to, or go on a date " +
            "with someone you're seeing. Or stay in: the workroom's always there.");
        var options = new List<(string, System.Action)>();
        foreach (var c in SaturdayFriends())
        {
            var friend = c;
            bool date = GM.GetRelationship(c.customerId).romance;
            options.Add((date ? $"Go on a date with {c.displayName}" : $"Visit {c.displayName}", () => Visit(friend, date)));
        }
        options.Add(("Walk along the harbour", () => Narrate(null,
            Pick("Gulls, salt wind, a fishing boat coming in. You buy chips and eat them on the harbour wall.",
                 "The tide's out. You find a piece of sea glass the colour of mint floss and keep it."),
            ("Head home", EndSaturday))));
        options.Add(("Stay in and craft", EndSaturday));
        Narrate(null, options.Count > 2
            ? "Saturday. The sign stays on CLOSED today. How will you spend it?"
            : "Saturday. The sign stays on CLOSED today. No one's close enough to drop in on yet... maybe soon.",
            options.ToArray());
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
            Progress.AddNews(date ? $"A Saturday date with {c.displayName}." : $"Spent Saturday with {c.displayName}.");
            Stamps.Earn(Save, date ? "weekend:date" : "weekend:visit", date ? "A Saturday date" : "A day with a friend", c.displayName,
                        date ? StampTheme.Romance : StampTheme.Friendship);
            EndSaturday();
        };
        if (knot != null) { Play(c, new List<string> { knot }, done); return; }
        Say(c, date ? Pick("I've been looking forward to this all week.", "Just us. No shop, no customers. Perfect.")
                    : Pick("Come in, come in! I'll put the kettle on.", "A whole Saturday? Brilliant. I know just the place."),
            ("Continue", () => Narrate(c, date ? "The day goes by far too quickly." : "You talk until the afternoon light goes gold.", ("Head home", done))));
    }

    void EndSaturday()
    {
        if (!Save.flags.Contains(SaturdayDoneFlag)) Save.flags.Add(SaturdayDoneFlag);
        LastMessage = "";
        CloseShop();   // on to the evening, as after any day
    }

    // ---- Sunday: the market stall ----

    public bool StallOpen => Save != null && Save.flags.Contains(StallFlag);

    public void StartSunday()
    {
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
    }

    public Shopper CurrentShopper => shopper;
    Shopper shopper;
    int nextShopper;
    readonly Dictionary<string, int> stallSoldToday = new Dictionary<string, int>();
    public int StallSales { get; private set; }

    /// Called by the stall screen every frame: the clock moves and shoppers come and go.
    public void StallTick()
    {
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
        foreach (var p in Save.patterns)
        {
            if (!PatternShown(p) || !Progress.CanMake(ItemFor(p.itemId))) continue;
            stallSoldToday.TryGetValue(p.id, out int sold);
            if (sold >= 2) continue;
            wants.Add(new Shopper { pattern = p, offer = p.price,
                line = Pick($"I make these myself. Could I buy a copy of your {p.name} pattern? {p.price} coins?",
                            $"That {p.name} chart is lovely. {p.price} for a copy?") });
        }
        int leaves = Save.clock + 20;
        if (wants.Count == 0 || Random.value < 0.15f)
            return new Shopper { line = Pick("Just looking, thanks! Lovely stall.", "Gorgeous work. I'll come back when I've been paid.", "Ooh. Hmm. Maybe next week!"), leavesAt = Save.clock + 4 };
        var s = wants[Random.Range(0, wants.Count)];
        return new Shopper { line = s.line, piece = s.piece, pattern = s.pattern, offer = s.offer, leavesAt = leaves };
    }

    public void StallSell()
    {
        var s = shopper;
        if (s == null || (s.piece == null && s.pattern == null)) { shopper = null; return; }
        GM.AddCoins(s.offer);
        Save.coinsToday += s.offer;
        AudioManager.Play(Sfx.Coin);
        if (s.piece != null) { Save.stall.Remove(s.piece); LastMessage = $"Sold the {s.piece.name} for {s.offer} coins."; }
        else
        {
            stallSoldToday.TryGetValue(s.pattern.id, out int sold);
            stallSoldToday[s.pattern.id] = sold + 1;
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
    public void MakeForStall()
    {
        var p = SelectedPattern;
        var item = p != null ? content.Item(p.itemId) : null;
        if (item == null || !StallOpen) return;
        if (!Progress.CanMake(item)) { LastMessage = $"You haven't learned {Progression.CraftName(item.craft)} yet."; GM.ChangeScreen(ScreenId.PatternEdit); return; }
        Save.orders.Add(new Order
        {
            id = System.Guid.NewGuid().ToString("N"), customerId = StallId, itemId = item.itemId, item = item,
            dayPlaced = Save.day, target = DesignFor(p),
        });
        LastMessage = $"A {item.itemName.ToLower()} for your market stall is in the order book. Work on it at night.";
        GM.Persist();
        GM.ChangeScreen(ScreenId.PatternEdit);
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
