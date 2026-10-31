using System.Collections.Generic;
using UnityEngine;

/// A step where the player drags along a dashed line: hemming, cutting along a pattern line,
/// sewing a seam, closing an opening. Subclasses give the line and draw the piece; this class
/// records the strokes (saved in the order) and scores how much of the line was followed and how closely.
public abstract class TracingStep : PieceStep, IMistakes
{
    /// The dashed line to follow, in 0..1 of the area.
    protected abstract List<Vector2> Guide();
    /// Several separate lines to follow (e.g. a sweater's two side seams). Default: just Guide().
    protected virtual List<List<Vector2>> Guides() => new List<List<Vector2>> { Guide() };
    protected virtual bool ClosedGuide => false;
    protected virtual Color ThreadColor => new Color(0.30f, 0.25f, 0.30f);
    protected virtual Color GuideColor => new Color(1f, 1f, 1f, 0.85f);
    /// Length of one stitch, in 0..1 of the area; 0 draws a continuous line (a cut, a strand of yarn).
    protected virtual float StitchLength => 0.028f;
    /// Whether a needle is shown making the stitches (not for cuts, yarn strands, or the machine).
    protected virtual bool HandNeedle => StitchLength > 0;
    /// A twisted, rope-like line (stem stitch): a lighter twist laid over each stitch.
    protected virtual bool Twisted => false;
    Vector2 lastPoke;

    /// Draws what's being worked on, under the line. Default: a square sheet of the fabric.
    protected virtual void DrawPiece() => Square(Vector2.one * 0.5f, 0.9f, area.fabricColor, 0);

    const float BaseTolerance = 0.035f;    // how far from the line still counts, as a share of the area
    float Tolerance => BaseTolerance * (1f + Assist);   // sharp scissors, good needles: more forgiving
    const int Samples = 160;           // checkpoints along the line

    readonly List<Vector2> points = new List<Vector2>();
    readonly List<int> strokeStarts = new List<int>();
    readonly List<LineRenderer[]> strokeLines = new List<LineRenderer[]>();   // shadow and thread for each stroke
    List<Vector2> samples;
    readonly List<List<Vector2>> segments = new List<List<Vector2>>();
    bool[] covered;

    /// How much of the line has been followed (0..1).
    public float Coverage { get; private set; }

    protected override void Build()
    {
        DrawPiece();
        var guides = Guides();
        samples = new List<Vector2>();
        foreach (var g in guides)
        {
            var seg = PieceShape.Resample(g, ClosedGuide, Samples / guides.Count);
            segments.Add(seg);
            samples.AddRange(seg);
            Line(seg, new Color(0, 0, 0, 0.28f), 0.014f, 1);   // a soft shadow, so the line shows on any fabric
            Dashes(seg, GuideColor, 0.009f, 2);
        }
        covered = new bool[samples.Count];
        points.AddRange(State.points);
        strokeStarts.AddRange(State.strokeStarts);
        Redraw();
    }

    public override float SaveWork()
    {
        State.points = new List<Vector2>(points);
        State.strokeStarts = new List<int>(strokeStarts);
        State.progress = Coverage;
        return Score();
    }

    /// Followed share of the line, lowered by stitches that wander off it.
    public float Score()
    {
        if (points.Count == 0) return 0f;
        int onLine = 0;
        foreach (var p in points) if (Distance(p) <= Tolerance * 1.5f) onLine++;
        return Coverage * onLine / points.Count;
    }

    /// Strokes that mostly missed the line (undo them to take them out).
    public string MistakesLeft
    {
        get
        {
            int n = 0;
            for (int s = 0; s < strokeStarts.Count; s++)
            {
                int from = strokeStarts[s], to = s + 1 < strokeStarts.Count ? strokeStarts[s + 1] : points.Count, off = 0;
                for (int i = from; i < to; i++) if (Distance(points[i]) > Tolerance * 1.5f) off++;
                if (to > from && off > (to - from) * 0.4f) n++;
            }
            return n == 0 ? null : n == 1 ? $"1 {StrokeName} off the line" : $"{n} {StrokeName}s off the line";
        }
    }

