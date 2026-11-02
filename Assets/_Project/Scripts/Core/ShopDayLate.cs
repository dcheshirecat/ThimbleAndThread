using System.Collections.Generic;
using UnityEngine;

/// Later in the game (owner, 2026-11-01):
///  - Quill's Curiosities (level 6, Jasper Quill): decorations that do something: more experience, more visitors in certain
///    hours, more friendship with one person (DecorDef.boost).
///  - Online orders (level 7): a computer appears on the counter; people order online, and a courier collects the finished
///    parcels every Friday.
///  - Byte & Bobbin (level 9, Ravi Sen): automations, each with a cost: the door answers itself (orders, but no friendship),
///    online orders accept themselves.
///  - Expansions from Marsh's Workshop: the attic (level 7), then the basement (level 9). Sort through them once a day for
///    your great-aunt's second-hand things, which customers and stall shoppers buy.
public partial class ShopDay
{
    public const string CuriosOwnerId = "jasper", TechOwnerId = "ravi";
    public const string CuriosFlag = "shop:curios", TechFlag = "shop:tech", OnlineFlag = "online:open";
    public const int CuriosLevel = 6, OnlineLevel = 7, TechLevel = 9;

    // ---- Decorations that do something ----

    IEnumerable<DecorDef> PlacedDecor()
    {
        foreach (var p in PlacedPieces()) yield return p.def;
    }

    /// Extra experience, in percent, from what's placed.
    public int XpBoost()
    {
        int n = 0;
        foreach (var d in PlacedDecor()) if (d.boost == DecorBoost.Experience) n += d.boostAmount;
        return n;
    }

    /// How much sooner visitors come at this time of day, in percent.
    int VisitorBoost(int clock)
    {
        int n = 0;
        foreach (var d in PlacedDecor())
            if (d.boost == DecorBoost.Visitors && clock >= d.boostFrom * 60 && clock < d.boostTo * 60) n += d.boostAmount;
        return Mathf.Min(n, 60);
    }

    int NextGap(int clock) => Mathf.Max(3, Gap(clock) * (100 - VisitorBoost(clock)) / 100);

    bool boosting;
    /// Friendship points with someone a placed piece is about count for more.
    void FriendshipBoost(string customerId, int points)
    {
        if (boosting || points <= 0) return;
        int extra = 0;
        foreach (var d in PlacedDecor())
            if (d.boost == DecorBoost.Friendship && d.boostFriend != null && d.boostFriend.customerId == customerId)
                extra += Mathf.Max(1, points * d.boostAmount / 100);
        if (extra == 0) return;
        boosting = true;
        GM.AddRelationshipPoints(customerId, extra);
        boosting = false;
    }

    /// What a piece does, in words.
    public static string BoostText(DecorDef d) => d.boost switch
    {
        DecorBoost.Experience => $"+{d.boostAmount}% experience",
        DecorBoost.Visitors => $"visitors come {d.boostAmount}% sooner, {TimeText(d.boostFrom * 60)} to {TimeText(d.boostTo * 60)}",
        DecorBoost.Friendship => d.boostFriend != null ? $"+{d.boostAmount}% friendship with {d.boostFriend.displayName.Split(' ')[0]}" : "",
        _ => "",
    };

    // ---- Online orders ----

    public const string OnlineId = "online";
    public bool OnlineOpen => Save != null && Save.flags.Contains(OnlineFlag);
    public static bool IsCourierDay(int day) => Week.Index(day) == 4;   // Fridays

    /// A fresh handful of online orders each morning: things you can make, from patterns in your book.
    void NewOnlineOffers()
    {
        Save.onlineOffers.Clear();
        if (!OnlineOpen) return;
        var pool = Save.patterns.FindAll(p => !p.fromMagazine || !IsMystery(p));
        pool.RemoveAll(p => !Progress.CanMake(ItemFor(p.itemId)) || (p.designed && p.design.cells.Count == 0));
        int n = Random.Range(1, 4);
        for (int i = 0; i < n && pool.Count > 0; i++)
        {
            var p = pool[Random.Range(0, pool.Count)];
            var item = ItemFor(p.itemId);
            Save.onlineOffers.Add(new OnlineOffer { patternId = p.id, itemId = item.itemId, price = Mathf.RoundToInt(item.basePrice * Random.Range(0.9f, 1.3f)) });
        }
        if (AutomationOn("inbox")) for (int i = Save.onlineOffers.Count - 1; i >= 0; i--) AcceptOnline(i, quiet: true);
    }

    public void AcceptOnline(int index) => AcceptOnline(index, false);

