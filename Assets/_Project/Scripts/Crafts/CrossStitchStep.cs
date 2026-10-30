using UnityEngine;

/// Recipe step: stitch (or, for a pattern-only order, fill in) the order's pattern on the cross stitch board.
/// The stitches are saved in Order.work.
public class CrossStitchStep : MonoBehaviour, ICraftStep
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
