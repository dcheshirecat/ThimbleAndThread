using System.Collections.Generic;
using UnityEngine;

/// Knitting finishing step for a scarf: the fringe. At each mark along both ends, a folded bundle of yarn
/// is pulled through the edge with a crochet hook and knotted (a lark's head knot). Tap each mark; the
/// closer to it, the neater the fringe hangs.
public class FringeStep : PlacementStep
{
    protected override bool SewnOn => true;
    protected override CraftTool.Kind PlaceTool => CraftTool.Kind.Hook;
    protected override Color MarkColor => new Color(1, 1, 1, 0.45f);

    List<Vector2> marks;

    protected override List<Vector2> Marks
    {
        get
        {
            if (marks != null) return marks;
            marks = new List<Vector2>();
            var bottom = Knit.CastOnStitches();
            var top = Knit.TopEdge();
            float topY = top[0].y, bottomY = Knit.origin.y;
            for (int i = 0; i < bottom.Count; i += 2)
            {
                float x = Knit.CellCenter(bottom[i]).x;
                marks.Add(new Vector2(x, bottomY));
                marks.Add(new Vector2(x, topY));
            }
            return marks;
        }
    }

    protected override void DrawPiece() => DrawKnitted(Knit, 0);

    /// A knot at the edge and a few strands hanging away from the scarf.
    protected override GameObject DrawPlaced(Vector2 p)
    {
        var tassel = new GameObject("Tassel");
        tassel.transform.SetParent(transform, false);
        float dir = p.y < Knit.Center.y ? -1f : 1f;
        float len = Knit.cell * 2.2f;
        for (int s = -1; s <= 1; s++)
        {
            var strand = Line(new[] { p, p + new Vector2(s * Knit.cell * 0.18f, dir * len) }, Yarn, 0.008f, 3);
            strand.transform.SetParent(tassel.transform, true);
        }
        Dot(p, Knit.cell * 0.35f, Color.Lerp(Yarn, Color.black, 0.2f), 4).transform.SetParent(tassel.transform, true);
        return tassel;
    }
}
