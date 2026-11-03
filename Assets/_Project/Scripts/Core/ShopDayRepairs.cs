using System.Collections.Generic;
using UnityEngine;

/// Repairs (owner, 2026-11-02 evening): now and then a customer brings something to mend instead of ordering something
/// new. A jumper with a hole is darned (DarnStep: long threads, then rows woven across); a torn bag gets a patch (cut it,
/// pin it over the tear, stitch it on, press it). Repairs are items with CraftItemDef.repair set: they're made from a
/// small plain pattern of their own (no pattern from your book), never designed, and paid on pickup like any order.
public partial class ShopDay
{
    const float RepairChance = 0.12f;

    static readonly (string item, string[] asks)[] RepairAsks =
    {
        ("repair_darn", new[] { "My favourite jumper's gone at the elbow. Could you darn it? I can't bear to throw it out.",
                                "The moths have been at my cardigan. There's a hole you could put a finger through. Can you mend it?",
                                "I caught my jumper on a nail on the boat. Is it beyond saving?" }),
        ("repair_patch", new[] { "The strap's pulled a tear in my bag. Could you put a patch on it?",
                                 "There's a rip in my old canvas bag. A patch would do it, I think. Something cheerful?",
                                 "My tote's been everywhere with me and it shows. Could you patch the hole?" }),
    };

    /// Now and then they've brought something to mend.
    bool MaybeRepair(CustomerDef c)
    {
        if (Random.value > RepairChance || Save.orders.Exists(o => o.customerId == c.customerId && IsRepair(o))) return false;
        var options = new List<(CraftItemDef item, string ask)>();
        foreach (var (id, asks) in RepairAsks)
        {
            var item = content.Item(id);
            if (item != null && Progress.CanMake(item)) options.Add((item, asks[Random.Range(0, asks.Length)]));
        }
        if (options.Count == 0) return false;
        var (it, line) = options[Random.Range(0, options.Count)];
        Offer = new Order
        {
            id = System.Guid.NewGuid().ToString("N"), customerId = c.customerId, itemId = it.itemId, item = it, dayPlaced = Save.day,
            target = RepairPatch(it),
        };
        Say(c, $"{line}\n\n(A repair: mend their {(it.itemId == "repair_darn" ? "jumper" : "bag")} at night and they'll collect it.)",
            ("Take the Repair", AcceptOffer), ("Not Today", DeclineOffer));
        Stamps.Earn(Save, "repair:first", "Mend and make do", "Your first repair job.", StampTheme.Craft);
        return true;
    }

    public static bool IsRepair(Order o) => o != null && o.item != null && o.item.repair;

    /// The shape to work to: a small square patch (the darn uses only its colour).
    static Pattern RepairPatch(CraftItemDef item)
    {
        var p = new Pattern { displayName = item.itemName, craft = item.craft, width = 6, height = 6 };
        p.palette.Add(item.itemId == "repair_darn" ? new Color(0.35f, 0.45f, 0.65f) : new Color(0.85f, 0.45f, 0.40f));
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
                p.cells.Add(new StitchCell { x = x, y = y, colorIndex = 0 });
        return p;
    }
}
