using UnityEngine;

// Every customer, item, supply, pattern and commission in the game, in one asset the scene points at.
// It fills itself: whenever an asset under Assets/_Project/Data is added, changed or removed,
// Editor/ContentLibraryRefresher.cs updates these lists. You never need to edit it by hand.
public class ContentLibrary : ScriptableObject
{
    public CustomerDef[] customers = new CustomerDef[0];
    public CraftItemDef[] items = new CraftItemDef[0];
    public SupplyDef[] supplies = new SupplyDef[0];
    public PatternDef[] patterns = new PatternDef[0];
    public CommissionDef[] commissions = new CommissionDef[0];

    public CustomerDef Customer(string id) => System.Array.Find(customers, c => c != null && c.customerId == id);
    public CraftItemDef Item(string id) => System.Array.Find(items, i => i != null && i.itemId == id);
    public SupplyDef Supply(string id) => System.Array.Find(supplies, s => s != null && s.supplyId == id);
    public CommissionDef Commission(string id) => System.Array.Find(commissions, c => c != null && c.commissionId == id);
    public PatternDef Pattern(string id) => System.Array.Find(patterns, p => p != null && p.patternId == id);
}
