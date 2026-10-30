using System.Collections.Generic;
using UnityEngine;

/// The player's supply cupboard: how much of each supply they have, buying packs, and using them up.
public class ShopManager : MonoBehaviour
{
    public ContentLibrary content;

    SaveData Save => GameManager.Instance.Save;

    public int Stock(string supplyId)
    {
        var s = Save.stock.Find(x => x.supplyId == supplyId);
        return s != null ? s.amount : 0;
    }

    public bool Owns(string supplyId) => Stock(supplyId) > 0;

    /// Adds to the cupboard (bought, or a friend's gift).
    public void Add(string supplyId, int amount)
    {
        var s = Save.stock.Find(x => x.supplyId == supplyId);
        if (s == null) Save.stock.Add(s = new SupplyStock { supplyId = supplyId });
        s.amount += amount;
    }

    // ---- Market: always-on basics, a selection that changes every 4 days, and now and then a rare item ----

    public const int DaysPerRotation = 4;
    public const int RotatingCount = 14;
    const float RareChance = 0.4f;

    public int DaysUntilRestock => DaysPerRotation - (Save.day - 1) % DaysPerRotation;

    /// What the Supply Market sells today. The same all through a 4-day period, different per save.
    public List<SupplyDef> OnSale()
    {
        if (Save.marketSeed == 0)
        {
            Save.marketSeed = Random.Range(1, int.MaxValue);
            GameManager.Instance.Persist();   // keep this save's market the same after a restart
        }
        int period = (Save.day - 1) / DaysPerRotation;
        var rng = new System.Random(unchecked(Save.marketSeed * 31 + period));

        var sale = new List<SupplyDef>();
        var rotating = new List<SupplyDef>();
        var rare = new List<SupplyDef>();
        foreach (var s in content.supplies)
        {
            if (s == null) continue;
            if (s.market == MarketSlot.Always) sale.Add(s);
            else if (s.market == MarketSlot.Rotating) rotating.Add(s);
            else if (s.market == MarketSlot.Rare) rare.Add(s);
        }
        for (int i = rotating.Count - 1; i > 0; i--)   // shuffle, then take the first few
        {
            int j = rng.Next(i + 1);
            (rotating[i], rotating[j]) = (rotating[j], rotating[i]);
        }
        sale.AddRange(rotating.GetRange(0, Mathf.Min(RotatingCount, rotating.Count)));
        if (rare.Count > 0 && rng.NextDouble() < RareChance) sale.Add(rare[rng.Next(rare.Count)]);
        return sale;
    }

    public bool IsOnSale(SupplyDef s) => OnSale().Contains(s);

    // ---- Rarity and how many are left on the market's shelf ----

    public const int SoldOutDays = 2;   // once the last pack is bought, it's gone for the next two days

    /// Common (always stocked), Uncommon (in the rotation) or Rare (now and then).
    public static string Rarity(SupplyDef s) => s.market == MarketSlot.Rare ? "Rare" : s.market == MarketSlot.Rotating ? "Uncommon" : "Common";

    /// Packs the market has on its shelf at a time: plenty of basics, a few of the rotation, one rare find.
    public static int ShelfPacks(SupplyDef s) => s.market == MarketSlot.Rare ? 1 : s.market == MarketSlot.Rotating ? 3 : 6;

    MarketPurchase Purchase(SupplyDef s)
    {
        var p = Save.marketBought.Find(x => x.supplyId == s.supplyId);
        if (p != null && p.soldOutUntil > 0 && Save.day > p.soldOutUntil) { p.bought = 0; p.soldOutUntil = 0; }   // restocked
        return p;
    }

    /// Packs still on the market's shelf (0 = sold out).
    public int PacksLeft(SupplyDef s)
    {
        var p = Purchase(s);
        if (p == null) return ShelfPacks(s);
        return p.soldOutUntil > 0 ? 0 : Mathf.Max(0, ShelfPacks(s) - p.bought);
    }