    void AcceptOnline(int index, bool quiet)
    {
        if (index < 0 || index >= Save.onlineOffers.Count) return;
        var offer = Save.onlineOffers[index];
        var p = Save.patterns.Find(x => x.id == offer.patternId);
        var item = ItemFor(offer.itemId);
        Save.onlineOffers.RemoveAt(index);
        if (p == null || item == null) return;
        Save.orders.Add(new Order
        {
            id = System.Guid.NewGuid().ToString("N"), customerId = OnlineId, itemId = item.itemId, item = item, dayPlaced = Save.day,
            target = DesignFor(p), agreedPrice = offer.price,
        });
        if (!quiet) Stamps.Notice($"Online order taken: {item.itemName.ToLower()} ({p.name}).\n<size=68>It goes out with Friday's courier.</size>");
        GM.Persist();
    }

    /// Friday: the courier collects every finished online order and pays for it.
    void Courier()
    {
        if (!IsCourierDay(Save.day)) return;
        var parcels = Save.orders.FindAll(o => o.customerId == OnlineId && o.finished);
        if (parcels.Count == 0) return;
        int coins = 0;
        foreach (var o in parcels)
        {
            int pay = Mathf.RoundToInt(o.agreedPrice * Mathf.Lerp(0.6f, 1.1f, o.accuracy) * (1f + Charm / 100f));
            coins += pay;
            Save.orders.Remove(o);
            Save.totalDelivered++;
            if (o.item != null) Progress.CountOrder(o.item.craft);
        }
        GM.AddCoins(coins);
        Stamps.Earn(Save, "online:courier", "The first courier", "Your parcels are going all over the country.", StampTheme.Shop);
        Stamps.Announce($"Friday: the courier came\n<size=68>{parcels.Count} parcel{(parcels.Count == 1 ? "" : "s")} collected: +{coins} coins.</size>");
        GM.Persist();
    }

    // ---- Automations from Byte & Bobbin ----

    public struct Automation
    {
        public string id, name, what;
        public int price;
        public Automation(string id, string name, string what, int price) { this.id = id; this.name = name; this.what = what; this.price = price; }
    }

    public static readonly Automation[] Automations =
    {
        new Automation("door", "The Door-Answering Bell", "Answers the door for you: customers order something on display, or buy a pattern, all by themselves. You won't get to know them that way: no friendship.", 300),
        new Automation("inbox", "The Order Inbox", "Accepts every online order each morning by itself.", 200),
    };

    public bool Owns(string automationId) => Save.automations.Contains(automationId);
    public bool AutomationOn(string automationId) => Owns(automationId) && !Save.flags.Contains($"auto_off:{automationId}");

    public void BuyOrToggleAutomation(string automationId)
    {
        var a = System.Array.Find(Automations, x => x.id == automationId);
        if (a.id == null || Save.night) return;
        if (Owns(a.id))
        {
            string off = $"auto_off:{a.id}";
            if (!Save.flags.Remove(off)) Save.flags.Add(off);
            LastMessage = AutomationOn(a.id) ? $"{a.name}: on." : $"{a.name}: off.";
        }
        else if (!GM.TrySpend(a.price)) LastMessage = $"{a.name} is {a.price} coins.";
        else
        {
            Save.automations.Add(a.id);
            GM.AddRelationshipPoints(TechOwnerId, 4);
            boughtThisVisit = true;
            Stamps.Earn(Save, $"auto:{a.id}", a.name, "Ravi installed it. Mostly.", StampTheme.Shop);
            LastMessage = $"{a.name} is installed. Switch it off here any time.";
        }
        GM.Persist();
        GM.ChangeScreen(ScreenId.TechShop);
    }

    /// The door answers itself: the visitor orders something on display they'd like, buys a pattern, or leaves.
    bool AutoServe()
    {
        if (!AutomationOn("door") || Save.visitorsToday.Count == 0) return false;
        var c = CustomerFor(Save.visitorsToday[0]);
        if (c == null || !Save.met.Contains(c.customerId) || PickupFor(c) != null) return false;   // new faces and pickups still ring
        Save.visitorsToday.RemoveAt(0);
        var wants = OrderGenerator.Wants(c, GM.GetRelationship(c.customerId).level).FindAll(i => Progress.CanMake(i));
        var shown = Shelf.Find(p => wants.Exists(i => i.itemId == p.itemId));
        if (shown != null && !Save.orders.Exists(o => o.customerId == c.customerId))
        {
            var item = ItemFor(shown.itemId);
            Save.orders.Add(OrderGenerator.FromPattern(c, item, DesignFor(shown), Save.day));
            shown.ordered++;
            Stamps.Notice($"The bell answered the door: {c.displayName} ordered a {item.itemName.ToLower()}.");
        }
        else if (Shelf.Count > 0)
        {
            var p = Shelf[Random.Range(0, Shelf.Count)];
            GM.AddCoins(p.price);
            p.sold++; Save.totalPatternsSold++; Save.patternsSoldToday++; Save.coinsToday += p.price;
            Stamps.Notice($"The bell answered the door: {c.displayName} bought the {p.name} pattern. (+{p.price} coins)");
        }
        GM.Persist();
        return true;
    }

