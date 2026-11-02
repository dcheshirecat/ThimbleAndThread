using System.Collections.Generic;
using UnityEngine;

/// Furniture and decorations (owner, 2026-10-31): bought at Marsh's Workshop (UI/FurnitureUI.cs), placed from your
/// portrait (Customize, "Your Shop") or by dragging them about in the shop (the pencil, UI/DecorEditor.cs). Placed pieces add charm: customers pay 1% more per point.
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
        return entry != null ? Decor(Parse(entry).id) : null;
    }

    // ---- Free placement (owner, 2026-11-02): pieces go anywhere in the shop, as many of a kind as you like. ----
    // decorPlaced entries: "slot:id" = at the slot's usual spot (older saves, the Customize tab), or
    // "slot:id@page,x,y" = wherever it was dropped (page 0 pickups, 1 the shop, 2 the display; x, y = 0..1 of the page).

    public struct PlacedPiece { public DecorDef def; public bool free; public int page; public Vector2 pos; }

    static (int slot, string id, bool free, int page, Vector2 pos) Parse(string entry)
    {
        int colon = entry.IndexOf(':');
        int.TryParse(entry.Substring(0, Mathf.Max(0, colon)), out int slot);
        string rest = entry.Substring(colon + 1);
        int at = rest.IndexOf('@');
        if (at < 0) return (slot, rest, false, 0, Vector2.zero);
        var parts = rest.Substring(at + 1).Split(',');
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        int.TryParse(parts[0], out int page);
        float x = parts.Length > 1 && float.TryParse(parts[1], System.Globalization.NumberStyles.Float, inv, out var px) ? px : 0.5f;
        float y = parts.Length > 2 && float.TryParse(parts[2], System.Globalization.NumberStyles.Float, inv, out var py) ? py : 0.5f;
        return (slot, rest.Substring(0, at), true, page, new Vector2(x, y));
    }

    /// Every piece out in the shop.
    public List<PlacedPiece> PlacedPieces()
    {
        var list = new List<PlacedPiece>();
        foreach (var e in Save.decorPlaced)
        {
            var p = Parse(e);
            var d = Decor(p.id);
            if (d != null && Save.decor.Contains(p.id)) list.Add(new PlacedPiece { def = d, free = p.free, page = p.page, pos = p.pos });
        }
        return list;
    }

    public bool IsOut(string decorId) => Save.decorPlaced.Exists(e => Parse(e).id == decorId);

    /// Put a piece down at a point on a page (moves it if it was out already).
    public void PutOut(string decorId, int page, Vector2 pos)
    {
        var d = Decor(decorId);
        if (d == null || !Save.decor.Contains(decorId)) return;
        Save.decorPlaced.RemoveAll(e => Parse(e).id == decorId);
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        Save.decorPlaced.Add($"{(int)d.slot}:{decorId}@{page},{Mathf.Clamp01(pos.x).ToString("0.###", inv)},{Mathf.Clamp01(pos.y).ToString("0.###", inv)}");
        GM.Persist();
    }

    public void PutAway(string decorId)
    {
        Save.decorPlaced.RemoveAll(e => Parse(e).id == decorId);
        GM.Persist();
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
            foreach (var p in PlacedPieces()) n += p.def.charm;
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
            LastMessage = $"Bought the {d.displayName.ToLower()}!" + (placed ? " It's in your shop already." : " Tap the pencil in your shop to put it out.");
            if (Save.decor.Count == 1) Stamps.Earn(Save, "decor:first", "Making it yours", d.displayName, StampTheme.Shop);
            GM.Persist();
        }
        GM.ChangeScreen(where);
    }

    public void LeaveFurniture() => LeaveShop(FurnitureOwnerId);
    public void ChatWithFurnitureOwner() => ChatWith(FurnitureOwnerId, ScreenId.Furniture);
}
