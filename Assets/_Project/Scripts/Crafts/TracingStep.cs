using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// A step where the player drags along a dashed line on a piece of fabric: hemming, sewing a seam,
/// cutting along a pattern line. Subclasses give the line; this class draws the piece, records the
/// strokes (saved in the order), and scores how much of the line was followed and how closely.
/// Points are kept in 0..1 of the piece, so saved work redraws the same on any screen.
public abstract class TracingStep : MonoBehaviour, ICraftStep
{
    /// The dashed line to follow, in 0..1 of the piece.
    protected abstract List<Vector2> Guide();
    protected virtual bool ClosedGuide => false;
    protected virtual Color ThreadColor => new Color(0.30f, 0.25f, 0.30f);

    const float Tolerance = 0.04f;     // how far from the line still counts, as a share of the piece's side
    const int Samples = 160;           // checkpoints along the line

    Order order;
    int stepIndex;
    StepArea area;
    Vector2 origin;                    // world position of the piece's bottom-left corner
    float side;                        // world size of the piece

    readonly List<Vector2> points = new List<Vector2>();
    readonly List<int> strokeStarts = new List<int>();
    readonly List<LineRenderer> strokeLines = new List<LineRenderer>();
    List<Vector2> samples;
    bool[] covered;
    bool drawing;

    /// How much of the line has been followed (0..1).
    public float Coverage { get; private set; }

    public void Begin(Order forOrder, int index, StepArea stepArea)
    {
        order = forOrder;
        stepIndex = index;
        area = stepArea;
        side = area.size * 0.9f;
        origin = area.center - Vector2.one * side / 2f;

        var fabric = new GameObject("Fabric").AddComponent<SpriteRenderer>();
        fabric.transform.SetParent(transform, false);
        fabric.transform.position = area.center;
        fabric.transform.localScale = Vector3.one * side;
        fabric.sprite = area.square;
        fabric.sharedMaterial = area.material;
        fabric.color = area.fabricColor;

        samples = Resample(Guide(), ClosedGuide, Samples);
        covered = new bool[samples.Count];
        for (int i = 0; i + 1 < samples.Count; i += 2)   // dashes: every other gap between checkpoints
            Line(new[] { World(samples[i]), World(samples[i + 1]) }, new Color(1f, 1f, 1f, 0.8f), side * 0.008f, 1);

        var saved = order.StepState(stepIndex);
        points.AddRange(saved.points);
        strokeStarts.AddRange(saved.strokeStarts);
        Redraw();
    }

    public float SaveWork()
    {
        var state = order.StepState(stepIndex);
        state.points = new List<Vector2>(points);
        state.strokeStarts = new List<int>(strokeStarts);
        state.progress = Coverage;
        return Score();
    }

    /// Followed share of the line, lowered by stitches that wander off it.
    public float Score()
    {
        if (points.Count == 0) return 0f;
        int onLine = 0;
        foreach (var p in points) if (DistanceToGuide(p) <= Tolerance * 1.5f) onLine++;
        return Coverage * onLine / points.Count;
    }

    /// Removes the last stroke.
    public void Undo()
    {
        if (strokeStarts.Count == 0) return;
        int from = strokeStarts[strokeStarts.Count - 1];
        strokeStarts.RemoveAt(strokeStarts.Count - 1);
        points.RemoveRange(from, points.Count - from);
        Redraw();
    }

    // ---- Input (mouse in the editor, one finger on a phone) ----

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.GetMouseButtonDown(0)) OnPress(Normalized(Input.mousePosition));
        else if (Input.GetMouseButton(0) && drawing) OnDrag(Normalized(Input.mousePosition));
        else if (Input.GetMouseButtonUp(0)) drawing = false;
    }

    Vector2 Normalized(Vector3 screen)
    {
        Vector2 w = area.cam.ScreenToWorldPoint(screen);
        return (w - origin) / side;
    }

    Vector3 World(Vector2 n) => origin + n * side;

    void OnPress(Vector2 n)
    {
        drawing = true;
        strokeStarts.Add(points.Count);
        strokeLines.Add(Line(new Vector3[0], ThreadColor, side * 0.012f, 2));
        AddPoint(n);
    }

    void OnDrag(Vector2 n)
    {
        if (points.Count > 0 && (points[points.Count - 1] - n).sqrMagnitude < 0.005f * 0.005f) return;
        AddPoint(n);
    }

    void AddPoint(Vector2 n)
    {
        points.Add(n);
        Cover(n);
        var line = strokeLines[strokeLines.Count - 1];
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, World(n));
    }

    void Cover(Vector2 n)
    {
        for (int i = 0; i < samples.Count; i++)
            if (!covered[i] && (samples[i] - n).sqrMagnitude <= Tolerance * Tolerance) covered[i] = true;
        int c = 0;
        foreach (bool b in covered) if (b) c++;
        Coverage = (float)c / covered.Length;
    }

    void Redraw()
    {
        foreach (var l in strokeLines) if (l) Destroy(l.gameObject);
        strokeLines.Clear();
        System.Array.Clear(covered, 0, covered.Length);
        Coverage = 0f;
        for (int s = 0; s < strokeStarts.Count; s++)
        {
            int from = strokeStarts[s], to = s + 1 < strokeStarts.Count ? strokeStarts[s + 1] : points.Count;
            var line = Line(new Vector3[0], ThreadColor, side * 0.012f, 2);
            strokeLines.Add(line);
            line.positionCount = to - from;
            for (int i = from; i < to; i++) { line.SetPosition(i - from, World(points[i])); Cover(points[i]); }
        }
    }

    LineRenderer Line(Vector3[] positions, Color color, float width, int sorting)
    {
        var go = new GameObject("Line");
        go.transform.SetParent(transform, false);
        var l = go.AddComponent<LineRenderer>();
        l.sharedMaterial = area.material;
        l.useWorldSpace = true;
        l.startColor = l.endColor = color;
        l.startWidth = l.endWidth = width;
        l.numCapVertices = 4;
        l.numCornerVertices = 2;
        l.sortingOrder = sorting;
        l.positionCount = positions.Length;
        l.SetPositions(positions);
        return l;
    }

    // ---- Geometry ----

    float DistanceToGuide(Vector2 p)
    {
        float best = float.MaxValue;
        for (int i = 0; i + 1 < samples.Count; i++) best = Mathf.Min(best, DistanceToSegment(p, samples[i], samples[i + 1]));
        return best;
    }

    static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = ab.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
        return Vector2.Distance(p, a + ab * t);
    }

    /// Evenly spaced points along a line through the given corners.
    static List<Vector2> Resample(List<Vector2> corners, bool closed, int count)
    {
        var path = new List<Vector2>(corners);
        if (closed && path.Count > 0) path.Add(path[0]);
        float length = 0f;
        for (int i = 0; i + 1 < path.Count; i++) length += Vector2.Distance(path[i], path[i + 1]);
        var result = new List<Vector2>();
        if (path.Count < 2 || length <= 0) return result;
        float stepLength = length / (count - 1);
        int seg = 0;
        float segStart = 0f;
        for (int k = 0; k < count; k++)
        {
            float d = Mathf.Min(k * stepLength, length);
            while (seg < path.Count - 2 && segStart + Vector2.Distance(path[seg], path[seg + 1]) < d)
            {
                segStart += Vector2.Distance(path[seg], path[seg + 1]);
                seg++;
            }
            float segLength = Vector2.Distance(path[seg], path[seg + 1]);
            result.Add(Vector2.Lerp(path[seg], path[seg + 1], segLength > 0 ? (d - segStart) / segLength : 0f));
        }
        return result;
    }
}
