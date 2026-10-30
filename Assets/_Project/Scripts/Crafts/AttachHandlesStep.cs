using System.Collections.Generic;
using UnityEngine;

/// Recipe step: sew the handle on. Tap where each end of the strap goes, just below the top edge;
/// once both ends are on, the handle loops above the bag.
public class AttachHandlesStep : PlacementStep
{
    static readonly Color Strap = new Color(0.55f, 0.42f, 0.28f);   // cotton webbing

    LineRenderer handle;

    protected override List<Vector2> Marks => Shape.HandleEnds;

    protected override void DrawPiece() => DrawBag(sidesSewn: true, hemmed: true);

    protected override GameObject DrawPlaced(Vector2 p) => Square(p, 0.06f, Strap, 3).gameObject;

    protected override bool CanPlace(Vector2 p) => PieceShape.Inside(Shape.Outline, p);

    protected override void Changed()
    {
        if (handle != null) Destroy(handle.gameObject);
        handle = null;
        if (Placed.Count < 2) return;
        var a = Placed[0]; var b = Placed[1];
        var arc = new List<Vector2>();
        for (int i = 0; i <= 16; i++)
        {
            float t = i / 16f;
            arc.Add(Vector2.Lerp(a, b, t) + Vector2.up * Mathf.Sin(t * Mathf.PI) * 0.15f);
        }
        handle = Line(arc, Strap, 0.03f, 2);
    }
}
