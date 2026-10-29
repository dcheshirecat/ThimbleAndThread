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
            if (work.Exists(w => w.x == c.x && w.y == c.y && w.mask == 3 && w.color == c.colorIndex)) done++;
        return (float)done / target.cells.Count;
    }
}

public static class OrderGenerator
{
    /// Items this customer may ask for at their friendship level.
    public static List<CraftItemDef> Wants(CustomerDef customer, int relationshipLevel)
    {
        var wants = new List<CraftItemDef>();
        foreach (var o in customer.requestable)
            if (o.item != null && o.minRelationshipLevel <= relationshipLevel) wants.Add(o.item);
        return wants;
    }

    /// A pattern order: the customer picked a pattern off the shelf and wants the item made from it.
    public static Order FromPattern(CustomerDef customer, CraftItemDef item, Pattern design, int today)
    {
        return new Order
        {
            id = Guid.NewGuid().ToString("N"),
            customerId = customer.customerId,
            itemId = item.itemId,
            item = item,
            dayPlaced = today,
            target = design,
        };
    }
}
