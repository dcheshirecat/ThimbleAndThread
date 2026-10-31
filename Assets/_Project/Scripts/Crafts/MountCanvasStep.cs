using System.Collections.Generic;
using UnityEngine;

/// Needlepoint recipe step: tack the canvas to the stretcher bars. Tap each mark round the edge to put a tack in,
/// pulling the canvas straight as you go; tacks off the marks leave it crooked (Undo pulls one out).
public class MountCanvasStep : PlacementStep
{
    const float Edge = 0.1f;   // where the bars are, from the area's edge

    protected override List<Vector2> Marks
    {
        get
        {
            var marks = new List<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                float t = 0.2f + i * 0.2f;
                marks.Add(new Vector2(t, Edge)); marks.Add(new Vector2(t, 1 - Edge));
                marks.Add(new Vector2(Edge, t)); marks.Add(new Vector2(1 - Edge, t));
            }
            return marks;
        }
    }

    protected override void DrawPiece()
    {
        var wood = new Color(0.72f, 0.55f, 0.36f);
        Square(new Vector2(0.5f, 0.5f), 0.86f, area.fabricColor, 0);   // the canvas
        var hole = Color.Lerp(area.fabricColor, Color.black, 0.18f);
        for (int x = 0; x <= 20; x++)   // its open mesh
            for (int y = 0; y <= 20; y++) Square(new Vector2(0.12f + x * 0.038f, 0.12f + y * 0.038f), 0.008f, hole, 1);
        foreach (var (a, b) in new[] { (new Vector2(Edge, Edge), new Vector2(1 - Edge, Edge)), (new Vector2(Edge, 1 - Edge), new Vector2(1 - Edge, 1 - Edge)),
                                       (new Vector2(Edge, Edge), new Vector2(Edge, 1 - Edge)), (new Vector2(1 - Edge, Edge), new Vector2(1 - Edge, 1 - Edge)) })
            Line(new List<Vector2> { a, b }, wood, 0.06f, 2);   // the stretcher bars under the canvas edge
    }

    protected override GameObject DrawPlaced(Vector2 p) => Dot(p, 0.03f, new Color(0.85f, 0.70f, 0.30f), 4, shine: true);   // a brass tack
}
