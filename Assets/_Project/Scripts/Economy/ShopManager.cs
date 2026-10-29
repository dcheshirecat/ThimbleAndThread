using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public SupplyDef[] catalog;

    public bool Owns(string supplyId) => GameManager.Instance.Save.ownedSupplies.Contains(supplyId);

    public bool Buy(SupplyDef s)
    {
        if (Owns(s.supplyId)) return false;
        if (!GameManager.Instance.TrySpend(s.price)) return false;
        GameManager.Instance.Save.ownedSupplies.Add(s.supplyId);
        GameManager.Instance.Persist();
        return true;
    }

    /// Best material tier the player owns for a craft (1 if none), used for pricing.
    public int BestTier(CraftType craft)
    {
        int best = 1;
        foreach (var s in catalog)
            if (s.craft == craft && Owns(s.supplyId) && s.tier > best) best = s.tier;
        return best;
    }
}
