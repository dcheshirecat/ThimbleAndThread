using System.Collections.Generic;
using UnityEngine;

/// Commissions with a brief (owner, 2026-11-02 evening): when a friend takes up your offer to make them something
/// special, they often add a condition: no more than N colours, small (no bigger than N x N), or made with a
/// particular floss, yarn or fabric. It shows on the Workbench while you design and make it, and it's judged at
/// pickup: kept to = more coins and friendship, broken = a little less (Order.brief, briefValue, briefSupply).
public partial class ShopDay
{
    const float BriefChance = 0.6f;

    /// Gives a new own-commission a brief, sometimes. Returns the line they say about it ("" for none).
    string AddBrief(Order o)
    {
        if (o?.item == null || Random.value > BriefChance) return "";
        var kinds = new List<string> { "colours", "small" };
        var supply = BriefSupplyFor(o.item);
        if (supply != null) kinds.Add("mustuse");
        o.brief = kinds[Random.Range(0, kinds.Count)];
        switch (o.brief)
        {
            case "colours":
                o.briefValue = Random.value < 0.5f ? 2 : 3;
                return Pick($" Nothing too busy, mind: no more than {o.briefValue} colours.", $" Keep it simple? {o.briefValue} colours at most.");
            case "small":
                o.briefValue = Random.value < 0.5f ? 8 : 10;
                return Pick($" Something small, please. No bigger than {o.briefValue} by {o.briefValue}.", $" Not too big: {o.briefValue} stitches across at most.");
            default:
                o.briefSupply = supply.supplyId;
                return Pick($" And could you use {supply.displayName}? I do love it.", $" Oh, and it has to be {supply.displayName}. Humour me!");
        }
    }

    /// Something they could ask it to be made with: a floss for cross stitch-like crafts, else a yarn or fabric for it.
    SupplyDef BriefSupplyFor(CraftItemDef item)
    {
        var options = new List<SupplyDef>();
        foreach (var s in content.supplies)
        {
            if (s == null || s.market == MarketSlot.NotSold || s.season != 0 || !Progress.SupplyShown(s)) continue;
            bool floss = s.kind == SupplyKind.Thread && (item.craft == CraftType.CrossStitch || item.craft == CraftType.Embroidery || item.craft == CraftType.Needlepoint);
            bool yarn = s.kind == SupplyKind.Yarn && s.craft == item.craft && (item.craft == CraftType.Knitting || item.craft == CraftType.Crochet);
            if (floss || yarn) options.Add(s);
        }
        return options.Count > 0 ? options[Random.Range(0, options.Count)] : null;
    }

    /// The brief in words, for the Workbench ("" if none).
    public string BriefText(Order o)
    {
        if (o == null || string.IsNullOrEmpty(o.brief)) return "";
        return o.brief switch
        {
            "colours" => $"The brief: no more than {o.briefValue} colours.",
            "small" => $"The brief: no bigger than {o.briefValue} x {o.briefValue} stitches.",
            "mustuse" => $"The brief: use {content.Supply(o.briefSupply)?.displayName ?? o.briefSupply}.",
            _ => "",
        };
    }

    /// Whether the finished piece keeps to its brief.
    public bool BriefKept(Order o)
    {
        var p = o.target;
        if (p == null) return false;
        switch (o.brief)
        {
            case "colours":
            {
                var used = new HashSet<int>();
                foreach (var c in p.cells) if (c.colorIndex >= 0) used.Add(c.colorIndex);
                return used.Count > 0 && used.Count <= o.briefValue;
            }
            case "small":
            {
                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                foreach (var c in p.cells)
                {
                    if (c.colorIndex < 0) continue;
                    minX = Mathf.Min(minX, c.x); maxX = Mathf.Max(maxX, c.x); minY = Mathf.Min(minY, c.y); maxY = Mathf.Max(maxY, c.y);
                }
                return maxX >= minX && maxX - minX + 1 <= o.briefValue && maxY - minY + 1 <= o.briefValue;
            }
            case "mustuse":
                return o.fabricId == o.briefSupply || (p.threads != null && p.threads.Contains(o.briefSupply));
            default:
                return true;
        }
    }

    /// At pickup: how the brief went (adjusts the price and friendship); a line for them to say, or null.
    string BriefReaction(Order o, ref int price, ref int points)
    {
        if (string.IsNullOrEmpty(o.brief)) return null;
        if (BriefKept(o))
        {
            price += Mathf.RoundToInt(price * 0.2f);
            points += 8;
            Stamps.Earn(Save, "brief:kept", "To the letter", "A commission made exactly to its brief.", StampTheme.Craft);
            return Pick("And just as I asked. You listened!", "You stuck to the brief and it's perfect. Thank you.");
        }
        price -= Mathf.RoundToInt(price * 0.15f);
        points -= 4;
        return Pick("It's lovely... but it's not quite what I asked for, is it?", "Oh. I did say... never mind. It's still nice.");
    }
}
