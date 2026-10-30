using System.Collections.Generic;
using UnityEngine;

/// Knitting recipe step: cast off, binding the last row so it can't unravel. Drag along the top
/// edge of the piece.
public class CastOffStep : TracingStep
{
    protected override Color ThreadColor => Color.Lerp(area.fabricColor, Color.black, 0.4f);

    protected override void DrawPiece()
    {
        var knit = KnitLayout.For(order.target);
        Rect(knit.Center, knit.Size, area.fabricColor, 0);
        var yarn = Color.Lerp(area.fabricColor, Color.black, 0.25f);
        for (int i = 0; i < knit.Stitches; i++) Stitch(knit.CellCenter(knit.At(i)), knit.cell, knit.Purl(knit.At(i)), yarn, 1);
    }

    protected override List<Vector2> Guide()
    {
        var knit = KnitLayout.For(order.target);
        float top = knit.origin.y + knit.Size.y;
        return new List<Vector2> { new Vector2(knit.origin.x, top), new Vector2(knit.origin.x + knit.Size.x, top) };
    }
}
