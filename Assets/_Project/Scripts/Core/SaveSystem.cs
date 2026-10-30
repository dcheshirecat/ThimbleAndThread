using System;
using System.IO;
using UnityEngine;

/// Three save slots as JSON files in persistentDataPath.
/// Writes go to a temp file first and then replace the real one, so a crash mid-save can't corrupt a slot.
public static class SaveSystem
{
    public const int SlotCount = 3;

    static string PathFor(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public static bool Exists(int slot) => File.Exists(PathFor(slot));

    public static SaveData NewGame() => new SaveData();

    public static void Save(SaveData data, int slot)
    {
        data.version = SaveData.CurrentVersion;
        data.lastPlayed = DateTime.Now.ToString("o");
        string path = PathFor(slot), temp = path + ".tmp";
        File.WriteAllText(temp, JsonUtility.ToJson(data, true));
        if (File.Exists(path)) File.Replace(temp, path, null);
        else File.Move(temp, path);
    }

    /// Returns null if the slot is empty or unreadable.
    public static SaveData Load(int slot)
    {
        if (!Exists(slot)) return null;
        try { return Upgrade(JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot)))); }
        catch (Exception e) { Debug.LogWarning($"Save slot {slot} unreadable: {e.Message}"); return null; }
    }

    public static void Delete(int slot)
    {
        if (Exists(slot)) File.Delete(PathFor(slot));
    }

    /// Brings saves from older versions up to date. Add a step here whenever SaveData.CurrentVersion goes up.
    static SaveData Upgrade(SaveData data)
    {
        if (data == null) return null;
        // Version 0 (before versioning) had no order book or day tracking; the defaults are correct.
        // Version 1 had no night flag; false (daytime) is correct.
        // Version 2 had no patterns; ShopDay gives the starter book patterns to a save that has none.
        data.orders ??= new System.Collections.Generic.List<Order>();
        data.visitorsToday ??= new System.Collections.Generic.List<string>();
        data.ownedSupplies ??= new System.Collections.Generic.List<string>();
        data.unlockedItems ??= new System.Collections.Generic.List<string>();
        data.relationships ??= new System.Collections.Generic.List<CustomerRelationship>();
        data.patterns ??= new System.Collections.Generic.List<OwnedPattern>();
        // Version 3 only listed owned supply ids; ShopManager.SetUpStock turns each into one pack of stock.
        // Patterns from before threads were recorded get matching threads from Materials.EnsureThreads.
        data.stock ??= new System.Collections.Generic.List<SupplyStock>();
        // Version 4 orders had no recipe steps: step 0 with no saved step progress is right, since every
        // item was a single cross stitch step and its stitches are still in Order.work.
        foreach (var o in data.orders) o.steps ??= new System.Collections.Generic.List<StepProgress>();
        // Version 5 had no commissions; none done is right.
        data.commissionsDone ??= new System.Collections.Generic.List<string>();
        // Version 6 had no display limit or market sell-outs: 4 spots (patterns already shown stay shown).
        if (data.version < 7) data.displaySpots = 4;
        data.marketBought ??= new System.Collections.Generic.List<MarketPurchase>();
        // Version 7 had no chapters or skills. Those saves were made while every craft was open, so they
        // keep everything (ShopDay learns every skill for them on load); new games start at chapter 1.
        data.skills ??= new System.Collections.Generic.List<string>();
        data.craftXp ??= new System.Collections.Generic.List<CraftXp>();
        data.upgrades ??= new System.Collections.Generic.List<string>();
        data.journalNews ??= new System.Collections.Generic.List<string>();
        if (data.version < 8) { data.chapter = 5; data.learnEverything = true; }
        // Version 8 had no dialogue: no flags, no scenes waiting.
        data.flags ??= new System.Collections.Generic.List<string>();
        data.pendingScenes ??= new System.Collections.Generic.List<string>();
        data.version = SaveData.CurrentVersion;
        return data;
    }
}
