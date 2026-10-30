using System.Collections.Generic;
using UnityEngine;

/// Recipe step: sew the two pieces together on the machine, all the way round except for a gap
/// at the bottom (left open for turning and stuffing).
public class SewSeamStep : MachineSewingStep
{
    protected override void DrawPiece()
    {
        Piece(Shape.Outline, Color.Lerp(area.fabricColor, Color.black, 0.2f), -1, new Vector2(0.012f, -0.012f));   // the back piece
        Piece(Shape.Outline, area.fabricColor, 0);
    }

    protected override List<Vector2> Guide() => Shape.Seam;
}
