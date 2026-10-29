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
    public bool materialsUsed;        // fabric cut and thread set aside when work first started
    public string fabricId;           // the fabric it's stitched on
    public bool finished;             // set aside at night, ready to hand over during the day
    public float accuracy;            // recorded when finished
    public int step;                  // the recipe step being worked on (index into Steps)
    public List<StepProgress> steps = new List<StepProgress>();   // saved progress per step

    [NonSerialized] public CraftItemDef item;   // looked up from itemId after loading

    /// How it's made. A pattern-only order is just the design step.
    public RecipeStep[] Steps => item == null || patternOnly ? DesignOnly : item.Steps;
    static readonly RecipeStep[] DesignOnly = { new RecipeStep { kind = StepKind.CrossStitch, label = "Design the pattern" } };

    public RecipeStep CurrentStep => Steps[Mathf.Clamp(step, 0, Steps.Length - 1)];
    public bool IsLastStep => step >= Steps.Length - 1;

    /// Saved progress for a step, created when first needed.
    public StepProgress StepState(int index)
    {
        while (steps.Count <= index) steps.Add(new StepProgress());
        return steps[index];
    }

    /// How far along the whole order is (0..1), for the order book list: each step counts equally.
    public float Progress()
    {
        var recipe = Steps;
        float sum = 0f;
        for (int i = 0; i < recipe.Length; i++)
        {
            if (i < step) sum += 1f;
            else if (i == step) sum += recipe[i].kind == StepKind.CrossStitch ? StitchProgress() : StepState(i).progress;
        }
        return sum / recipe.Length;
    }

    /// Share of target cells stitched (0..1). Cross stitch work lives in "work", as before recipes existed.
    public float StitchProgress()
    {
        if (target == null || target.cells.Count == 0) return 0f;
        int done = 0;
        foreach (var c in target.cells)
            if (work.Exists(w => w.x == c.x && w.y == c.y && w.mask == 3 && w.color == c.colorIndex)) done++;
        return (float)done / target.cells.Count;
    }

    /// Final quality: the average of the finished steps' scores.
    public float RecipeScore()
    {
        float sum = 0f;
        int n = Steps.Length;
        for (int i = 0; i < n; i++) sum += StepState(i).score;
        return n > 0 ? sum / n : 0f;
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
