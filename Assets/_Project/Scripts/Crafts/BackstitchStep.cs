using System.Collections.Generic;
using UnityEngine;

/// Embroidery recipe step: backstitch along the printed outline. How closely the needle follows
/// the line sets the quality.
public class BackstitchStep : TracingStep
{
    protected override float ShapePadding => 0.25f;
    protected override bool ClosedGuide => true;
    protected override Color ThreadColor => Color.Lerp(Floss(0), Color.black, 0.3f);
    protected override Color GuideColor => new Color(0.55f, 0.6f, 0.75f, 0.9f);   // the printed line

    protected override void DrawPiece()
    {
        DrawHoop(outlined: false, filled: false);
        // On a cross stitch piece, the outline goes round the stitching already done.
        if (order.item == null || order.item.craft != CraftType.CrossStitch || order.target == null) return;
        foreach (var c in order.target.cells)
        {
            var at = PieceShape.SquareCenter(order.target, ShapePadding, c.x, c.y, out float size);
            Square(at, size * 0.92f, c.colorIndex < order.target.palette.Count ? order.target.palette[c.colorIndex] : Floss(0), 1);
        }
    }

    protected override List<Vector2> Guide() => Shape.Outline;
}
