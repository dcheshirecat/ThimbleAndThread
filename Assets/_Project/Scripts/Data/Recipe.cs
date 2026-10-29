using System;
using System.Collections.Generic;
using UnityEngine;

// The kinds of step a recipe can use. Each one is a minigame (see Crafts/ICraftStep.cs).
// Append new values at the end: item assets store these as numbers.
public enum StepKind { CrossStitch, Hem }

// One step of making an item, e.g. "Stitch the motif" then "Hem the edges".
// Items list their steps in order in CraftItemDef.steps.
[Serializable]
public class RecipeStep
{
    public StepKind kind;
    public string label;                    // what the player sees, e.g. "Hem the edges"
    public ToolNeed[] tools = new ToolNeed[0];   // tools this step needs on top of the step's own (e.g. scissors)
}

// Saved progress on one step of an order.
// Tracing steps (hems, seams, cutting) keep the strokes the player drew, as points in 0..1 of the piece,
// so they redraw the same on any screen size.
[Serializable]
public class StepProgress
{
    public bool done;
    public float score;                     // quality 0..1, set when the step is finished
    public float progress;                  // how far along 0..1, for the order book
    public List<Vector2> points = new List<Vector2>();
    public List<int> strokeStarts = new List<int>();   // index in points where each stroke begins
}
