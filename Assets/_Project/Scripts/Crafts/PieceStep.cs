using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

static class LoopExtensions
{
    /// A closed loop as a line: the points with the first one repeated at the end.
    public static List<Vector2> Closed(this List<Vector2> loop) => new List<Vector2>(loop) { loop[0] };
}

/// Base for steps played on the piece in the step area: maps touches to 0..1 of the area,
/// gives the piece's shape (from the order's pattern) and small drawing helpers.
/// Subclasses draw in Build() and react in OnPress / OnDrag / OnRelease.
public abstract class PieceStep : MonoBehaviour, ICraftStep
{
    protected Order order;
    protected int stepIndex;
    protected StepArea area;
    Vector2 origin;                    // world position of the area's bottom-left corner
    float side;                        // world size of the area
    PieceShape shape;
    bool pressing;

    protected StepProgress State => order.StepState(stepIndex);
    protected PieceShape Shape => shape ??= PieceShape.For(order.target, ShapePadding);
    /// Space around the piece's shape; embroidery leaves more, for the hoop.
    protected virtual float ShapePadding => 0.1f;
    protected float Side => side;

    public void Begin(Order forOrder, int index, StepArea stepArea)
    {
        order = forOrder;
        stepIndex = index;
        area = stepArea;
        side = area.size;
        origin = area.center - Vector2.one * side / 2f;
        Build();
    }

    /// The saved points of an earlier step of this order (e.g. where the eyes went), or an empty list.
    protected List<Vector2> PointsFrom(StepKind kind)
    {
        int i = System.Array.FindIndex(order.Steps, s => s.kind == kind);
        return i >= 0 && i < order.steps.Count ? order.steps[i].points : new List<Vector2>();
    }

    protected abstract void Build();
    public abstract float SaveWork();
    public virtual void Undo() { }
    public virtual string FinishBlocker => null;

    protected virtual void OnPress(Vector2 p) { }
    protected virtual void OnDrag(Vector2 p) { }
    protected virtual void OnRelease(Vector2 p) { }

