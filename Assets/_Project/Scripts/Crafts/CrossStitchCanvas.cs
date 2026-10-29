using System.Collections.Generic;
using UnityEngine;

/// Cross stitch minigame.
/// Two modes:
///  - Stitch: drag from one grid corner to the diagonal corner to make a half stitch;
///            drag the opposite diagonal to complete the X.
///  - PatternFill: tap/drag cells to paint pixels (pattern-making mode).
/// Attach to an empty GameObject. Assign a Camera and a 1x1 square Sprite.
public class CrossStitchCanvas : MonoBehaviour
{
    public enum Mode { Stitch, PatternFill }

    [Header("Setup")]
    public Camera cam;
    public Sprite cellSprite;          // plain white square sprite
    public int width = 16, height = 16;
    public float cellSize = 0.5f;
    public Mode mode = Mode.Stitch;

    [Header("State")]
    public Pattern pattern = new Pattern();
    public int currentColor = 0;

    // Which cells are fully / half stitched by the player
    readonly Dictionary<Vector2Int, int> stitchProgress = new Dictionary<Vector2Int, int>(); // 0,1,2 diagonals done
    readonly Dictionary<Vector2Int, SpriteRenderer> cellViews = new Dictionary<Vector2Int, SpriteRenderer>();
    readonly List<LineRenderer> stitchLines = new List<LineRenderer>();

    Vector2Int? dragStartCorner;
    LineRenderer previewLine;

    void Start()
    {
        if (pattern.palette.Count == 0)
            pattern.palette.AddRange(new[] { Color.red, Color.blue, Color.green, Color.black });
        pattern.width = width; pattern.height = height;
        BuildGrid();
    }

    void BuildGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var go = new GameObject($"cell_{x}_{y}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = CellCenter(x, y);
                go.transform.localScale = Vector3.one * cellSize * 0.95f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = cellSprite;
                sr.color = new Color(1, 1, 1, 0.15f);
                cellViews[new Vector2Int(x, y)] = sr;
            }
    }

    Vector3 CellCenter(int x, int y) => new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0);
    Vector3 CornerPos(Vector2Int c) => transform.TransformPoint(new Vector3(c.x * cellSize, c.y * cellSize, 0));

    // Works with mouse in the editor and a single touch on iPad (Unity maps touch to mouse
    // with the legacy Input Manager; for the new Input System, swap in Pointer.current).
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) OnPress(WorldPoint());
        else if (Input.GetMouseButton(0)) OnDrag(WorldPoint());
        else if (Input.GetMouseButtonUp(0)) OnRelease(WorldPoint());
    }

    Vector3 WorldPoint()
    {
        var p = cam.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0;
        return transform.InverseTransformPoint(p);
    }

    Vector2Int NearestCorner(Vector3 local) =>
        new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(local.x / cellSize), 0, width),
                       Mathf.Clamp(Mathf.RoundToInt(local.y / cellSize), 0, height));

    Vector2Int CellAt(Vector3 local) =>
        new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(local.x / cellSize), 0, width - 1),
                       Mathf.Clamp(Mathf.FloorToInt(local.y / cellSize), 0, height - 1));

    void OnPress(Vector3 local)
    {
        if (mode == Mode.PatternFill) { Paint(CellAt(local)); return; }
        dragStartCorner = NearestCorner(local);
        previewLine = MakeLine(Color.gray, 0.04f);
        previewLine.SetPosition(0, CornerPos(dragStartCorner.Value));
        previewLine.SetPosition(1, CornerPos(dragStartCorner.Value));
    }

    void OnDrag(Vector3 local)
    {
        if (mode == Mode.PatternFill) { Paint(CellAt(local)); return; }
        if (previewLine != null)
            previewLine.SetPosition(1, transform.TransformPoint(local));
    }

    void OnRelease(Vector3 local)
    {
        if (mode != Mode.Stitch || dragStartCorner == null) return;

        var a = dragStartCorner.Value;
        var b = NearestCorner(local);
        dragStartCorner = null;
        if (previewLine) Destroy(previewLine.gameObject);

        // Valid stitch = diagonal across exactly one cell
        int dx = b.x - a.x, dy = b.y - a.y;
        if (Mathf.Abs(dx) != 1 || Mathf.Abs(dy) != 1) return; // missed: gentle fail feedback here

        var cell = new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        // Which diagonal? "/" connects (0,0)-(1,1) ; "\" connects (0,1)-(1,0)
        bool isSlash = (dx * dy) > 0;
        int bit = isSlash ? 1 : 2;

        stitchProgress.TryGetValue(cell, out int done);
        if ((done & bit) != 0) return; // already stitched that diagonal
        done |= bit;
        stitchProgress[cell] = done;

        var line = MakeLine(pattern.palette[currentColor], 0.08f);
        line.SetPosition(0, CornerPos(a));
        line.SetPosition(1, CornerPos(b));
        stitchLines.Add(line);

        if (done == 3) CompleteCell(cell);
    }

    void CompleteCell(Vector2Int cell)
    {
        cellViews[cell].color = pattern.palette[currentColor];
        pattern.cells.Add(new StitchCell { x = cell.x, y = cell.y, colorIndex = currentColor });
        // TODO: play satisfying "pop" sound, award tiny progress to the commission
    }

    void Paint(Vector2Int cell)
    {
        cellViews[cell].color = pattern.palette[currentColor];
        pattern.cells.RemoveAll(c => c.x == cell.x && c.y == cell.y);
        pattern.cells.Add(new StitchCell { x = cell.x, y = cell.y, colorIndex = currentColor });
    }

    LineRenderer MakeLine(Color color, float w)
    {
        var go = new GameObject("stitch");
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lr.endWidth = w;
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;
        lr.sortingOrder = 5;
        lr.numCapVertices = 4;
        return lr;
    }

    /// How close the player's work matches a target pattern (0..1) - use for quality/price.
    public float Accuracy(Pattern target)
    {
        if (target.cells.Count == 0) return 1f;
        int correct = 0;
        foreach (var t in target.cells)
            if (stitchProgress.TryGetValue(new Vector2Int(t.x, t.y), out int d) && d == 3)
                correct++;
        return (float)correct / target.cells.Count;
    }
}
