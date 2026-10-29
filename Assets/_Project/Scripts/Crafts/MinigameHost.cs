using UnityEngine;

/// Spawns the craft minigame for an order, restoring any work saved earlier, and reads the work back.
/// Cross stitch stands in for every craft until each one has its own minigame.
public class MinigameHost : MonoBehaviour
{
    public Camera cam;
    public Material material;                            // sprite material for cells and threads
    public float boardSize = 7f;                         // world units; camera shows 10 units of height
    public Vector2 boardCenter = new Vector2(0, -0.2f);

    CrossStitchCanvas board;
    Order order;
    Sprite cellSprite;

    public void Open(Order forOrder)
    {
        Close();
        order = forOrder;
        if (cellSprite == null)
            cellSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);

        var target = order.target;
        var go = new GameObject("CrossStitchBoard");
        go.transform.SetParent(transform, false);
        go.transform.position = (Vector3)(boardCenter - Vector2.one * boardSize / 2);
        board = go.AddComponent<CrossStitchCanvas>();
        board.cam = cam;
        board.material = material;
        board.cellSprite = cellSprite;
        board.width = target.width;
        board.height = target.height;
        board.cellSize = boardSize / Mathf.Max(target.width, target.height);
        board.mode = order.patternOnly ? CrossStitchCanvas.Mode.PatternFill : CrossStitchCanvas.Mode.Stitch;
        board.target = target;
        board.restore = order.work;
    }

    /// Copies the work on the board into the order (for saving). Returns accuracy (0..1) against the target.
    public float SaveWork()
    {
        if (order == null) return 0f;
        if (board != null && board.Ready) order.work = board.Marks();   // before Ready the board is still empty
        return CrossStitchCanvas.Score(order.work, order.target);
    }

    public void Close()
    {
        if (board != null) Destroy(board.gameObject);
        board = null;
        order = null;
    }
}
