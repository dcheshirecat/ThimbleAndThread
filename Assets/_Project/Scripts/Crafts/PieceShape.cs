using System.Collections.Generic;
using UnityEngine;

/// The shape of a sewn piece, worked out from its pattern: the filled squares of the grid are the
/// piece, and squares in the pattern's second colour mark where the eyes go.
/// All points are in 0..1 of the area a step draws in.
public class PieceShape
{
    const float Padding = 0.1f;          // space around the piece
    const float OpeningShare = 0.12f;    // the gap left open for turning and stuffing, as a share of the outline
    const float SeamAllowance = 0.025f;  // how far inside the cut edge the seam runs

    public List<Vector2> Outline;        // closed, counter-clockwise, smoothed
    public List<Vector2> Seam;           // the outline minus the opening (an open line)
    public List<Vector2> Opening;        // the gap left open (an open line), at the bottom
    public List<Vector2> Eyes;           // where the eyes go

    public static PieceShape For(Pattern pattern)
    {
        var shape = new PieceShape();
        var filled = new HashSet<Vector2Int>();
        var eyes = new List<Vector2Int>();
        int w = 8, h = 8;
        if (pattern != null && pattern.cells.Count > 0)
        {
            w = pattern.width; h = pattern.height;
            foreach (var c in pattern.cells)
            {
                filled.Add(new Vector2Int(c.x, c.y));
                if (c.colorIndex == 1) eyes.Add(new Vector2Int(c.x, c.y));
            }
        }
        else   // an empty design: a plain round piece
            for (int x = 1; x < 7; x++) for (int y = 1; y < 7; y++)
                if ((new Vector2(x + 0.5f, y + 0.5f) - new Vector2(4, 4)).sqrMagnitude < 9.5f) filled.Add(new Vector2Int(x, y));

        float scale = (1 - 2 * Padding) / Mathf.Max(w, h);
        var offset = new Vector2((1 - w * scale) / 2f, (1 - h * scale) / 2f);
        Vector2 ToPiece(Vector2 g) => offset + g * scale;

        var loop = LongestLoop(filled);
        loop = Smooth(Simplify(loop), 2);
        shape.Outline = loop.ConvertAll(ToPiece);

        shape.Eyes = eyes.ConvertAll(c => ToPiece(new Vector2(c.x + 0.5f, c.y + 0.5f)));
        if (shape.Eyes.Count == 0)   // no eyes drawn on the pattern: two near the top
        {
            Bounds(shape.Outline, out var min, out var max);
            shape.Eyes.Add(new Vector2(Mathf.Lerp(min.x, max.x, 0.35f), Mathf.Lerp(min.y, max.y, 0.7f)));
            shape.Eyes.Add(new Vector2(Mathf.Lerp(min.x, max.x, 0.65f), Mathf.Lerp(min.y, max.y, 0.7f)));
        }
        shape.SplitOpening();
        return shape;
    }

    /// Puts the opening around the lowest point of the outline. Seam and opening run a little inside
    /// the cut edge, like a real seam allowance.
    void SplitOpening()
    {
        var ring = Resample(Outline, true, 200);
        ring.RemoveAt(ring.Count - 1);   // last point repeats the first
        ring = Inset(ring, SeamAllowance);
        int lowest = 0;
        for (int i = 1; i < ring.Count; i++) if (ring[i].y < ring[lowest].y) lowest = i;
        int half = Mathf.RoundToInt(ring.Count * OpeningShare / 2f);
        Opening = new List<Vector2>();
        Seam = new List<Vector2>();
        for (int k = -half; k <= half; k++) Opening.Add(ring[(lowest + k + ring.Count) % ring.Count]);
        for (int k = half; k <= ring.Count - half; k++) Seam.Add(ring[(lowest + k) % ring.Count]);
    }

    // ---- Outline from grid squares ----

    /// The boundary of the filled squares, walked counter-clockwise; the longest loop if there are several.
    static List<Vector2> LongestLoop(HashSet<Vector2Int> filled)
    {
        var edges = new Dictionary<Vector2Int, List<Vector2Int>>();
        void Edge(Vector2Int a, Vector2Int b)
        {
            if (!edges.TryGetValue(a, out var list)) edges[a] = list = new List<Vector2Int>();
            list.Add(b);
        }
        foreach (var c in filled)
        {
            if (!filled.Contains(c + Vector2Int.down)) Edge(c, c + Vector2Int.right);
            if (!filled.Contains(c + Vector2Int.right)) Edge(c + Vector2Int.right, c + Vector2Int.one);
            if (!filled.Contains(c + Vector2Int.up)) Edge(c + Vector2Int.one, c + Vector2Int.up);
            if (!filled.Contains(c + Vector2Int.left)) Edge(c + Vector2Int.up, c);
        }
        var best = new List<Vector2>();
        while (edges.Count > 0)
        {
            var start = default(Vector2Int);
            foreach (var k in edges.Keys) { start = k; break; }
            var loop = new List<Vector2>();
            var at = start;
            while (edges.TryGetValue(at, out var outs))
            {
                loop.Add(at);
                var nextPoint = outs[outs.Count - 1];
                outs.RemoveAt(outs.Count - 1);
                if (outs.Count == 0) edges.Remove(at);
                at = nextPoint;
                if (at == start) break;
            }
            if (loop.Count > best.Count) best = loop;
        }
        return best;
    }

