using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// Cross stitch minigame.
/// Two modes:
///  - Stitch: drag from one grid corner to the diagonal corner to make a half stitch;
///            drag the opposite diagonal to complete the X.
///  - PatternFill: tap/drag cells to paint pixels (pattern-making mode).
/// Each stitch uses the current thread colour. Undo steps back one drag at a time.
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
    public Color fabricColor = new Color(0.96f, 0.94f, 0.88f);   // Stitch mode: the Aida the piece is stitched on

    [Header("State")]
    public Pattern pattern = new Pattern();
    public Pattern target;             // optional: shown as a faint guide to stitch over
    public int currentColor = 0;       // thread colour: index into pattern.palette
    public List<StitchMark> restore;   // optional: work saved earlier, redrawn when the board starts
    public Pattern design;             // optional, PatternFill: a design to keep editing (palette and coloured cells)
    public bool erase;                 // PatternFill: painting clears squares instead
    public bool Ready { get; private set; }   // true once the grid is built and restored

    // Which cells are fully / half stitched by the player, and in which thread colour
    readonly Dictionary<Vector2Int, int> stitchProgress = new Dictionary<Vector2Int, int>(); // bits: 1 = "/", 2 = "\"
    readonly Dictionary<Vector2Int, int> stitchColor = new Dictionary<Vector2Int, int>();
    readonly Dictionary<Vector2Int, SpriteRenderer> cellViews = new Dictionary<Vector2Int, SpriteRenderer>();
    readonly List<LineRenderer> stitchLines = new List<LineRenderer>();

    // Undo: the work as it was before each drag that changed something
    readonly Stack<List<StitchMark>> history = new Stack<List<StitchMark>>();
    List<StitchMark> strokeStart;

    Vector2Int? dragStartCorner;
    LineRenderer previewLine;

    public bool CanUndo => history.Count > 0;

    void Start()
    {
        if (material == null) material = new Material(Shader.Find("Sprites/Default"));
        if (design != null) pattern.palette.AddRange(design.palette);
        if (pattern.palette.Count == 0)
        {
            if (target != null && target.palette.Count > 0) pattern.palette.AddRange(target.palette);
            else pattern.palette.AddRange(new[] { Color.red, Color.blue, Color.green, Color.black });
        }
        pattern.width = width; pattern.height = height;
        BuildGrid();

        var start = restore ?? new List<StitchMark>();
        if (design != null)
            foreach (var c in design.cells) start.Add(new StitchMark { x = c.x, y = c.y, mask = 3, color = c.colorIndex });
        Apply(start);
        Ready = true;
    }

    /// The work so far, for saving.
    public List<StitchMark> Marks()
    {
        var marks = new List<StitchMark>();
        if (mode == Mode.PatternFill)
            foreach (var c in pattern.cells) marks.Add(new StitchMark { x = c.x, y = c.y, mask = 3, color = c.colorIndex });
        else
            foreach (var kv in stitchProgress)
                marks.Add(new StitchMark { x = kv.Key.x, y = kv.Key.y, mask = kv.Value, color = stitchColor[kv.Key] });
        return marks;
    }

    /// Steps back one drag.
    public void Undo()
    {
        if (history.Count == 0) return;
        var previous = history.Pop();
        Clear();
        Apply(previous);
    }

    void Clear()
    {
        foreach (var l in stitchLines) if (l) Destroy(l.gameObject);
        stitchLines.Clear();
        stitchProgress.Clear();
        stitchColor.Clear();
        pattern.cells.Clear();
        foreach (var kv in cellViews) kv.Value.color = GuideColor(kv.Key.x, kv.Key.y);   // the chart shows again
    }

    void Apply(List<StitchMark> marks)
    {
        foreach (var m in marks)
        {
            var cell = new Vector2Int(m.x, m.y);
            if (!cellViews.ContainsKey(cell)) continue;
            int color = Mathf.Clamp(m.color, 0, pattern.palette.Count - 1);
            if (mode == Mode.PatternFill) { if (m.mask == 3) PaintCell(cell, color); continue; }
            if ((m.mask & 1) != 0) AddStitch(cell, 1, color);
            if ((m.mask & 2) != 0) AddStitch(cell, 2, color);
        }
    }

    void BuildGrid()
    {
        if (mode == Mode.Stitch) BuildFabric();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var go = new GameObject($"cell_{x}_{y}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = CellCenter(x, y);
                go.transform.localScale = Vector3.one * cellSize * (mode == Mode.Stitch ? 1f : 0.95f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = cellSprite;
                sr.sharedMaterial = material;
                sr.color = GuideColor(x, y);
                cellViews[new Vector2Int(x, y)] = sr;
            }
    }

    /// Stitch mode: a piece of Aida under the grid, with the little woven holes the needle goes through.
    void BuildFabric()
    {
        var fabric = new GameObject("fabric").AddComponent<SpriteRenderer>();
        fabric.transform.SetParent(transform, false);
        fabric.transform.localPosition = new Vector3(width * cellSize / 2f, height * cellSize / 2f, 0);
        fabric.transform.localScale = new Vector3((width + 0.6f) * cellSize, (height + 0.6f) * cellSize, 1);
        fabric.sprite = cellSprite;
        fabric.sharedMaterial = material;
        fabric.color = fabricColor;
        fabric.sortingOrder = -2;
        var hole = Color.Lerp(fabricColor, Color.black, 0.28f);
        for (int x = 0; x <= width; x++)
            for (int y = 0; y <= height; y++)
            {
                var h = new GameObject("hole").AddComponent<SpriteRenderer>();
                h.transform.SetParent(transform, false);
                h.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0);
                h.transform.localScale = Vector3.one * cellSize * 0.13f;
                h.sprite = cellSprite;
                h.sharedMaterial = material;
                h.color = hole;
                h.sortingOrder = 1;
            }
    }

    Color GuideColor(int x, int y)
    {
        var t = target?.cells.Find(c => c.x == x && c.y == y);
        if (mode == Mode.Stitch)
        {
            // The chart printed faintly on the fabric: a light tint of the square's thread colour.
            if (t == null) return Color.clear;
            var tint = Color.Lerp(fabricColor, target.palette[t.colorIndex], 0.35f);
            tint.a = 1f;
            return tint;
        }
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
        strokeStart = Marks();
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
        if (mode == Mode.Stitch && dragStartCorner != null)
        {
            var a = dragStartCorner.Value;
            var b = NearestCorner(local);
            dragStartCorner = null;
            if (previewLine) Destroy(previewLine.gameObject);

            // Valid stitch = diagonal across exactly one cell
            int dx = b.x - a.x, dy = b.y - a.y;
            if (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 1)   // otherwise missed: gentle fail feedback here
            {
                var cell = new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
                // Which diagonal? "/" connects (0,0)-(1,1) ; "\" connects (0,1)-(1,0)
                bool isSlash = (dx * dy) > 0;
                AddStitch(cell, isSlash ? 1 : 2, currentColor);
                AudioManager.Play(Sfx.Stitch);
            }
        }
        EndStroke();
    }

    /// Remembers the work from before this drag, if the drag changed anything.
    void EndStroke()
    {
        if (strokeStart != null && Key(strokeStart) != Key(Marks())) history.Push(strokeStart);
        strokeStart = null;
    }

    static string Key(List<StitchMark> marks)
    {
        var parts = new List<string>();
        foreach (var m in marks) parts.Add($"{m.x},{m.y},{m.mask},{m.color}");
        parts.Sort();
        return string.Join(";", parts);
    }

    void AddStitch(Vector2Int cell, int bit, int color)
    {
        stitchProgress.TryGetValue(cell, out int done);
        if ((done & bit) != 0) return; // already stitched that diagonal
        done |= bit;
        stitchProgress[cell] = done;
        stitchColor[cell] = color;

        bool slash = bit == 1;
        DrawThread(CornerPos(new Vector2Int(cell.x, slash ? cell.y : cell.y + 1)),
                   CornerPos(new Vector2Int(cell.x + 1, slash ? cell.y + 1 : cell.y)),
                   pattern.palette[color], slash ? 5 : 9);   // the second diagonal crosses over the first

        if (done == 3) CompleteCell(cell, color);
    }

    /// One stitch as real floss: a soft shadow, the thread itself and a sheen along it, running from
    /// hole to hole. The shadow keeps even white or pale thread visible on the fabric.
    void DrawThread(Vector3 from, Vector3 to, Color color, int sorting)
    {
        color.a = 1f;
        var inset = (to - from) * 0.05f;
        from += inset; to -= inset;
        var down = new Vector3(0.03f, -0.03f, 0) * cellSize;
        var up = new Vector3(-0.025f, 0.025f, 0) * cellSize;
        Thread(from + down, to + down, new Color(0, 0, 0, 0.35f), cellSize * 0.22f, sorting);
        Thread(from, to, color, cellSize * 0.17f, sorting + 1, Color.Lerp(color, Color.black, 0.18f));
        Thread(from + up, to + up, new Color(1, 1, 1, 0.45f) * Color.Lerp(color, Color.white, 0.6f), cellSize * 0.05f, sorting + 2);
    }

    void Thread(Vector3 from, Vector3 to, Color color, float width, int sorting, Color? endColor = null)
    {
        var line = MakeLine(color, width);
        line.endColor = endColor ?? color;
        line.sortingOrder = sorting;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        stitchLines.Add(line);
    }

    void CompleteCell(Vector2Int cell, int color)
    {
        // The thread covers the square now, so the printed chart under it no longer shows.
        if (mode == Mode.Stitch) cellViews[cell].color = Color.clear;
        else cellViews[cell].color = pattern.palette[color];
        pattern.cells.RemoveAll(c => c.x == cell.x && c.y == cell.y);
        pattern.cells.Add(new StitchCell { x = cell.x, y = cell.y, colorIndex = color });
        // TODO: play satisfying "pop" sound, award tiny progress to the commission
    }

    void Paint(Vector2Int cell)
    {
        if (erase)
        {
            pattern.cells.RemoveAll(c => c.x == cell.x && c.y == cell.y);
            cellViews[cell].color = GuideColor(cell.x, cell.y);
            return;
        }
        PaintCell(cell, currentColor);
    }

    void PaintCell(Vector2Int cell, int color)
    {
        pattern.cells.RemoveAll(c => c.x == cell.x && c.y == cell.y);
        cellViews[cell].color = pattern.palette[color];
        pattern.cells.Add(new StitchCell { x = cell.x, y = cell.y, colorIndex = color });
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

    /// A finished square counts when it's in the target and in the right thread colour.
    /// Squares finished outside the target count against the score.
    public static float Score(List<StitchMark> marks, Pattern target)
    {
        if (target.cells.Count == 0) return 1f;
        var wanted = new Dictionary<Vector2Int, int>();
        foreach (var t in target.cells) wanted[new Vector2Int(t.x, t.y)] = t.colorIndex;

        int correct = 0, extra = 0;
        foreach (var m in marks)
        {
            if (m.mask != 3) continue;
            if (!wanted.TryGetValue(new Vector2Int(m.x, m.y), out int color)) extra++;
            else if (color == m.color) correct++;
        }
        return (float)correct / (wanted.Count + extra);
    }
}
