using UnityEngine;

/// Recipe step: stitch (or, for a pattern-only order, fill in) the order's pattern on the cross stitch board.
/// The stitches are saved in Order.work.
public class CrossStitchStep : MonoBehaviour, ICraftStep, IMistakes
{
    Order order;

    public CrossStitchCanvas Board { get; private set; }

    public void Begin(Order forOrder, int stepIndex, StepArea area)
    {
        order = forOrder;
        var target = order.target;
        Board = MakeBoard(transform, area, target.width, target.height);
        Board.mode = order.patternOnly ? CrossStitchCanvas.Mode.PatternFill : CrossStitchCanvas.Mode.Stitch;
        Board.target = target;
        Board.restore = order.work;
        Board.fabricColor = area.fabricColor;
        Board.assist = area.assist;
    }

    public float SaveWork()
    {
        if (Board != null && Board.Ready) order.work = Board.Marks();   // before Ready the board is still empty
        return CrossStitchCanvas.Score(order.work, order.target);
    }

    public void Undo()
    {
        if (Board != null) Board.Undo();
    }

    public string FinishBlocker => null;

    /// Stitches in the wrong colour or outside the pattern, and crosses made the wrong way round.
    public string MistakesLeft
    {
        get
        {
            if (Board == null || !Board.Ready || order.patternOnly) return null;
            int wrong = 0;
            foreach (var m in Board.Marks())
            {
                var cell = order.target.cells.Find(c => c.x == m.x && c.y == m.y);
                if (cell == null || cell.colorIndex != m.color) wrong++;
            }
            int crossed = Board.WrongCrosses();
            var parts = new System.Collections.Generic.List<string>();
            if (wrong > 0) parts.Add(wrong == 1 ? "1 stitch in the wrong place or colour" : $"{wrong} stitches in the wrong place or colour");
            if (crossed > 0) parts.Add(crossed == 1 ? "1 cross the wrong way round" : $"{crossed} crosses the wrong way round");
            return parts.Count == 0 ? null : string.Join(" and ", parts);
        }
    }

    /// A board fitted into the area, as big as it can be while its squares stay square.
    public static CrossStitchCanvas MakeBoard(Transform parent, StepArea area, int width, int height)
    {
        float cell = area.size / Mathf.Max(width, height);
        var go = new GameObject("CrossStitchBoard");
        go.transform.SetParent(parent, false);
        go.transform.position = (Vector3)(area.center - new Vector2(width * cell, height * cell) / 2f);
        var b = go.AddComponent<CrossStitchCanvas>();
        b.cam = area.cam;
        b.material = area.material;
        b.cellSprite = area.square;
        b.width = width;
        b.height = height;
        b.cellSize = cell;
        return b;
    }
}
