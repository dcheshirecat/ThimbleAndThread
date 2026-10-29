using UnityEngine;

/// Spawns the craft minigame for an order, restoring any work saved earlier, and reads the work back.
/// Also opens the same grid as a paint canvas for the Pattern Studio.
/// Cross stitch stands in for every craft until each one has its own minigame.
public class MinigameHost : MonoBehaviour
{
    public Camera cam;
    public Material material;                // sprite material for cells and threads

    [Header("Board placement, as fractions of the screen (portrait)")]
    [Range(0, 1)] public float bandBottom = 0.40f;   // controls go below this
    [Range(0, 1)] public float bandTop = 0.86f;      // title/hint above this
    [Range(0, 1)] public float maxWidth = 0.92f;

    CrossStitchCanvas board;
    Order order;
    Sprite cellSprite;

    public CrossStitchCanvas Board => board;

    public void Open(Order forOrder)
    {
        Close();
        order = forOrder;
        var target = order.target;
        board = NewBoard(target.width, target.height);
        board.mode = order.patternOnly ? CrossStitchCanvas.Mode.PatternFill : CrossStitchCanvas.Mode.Stitch;
        board.target = target;
        board.restore = order.work;
    }

    /// Pattern Studio: a paint grid for designing. Read the result from Board.pattern before Close().
    public void OpenDesign(Pattern design)
    {
        Close();
        board = NewBoard(design.width, design.height);
        board.mode = CrossStitchCanvas.Mode.PatternFill;
        board.design = design;
    }

    public void SelectColour(int index)
    {
        if (board == null) return;
        board.erase = index < 0;
        if (index >= 0) board.currentColor = index;
    }

    public void Undo()
    {
        if (board != null) board.Undo();
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

    /// Fits the grid into the band of the screen set above, as big as it can be while staying square.
    CrossStitchCanvas NewBoard(int width, int height)
    {
        if (cellSprite == null)
            cellSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);

        float viewH = cam.orthographicSize * 2f, viewW = viewH * cam.aspect;
        float size = Mathf.Min(viewW * maxWidth, viewH * (bandTop - bandBottom));
        float cell = size / Mathf.Max(width, height);
        var center = new Vector2(cam.transform.position.x, cam.transform.position.y + viewH * ((bandTop + bandBottom) / 2f - 0.5f));

        var go = new GameObject("CrossStitchBoard");
        go.transform.SetParent(transform, false);
        go.transform.position = (Vector3)(center - new Vector2(width * cell, height * cell) / 2f);
        var b = go.AddComponent<CrossStitchCanvas>();
        b.cam = cam;
        b.material = material;
        b.cellSprite = cellSprite;
        b.width = width;
        b.height = height;
        b.cellSize = cell;
        return b;
    }
}
