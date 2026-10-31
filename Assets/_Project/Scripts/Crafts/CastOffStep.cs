using System.Collections.Generic;
using UnityEngine;

/// Knitting recipe step: cast off, binding the last row so it can't unravel. Drag along the top
/// edge of the piece.
public class CastOffStep : TracingStep
{
    protected override float StitchLength => 0f;   // a continuous line, not stitches
    protected override Color ThreadColor => Color.Lerp(area.fabricColor, Color.black, 0.4f);

    protected override void DrawPiece() => DrawKnitted(Knit, 0);

    protected override List<Vector2> Guide() => Knit.TopEdge();
}
