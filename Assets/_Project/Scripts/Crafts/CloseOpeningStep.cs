using System.Collections.Generic;
using UnityEngine;

/// Recipe step: sew the gap shut by hand once the piece is stuffed.
public class CloseOpeningStep : TracingStep
{
    protected override void DrawPiece()
    {
        Piece(Shape.Outline, area.fabricColor, 0);
        Line(Shape.Seam, Color.Lerp(area.fabricColor, Color.black, 0.35f), 0.006f, 1);   // the machine seam
        foreach (var eye in PointsFrom(StepKind.AttachEyes)) Dot(eye, AttachEyesStep.EyeSize * 1.2f, AttachEyesStep.EyeColor, 1, shine: true);
    }

    protected override List<Vector2> Guide() => Shape.Opening;
}
