using System;
using System.Collections.Generic;
using UnityEngine;

/// One entry in the order book. Saved with the game, including the work done so far.
/// There are no deadlines: an order stays open until it is delivered.
[Serializable]
public class Order
{
    public string id;
    public string customerId;
    public string itemId;
    public bool patternOnly;          // customer only wants the pattern
    public int dayPlaced;
    public Pattern target;            // what to stitch or design
    public List<StitchMark> work = new List<StitchMark>();
    public bool finished;             // set aside at night, ready to hand over during the day
    public float accuracy;            // recorded when finished

    [NonSerialized] public CraftItemDef item;   // looked up from itemId after loading

    /// Share of target cells finished (0..1), for the order book list.
    public float Progress()
    {
        if (target == null || target.cells.Count == 0) return 0f;
        int done = 0;
        foreach (var c in target.cells)
            if (work.Exists(w => w.x == c.x && w.y == c.y && w.mask == 3)) done++;
        return (float)done / target.cells.Count;
    }
}

public static class OrderGenerator
{
    /// Picks something this customer is allowed to ask for at their friendship level.
    public static Order Generate(CustomerDef customer, int relationshipLevel, int today)
    {
        var options = new List<OrderOption>();
        foreach (var o in customer.requestable)
            if (o.item != null && o.minRelationshipLevel <= relationshipLevel) options.Add(o);
        if (options.Count == 0) return null;

        var pick = options[UnityEngine.Random.Range(0, options.Count)];
        return new Order
        {
            id = Guid.NewGuid().ToString("N"),
            customerId = customer.customerId,
            itemId = pick.item.itemId,
            item = pick.item,
            patternOnly = UnityEngine.Random.value < 0.3f,
            dayPlaced = today,
            target = PlaceholderPatterns.For(pick.item),
        };
    }
}
