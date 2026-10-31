using System.Collections.Generic;
using UnityEngine;

/// A step sewn on the sewing machine. The needle runs along the seam on its own while the finger is
/// down (the foot pedal): slide up to go faster, down to go slower. Straight stretches can go at full
/// speed, but corners and curves need slowing down; take them too fast and the fabric skids, so the
/// stitches swing off the line and settle back only once the speed drops. Each press is one stroke
/// (Undo unpicks it); the work is saved and scored like any traced line.
public abstract class MachineSewingStep : TracingStep
{
    const float MaxSpeed = 0.42f;        // share of the area per second at full pedal
    const float CornerSpeed = 0.1f;      // safe speed through the sharpest corner
    const float SharpTurn = 70f;         // degrees of turn (over a short stretch) that count as the sharpest
    const float Skid = 3.5f;             // how fast stitches swing off the line per unit of excess speed
    const float Settle = 3f;             // how fast they come back once slow enough
    const float StitchGap = 0.007f;      // a new stitch point every this far

    float along;                         // needle position, in samples along the line (fractional)
    float skid;                          // how far off the line the fabric has skidded (signed, along the normal)
    float pedal;                         // 0..1
    float pressY;
    bool running;
    float[] safeSpeed;                   // per sample
    Vector2 lastPoint;
    int sinceSound;

    GameObject needle;
    Vector3 needleBase;
    Vector2 needleStart;
    TextMesh gauge;

    protected override float StitchLength => 0.024f;

    protected override void Build()
    {
        base.Build();
        var s = GuidePoints;
        safeSpeed = new float[s.Count];
        for (int i = 0; i < s.Count; i++)
        {
            float turn = Mathf.Abs(Turn(i));
            safeSpeed[i] = Mathf.Lerp(MaxSpeed * 1.05f, CornerSpeed * (1f + 1.5f * Assist), Mathf.Clamp01(turn / SharpTurn));   // a better machine holds corners
        }
        along = ResumePoint();
        needleStart = s.Count > 0 ? s[0] : Vector2.one * 0.5f;
        needle = Dot(needleStart, 0.035f, new Color(0.75f, 0.75f, 0.8f), 6, shine: true);
        needleBase = needle.transform.position;
        gauge = Label(new Vector2(0.5f, 0.035f), "", 0.035f, Color.white, 7);
        MoveNeedle(Position(along, 0));
        ShowGauge();
    }

    /// Where the needle starts: just after the furthest stitch so far (the start for a new seam).
    float ResumePoint()
    {
        if (Points.Count == 0) return 0;
        var last = Points[Points.Count - 1];
        int best = 0;
        float bestD = float.MaxValue;
        for (int i = 0; i < GuidePoints.Count; i++)
        {
            float d = (GuidePoints[i] - last).sqrMagnitude;
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    /// Signed change of direction around sample i, in degrees (over a short stretch either side).
    float Turn(int i)
    {
        var s = GuidePoints;
        int a = Mathf.Max(0, i - 3), b = Mathf.Min(s.Count - 1, i + 3);
        if (b - a < 2) return 0;
        var d1 = s[i] - s[a];
        var d2 = s[b] - s[i];
        if (d1.sqrMagnitude < 1e-8f || d2.sqrMagnitude < 1e-8f) return 0;
        return Vector2.SignedAngle(d1, d2);
    }

    /// The point on the line at a fractional sample position, pushed sideways by the skid.
    Vector2 Position(float at, float sideways)
    {
        var s = GuidePoints;
        int i = Mathf.Clamp(Mathf.FloorToInt(at), 0, s.Count - 1);
        int j = Mathf.Min(i + 1, s.Count - 1);
        var p = Vector2.Lerp(s[i], s[j], at - i);
        var dir = (s[j] - s[i]).sqrMagnitude > 1e-8f ? (s[j] - s[i]).normalized : (i > 0 ? (s[i] - s[i - 1]).normalized : Vector2.right);
        return p + new Vector2(-dir.y, dir.x) * sideways;
    }

    float SampleSpacing => GuidePoints.Count > 1 ? (GuidePoints[1] - GuidePoints[0]).magnitude : 0.01f;
    bool AtEnd => along >= GuidePoints.Count - 1;

    protected override void OnPress(Vector2 p)
    {
        if (AtEnd) return;
        running = true;
        pressY = p.y;
        pedal = 0.4f;
        skid = 0;
        StartStroke();
        lastPoint = Position(along, 0);
        AddPoint(lastPoint);
    }

    protected override void OnDrag(Vector2 p)
    {
        if (running) pedal = Mathf.Clamp01(0.4f + (p.y - pressY) * 2.5f);   // a quarter of the area up = full speed
    }

    protected override void OnRelease(Vector2 p)
    {
        running = false;
        pedal = 0;
        ShowGauge();
    }

    protected override void Tick(float dt)
    {
        if (!running || GuidePoints == null || GuidePoints.Count < 2) return;
        if (AtEnd) { running = false; ShowGauge(); return; }

        float speed = pedal * MaxSpeed;
        float safe = safeSpeed[Mathf.Clamp(Mathf.RoundToInt(along), 0, safeSpeed.Length - 1)];
        if (speed > safe)
        {
            // Too fast for this bend: the fabric skids outward, away from the turn.
            float outward = -Mathf.Sign(Turn(Mathf.RoundToInt(along)));
            skid += outward * (speed - safe) * Skid * (1f - 0.5f * Assist) * dt;
        }
        else skid = Mathf.MoveTowards(skid, 0, Settle * Mathf.Abs(skid) * dt + 0.01f * dt);
        skid = Mathf.Clamp(skid, -0.12f, 0.12f);

        along = Mathf.Min(GuidePoints.Count - 1, along + speed * dt / SampleSpacing);
        var p = Position(along, skid);
        MoveNeedle(p);
        if ((p - lastPoint).sqrMagnitude >= StitchGap * StitchGap)
        {
            AddPoint(p);
            lastPoint = p;
            if (++sinceSound % 4 == 0) AudioManager.Play(Sfx.Stitch);
        }
        ShowGauge();
    }

    public override void Undo()
    {
        base.Undo();
        running = false;
        along = ResumePoint();
        MoveNeedle(Position(along, 0));
        ShowGauge();
    }

    void MoveNeedle(Vector2 p)
    {
        if (needle != null) needle.transform.position = needleBase + (World(p) - World(needleStart));
    }

    void ShowGauge()
    {
        if (gauge == null) return;
        if (AtEnd) { gauge.text = "Seam done!"; return; }
        if (!running) { gauge.text = "Hold to sew"; return; }
        int bars = Mathf.RoundToInt(pedal * 5);
        bool tooFast = pedal * MaxSpeed > safeSpeed[Mathf.Clamp(Mathf.RoundToInt(along), 0, safeSpeed.Length - 1)] + 0.01f;
        gauge.text = "Speed " + new string('|', bars) + new string('.', 5 - bars) + (tooFast ? "  Slow down!" : "");
    }
}
