using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    protected PieceShape Shape => shape ??= PieceShape.For(order.target);
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

    /// Recolours a piece drawn with Piece().
    protected static void Recolor(MeshRenderer piece, Color color)
    {
        var mesh = piece.GetComponent<MeshFilter>().mesh;
        var colors = new Color[mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++) colors[i] = color;
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
