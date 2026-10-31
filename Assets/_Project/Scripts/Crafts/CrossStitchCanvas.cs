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
    public float assist;               // 0..1 from a better needle or frame: a drag that's a bit long or short still lands
    public Color fabricColor = new Color(0.96f, 0.94f, 0.88f);   // Stitch mode: the Aida the piece is stitched on

    [Header("State")]
    public Pattern pattern = new Pattern();
    public Pattern target;             // optional: shown as a faint guide to stitch over
    public int currentColor = 0;       // thread colour: index into pattern.palette
    public List<StitchMark> restore;   // optional: work saved earlier, redrawn when the board starts
    public Pattern design;             // optional, PatternFill: a design to keep editing (palette and coloured cells)
    public bool paintHalf;             // designing: squares painted now are half stitches (just the "/" leg)
    public bool erase;                 // PatternFill: painting clears squares instead
    public DesignFrame frame;          // PatternFill: how the item is really made, shown on the board (owner, 2026-10-31)

    /// What a design is made into, drawn on the designer's board: a round hoop of fabric (embroidery), rounds
    /// spiralling out from a ring (crochet), knitted stitches (knitting), or plain squares (cross stitch, sewing).
    public enum DesignFrame { Squares, Hoop, Rounds, Knitted }

    public static DesignFrame FrameFor(CraftItemDef item) => item == null ? DesignFrame.Squares : item.craft switch
    {
        CraftType.Embroidery => DesignFrame.Hoop,
        CraftType.Crochet => DesignFrame.Rounds,
        CraftType.Knitting => DesignFrame.Knitted,
        _ => DesignFrame.Squares,
    };

    bool Round => mode == Mode.PatternFill && (frame == DesignFrame.Hoop || frame == DesignFrame.Rounds);

    /// Whether a square can hold a stitch: in a hoop or a crocheted piece, only those inside the circle.
    public bool Inside(int x, int y)
    {
        if (!Round) return true;
        float r = Mathf.Min(width, height) / 2f;
        float dx = x + 0.5f - width / 2f, dy = y + 0.5f - height / 2f;
        return dx * dx + dy * dy <= r * r;
    }
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
            foreach (var c in design.cells) start.Add(new StitchMark { x = c.x, y = c.y, mask = c.halfStitch ? 1 : 3, color = c.colorIndex });
        Apply(start);
        Ready = true;
    }

    /// The work so far, for saving.
    public List<StitchMark> Marks()
    {
        var marks = new List<StitchMark>();
        if (mode == Mode.PatternFill)
            foreach (var c in pattern.cells) marks.Add(new StitchMark { x = c.x, y = c.y, mask = c.halfStitch ? 1 : 3, color = c.colorIndex });
        else
            foreach (var kv in stitchProgress)
                marks.Add(new StitchMark { x = kv.Key.x, y = kv.Key.y, mask = kv.Value, color = stitchColor[kv.Key], crossedWrong = CrossedWrong(kv.Key) });
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
        firstLeg.Clear();
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
            if (mode == Mode.PatternFill) { if ((m.mask == 3 || m.mask == 1) && Inside(cell.x, cell.y)) PaintCell(cell, color, m.mask == 1); continue; }
            int first = m.crossedWrong ? 2 : 1, second = 3 - first;   // replay the legs in the order they were made
            if ((m.mask & first) != 0) AddStitch(cell, first, color);
            if ((m.mask & second) != 0) AddStitch(cell, second, color);
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
        if (mode == Mode.PatternFill) BuildFrame();
        // Half stitches on the chart: a printed "/" across the square.
        if (mode == Mode.Stitch && target != null)
            foreach (var t in target.cells)
            {
                if (!t.halfStitch) continue;
                var mark = MakeLine(Color.Lerp(fabricColor, target.palette[t.colorIndex], 0.6f), cellSize * 0.08f);
                mark.sortingOrder = 1;
                mark.SetPosition(0, transform.TransformPoint(new Vector3((t.x + 0.15f) * cellSize, (t.y + 0.15f) * cellSize, 0)));
                mark.SetPosition(1, transform.TransformPoint(new Vector3((t.x + 0.85f) * cellSize, (t.y + 0.85f) * cellSize, 0)));
            }
    }

    /// Designing: shows what the design becomes (see DesignFrame).
    void BuildFrame()
    {
        var centre = new Vector3(width * cellSize / 2f, height * cellSize / 2f, 0);
        float r = Mathf.Min(width, height) / 2f * cellSize;
        if (frame == DesignFrame.Hoop)   // the wooden hoop round the fabric, with its screw at the top
        {
            Ring(centre, r + cellSize * 0.3f, cellSize * 0.45f, new Color(0.72f, 0.52f, 0.32f), 6);
            Ring(centre, r + cellSize * 0.3f, cellSize * 0.12f, new Color(0.86f, 0.68f, 0.45f), 7);
            var screw = MakeLine(new Color(0.55f, 0.55f, 0.58f), cellSize * 0.5f);
            screw.SetPosition(0, transform.TransformPoint(centre + Vector3.up * (r + cellSize * 0.4f)));
            screw.SetPosition(1, transform.TransformPoint(centre + Vector3.up * (r + cellSize * 1.1f)));
        }
        else if (frame == DesignFrame.Rounds)   // the rounds, spiralling out from the magic ring
            for (float rr = cellSize * 1.5f; rr <= r + 0.01f; rr += cellSize * 1.5f)
                Ring(centre, rr, cellSize * 0.06f, new Color(1, 1, 1, 0.3f), 6);
        else if (frame == DesignFrame.Knitted)   // each square is a knit stitch: a V
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    var v = MakeLine(new Color(1, 1, 1, 0.35f), cellSize * 0.07f);
                    v.positionCount = 3;
                    v.SetPosition(0, transform.TransformPoint(new Vector3((x + 0.2f) * cellSize, (y + 0.85f) * cellSize, 0)));
                    v.SetPosition(1, transform.TransformPoint(new Vector3((x + 0.5f) * cellSize, (y + 0.15f) * cellSize, 0)));
                    v.SetPosition(2, transform.TransformPoint(new Vector3((x + 0.8f) * cellSize, (y + 0.85f) * cellSize, 0)));
                }
    }

    void Ring(Vector3 centre, float radius, float w, Color color, int sorting)
    {
        var ring = MakeLine(color, w);
        ring.positionCount = 64;
        ring.loop = true;
        ring.sortingOrder = sorting;
        for (int i = 0; i < 64; i++)
        {
            float a = i * Mathf.PI * 2f / 64;
            ring.SetPosition(i, transform.TransformPoint(centre + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * radius));
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
            var tint = Color.Lerp(fabricColor, target.palette[t.colorIndex], t.halfStitch ? 0.15f : 0.35f);
            tint.a = 1f;
            return tint;
        }
        if (!Inside(x, y)) return Color.clear;   // outside the hoop or the piece
        if (t == null) return new Color(1, 1, 1, 0.15f);
        var c = target.palette[t.colorIndex];
        c.a = 0.4f;
        return c;
    }

    Vector3 CellCenter(int x, int y) => new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0);
    CraftTool needle;
    /// The needle in hand while stitching (see CraftTool).
    CraftTool Needle => needle ??= CraftTool.Make(transform, CraftTool.Kind.Needle, cellSize * 2.4f, material, 40);

    Vector3 CornerPos(Vector2Int c) => transform.TransformPoint(new Vector3(c.x * cellSize, c.y * cellSize, 0));

    // Works with mouse in the editor and a single touch on iPad (Unity maps touch to mouse
    // with the legacy Input Manager; for the new Input System, swap in Pointer.current).
    // A drag only counts if it started on the board itself: presses on a UI button (a floss colour,
    // Finish...) or anywhere else off the grid are ignored for their whole length. (Before, only the
    // first frame was ignored, so tapping a button painted the nearest edge square.)
    bool pressing;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressing = !OverUI() && OnBoard(WorldPoint());
            if (pressing) OnPress(WorldPoint());
        }
        else if (pressing && Input.GetMouseButton(0)) OnDrag(WorldPoint());
        else if (pressing && Input.GetMouseButtonUp(0)) { pressing = false; OnRelease(WorldPoint()); }
    }

    bool OnBoard(Vector3 local)
    {
        float margin = cellSize * 0.5f;   // a little slack around the edge for corner-to-corner stitches
        return local.x >= -margin && local.y >= -margin && local.x <= width * cellSize + margin && local.y <= height * cellSize + margin;
    }

    /// True when the pointer is over a UI element. Checks touches by finger id too, since on phones
    /// IsPointerOverGameObject() with no id only knows about the mouse.
    static bool OverUI()
    {
        var es = EventSystem.current;
        if (es == null) return false;
        if (es.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
            if (es.IsPointerOverGameObject(Input.GetTouch(i).fingerId)) return true;
        return false;
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
        Needle.MoveTo(CornerPos(dragStartCorner.Value), true);
        Needle.Poke();
        previewLine = MakeLine(Color.gray, 0.04f);
        previewLine.SetPosition(0, CornerPos(dragStartCorner.Value));
        previewLine.SetPosition(1, CornerPos(dragStartCorner.Value));
    }

    void OnDrag(Vector3 local)
    {
        if (mode == Mode.PatternFill) { if (OnBoard(local)) Paint(CellAt(local)); return; }
        if (previewLine != null)
            previewLine.SetPosition(1, transform.TransformPoint(local));
        if (mode == Mode.Stitch && needle != null) needle.MoveTo(transform.TransformPoint(local));
    }

    void OnRelease(Vector3 local)
    {
        if (mode == Mode.Stitch && dragStartCorner != null)
        {
            var a = dragStartCorner.Value;
            var b = NearestCorner(local);
            // A better tool forgives a drag that overshoots or falls short: it lands on the next corner diagonally.
            if (assist > 0 && !(Mathf.Abs(b.x - a.x) == 1 && Mathf.Abs(b.y - a.y) == 1))
            {
                var v = new Vector2(local.x / cellSize - a.x, local.y / cellSize - a.y);   // the drag, in squares
                float reach = 1.5f + assist;
                if (Mathf.Abs(v.x) >= 0.4f && Mathf.Abs(v.y) >= 0.4f && Mathf.Abs(v.x) <= reach && Mathf.Abs(v.y) <= reach)
                {
                    var c = new Vector2Int(a.x + (int)Mathf.Sign(v.x), a.y + (int)Mathf.Sign(v.y));
                    if (c.x >= 0 && c.y >= 0 && c.x <= width && c.y <= height) b = c;
                }
            }
            dragStartCorner = null;
            Needle.MoveTo(CornerPos(b));
            Needle.Poke();
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

    // Real cross stitch: either leg may go on top, as long as every X in the piece crosses the SAME way, so
    // it looks even (owner, 2026-10-31; was "/ under, \ over" only). Crosses that go against the rest are
    // mistakes; the more of them, the fewer points. The first leg made in each square is remembered here.
    readonly Dictionary<Vector2Int, int> firstLeg = new Dictionary<Vector2Int, int>();

    /// True when this square's X was made "\" first, so "/" lies on top (StitchMark.crossedWrong; the name is
    /// from when only one way was right).
    public bool CrossedWrong(Vector2Int cell) =>
        stitchProgress.TryGetValue(cell, out int done) && done == 3 && firstLeg.TryGetValue(cell, out int first) && first == 2;

    /// Finished crosses that lie the other way from most of the piece.
    public int WrongCrosses()
    {
        int flipped = 0, full = 0;
        foreach (var kv in stitchProgress) if (kv.Value == 3) { full++; if (CrossedWrong(kv.Key)) flipped++; }
        return Mathf.Min(flipped, full - flipped);
    }

    /// Which way most crosses lie: true when most have "/" on top. Ties keep the classic "\" on top.
    public static bool MostFlipped(List<StitchMark> marks)
    {
        int flipped = 0, full = 0;
        foreach (var m in marks) if (m.mask == 3) { full++; if (m.crossedWrong) flipped++; }
        return flipped * 2 > full;
    }

    void AddStitch(Vector2Int cell, int bit, int color)
    {
        stitchProgress.TryGetValue(cell, out int done);
        if ((done & bit) != 0) return; // already stitched that diagonal
        bool firstOfCell = done == 0;
        if (firstOfCell) firstLeg[cell] = bit;
        done |= bit;
        stitchProgress[cell] = done;
        stitchColor[cell] = color;

        bool slash = bit == 1;
        DrawThread(CornerPos(new Vector2Int(cell.x, slash ? cell.y : cell.y + 1)),
                   CornerPos(new Vector2Int(cell.x + 1, slash ? cell.y + 1 : cell.y)),
                   pattern.palette[color], firstOfCell ? 5 : 9);   // whichever leg comes second lies on top

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
        if (!Inside(cell.x, cell.y)) return;
        if (erase)
        {
            pattern.cells.RemoveAll(c => c.x == cell.x && c.y == cell.y);
            cellViews[cell].color = GuideColor(cell.x, cell.y);
            return;
        }
        // A pattern-only order copies its chart's half stitches; the designer paints them when Half is on.
        var chart = target?.cells.Find(c => c.x == cell.x && c.y == cell.y);
        PaintCell(cell, currentColor, chart != null ? chart.halfStitch : paintHalf);
    }

    void PaintCell(Vector2Int cell, int color, bool half = false)
    {
        pattern.cells.RemoveAll(c => c.x == cell.x && c.y == cell.y);
        cellViews[cell].color = half ? Color.Lerp(pattern.palette[color], Color.white, 0.55f) : pattern.palette[color];   // half: paler
        pattern.cells.Add(new StitchCell { x = cell.x, y = cell.y, colorIndex = color, halfStitch = half });
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

    /// A finished square counts when it's in the target and in the right thread colour (half if its X
    /// was crossed the wrong way).
    /// Squares finished outside the target count against the score.
    public static float Score(List<StitchMark> marks, Pattern target)
    {
        if (target.cells.Count == 0) return 1f;
        var wanted = new Dictionary<Vector2Int, int>();
        foreach (var t in target.cells) wanted[new Vector2Int(t.x, t.y)] = t.colorIndex;

        float correct = 0;
        int extra = 0;
        bool flipped = MostFlipped(marks);   // the piece's own way of crossing
        var halves = new HashSet<Vector2Int>();
        foreach (var t in target.cells) if (t.halfStitch) halves.Add(new Vector2Int(t.x, t.y));
        foreach (var m in marks)
        {
            var at = new Vector2Int(m.x, m.y);
            if (halves.Contains(at))   // a half stitch: just the "/" leg; crossed into a full X it's a mistake
            {
                if (wanted[at] == m.color) correct += m.mask == 1 ? 1f : m.mask == 3 ? 0.25f : 0f;
                continue;
            }
            if (m.mask != 3) continue;
            if (!wanted.TryGetValue(at, out int color)) extra++;
            else if (color == m.color) correct += m.crossedWrong != flipped ? 0.6f : 1f;   // against the rest: it shows
        }
        return correct / (wanted.Count + extra);
    }
}
