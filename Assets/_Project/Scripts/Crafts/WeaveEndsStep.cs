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
        DrawKnitted(Knit, 0);
        var corner = Corner();   // the loose tail, hanging off the last stitch
        Line(new[] { corner, corner + new Vector2(0.05f, 0.06f) }, Yarn, 0.012f, 3);
    }

    /// Down the side from the top corner, through the edge stitches.
    protected override List<Vector2> Guide()
    {
        var corner = Corner();
        return new List<Vector2> { corner - new Vector2(0.02f, 0.01f), corner - new Vector2(0.02f, Knit.Size.y * 0.5f) };
    }

    /// The top right corner of the piece.
    Vector2 Corner()
    {
        var edge = Knit.TopEdge();
        return edge[edge.Count - 1];
    }
}
