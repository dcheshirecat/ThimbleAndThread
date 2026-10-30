using System.Collections.Generic;
using UnityEngine;

/// Knitting finishing step: weave in the yarn tail left from casting off, so it doesn't come loose.
/// Drag the tail along the dashed path through the back of the stitches.
public class WeaveEndsStep : TracingStep
{
    protected override float StitchLength => 0f;   // a strand of yarn, not stitches
    protected override Color ThreadColor => Color.Lerp(area.fabricColor, Color.black, 0.25f);

    protected override void DrawPiece()
    {
        var knit = KnitLayout.For(order.target);
        Rect(knit.Center, knit.Size, area.fabricColor, 0);
        var yarn = Color.Lerp(area.fabricColor, Color.black, 0.25f);
        for (int i = 0; i < knit.Stitches; i++) Stitch(knit.CellCenter(knit.At(i)), knit.cell, knit.Purl(knit.At(i)), yarn, 1);
        var corner = knit.origin + knit.Size;   // the loose tail, hanging off the last stitch
        Line(new[] { corner, corner + new Vector2(0.05f, 0.06f) }, yarn, 0.012f, 2);
    }

    /// Down the side from the top corner, through the edge stitches.
    protected override List<Vector2> Guide()
    {
        var knit = KnitLayout.For(order.target);
        var corner = knit.origin + knit.Size;
        return new List<Vector2> { corner - new Vector2(0.02f, 0.01f), corner - new Vector2(0.02f, knit.Size.y * 0.5f) };
    }
}
