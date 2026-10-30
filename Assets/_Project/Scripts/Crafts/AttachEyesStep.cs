using System.Collections.Generic;
using UnityEngine;

/// Recipe step: put the safety eyes in. Tap where each eye goes; the faint marks come from the
/// pattern (squares in its second colour). Closer to the marks is better.
public class AttachEyesStep : PlacementStep
{
    public const float EyeSize = 0.05f;
    public static readonly Color EyeColor = new Color(0.08f, 0.08f, 0.1f);

    protected override List<Vector2> Marks => Shape.Eyes;

    protected override void DrawPiece()
    {
        Piece(Shape.Outline, area.fabricColor, 0);
        Line(Shape.Seam, Color.Lerp(area.fabricColor, Color.black, 0.35f), 0.006f, 1);
    }

    protected override GameObject DrawPlaced(Vector2 p) => Square(p, EyeSize, EyeColor, 2).gameObject;

    protected override bool CanPlace(Vector2 p) => PieceShape.Inside(Shape.Outline, p);
}
