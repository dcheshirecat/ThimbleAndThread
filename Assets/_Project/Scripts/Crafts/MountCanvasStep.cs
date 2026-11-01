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

/// Needlepoint: before mounting, tape round the canvas edges so the cut mesh can't fray or snag the yarn. Drag the tape
/// along all four edges.
public class BindCanvasStep : TracingStep
{
    protected override float StitchLength => 0f;   // a strip of tape
    protected override bool UseLoupe => false;
    protected override Color ThreadColor => new Color(0.93f, 0.86f, 0.62f);
    protected override string StrokeName => "strip of tape";
    protected override bool ClosedGuide => true;

    protected override void DrawPiece()
    {
        Square(new Vector2(0.5f, 0.5f), 0.86f, area.fabricColor, 0);   // the cut canvas
        var hole = Color.Lerp(area.fabricColor, Color.black, 0.18f);
        for (int x = 0; x <= 20; x++)
            for (int y = 0; y <= 20; y++) Square(new Vector2(0.12f + x * 0.038f, 0.12f + y * 0.038f), 0.008f, hole, 1);
        var frayed = Color.Lerp(area.fabricColor, Color.white, 0.4f);   // loose threads at the cut edge
        for (int i = 0; i < 24; i++)
        {
            float t = 0.1f + i * 0.033f;
            Line(new List<Vector2> { new Vector2(t, 0.07f), new Vector2(t + 0.01f, 0.04f) }, frayed, 0.004f, 1);
            Line(new List<Vector2> { new Vector2(t, 0.93f), new Vector2(t - 0.01f, 0.96f) }, frayed, 0.004f, 1);
        }
    }

    protected override List<Vector2> Guide() => new List<Vector2>
    {
        new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.1f), new Vector2(0.9f, 0.9f), new Vector2(0.1f, 0.9f),
    };
}
