using System.Collections.Generic;
using UnityEngine;

/// Knitting recipe step: knit the piece, one swipe per stitch. Swipe down for a knit stitch, up for
/// a purl; the faint marks show which is due (purls make the pattern's texture). A steady rhythm
/// makes even stitches. A wrong swipe makes the wrong stitch (a purl where a knit was due): it shows in
/// the fabric, and the only fix is to undo back to it, as knitters "tink" back.
/// Garments are knitted piece by piece (KnitLayout): ribbed edges alternate knit and purl, and colourwork
/// stitches are knitted in the pattern's colour (the next stitch's marker shows which yarn it takes).
/// Each finished stitch is saved as a point whose x is its evenness (0..1) and y is 1 for a wrong stitch.
public class KnitRowsStep : PieceStep, IMistakes
{
    const float MinSwipe = 0.04f;       // shorter drags aren't swipes

    readonly List<GameObject> stitches = new List<GameObject>();
    readonly List<GameObject> guide = new List<GameObject>();
    SpriteRenderer current;
    Vector2 pressAt;
    float lastStitchTime = -1f, rhythm = -1f;   // seconds between stitches, a running average

    int Done => State.points.Count;

    protected override void Build()
    {
        for (int i = 0; i < Knit.Stitches; i++) Square(Knit.CellCenter(i), Knit.cell, new Color(1, 1, 1, 0.08f), 0);   // the piece's shape
        current = Square(Vector2.zero, Knit.cell, new Color(1, 1, 1, 0.35f), 1);
        for (int i = 0; i < Done; i++) DrawStitch(i);
        ShowNext();
    }

    void DrawStitch(int i)
    {
        float evenness = State.points[i].x;
        // Uneven stitches sit a little off; the offset is fixed per stitch so it redraws the same.
        var wobble = new Vector2(Mathf.Sin(i * 12.9898f), Mathf.Cos(i * 78.233f)) * (1 - evenness) * Knit.cell * 0.25f;
        var stitch = new GameObject($"Stitch_{i}");
        stitch.transform.SetParent(transform, false);
        Rect(Knit.CellCenter(i), Vector2.one * Knit.cell * 1.02f, area.fabricColor, 1).transform.SetParent(stitch.transform, true);
        bool wrong = State.points[i].y > 0.5f;
        Stitch(Knit.CellCenter(i) + wobble, Knit.cell, Knit.Purl(i) != wrong, Knit.ColorOf(i, Yarn), 2).transform.SetParent(stitch.transform, true);
        stitches.Add(stitch);
    }

    /// Highlights the next stitch and shows faint marks for the rest of its row.
    void ShowNext()
    {
        foreach (var g in guide) Destroy(g);
        guide.Clear();
        current.enabled = Done < Knit.Stitches;
        if (!current.enabled) { Tool(CraftTool.Kind.KnittingNeedles, Knit.cell * 1.6f).Hide(); return; }
        current.transform.position = World(Knit.CellCenter(Done));
        Tool(CraftTool.Kind.KnittingNeedles, Knit.cell * 1.6f).MoveTo(World(Knit.CellCenter(Done)));
        // The marker takes the colour of the yarn the next stitch needs (colourwork changes yarn).
        var needs = Knit.ColorOf(Done, Color.white);
        current.color = new Color(needs.r, needs.g, needs.b, 0.55f);
        var row = Knit.stitches[Done];
        for (int i = Done; i < Knit.Stitches && Knit.stitches[i].row == row.row && Knit.stitches[i].part == row.part; i++)
        {
            var c = Knit.ColorOf(i, Color.white);
            guide.Add(Stitch(Knit.CellCenter(i), Knit.cell, Knit.Purl(i), new Color(c.r, c.g, c.b, 0.5f), 2));
        }
    }

    protected override void OnPress(Vector2 p) => pressAt = p;

    protected override void OnRelease(Vector2 p)
    {
        var d = p - pressAt;
        if (Done >= Knit.Stitches || d.magnitude < MinSwipe || Mathf.Abs(d.y) < Mathf.Abs(d.x)) return;
        bool purl = d.y > 0;
        bool wrong = purl != Knit.Purl(Done);

        float now = Time.realtimeSinceStartup, evenness = 1f;
        if (lastStitchTime >= 0)
        {
            float gap = now - lastStitchTime;
            if (rhythm > 0.08f) evenness = 1f - 0.6f * (1f - 0.7f * Assist) * Mathf.Clamp01(Mathf.Abs(gap - rhythm) / rhythm);   // smooth needles forgive
            rhythm = rhythm < 0 ? gap : Mathf.Lerp(rhythm, gap, 0.3f);
        }
        lastStitchTime = now;
        State.points.Add(new Vector2(evenness, wrong ? 1 : 0));
        if (wrong) AudioManager.Play(Sfx.Click);
        DrawStitch(Done - 1);
        Tool(CraftTool.Kind.KnittingNeedles).Poke();
        ShowNext();
    }

    public override void Undo()
    {
        if (Done == 0) return;
        State.points.RemoveAt(Done - 1);
        Destroy(stitches[stitches.Count - 1]);
        stitches.RemoveAt(stitches.Count - 1);
        ShowNext();
    }

    public string MistakesLeft
    {
        get
        {
            int n = 0;
            foreach (var p in State.points) if (p.y > 0.5f) n++;
            return n == 0 ? null : n == 1 ? "1 wrong stitch" : $"{n} wrong stitches";
        }
    }

    /// Average evenness of the stitches, lowered by wrong stitches (and by mistakes counted in older saves).
    public override float SaveWork()
    {
        State.progress = Done / (float)Knit.Stitches;
        if (Done == 0) return 0f;
        float sum = 0f;
        foreach (var p in State.points) sum += p.y > 0.5f ? p.x * 0.2f : p.x;   // a wrong stitch spoils the texture
        float mistakes = Mathf.Clamp01(State.mistakes / (float)Knit.Stitches);
        return State.progress * (sum / Done) * (1f - 0.5f * mistakes);
    }
}
