using System.Collections.Generic;
using UnityEngine;

/// Recipe step: hem a square piece (e.g. a tablecloth) by sewing along a line just inside all four edges.
public class HemStep : TracingStep
{
    const float Inset = 0.12f;   // the sheet covers 0.05..0.95; the hem line sits just inside its edge

    protected override bool ClosedGuide => true;

    protected override List<Vector2> Guide() => new List<Vector2>
    {
        new Vector2(Inset, Inset), new Vector2(1 - Inset, Inset),
        new Vector2(1 - Inset, 1 - Inset), new Vector2(Inset, 1 - Inset),
    };
}