    // ---- Expansions: the attic and the basement, and what's in them ----

    public struct Expansion
    {
        public string id, name, what;
        public int price, level;
        public Expansion(string id, string name, string what, int price, int level) { this.id = id; this.name = name; this.what = what; this.price = price; this.level = level; }
    }

    public static readonly Expansion[] Expansions =
    {
        new Expansion("attic", "Open Up the Attic", "Benji fixes the ladder and the floor: your great-aunt's attic, full of things to sell second-hand.", 300, 7),
        new Expansion("basement", "Clear Out the Basement", "Damp-proofing, shelves and a light: the basement's older, rarer finds.", 500, 9),
    };

    static readonly (string name, int price)[] AtticFinds =
    {
        ("a tin of old buttons", 6), ("a brass thimble", 8), ("a darning mushroom", 5), ("a bundle of lace offcuts", 7), ("a 1950s knitting pattern", 9),
        ("a wooden spool rack", 14), ("a sewing-themed tea towel", 6), ("a hand-painted button box", 12),
    };
    static readonly (string name, int price)[] BasementFinds =
    {
        ("a cast-iron flat iron", 22), ("a treadle sewing machine drawer", 30), ("a sampler from 1888", 45), ("a bolt of real Irish linen", 35),
        ("a pair of silver stork scissors", 28),
    };

    public bool HasExpansion(string id) => Save.expansions.Contains(id);
    public bool CanSort(string id) => HasExpansion(id) && !Save.night && !Save.flags.Contains($"sorted:{id}:{Save.day}");

    public void BuyExpansion(string id)
    {
        var e = System.Array.Find(Expansions, x => x.id == id);
        if (e.id == null || HasExpansion(id) || Save.night) return;
        if (id == "basement" && !HasExpansion("attic")) LastMessage = "The attic first: Benji's ladder only goes one way at a time.";
        else if (Progress.PlayerLevel < e.level) LastMessage = $"Benji can do that from level {e.level}.";
        else if (!GM.TrySpend(e.price)) LastMessage = $"That's {e.price} coins.";
        else
        {
            Save.expansions.Add(id);
            GM.AddRelationshipPoints(FurnitureOwnerId, 5);
            boughtThisVisit = true;
            Stamps.Earn(Save, $"expansion:{id}", id == "attic" ? "The attic" : "The basement", e.what, StampTheme.Shop);
            LastMessage = $"Done! Sort through the {id} from your shop once a day.";
        }
        GM.Persist();
        GM.ChangeScreen(ScreenId.Furniture);
    }

    /// Once a day: an hour among the boxes turns up a few things to sell.
    public void SortThrough(string id)
    {
        if (!CanSort(id)) return;
        Save.flags.Add($"sorted:{id}:{Save.day}");
        var finds = id == "basement" ? BasementFinds : AtticFinds;
        var got = new List<string>();
        for (int n = Random.Range(1, 4); n > 0; n--)
        {
            var f = finds[Random.Range(0, finds.Length)];
            Save.secondhand.Add(new SecondHand { name = f.name, price = f.price });
            got.Add(f.name);
        }
        PassMinutes(60);
        Stamps.Notice($"An hour in the {id}.\n<size=68>You found {string.Join(", ", got)}. They're on your second-hand shelf.</size>");
        GM.Persist();
    }

    /// A visitor spots something on the second-hand shelf.
    bool SecondHandSale(CustomerDef c, System.Action then)
    {
        if (Save.secondhand.Count == 0 || Random.value > 0.3f) return false;
        var s = Save.secondhand[Random.Range(0, Save.secondhand.Count)];
        Save.secondhand.Remove(s);
        GM.AddCoins(s.price);
        Save.coinsToday += s.price;
        Narrate(c, $"{c.displayName} picks up {s.name} from your second-hand shelf. \"I'll take this!\" (+{s.price} coins)", ("Continue", then));
        return true;
    }
}
