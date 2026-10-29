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

    void Add(string supplyId, int amount)
    {
        var s = Save.stock.Find(x => x.supplyId == supplyId);
        if (s == null) Save.stock.Add(s = new SupplyStock { supplyId = supplyId });
        s.amount += amount;
    }

    /// Buys one pack. Returns false if the player can't afford it.
    public bool Buy(SupplyDef s)
    {
        if (!GameManager.Instance.TrySpend(s.price)) return false;
        Add(s.supplyId, s.perPack);
        GameManager.Instance.Persist();
        return true;
    }

    /// Fills a new game's cupboard with the starter kit, and converts saves from before stock was counted.
    public void SetUpStock(bool isNewGame)
    {
        if (isNewGame)
            foreach (var s in content.supplies)
                if (s != null && s.starterPacks > 0) Add(s.supplyId, s.starterPacks * s.perPack);
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
