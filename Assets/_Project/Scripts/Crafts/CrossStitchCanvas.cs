using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public Material material;          // sprite material for cells and threads; falls back to Sprites/Default

    [Header("State")]
    public Pattern pattern = new Pattern();
    public Pattern target;             // optional: shown as a faint guide to stitch over
    public int currentColor = 0;
    public List<StitchMark> restore;   // optional: work saved earlier, redrawn when the board starts
    public bool Ready { get; private set; }   // true once the grid is built and restored

    // Which cells are fully / half stitched by the player
    readonly Dictionary<Vector2Int, int> stitchProgress = new Dictionary<Vector2Int, int>(); // 0,1,2 diagonals done
    readonly Dictionary<Vector2Int, SpriteRenderer> cellViews = new Dictionary<Vector2Int, SpriteRenderer>();
    readonly List<LineRenderer> stitchLines = new List<LineRenderer>();

    Vector2Int? dragStartCorner;
    LineRenderer previewLine;

    void Start()
    {
        if (material == null) material = new Material(Shader.Find("Sprites/Default"));
        if (pattern.palette.Count == 0)
        {
            if (target != null && target.palette.Count > 0) pattern.palette.AddRange(target.palette);
            else pattern.palette.AddRange(new[] { Color.red, Color.blue, Color.green, Color.black });
        }
        pattern.width = width; pattern.height = height;
        BuildGrid();
        if (restore != null)
            foreach (var m in restore)
            {
                var cell = new Vector2Int(m.x, m.y);
                if (!cellViews.ContainsKey(cell)) continue;
                if (mode == Mode.PatternFill) { if (m.mask == 3) Paint(cell); continue; }
                if ((m.mask & 1) != 0) AddStitch(cell, 1);
                if ((m.mask & 2) != 0) AddStitch(cell, 2);
            }
        Ready = true;
    }

    /// The work so far, for saving.
    public List<StitchMark> Marks()
    {
        var marks = new List<StitchMark>();
        if (mode == Mode.PatternFill)
            foreach (var c in pattern.cells) marks.Add(new StitchMark { x = c.x, y = c.y, mask = 3 });
        else
            foreach (var kv in stitchProgress) marks.Add(new StitchMark { x = kv.Key.x, y = kv.Key.y, mask = kv.Value });
        return marks;
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
                sr.sharedMaterial = material;
                sr.color = GuideColor(x, y);
                cellViews[new Vector2Int(x, y)] = sr;
            }
    }

    Color GuideColor(int x, int y)
    {
        var t = target?.cells.Find(c => c.x == x && c.y == y);
        if (t == null) return new Color(1, 1, 1, 0.15f);
        var c = target.palette[t.colorIndex];
        c.a = 0.4f;
        return c;
    }

    Vector3 CellCenter(int x, int y) => new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0);
    Vector3 CornerPos(Vector2Int c) => transform.TransformPoint(new Vector3(c.x * cellSize, c.y * cellSize, 0));

    // Works with mouse in the editor and a single touch on iPad (Unity maps touch to mouse
    // with the legacy Input Manager; for the new Input System, swap in Pointer.current).
    void Update()
    {
        // Presses that start on a UI button (e.g. Finish) are not stitches.
        if (Input.GetMouseButtonDown(0) && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
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
        AddStitch(cell, isSlash ? 1 : 2);
    }

    void AddStitch(Vector2Int cell, int bit)
    {
        stitchProgress.TryGetValue(cell, out int done);
        if ((done & bit) != 0) return; // already stitched that diagonal
        done |= bit;
        stitchProgress[cell] = done;

        var line = MakeLine(pattern.palette[currentColor], 0.08f);
        bool slash = bit == 1;
        line.SetPosition(0, CornerPos(new Vector2Int(cell.x, slash ? cell.y : cell.y + 1)));
        line.SetPosition(1, CornerPos(new Vector2Int(cell.x + 1, slash ? cell.y + 1 : cell.y)));
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
        lr.sharedMaterial = material;
        lr.startColor = lr.endColor = color;
        lr.sortingOrder = 5;
        lr.numCapVertices = 4;
        return lr;
    }

    /// How close the player's work matches a target pattern (0..1) - use for quality/price.
    public float Accuracy(Pattern target) => Score(Marks(), target);

    /// Counts finished cells in the target; cells finished outside it count against the score.
    public static float Score(List<StitchMark> marks, Pattern target)
    {
        if (target.cells.Count == 0) return 1f;
        var wanted = new HashSet<Vector2Int>();
        foreach (var t in target.cells) wanted.Add(new Vector2Int(t.x, t.y));

        int correct = 0, extra = 0;
        foreach (var m in marks)
            if (m.mask == 3) { if (wanted.Contains(new Vector2Int(m.x, m.y))) correct++; else extra++; }
        return (float)correct / (wanted.Count + extra);
    }
}
