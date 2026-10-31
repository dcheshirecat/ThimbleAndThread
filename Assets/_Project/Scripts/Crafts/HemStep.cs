using System.Collections.Generic;
using UnityEngine;

/// Recipe step: hem a square piece (e.g. a tablecloth) by sewing along a line just inside all four edges.
public class HemStep : TracingStep
{
    const float Inset = 0.12f;   // the sheet covers 0.05..0.95; the hem line sits just inside its edge

    protected override bool ClosedGuide => true;

    /// A quilt's binding goes round the patchwork itself.
    protected override void DrawPiece()
    {
        if (order.item == null || order.item.craft != CraftType.Quilting) { base.DrawPiece(); return; }
        Square(Vector2.one * 0.5f, 0.86f, Color.Lerp(area.fabricColor, Color.black, 0.15f), -1);   // the backing
        DrawPatchwork(order.target);
    }

    protected override List<Vector2> Guide() => new List<Vector2>
    {
        new Vector2(Inset, Inset), new Vector2(1 - Inset, Inset),
        new Vector2(1 - Inset, 1 - Inset), new Vector2(Inset, 1 - Inset),
    };
}
