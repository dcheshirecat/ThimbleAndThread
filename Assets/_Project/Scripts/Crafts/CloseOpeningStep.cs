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

    /// A ladder stitch: small stitches going back and forth between the two folded edges of the gap, which
    /// pulls them together invisibly.
    protected override List<Vector2> Guide()
    {
        var along = PieceShape.Resample(Shape.Opening, false, 9);
        var ladder = new List<Vector2>();
        for (int i = 0; i < along.Count; i++)
        {
            var dir = (along[Mathf.Min(i + 1, along.Count - 1)] - along[Mathf.Max(i - 1, 0)]).normalized;
            var across = new Vector2(-dir.y, dir.x) * 0.018f;
            ladder.Add(along[i] + (i % 2 == 0 ? across : -across));
        }
        return ladder;
    }
}
