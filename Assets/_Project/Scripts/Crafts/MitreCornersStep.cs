using System.Collections.Generic;
using UnityEngine;

/// Tablecloth step: mitre the corners before hemming. Each corner is folded in along a diagonal so the two
/// hems meet in a neat 45-degree seam instead of a lumpy overlap. Drag along each dashed diagonal.
public class MitreCornersStep : TracingStep
{
    const float Edge = 0.08f, Fold = 0.12f;   // the cloth's edge, and how far in the corner folds
    protected override float StitchLength => 0f;   // a fold, not stitches
    protected override bool HandNeedle => false;
    protected override Color ThreadColor => Color.Lerp(area.fabricColor, Color.black, 0.3f);

    protected override void DrawPiece()
    {
        Square(Vector2.one * 0.5f, 1f - 2 * Edge, area.fabricColor, 0);
        // The stitched motif, in the middle of the cloth.
        if (order.target != null)
            foreach (var c in order.target.cells)
            {
                var at = PieceShape.SquareCenter(order.target, 0.3f, c.x, c.y, out float size);
                Square(at, size * 0.92f, c.colorIndex < order.target.palette.Count ? order.target.palette[c.colorIndex] : Color.gray, 1);
            }
        // The pressed-in hem lines, a fold's width in from each edge.
        float a = Edge + Fold, b = 1 - Edge - Fold;
        Line(new List<Vector2> { new Vector2(a, a), new Vector2(b, a), new Vector2(b, b), new Vector2(a, b), new Vector2(a, a) },
             new Color(0, 0, 0, 0.12f), 0.004f, 1);
    }

    protected override List<Vector2> Guide() => Corner(0);

    protected override List<List<Vector2>> Guides() => new List<List<Vector2>> { Corner(0), Corner(1), Corner(2), Corner(3) };

    /// The diagonal fold across one corner, from edge to edge.
    List<Vector2> Corner(int i)
    {
        float lo = Edge, hi = 1 - Edge, d = Fold * 1.6f;
        var corner = new Vector2(i % 2 == 0 ? lo : hi, i < 2 ? lo : hi);
        var alongX = new Vector2(i % 2 == 0 ? d : -d, 0);
        var alongY = new Vector2(0, i < 2 ? d : -d);
        return new List<Vector2> { corner + alongX, corner + alongY };
    }
}
