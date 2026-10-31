using System.Collections.Generic;
using UnityEngine;

/// Special orders (owner, 2026-10-31): anything the shops can get, ordered and delivered later. The rarer it is,
/// the longer it takes and the higher your level must be; only things for crafts you've learned. Supplies come from
/// the Supply Market, patterns from the pattern shop. Deliveries arrive in the morning (ReceiveDeliveries).
public partial class ShopDay
{
    public static int DeliveryDays(MarketSlot rarity) => rarity switch
    {
        MarketSlot.Rare => 4,
        MarketSlot.Rotating => 2,
        _ => 1,
    };

    /// The level you need to order something this rare.
    public static int OrderLevel(MarketSlot rarity) => rarity switch
    {
        MarketSlot.Rare => 4,
        MarketSlot.Rotating => 2,
        _ => 1,
    };

    public bool CanOrderAtLevel(MarketSlot rarity) => Progress.PlayerLevel >= OrderLevel(rarity);

    /// What the market can order in: every supply for a craft you've learned, except what's never sold and
    /// tools or books you already have.
    public List<SupplyDef> Orderable()
    {
        var list = new List<SupplyDef>();
        foreach (var s in content.supplies)
        {
            if (s == null || s.market == MarketSlot.NotSold || !Progress.SupplyShown(s)) continue;
            if ((s.kind == SupplyKind.Tool || s.kind == SupplyKind.Book) && (shop.Owns(s.supplyId) || OnOrder("supply", s.supplyId) > 0)) continue;
            list.Add(s);
        }
        return list;
    }

    /// Packs (or patterns) of this already on their way.
    public int OnOrder(string kind, string id)
    {
        int n = 0;
        foreach (var d in Save.deliveries) if (d.kind == kind && d.id == id) n += d.amount;
        return n;
    }

    /// The first morning something on order arrives, or 0.
    public int ArrivesDay(string kind, string id)
    {
        int day = 0;
        foreach (var d in Save.deliveries) if (d.kind == kind && d.id == id && (day == 0 || d.arrives < day)) day = d.arrives;
        return day;
    }

    public void OrderSupply(string supplyId)
    {
        var s = content.Supply(supplyId);
        if (s == null || Save.night || !Orderable().Contains(s)) return;
        if (!CanOrderAtLevel(s.market)) LastMessage = $"Bram can only get {ShopManager.Rarity(s).ToLower()} things for you once you're level {OrderLevel(s.market)}.";
        else if (!GM.TrySpend(s.price)) LastMessage = $"Not enough coins for {s.displayName} ({s.price} coins).";
        else
        {
            int days = DeliveryDays(s.market);
            Save.deliveries.Add(new Delivery { kind = "supply", id = supplyId, amount = 1, arrives = Save.day + days });
            GM.AddRelationshipPoints(MarketOwnerId, 1);
            boughtThisVisit = true;
            LastMessage = $"Ordered {s.displayName}. It arrives {(days == 1 ? "tomorrow morning" : $"in {days} days")}.";
            GM.Persist();
        }
        GM.ChangeScreen(ScreenId.Market);   // refresh
    }

    /// Patterns the pattern shop can order in: any for a craft you've learned that you don't own and aren't on the
    /// shelf right now.
    public List<PatternDef> OrderablePatterns()
    {
        var list = new List<PatternDef>();
        var onSale = PatternsForSale();
        foreach (var d in content.patterns)
            if (d != null && d.forSale && d.item != null && Progress.CraftOpen(d.item.craft) && !Save.patterns.Exists(p => p.id == d.patternId)
                && !onSale.Contains(d) && OnOrder("pattern", d.patternId) == 0) list.Add(d);
        return list;
    }

    public void OrderPattern(string patternId)
    {
        var d = System.Array.Find(content.patterns, x => x != null && x.patternId == patternId);
        if (d == null || Save.night || !OrderablePatterns().Contains(d)) return;
        if (!CanOrderAtLevel(d.rarity)) LastMessage = $"Ada can only send for patterns that rare once you're level {OrderLevel(d.rarity)}.";
        else if (!GM.TrySpend(d.shopPrice)) LastMessage = $"Not enough coins for {d.displayName} ({d.shopPrice} coins).";
        else
        {
            int days = DeliveryDays(d.rarity);
            Save.deliveries.Add(new Delivery { kind = "pattern", id = patternId, amount = 1, arrives = Save.day + days });
            GM.AddRelationshipPoints(PatternShopOwnerId, 1);
            boughtThisVisit = true;
            LastMessage = $"Ordered the {d.displayName} pattern. It arrives {(days == 1 ? "tomorrow morning" : $"in {days} days")}.";
            GM.Persist();
        }
        GM.ChangeScreen(ScreenId.PatternShop);
    }

    /// Morning: everything due today is on the doorstep. Returns what came, for the good-morning message.
    string ReceiveDeliveries()
    {
        var came = new List<string>();
        for (int i = Save.deliveries.Count - 1; i >= 0; i--)
        {
            var d = Save.deliveries[i];
            if (d.arrives > Save.day) continue;
            Save.deliveries.RemoveAt(i);
            if (d.kind == "supply" && content.Supply(d.id) is SupplyDef s)
            {
                shop.Add(s.supplyId, Mathf.Max(1, s.perPack) * d.amount);
                came.Add(s.displayName);
            }
            else if (d.kind == "pattern" && System.Array.Find(content.patterns, x => x != null && x.patternId == d.id) is PatternDef p
                     && !Save.patterns.Exists(x => x.id == p.patternId))
            {
                Save.patterns.Add(new OwnedPattern { id = p.patternId, name = p.displayName, itemId = p.item.itemId, price = p.price, onShelf = false });
                came.Add($"the {p.displayName} pattern");
            }
        }
        if (came.Count == 0) return "";
        came.Reverse();
        return $" A delivery came: {string.Join(", ", came)}.";
    }
}