    /// Drops points in the middle of straight runs.
    static List<Vector2> Simplify(List<Vector2> loop)
    {
        var result = new List<Vector2>();
        for (int i = 0; i < loop.Count; i++)
        {
            var prev = loop[(i - 1 + loop.Count) % loop.Count];
            var next = loop[(i + 1) % loop.Count];
            var a = loop[i] - prev; var b = next - loop[i];
            if (Mathf.Abs(a.x * b.y - a.y * b.x) > 1e-4f) result.Add(loop[i]);
        }
        return result.Count >= 3 ? result : loop;
    }

    /// Rounds the corners (Chaikin's corner cutting).
    static List<Vector2> Smooth(List<Vector2> loop, int passes)
    {
        for (int p = 0; p < passes; p++)
        {
            var next = new List<Vector2>();
            for (int i = 0; i < loop.Count; i++)
            {
                var a = loop[i]; var b = loop[(i + 1) % loop.Count];
                next.Add(Vector2.Lerp(a, b, 0.25f));
                next.Add(Vector2.Lerp(a, b, 0.75f));
            }
            loop = next;
        }
        return loop;
    }

    /// Moves each point of a counter-clockwise loop inwards by "distance".
    static List<Vector2> Inset(List<Vector2> loop, float distance)
    {
        var result = new List<Vector2>();
        for (int i = 0; i < loop.Count; i++)
        {
            var along = (loop[(i + 1) % loop.Count] - loop[(i - 1 + loop.Count) % loop.Count]).normalized;
            result.Add(loop[i] + new Vector2(-along.y, along.x) * distance);   // left of the direction = inside
        }
        return result;
    }

    // ---- Shared geometry ----

    public static void Bounds(List<Vector2> points, out Vector2 min, out Vector2 max)
    {
        min = Vector2.one * float.MaxValue; max = Vector2.one * float.MinValue;
        foreach (var p in points) { min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
    }

    public static bool Inside(List<Vector2> polygon, Vector2 p)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            var a = polygon[i]; var b = polygon[j];
            if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
        }
        return inside;
    }

    public static float DistanceToLine(List<Vector2> line, Vector2 p)
    {
        float best = float.MaxValue;
        for (int i = 0; i + 1 < line.Count; i++) best = Mathf.Min(best, DistanceToSegment(p, line[i], line[i + 1]));
        return best;
    }

    public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = ab.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
        return Vector2.Distance(p, a + ab * t);
    }

    /// Evenly spaced points along a line through the given corners (closed: back to the first).
    public static List<Vector2> Resample(List<Vector2> corners, bool closed, int count)
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

    /// A filled mesh of a simple polygon (ear clipping), for drawing the piece.
    public static Mesh FillMesh(List<Vector2> polygon, Color color)
    {
        var idx = new List<int>();
        for (int i = 0; i < polygon.Count; i++) idx.Add(i);
        var tris = new List<int>();
        int guard = polygon.Count * polygon.Count;
        while (idx.Count > 3 && guard-- > 0)
        {
            for (int k = 0; k < idx.Count; k++)
            {
                int i0 = idx[(k - 1 + idx.Count) % idx.Count], i1 = idx[k], i2 = idx[(k + 1) % idx.Count];
                var a = polygon[i0]; var b = polygon[i1]; var c = polygon[i2];
                if ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) <= 0) continue;   // not convex here
                bool empty = true;
                foreach (int j in idx)
                    if (j != i0 && j != i1 && j != i2 && InTriangle(polygon[j], a, b, c)) { empty = false; break; }
                if (!empty) continue;
                tris.AddRange(new[] { i0, i1, i2 });
                idx.RemoveAt(k);
                break;
            }
        }
        if (idx.Count == 3) tris.AddRange(idx);
        var mesh = new Mesh();
        mesh.SetVertices(polygon.ConvertAll(p => (Vector3)p));
        mesh.SetTriangles(tris, 0);
        var colors = new Color[polygon.Count];
        for (int i = 0; i < colors.Length; i++) colors[i] = color;
        mesh.colors = colors;
        return mesh;
    }

    static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(p, a, b), d2 = Cross(p, b, c), d3 = Cross(p, c, a);
        bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(neg && pos);
    }

    static float Cross(Vector2 p, Vector2 a, Vector2 b) => (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);
}
