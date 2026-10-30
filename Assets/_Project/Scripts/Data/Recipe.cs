using System;
using System.Collections.Generic;
using UnityEngine;

// The kinds of step a recipe can use. Each one is a minigame (see Crafts/ICraftStep.cs).
// Append new values at the end: item assets store these as numbers.
public enum StepKind { CrossStitch, Hem, ChooseFabric, CutPieces, Pin, SewSeam, TurnRightSideOut, Stuff, AttachEyes, CloseOpening, SewSides, HemTop, AttachHandles }

// One step of making an item, e.g. "Stitch the motif" then "Hem the edges".
// Items list their steps in order in CraftItemDef.steps.
// A step's tools and materials are checked when the player starts it, and the materials are used then.
[Serializable]
public class RecipeStep
{
    public StepKind kind;
    public string label;                    // what the player sees, e.g. "Hem the edges"
    public ToolNeed[] tools = new ToolNeed[0];          // tools this step needs (cross stitch adds its hoop and needle itself)
    public MaterialNeed[] materials = new MaterialNeed[0];   // supplies used up, e.g. 1 bag of stuffing
    public int fabricPieces;                // pieces of the fabric chosen in a Choose Fabric step that this step uses

    /// Whether the step's score counts towards the piece's quality (choosing a fabric isn't judged).
    public bool Judged => kind != StepKind.ChooseFabric;
}

// Saved progress on one step of an order.
// Steps that draw or tap on the piece keep those points, in 0..1 of the piece,
// so they redraw the same on any screen size.
[Serializable]
public class StepProgress
{
    public bool done;
    public bool materialsUsed;              // this step's materials have been taken from the cupboard
    public float score;                     // quality 0..1, set when the step is finished
    public float progress;                  // how far along 0..1, for the order book
    public List<Vector2> points = new List<Vector2>();
    public List<int> strokeStarts = new List<int>();   // index in points where each stroke begins
}
