using System.Collections.Generic;
using UnityEngine;

/// Recipe step: hem a square piece (e.g. a tablecloth) by sewing along a line just inside all four edges.
public class HemStep : TracingStep
{
    const float Margin = 0.08f;

    protected override bool ClosedGuide => true;

    protected override List<Vector2> Guide() => new List<Vector2>
    {
        new Vector2(Margin, Margin), new Vector2(1 - Margin, Margin),
        new Vector2(1 - Margin, 1 - Margin), new Vector2(Margin, 1 - Margin),
    };
}
