using System;
using System.Collections.Generic;
using UnityEngine;

// The kinds of step a recipe can use. Each one is a minigame (see Crafts/ICraftStep.cs).
// Append new values at the end: item assets store these as numbers.
public enum StepKind { CrossStitch, Hem, ChooseFabric, CutPieces, Pin, SewSeam, TurnRightSideOut, Stuff, AttachEyes, CloseOpening, SewSides, HemTop, AttachHandles,
                       ChooseYarn, CastOn, KnitRows, CastOff, MagicRing, CrochetRounds,
                       HoopFabric, Backstitch, SatinFill, FrenchKnots, Press, WeaveEnds, SewUp, Fringe, MitreCorners, PinPattern, ClipCurves, BoxCorners }

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
    public int fabricPieces;                // how much of the fabric or yarn chosen in a Choose step this step uses
                                            // (fabric: pieces; yarn: stitches)
    public string requiredSkill;            // a technique the player must have learned for this step (SkillDef id), or empty
    public bool perPatternSquare;           // fabricPieces is per square of the pattern (e.g. 2 stitches of yarn per square)
    public int patternThread;               // embroidery: which of the pattern's thread colours this step uses (0 = first)
    public int threadAmount;                // ...and how much of that floss (in squares, like cross stitch)
    public string piece;                    // knitting: which piece of the garment ("back", "front", "sleeves"; see KnitLayout)

    /// Whether the step's score counts towards the piece's quality (choosing a material isn't judged).
    public bool Judged => kind != StepKind.ChooseFabric && kind != StepKind.ChooseYarn;

    /// How much of the chosen material this step uses for a pattern.
    public int ChosenAmount(Pattern pattern) => perPatternSquare && pattern != null ? fabricPieces * pattern.width * pattern.height : fabricPieces;
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
    public int mistakes;                    // e.g. swiping a knit where a purl was due
}
