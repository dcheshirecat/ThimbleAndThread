using System.Collections.Generic;

/// Recipe step for bags: fold the top edge over and sew the hem along it.
public class HemTopStep : TracingStep
{
    protected override void DrawPiece() => DrawBag(sidesSewn: true, hemmed: false);

    protected override List<UnityEngine.Vector2> Guide() => Shape.Mouth;
}
