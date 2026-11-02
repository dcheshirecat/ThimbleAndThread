using System.Collections.Generic;
using UnityEngine;

/// Furniture and decorations (owner, 2026-10-31): bought at Marsh's Workshop (UI/FurnitureUI.cs), placed from your
/// portrait (Customize, "Your Shop"), shown in the shop scene. Placed pieces add charm: customers pay 1% more per point.
public partial class ShopDay
{
    public const string FurnitureOwnerId = "benji";
    public const string FurnitureFlag = "shop:furniture";
    public const int FurnitureLevel = 3, MaxCharm = 25;

    public DecorDef Decor(string id) => string.IsNullOrEmpty(id) ? null : System.Array.Find(content.decor, d => d != null && d.decorId == id);

    /// What's placed in a spot, or null.
    public DecorDef Placed(DecorSlot slot)
    {
        string prefix = $"{(int)slot}:";
        var entry = Save.decorPlaced.Find(x => x.StartsWith(prefix));
        return entry != null ? Decor(entry.Substring(prefix.Length)) : null;
    }

    public void Place(DecorSlot slot, string decorId)
    {
        string prefix = $"{(int)slot}:";
        Save.decorPlaced.RemoveAll(x => x.StartsWith(prefix));
        if (!string.IsNullOrEmpty(decorId) && Save.decor.Contains(decorId)) Save.decorPlaced.Add(prefix + decorId);
        GM.Persist();
    }

    /// Owned pieces that fit a spot.
    public List<DecorDef> OwnedFor(DecorSlot slot)
    {
        var list = new List<DecorDef>();
        foreach (var d in content.decor) if (d != null && d.slot == slot && Save.decor.Contains(d.decorId)) list.Add(d);
        return list;
    }

    /// How charming the shop is: the placed pieces' charm, up to MaxCharm.
    public int Charm
    {
        get
        {
            int n = 0;
            foreach (DecorSlot s in System.Enum.GetValues(typeof(DecorSlot))) if (Placed(s) is DecorDef d) n += d.charm;
            return Mathf.Min(MaxCharm, n + ChoreCharmToday());
        }
    }

    public void BuyDecor(string decorId)
    {
        var d = Decor(decorId);
        if (d == null || Save.night || Save.decor.Contains(decorId)) return;
        var where = d.shop == "curios" ? ScreenId.Curios : ScreenId.Furniture;
        if (Progress.PlayerLevel < d.minLevel) LastMessage = $"Not for sale to you yet. (From level {d.minLevel}.)";
        else if (!GM.TrySpend(d.price)) LastMessage = $"The {d.displayName.ToLower()} is {d.price} coins.";
        else
        {
            Save.decor.Add(decorId);
            GM.AddRelationshipPoints(OwnerOf(where), 3);
            boughtThisVisit = true;
            bool placed = Placed(d.slot) == null;
            if (placed) Place(d.slot, decorId);
            LastMessage = $"Bought the {d.displayName.ToLower()}!" + (placed ? " It's in your shop already." : " Tap your portrait (top left) to put it out.");
            if (Save.decor.Count == 1) Stamps.Earn(Save, "decor:first", "Making it yours", d.displayName, StampTheme.Shop);
            GM.Persist();
        }
        GM.ChangeScreen(where);
    }

    public void LeaveFurniture() => LeaveShop(FurnitureOwnerId);
    public void ChatWithFurnitureOwner() => ChatWith(FurnitureOwnerId, ScreenId.Furniture);
}
