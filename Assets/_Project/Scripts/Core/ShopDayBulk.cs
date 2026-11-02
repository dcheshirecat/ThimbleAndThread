using System.Collections.Generic;
using UnityEngine;

/// Bulk orders (owner, 2026-11-02 evening): a business in town orders several of the same thing (four cushions for the
/// café, six totes for the florist...). Bigger jobs over several evenings: finishing one copy starts the next
/// (Order.copies, copiesDone), and it's handed over once they're all made, paid per copy (a little off for the
/// quantity) with extra friendship. One open bulk order per person; friendship level 1 and up.
public partial class ShopDay
{
    const float BulkChance = 0.12f, BulkDiscount = 0.9f;

    static readonly (string who, string item, int copies, string ask)[] BulkJobs =
    {
        ("eli", "cushion", 4, "The café's window seats are looking sad. Could you make me {n} cushions? Same pattern for all of them."),
        ("priya", "plushie_sewn", 5, "I'd love {n} soft toys for the reading corner at school. The children would be over the moon."),
        ("felix", "tote", 6, "I want to send flowers out in proper bags. Could you make {n} totes, all matching?"),
        ("owen", "quilt", 2, "The church hall opens as a winter shelter this year. Could you make {n} quilts for it?"),
        ("gwen", "scarf", 4, "The harbour crew freeze on the night tides. {n} scarves, if you could? I'll pay the going rate."),
        ("benji", "hoop_art", 6, "For the wedding tables: {n} embroidered hoops, one a table. Sam picked the pattern, don't tell them I said."),
        ("dot", "tablecloth", 3, "My tea room cloths are older than I am. Could you make me {n} new ones?"),
        ("midge", "sampler", 3, "The bookshop walls need something. {n} framed samplers, all the same, would look grand."),
        ("isla", "wall_hanging", 2, "The lighthouse keepers' room is bare stone. {n} hangings would warm it up no end."),
    };

    /// A business job this customer could offer now: something you can make, with a pattern of it in your book.
    (CraftItemDef item, int copies, string ask, OwnedPattern pattern)? BulkJobFor(CustomerDef c)
    {
        if (c == null || GM.GetRelationship(c.customerId).level < 1) return null;
        if (Save.orders.Exists(o => o.customerId == c.customerId && o.copies > 1)) return null;   // one at a time
        foreach (var job in BulkJobs)
        {
            if (job.who != c.customerId) continue;
            var item = content.Item(job.item);
            if (item == null || !Progress.CanMake(item)) continue;
            var patterns = Save.patterns.FindAll(p => p.itemId == job.item && PatternShown(p) && !IsMystery(p));
            if (patterns.Count == 0) continue;
            var shown = patterns.FindAll(p => p.onShelf);   // they'd pick one they've seen on display
            var pick = (shown.Count > 0 ? shown : patterns)[Random.Range(0, (shown.Count > 0 ? shown : patterns).Count)];
            return (item, job.copies, job.ask.Replace("{n}", job.copies.ToString()), pick);
        }
        return null;
    }

    /// Now and then, a business order instead of their usual.
    bool MaybeBulkOrder(CustomerDef c)
    {
        if (Random.value > BulkChance) return false;
        var job = BulkJobFor(c);
        if (job == null) return false;
        var (item, copies, ask, pattern) = job.Value;
        Offer = OrderGenerator.FromPattern(c, item, DesignFor(pattern), Save.day);
        Offer.fromPatternId = pattern.id;
        Offer.copies = copies;
        Say(c, $"{ask}\n\n(A bulk order: {copies} {item.itemName.ToLower()}s from your {pattern.name} pattern. Take your time; they're collected all together.)",
            Offer.target, ("Take the Order", AcceptOffer), ("Not Today", DeclineOffer));
        return true;
    }

    /// Finishing one copy of a bulk order: the next one starts, unless that was the last.
    /// Returns true when there are more to make (the order stays in the book, fresh for the next copy).
    bool NextCopy(Order o, float accuracy)
    {
        if (o.copies <= 1) return false;
        o.copyScores += accuracy;
        o.copiesDone++;
        if (o.copiesDone >= o.copies) return false;
        o.ResetForNextCopy();
        GM.Persist();
        LastMessage = $"That's {o.copiesDone} of {o.copies} {o.item.itemName.ToLower()}s made. Start the next whenever you like.";
        Stamps.Earn(Save, "bulk:first", "Many hands", "Your first bulk order under way.", StampTheme.Shop);
        return true;
    }

    /// "2 of 6 made" for order lists.
    public static string CopiesText(Order o) => o.copies > 1 ? $" ({Mathf.Min(o.copiesDone, o.copies)} of {o.copies} made)" : "";
}