    /// Days until a sold-out supply is back (0 if it isn't sold out).
    public int DaysUntilBack(SupplyDef s)
    {
        var p = Purchase(s);
        return p != null && p.soldOutUntil > 0 ? p.soldOutUntil - Save.day + 1 : 0;
    }

    void TakeFromShelf(SupplyDef s)
    {
        var p = Purchase(s);
        if (p == null) Save.marketBought.Add(p = new MarketPurchase { supplyId = s.supplyId });
        p.bought++;
        if (p.bought >= ShelfPacks(s)) p.soldOutUntil = Save.day + SoldOutDays;
    }

    // ---- Tools ----

    /// The best owned tool of this type (null if none).
    public SupplyDef BestTool(string toolType)
    {
        SupplyDef best = null;
        foreach (var s in content.supplies)
            if (s != null && s.kind == SupplyKind.Tool && s.toolType == toolType && Owns(s.supplyId) && (best == null || s.size > best.size))
                best = s;
        return best;
    }

    public bool HasTool(ToolNeed need)
    {
        foreach (var s in content.supplies)
            if (s != null && s.kind == SupplyKind.Tool && s.toolType == need.toolType && Owns(s.supplyId)
                && (need.exactSize ? s.size == need.minSize : s.size >= need.minSize))
                return true;
        return false;
    }

    public List<ToolNeed> MissingTools(List<ToolNeed> needs) => needs.FindAll(n => !HasTool(n));

    /// Buys one pack. Returns false if the player can't afford it or it's sold out.
    public bool Buy(SupplyDef s)
    {
        if (PacksLeft(s) <= 0 || !GameManager.Instance.TrySpend(s.price)) return false;
        Add(s.supplyId, s.perPack);
        TakeFromShelf(s);
        GameManager.Instance.Persist();
        return true;
    }

    /// Fills a new game's cupboard with the starter kit, and converts saves from before stock was counted.
    public void SetUpStock(bool isNewGame)
    {
        if (isNewGame)
            foreach (var s in content.supplies)
                if (s != null && s.starterPacks > 0) Add(s.supplyId, s.starterPacks * s.perPack);
        // Tools that come with the shop (e.g. the great-aunt's sewing machine) and are never sold:
        // saves from before they existed get them too, or they could never be had.
        foreach (var s in content.supplies)
            if (s != null && s.kind == SupplyKind.Tool && s.market == MarketSlot.NotSold && s.starterPacks > 0 && !Owns(s.supplyId))
                Add(s.supplyId, s.starterPacks * s.perPack);
        foreach (var id in Save.ownedSupplies)   // version 3 and older: owned = one pack
        {
            var s = content.Supply(id);
            if (s != null && Stock(id) == 0) Add(id, s.perPack);
        }
        Save.ownedSupplies.Clear();
    }

    /// What's missing from the cupboard for these needs (empty when everything is there).
    public List<MaterialNeed> Missing(List<MaterialNeed> needs) =>
        needs.FindAll(n => Stock(n.supplyId) < n.amount);

    /// Uses up the materials. Call only when nothing is missing.
    public void Use(List<MaterialNeed> needs)
    {
        foreach (var n in needs) Add(n.supplyId, -n.amount);
        Save.stock.RemoveAll(x => x.amount <= 0);
    }

    /// Best material tier the player owns for a craft (1 if none), used for pricing.
    public int BestTier(CraftType craft)
    {
        int best = 1;
        foreach (var s in content.supplies)
            if (s != null && s.craft == craft && Owns(s.supplyId) && s.tier > best) best = s.tier;
        return best;
    }

    /// Packs to buy to cover a shortfall, e.g. "2 x Pink Floss".
    public int PacksFor(MaterialNeed n)
    {
        var s = content.Supply(n.supplyId);
        int short_ = n.amount - Stock(n.supplyId);
        return s == null || short_ <= 0 ? 0 : Mathf.CeilToInt(short_ / (float)Mathf.Max(1, s.perPack));
    }
}
