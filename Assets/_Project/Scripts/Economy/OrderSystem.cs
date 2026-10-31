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
    public List<string> extras = new List<string>();   // special touches added as a surprise (supplyIds), used when set aside
    public bool gathered;             // its supplies were picked from the cupboard (GatherUI) before starting
    public int pickupDay;             // the day the customer comes back for it (kept secret from the player; 0 = not set)
    public float accuracy;            // recorded when finished
    public string commissionId;       // a commission (see CommissionDef), or empty
    public bool needsDesign;          // a commission waits for the player to design its pattern first
    public string requiredSupplyId;   // a commission's must-use fabric or yarn, or empty
    public string fromPatternId;      // the shelf pattern the customer picked, for its sales record
    public int fit;                   // for an indirect request: 1 = the pattern fit, 2 = the best fit (more friendship)
    public int step;                  // the recipe step being worked on (index into Steps)
    public List<StepProgress> steps = new List<StepProgress>();   // saved progress per step

    [NonSerialized] public CraftItemDef item;   // looked up from itemId after loading

    /// How it's made. A pattern-only order is just the design step.
    public bool legacyKnit;           // started before garments (save version 13): knitted as one square, on the old recipe
    public RecipeStep[] Steps => item == null || patternOnly ? DesignOnly : legacyKnit ? LegacyKnitSteps(item) : item.Steps;

    static RecipeStep[] legacySweater;
    /// The knitting recipe from before garments: one square, cast on, knitted, cast off. Only the sweater
    /// changed recipe (the scarf only changed shape).
    static RecipeStep[] LegacyKnitSteps(CraftItemDef item)
    {
        if (item.itemId != "sweater") return item.Steps;
        if (legacySweater != null) return legacySweater;
        var needles = new ToolNeed { toolType = "knitting_needles", label = "Knitting needles", matchYarn = true };
        var tapestry = new ToolNeed { toolType = "tapestry_needle", label = "Tapestry needle" };
        RecipeStep S(StepKind k, string label, ToolNeed tool = null, int pieces = 0) =>
            new RecipeStep { kind = k, label = label, fabricPieces = pieces, perPatternSquare = pieces > 0, tools = tool != null ? new[] { tool } : new ToolNeed[0], materials = new MaterialNeed[0] };
        return legacySweater = new[]
        {
            S(StepKind.ChooseYarn, "Choose the yarn"), S(StepKind.CastOn, "Cast on", needles),
            S(StepKind.KnitRows, "Knit the front and back", needles, 5), S(StepKind.CastOff, "Cast off", needles),
            S(StepKind.WeaveEnds, "Weave in the ends", tapestry), S(StepKind.Press, "Block it"),
        };
    }
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

    /// Final quality: the average score of the judged steps.
    public float RecipeScore()
    {
        float sum = 0f;
        int n = 0;
        for (int i = 0; i < Steps.Length; i++)
            if (Steps[i].Judged) { sum += StepState(i).score; n++; }
        return n > 0 ? sum / n : 1f;
    }

    /// How much of the chosen fabric or yarn the whole recipe uses (fabric pieces, or stitches of yarn).
    public int ChosenAmount()
    {
        int n = 0;
        foreach (var s in Steps) n += s.ChosenAmount(target);
        return n;
    }

    /// The Choose Fabric or Choose Yarn step's kind, or null if the recipe has none.
    public StepKind? ChooseKind()
    {
        foreach (var s in Steps) if (s.kind == StepKind.ChooseFabric || s.kind == StepKind.ChooseYarn) return s.kind;
        return null;
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