    /// What one stroke is called in the warning ("stitch", "cut").
    protected virtual string StrokeName => StitchLength > 0 ? "row of stitches" : "stroke";

    float Distance(Vector2 p)
    {
        float best = float.MaxValue;
        foreach (var seg in segments) best = Mathf.Min(best, PieceShape.DistanceToLine(seg, p));
        return best;
    }

    /// Removes the last stroke.
    public override void Undo()
    {
        if (strokeStarts.Count == 0) return;
        int from = strokeStarts[strokeStarts.Count - 1];
        strokeStarts.RemoveAt(strokeStarts.Count - 1);
        points.RemoveRange(from, points.Count - from);
        Redraw();
    }

    protected override void OnPress(Vector2 p)
    {
        ShowLoupe(p);
        StartStroke();
        if (HandNeedle) { Tool(CraftTool.Kind.Needle).MoveTo(World(p), true); lastPoke = p; }
        AddPoint(p);
    }

    protected override void OnRelease(Vector2 p) => HideLoupe();

    // ---- The loupe: a magnifier above the finger, so the line and the thread under it stay visible
    //      (owner, 2026-10-31: backstitching was hard to see) ----

    /// Whether to show the magnifier while tracing (not on the machine, which guides itself).
    protected virtual bool UseLoupe => true;
    const float LoupeRadius = 0.13f, LoupeZoom = 2.4f;
    const int LoupeLayer = 30;           // the loupe itself, so its camera doesn't see it
    Camera loupeCam;
    GameObject loupe;
    RenderTexture loupeTexture;

    void ShowLoupe(Vector2 p)
    {
        if (!UseLoupe) return;
        if (loupe == null) MakeLoupe();
        // Above the finger; beside it near the top of the work.
        var at = p + new Vector2(0, 0.25f);
        if (at.y + LoupeRadius > 1.08f) at = p + new Vector2(p.x > 0.5f ? -0.27f : 0.27f, 0.05f);
        var look = World(p);
        loupeCam.transform.position = new Vector3(look.x, look.y, area.cam.transform.position.z);
        var spot = World(at);
        loupe.transform.position = new Vector3(spot.x, spot.y, 0);
        loupe.SetActive(true);
        loupeCam.enabled = true;
    }

    void HideLoupe()
    {
        if (loupe == null) return;
        loupe.SetActive(false);
        loupeCam.enabled = false;
    }

