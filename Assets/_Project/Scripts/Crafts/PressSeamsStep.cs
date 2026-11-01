using System.Collections.Generic;
using UnityEngine;

/// Press the seams (owner, round 6: "press seams"). Before a sewn piece is turned or boxed, a sewist irons each seam
/// so it lies flat: the piece lies wrong side up and the iron runs along every seam line. The care label sets the
/// heat, as in PressStep: too cool and the seam springs back up (the stroke doesn't count), too hot and it leaves a
/// scorch on the seam (a mistake; Undo dabs the last one off).
/// Saved: the strokes (TracingStep), the heat in State.setting, scorches in State.marks.
public class PressSeamsStep : TracingStep, IMistakes
{
    static readonly string[] HeatNames = { "Cool", "Warm", "Hot" };
    int heat = 1, right;
    readonly List<SpriteRenderer> dial = new List<SpriteRenderer>();
    readonly List<GameObject> scorches = new List<GameObject>();
    string warning;
    int pointsAtPress = -1;   // how many points there were when this press began (-1: a dial press)

    protected override float StitchLength => 0f;   // the iron's path: a smooth pressed line
    protected override bool UseLoupe => false;
    protected override Color ThreadColor => new Color(1f, 1f, 1f, 0.45f);   // the sheen of a pressed seam
    protected override string StrokeName => "pass of the iron";

    bool Bag => order.itemId == "tote";

    protected override void DrawPiece()
    {
        var wrong = Color.Lerp(area.fabricColor, Color.black, 0.12f);   // wrong side up
        Piece(Shape.Outline, Color.Lerp(area.fabricColor, Color.black, 0.25f), -1, new Vector2(0.012f, -0.012f));
        Piece(Shape.Outline, wrong, 0);
        Line(Guide(), Color.Lerp(area.fabricColor, Color.black, 0.45f), 0.006f, 1);   // the stitched seam
    }

    protected override List<Vector2> Guide() => Bag ? Shape.Sides : Shape.Seam;

    protected override void Build()
    {
        right = PressStep.RightHeat(area.content != null ? area.content.Supply(order.fabricId) : null);
        heat = State.setting >= 0 ? State.setting : 1;
        base.Build();
        Label(new Vector2(0.27f, 0.975f), right == 0 ? "Care: COOL iron" : right == 1 ? "Care: WARM iron" : "Care: HOT iron", 0.03f, Color.white, 5);
        for (int h = 0; h < 3; h++)
        {
            dial.Add(Square(DialSpot(h), 0.06f, Color.white, 4));
            Label(DialSpot(h) + Vector2.down * 0.05f, HeatNames[h], 0.025f, Color.white, 4);
        }
        foreach (var m in State.marks) scorches.Add(Scorch(m));
        ShowDial();
    }

    static Vector2 DialSpot(int h) => new Vector2(0.68f + h * 0.11f, 0.97f);

    GameObject Scorch(Vector2 at) => Dot(at, 0.03f, new Color(0.45f, 0.30f, 0.15f, 0.85f), 4);

    void ShowDial()
    {
        var colours = new[] { new Color(0.55f, 0.75f, 0.95f), new Color(0.98f, 0.75f, 0.35f), new Color(0.95f, 0.35f, 0.25f) };
        for (int h = 0; h < 3; h++) dial[h].color = h == heat ? colours[h] : new Color(1, 1, 1, 0.3f);
    }

    public override string LiveHint => warning ?? $"Iron: {HeatNames[heat].ToLower()}. Run it along each seam.";

    protected override void OnPress(Vector2 p)
    {
        for (int h = 0; h < 3; h++)
            if (Vector2.Distance(p, DialSpot(h)) < 0.05f)
            {
                heat = State.setting = h;
                warning = null;
                ShowDial();
                pointsAtPress = -1;
                return;
            }
        pointsAtPress = Points.Count;
        base.OnPress(p);
    }

    protected override void OnRelease(Vector2 p)
    {
        base.OnRelease(p);
        bool ironed = pointsAtPress >= 0 && Points.Count > pointsAtPress;
        pointsAtPress = -1;
        if (!ironed) return;
        if (heat < right)   // not hot enough: the seam springs back up
        {
            base.Undo();
            warning = "Not hot enough: the seam sprang back up. Check the care label.";
        }
        else if (heat > right)   // too hot: a scorch on the seam
        {
            var at = Points[Points.Count - 1];
            State.marks.Add(at);
            scorches.Add(Scorch(at));
            warning = "Too hot for this fabric: a scorch mark! Undo dabs it off; turn the iron down.";
            AudioManager.Play(Sfx.Click);
        }
        else warning = null;
    }

    /// Dabs off the scorches first; once there are none, takes back the last pass of the iron.
    public override void Undo()
    {
        if (State.marks.Count > 0)
        {
            State.marks.RemoveAt(State.marks.Count - 1);
            Destroy(scorches[scorches.Count - 1]);
            scorches.RemoveAt(scorches.Count - 1);
            warning = null;
            return;
        }
        base.Undo();
    }

    public new string MistakesLeft
    {
        get
        {
            string lines = base.MistakesLeft;
            string marks = scorches.Count == 0 ? null : scorches.Count == 1 ? "a scorch mark" : $"{scorches.Count} scorch marks";
            return lines == null ? marks : marks == null ? lines : lines + " and " + marks;
        }
    }

    public override float SaveWork()
    {
        float score = base.SaveWork();
        State.mistakes = scorches.Count;
        return score * Mathf.Clamp01(1f - 0.15f * scorches.Count);
    }
}
