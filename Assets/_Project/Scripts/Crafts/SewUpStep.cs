using System.Collections.Generic;
using UnityEngine;

/// Knitting finishing step for a sweater: the knitted back, front and sleeves are laid together and sewn
/// up with a tapestry needle and the same yarn (mattress stitch). The sweater lies flat, front up; the player
/// sews each side seam, from the sleeve cuff, along under the arm, down to the hem.
public class SewUpStep : TracingStep
{
    protected override float StitchLength => 0.02f;
    protected override Color ThreadColor => Color.Lerp(area.fabricColor, Color.black, 0.35f);

    // The flat sweater, in 0..1 of the area.
    const float BodyLeft = 0.32f, BodyRight = 0.68f, Hem = 0.14f, Shoulder = 0.72f, Armpit = 0.56f;
    const float CuffReach = 0.07f, CuffTop = 0.5f, CuffBottom = 0.38f;

    protected override void DrawPiece()
    {
        var front = KnitLayout.For(order, "front");
        // Sleeves first (they sit behind the body at the shoulder), then the body.
        Piece(Sleeve(-1), area.fabricColor, 0);
        Piece(Sleeve(1), area.fabricColor, 0);
        Piece(Body(), area.fabricColor, 1);
        // Ribbed hem and cuffs.
        Rect(new Vector2(0.5f, Hem + 0.02f), new Vector2(BodyRight - BodyLeft, 0.04f), Yarn, 2);
        Rect(new Vector2(CuffReach + 0.02f, (CuffTop + CuffBottom) / 2f), new Vector2(0.04f, CuffTop - CuffBottom), Yarn, 2);
        Rect(new Vector2(1 - CuffReach - 0.02f, (CuffTop + CuffBottom) / 2f), new Vector2(0.04f, CuffTop - CuffBottom), Yarn, 2);
        // The front's colourwork, scaled onto the body.
        float scale = (BodyRight - BodyLeft) / front.Size.x;
        var bodyOrigin = new Vector2(BodyLeft, Hem);
        for (int i = 0; i < front.Stitches; i++)
        {
            if (front.stitches[i].color < 0) continue;
            var at = bodyOrigin + (front.CellCenter(i) - front.origin) * scale;
            if (at.y < Shoulder - 0.02f) Square(at, front.cell * scale, front.ColorOf(i, Yarn), 3);
        }
    }

    List<Vector2> Body() => new List<Vector2>
    {
        new Vector2(BodyLeft, Hem), new Vector2(BodyRight, Hem), new Vector2(BodyRight, Shoulder),
        new Vector2(0.58f, Shoulder), new Vector2(0.5f, Shoulder - 0.05f), new Vector2(0.42f, Shoulder),   // the neckline
        new Vector2(BodyLeft, Shoulder),
    };

    /// A sleeve going out to the left (-1) or right (1) from the shoulder, narrowing to the cuff.
    List<Vector2> Sleeve(int side)
    {
        float inner = side < 0 ? BodyLeft + 0.02f : BodyRight - 0.02f;
        float cuff = side < 0 ? CuffReach : 1 - CuffReach;
        var outline = new List<Vector2>
        {
            new Vector2(inner, Shoulder), new Vector2(cuff, CuffTop), new Vector2(cuff, CuffBottom), new Vector2(inner, Armpit),
        };
        if (side > 0) outline.Reverse();   // keep the same winding as the left one, or the mesh faces away
        return outline;
    }

    protected override List<Vector2> Guide() => Seam(-1);

    protected override List<List<Vector2>> Guides() => new List<List<Vector2>> { Seam(-1), Seam(1) };

    /// One side seam: under the sleeve from the cuff to the armpit, then down the side to the hem.
    List<Vector2> Seam(int side)
    {
        float cuff = side < 0 ? CuffReach + 0.01f : 1 - CuffReach - 0.01f;
        float edge = side < 0 ? BodyLeft : BodyRight;
        return new List<Vector2> { new Vector2(cuff, CuffBottom + 0.005f), new Vector2(edge, Armpit), new Vector2(edge, Hem + 0.005f) };
    }
}