    void MakeLoupe()
    {
        loupeTexture = new RenderTexture(512, 512, 16);
        var camGo = new GameObject("LoupeCamera");
        camGo.transform.SetParent(transform, false);
        loupeCam = camGo.AddComponent<Camera>();
        loupeCam.orthographic = true;
        loupeCam.orthographicSize = LoupeRadius * Side / LoupeZoom;
        loupeCam.clearFlags = CameraClearFlags.SolidColor;
        loupeCam.backgroundColor = area.cam.backgroundColor;
        loupeCam.cullingMask = area.cam.cullingMask & ~(1 << LoupeLayer);
        loupeCam.targetTexture = loupeTexture;
        loupeCam.enabled = false;

        loupe = new GameObject("Loupe");
        loupe.transform.SetParent(transform, false);
        loupe.layer = LoupeLayer;
        // A round lens showing the camera's picture...
        float r = LoupeRadius * Side;
        const int n = 48;
        var verts = new Vector3[n + 1];
        var uvs = new Vector2[n + 1];
        var tris = new int[n * 3];
        verts[0] = Vector3.zero; uvs[0] = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < n; i++)
        {
            float a = i * Mathf.PI * 2 / n;
            verts[i + 1] = new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * r;
            uvs[i + 1] = new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f);
            tris[i * 3] = 0; tris[i * 3 + 1] = (i + 1) % n + 1; tris[i * 3 + 2] = i + 1;
        }
        var mesh = new Mesh { vertices = verts, uv = uvs, triangles = tris };
        loupe.AddComponent<MeshFilter>().mesh = mesh;
        var mr = loupe.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(area.material) { mainTexture = loupeTexture, color = Color.white };
        mr.sortingOrder = 60;
        // ...in a rim, with a small mark where the finger is.
        LoupeRing(r, r * 0.06f, new Color(0.35f, 0.3f, 0.35f), 61);
        LoupeRing(r * 0.06f, r * 0.02f, new Color(0.9f, 0.2f, 0.25f, 0.8f), 62);
        loupe.SetActive(false);
    }

    void LoupeRing(float radius, float width, Color color, int sorting)
    {
        var go = new GameObject("Rim") { layer = LoupeLayer };
        go.transform.SetParent(loupe.transform, false);
        var l = go.AddComponent<LineRenderer>();
        l.sharedMaterial = area.material;
        l.useWorldSpace = false;
        l.loop = true;
        l.startColor = l.endColor = color;
        l.startWidth = l.endWidth = width;
        l.sortingOrder = sorting;
        l.positionCount = 40;
        for (int i = 0; i < 40; i++) l.SetPosition(i, new Vector3(Mathf.Cos(i * Mathf.PI / 20), Mathf.Sin(i * Mathf.PI / 20)) * radius);
    }

    void OnDestroy()
    {
        if (loupeTexture != null) loupeTexture.Release();
    }

    /// The line resampled into evenly spaced checkpoints, in order along it.
    protected List<Vector2> GuidePoints => samples;
    /// Every traced point so far, in order.
    protected List<Vector2> Points => points;

    protected void StartStroke()
    {
        strokeStarts.Add(points.Count);
        strokeLines.Add(NewStroke());
    }

    protected override void OnDrag(Vector2 p)
    {
        ShowLoupe(p);
        if (points.Count > 0 && (points[points.Count - 1] - p).sqrMagnitude < 0.005f * 0.005f) return;
        AddPoint(p);
    }

    protected void AddPoint(Vector2 p)
    {
        points.Add(p);
        Cover(p);
        if (HandNeedle)
        {
            var needle = Tool(CraftTool.Kind.Needle);
            needle.MoveTo(World(p));
            if ((p - lastPoke).magnitude >= StitchLength * 2f) { needle.Poke(); lastPoke = p; }   // down and up: one stitch
        }
        foreach (var line in strokeLines[strokeLines.Count - 1]) Extend(line, p);
    }

    /// A stroke of thread: a soft shadow under it, so it shows on any fabric, and the thread itself.
    LineRenderer[] NewStroke()
    {
        var shadow = Line(new Vector2[0], new Color(0, 0, 0, 0.3f), 0.016f, 2);
        var thread = Line(new Vector2[0], ThreadColor, 0.011f, 3);
        if (StitchLength > 0) { MakeDashed(shadow, StitchLength); MakeDashed(thread, StitchLength); }
        if (!Twisted) return new[] { shadow, thread };
        var twist = Line(new Vector2[0], Color.Lerp(ThreadColor, Color.white, 0.35f), 0.005f, 4);
        MakeDashed(twist, StitchLength * 0.5f);
        return new[] { shadow, thread, twist };
    }

    void Cover(Vector2 p)
    {
        for (int i = 0; i < samples.Count; i++)
            if (!covered[i] && (samples[i] - p).sqrMagnitude <= Tolerance * Tolerance) covered[i] = true;
        int c = 0;
        foreach (bool b in covered) if (b) c++;
        Coverage = covered.Length > 0 ? (float)c / covered.Length : 0f;
    }

    void Redraw()
    {
        foreach (var stroke in strokeLines) foreach (var l in stroke) if (l) Destroy(l.gameObject);
        strokeLines.Clear();
        System.Array.Clear(covered, 0, covered.Length);
        Coverage = 0f;
        for (int s = 0; s < strokeStarts.Count; s++)
        {
            int from = strokeStarts[s], to = s + 1 < strokeStarts.Count ? strokeStarts[s + 1] : points.Count;
            var stroke = NewStroke();
            strokeLines.Add(stroke);
            for (int i = from; i < to; i++)
            {
                foreach (var line in stroke) Extend(line, points[i]);
                Cover(points[i]);
            }
        }
    }
}
