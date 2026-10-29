using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Order
{
    public string customerId;
    public CraftItemDef item;
    public bool patternOnly;     // customer only wants the pattern
    public int deadlineDay;
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
        bool patternOnly = UnityEngine.Random.value < 0.3f;
        return new Order
        {
            customerId = customer.customerId,
            item = pick.item,
            patternOnly = patternOnly,
            deadlineDay = today + (patternOnly ? 1 : 2)
        };
    }
}