    // ---- Input (mouse in the editor, one finger on a phone) ----

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Presses that start on a UI button (e.g. Finish) aren't part of the step.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            pressing = true;
            OnPress(Normalized(Input.mousePosition));
        }
        else if (pressing && Input.GetMouseButton(0)) OnDrag(Normalized(Input.mousePosition));
        else if (pressing && Input.GetMouseButtonUp(0)) { pressing = false; OnRelease(Normalized(Input.mousePosition)); }
    }

    Vector2 Normalized(Vector3 screen)
    {
        Vector2 w = area.cam.ScreenToWorldPoint(screen);
        return (w - origin) / side;
    }

    protected Vector3 World(Vector2 p) => origin + p * side;

    // ---- Drawing ----

    /// The piece's shape, filled with a colour.
    protected MeshRenderer Piece(List<Vector2> polygon, Color color, int sorting = 0, Vector2 offset = default)
    {
        var go = new GameObject("Piece", typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.transform.position = World(offset);
        go.transform.localScale = Vector3.one * side;
        go.GetComponent<MeshFilter>().mesh = PieceShape.FillMesh(polygon, color);
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = area.material;
        r.sortingOrder = sorting;
        return r;
    }

    /// Bags: the front panel over the back one, with the side seam and the hem shown once they're sewn.
    protected void DrawBag(bool sidesSewn, bool hemmed)
    {
        var stitch = Color.Lerp(area.fabricColor, Color.black, 0.35f);
        Piece(Shape.Outline, Color.Lerp(area.fabricColor, Color.black, 0.2f), 0, new Vector2(0.012f, -0.012f));
        Piece(Shape.Outline, area.fabricColor, 0);
        if (sidesSewn) Line(Shape.Sides, stitch, 0.006f, 1);
        if (hemmed) Line(Shape.Mouth, stitch, 0.006f, 1);
    }

    /// Embroidery: the hoop with its circle of fabric, the printed outline, and the work done so far.
    protected void DrawHoop(bool outlined, bool filled)
    {
        var circle = Circle(new Vector2(0.5f, 0.5f), HoopRadius, 48);
        Piece(circle, area.fabricColor, 0);
        Line(Shape.Outline.Closed(), new Color(0.55f, 0.6f, 0.75f, 0.6f), 0.004f, 1);   // printed on the fabric
        if (filled) Piece(Shape.Outline, Floss(0), 1);
        if (outlined) Line(Shape.Outline.Closed(), Color.Lerp(Floss(0), Color.black, 0.3f), 0.01f, 2);
        Line(circle.Closed(), new Color(0.72f, 0.55f, 0.35f), 0.035f, 5);   // the wooden hoop
    }

    protected const float HoopRadius = 0.46f;

    /// The colour of the pattern's thread number i (the last one if there are fewer).
    protected Color Floss(int i)
    {
        var palette = order.target != null ? order.target.palette : null;
        if (palette == null || palette.Count == 0) return new Color(0.2f, 0.3f, 0.55f);
        var c = palette[Mathf.Clamp(i, 0, palette.Count - 1)];
        c.a = 1f;
        return c;
    }

    protected static List<Vector2> Circle(Vector2 center, float radius, int points)
    {
        var list = new List<Vector2>();
        for (int i = 0; i < points; i++)
        {
            float a = i / (float)points * Mathf.PI * 2f;
            list.Add(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius);
        }
        return list;
    }

    /// Recolours a piece drawn with Piece().
    protected static void Recolor(MeshRenderer piece, Color color)
    {
        var mesh = piece.GetComponent<MeshFilter>().mesh;
        var colors = new Color[mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++) colors[i] = PieceShape.VertexColor(color);
        mesh.colors = colors;
    }

    /// A square of colour (a swatch, a dot of stuffing, an eye...), centred on p, "size" in 0..1 of the area.
    protected SpriteRenderer Square(Vector2 p, float size, Color color, int sorting)
    {
        var sr = new GameObject("Square").AddComponent<SpriteRenderer>();
        sr.transform.SetParent(transform, false);
        sr.transform.position = World(p);
        sr.transform.localScale = Vector3.one * size * side;
        sr.sprite = area.square;
        sr.sharedMaterial = area.material;
        sr.color = color;
        sr.sortingOrder = sorting;
        return sr;
    }

    /// A rectangle of colour centred on p, "size" in 0..1 of the area.
    protected SpriteRenderer Rect(Vector2 p, Vector2 size, Color color, int sorting)
    {
        var sr = Square(p, 1f, color, sorting);
        sr.transform.localScale = new Vector3(size.x * side, size.y * side, 1);
        return sr;
    }

    /// One knitted stitch in a cell: a knit stitch is a "V", a purl a little bump across.
    protected GameObject Stitch(Vector2 c, float cell, bool purl, Color color, int sorting)
    {
        var line = purl
            ? Line(new[] { c + new Vector2(-0.3f, 0) * cell, c + new Vector2(0.3f, 0) * cell }, color, cell * 0.22f, sorting)
            : Line(new[] { c + new Vector2(-0.3f, 0.3f) * cell, c + new Vector2(0, -0.3f) * cell, c + new Vector2(0.3f, 0.3f) * cell }, color, cell * 0.14f, sorting);
        return line.gameObject;
    }

    /// A line through points in 0..1 of the area; "width" is in 0..1 of the area too.
    protected LineRenderer Line(IList<Vector2> points, Color color, float width, int sorting)
    {
        var go = new GameObject("Line");
        go.transform.SetParent(transform, false);
        var l = go.AddComponent<LineRenderer>();
        l.sharedMaterial = area.material;
        l.useWorldSpace = true;
        l.startColor = l.endColor = color;
        l.startWidth = l.endWidth = width * side;
        l.numCapVertices = 4;
        l.numCornerVertices = 2;
        l.sortingOrder = sorting;
        l.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) l.SetPosition(i, World(points[i]));
        return l;
    }

    /// Adds a point to a line drawn with Line().
    protected void Extend(LineRenderer line, Vector2 p)
    {
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, World(p));
    }

    /// Short dashes along a line, to show where to cut or sew.
    protected void Dashes(List<Vector2> points, Color color, float width, int sorting)
    {
        for (int i = 0; i + 1 < points.Count; i += 2) Line(new[] { points[i], points[i + 1] }, color, width, sorting);
    }

    /// Text in the step area, centred on p (e.g. fabric names under their swatches).
    protected TextMesh Label(Vector2 p, string text, float height, Color color, int sorting)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject("Label", typeof(MeshRenderer), typeof(TextMesh));
        go.transform.SetParent(transform, false);
        go.transform.position = World(p);
        var t = go.GetComponent<TextMesh>();
        t.font = font;
        t.text = text;
        t.fontSize = 64;
        t.characterSize = height * side / 64f * 10f;
        t.anchor = TextAnchor.MiddleCenter;
        t.alignment = TextAlignment.Center;
        t.color = color;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = font.material;
        r.sortingOrder = sorting;
        return t;
    }
}
