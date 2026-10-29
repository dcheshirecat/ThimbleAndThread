using System.Collections.Generic;
using UnityEngine;

/// Works out what a piece of work needs from the supply cupboard.
/// For now every item is made on the cross stitch board, so it needs Aida (one square of fabric per
/// grid square) and floss (one square of thread per stitched square, per colour). Item recipes will
/// add their own materials (stuffing, eyes, felt...).
public static class Materials
{
    /// All thread supplies the market sells, in catalogue order: the designer's colour choices.
    public static List<SupplyDef> Threads(ContentLibrary content) =>
        new List<SupplyDef>(System.Array.FindAll(content.supplies, s => s != null && s.kind == SupplyKind.Thread));

    /// Gives a pattern a thread for each palette colour if it doesn't have them (older saves and
    /// book patterns without threads): the market thread with the closest colour.
    public static void EnsureThreads(Pattern p, ContentLibrary content)
    {
        if (p.threads.Count == p.palette.Count) return;
        var threads = Threads(content);
        p.threads.Clear();
        foreach (var c in p.palette)
        {
            SupplyDef best = null;
            float bestD = float.MaxValue;
            foreach (var t in threads)
            {
                float d = Sq(c.r - t.color.r) + Sq(c.g - t.color.g) + Sq(c.b - t.color.b);
                if (d < bestD) { bestD = d; best = t; }
            }
            p.threads.Add(best != null ? best.supplyId : "");
        }
    }

    static float Sq(float v) => v * v;

    /// The fabric for a project: the best Aida the player has enough of, or else the basic one to buy.
    public static SupplyDef FabricFor(Pattern p, ContentLibrary content, ShopManager shop)
    {
        int squares = p.width * p.height;
        var aidas = new List<SupplyDef>(System.Array.FindAll(content.supplies,
            s => s != null && s.kind == SupplyKind.Fabric && s.craft == CraftType.CrossStitch));
        aidas.Sort((a, b) => b.tier.CompareTo(a.tier));
        foreach (var a in aidas) if (shop.Stock(a.supplyId) >= squares) return a;
        aidas.Sort((a, b) => a.price.CompareTo(b.price));
        return aidas.Count > 0 ? aidas[0] : null;
    }

    /// Everything a stitched pattern needs: the fabric piece and the floss for each colour.
    public static List<MaterialNeed> For(Pattern p, SupplyDef fabric, ContentLibrary content)
    {
        EnsureThreads(p, content);
        var needs = new List<MaterialNeed>();
        if (fabric != null) needs.Add(new MaterialNeed { supplyId = fabric.supplyId, amount = p.width * p.height });
        var perThread = new Dictionary<string, int>();
        foreach (var c in p.cells)
        {
            if (c.colorIndex >= p.threads.Count || string.IsNullOrEmpty(p.threads[c.colorIndex])) continue;
            var id = p.threads[c.colorIndex];
            perThread.TryGetValue(id, out int n);
            perThread[id] = n + 1;
        }
        foreach (var kv in perThread) needs.Add(new MaterialNeed { supplyId = kv.Key, amount = kv.Value });
        return needs;
    }

    /// e.g. "Red Floss: need 20, have 12"
    public static string Describe(MaterialNeed n, ContentLibrary content, ShopManager shop)
    {
        var s = content.Supply(n.supplyId);
        string name = s != null ? s.displayName : n.supplyId;
        string unit = s != null ? s.unit : "";
        return $"{name}: {n.amount} {unit} (you have {shop.Stock(n.supplyId)})";
    }
}
