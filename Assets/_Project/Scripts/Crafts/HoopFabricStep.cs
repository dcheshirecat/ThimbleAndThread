using System.Collections.Generic;
using UnityEngine;

/// Embroidery recipe step: put the fabric in the hoop. Press the outer ring down all the way round
/// so the fabric is drum-tight.
public class HoopFabricStep : TracingStep
{
    protected override float ShapePadding => 0.25f;
    protected override bool ClosedGuide => true;
    protected override Color ThreadColor => new Color(0.72f, 0.55f, 0.35f);   // the outer ring, pressed on
    protected override Color GuideColor => new Color(0.72f, 0.55f, 0.35f, 0.6f);

    protected override void DrawPiece()
    {
        Square(new Vector2(0.5f, 0.5f), 0.96f, area.fabricColor, 0);
        Line(Shape.Outline.Closed(), new Color(0.55f, 0.6f, 0.75f, 0.6f), 0.004f, 1);   // the printed design
    }

    protected override List<Vector2> Guide() => Circle(new Vector2(0.5f, 0.5f), HoopRadius, 48);
}
