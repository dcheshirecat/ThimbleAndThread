using System.Collections.Generic;

/// Recipe step for bags: sew the two panels together on the machine, down one side, along the
/// bottom and up the other, leaving the top open.
public class SewSidesStep : MachineSewingStep
{
    protected override void DrawPiece() => DrawBag(sidesSewn: false, hemmed: false);

    protected override List<UnityEngine.Vector2> Guide() => Shape.Sides;
}
