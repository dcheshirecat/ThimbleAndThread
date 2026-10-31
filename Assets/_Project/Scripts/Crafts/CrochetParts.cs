using System.Collections.Generic;
using UnityEngine;

// The crocheted plushie's other steps (owner, 2026-10-31): safety eyes go into the head between rounds,
// before stuffing; each part is stuffed; then the parts are sewn together.

/// Crochet: fit the safety eyes into the head, between rounds 3 and 4, before it's stuffed and closed.
public class CrochetEyesStep : AttachEyesStep
{
    const float EyeRound = 0.2f;        // between rounds 3 and 4 of the head

    /// Either side of the snout (the magic ring at the front of the face), a little above it.
    public static List<Vector2> EyeSpots()
    {
        var c = new Vector2(0.5f, 0.5f);
        return new List<Vector2>
        {
            c + new Vector2(Mathf.Cos(125f * Mathf.Deg2Rad), Mathf.Sin(125f * Mathf.Deg2Rad)) * EyeRound,
            c + new Vector2(Mathf.Cos(55f * Mathf.Deg2Rad), Mathf.Sin(55f * Mathf.Deg2Rad)) * EyeRound,
        };
    }

    protected override List<Vector2> Marks => EyeSpots();
    protected override Color MarkColor => new Color(1, 1, 1, 0.35f);

    protected override void DrawPiece() => DrawBall("head");

    protected override bool CanPlace(Vector2 p) => PieceShape.Inside(CrochetWork.For("head").Outline(), p);
}

/// Crochet: stuff a part (the head before it's closed, then the body), firmly and evenly.
public class CrochetStuffStep : StuffStep
{
    string Part => string.IsNullOrEmpty(Recipe.piece) ? "head" : Recipe.piece;
    protected override List<Vector2> Region => CrochetWork.For(Part).Outline();
    protected override void DrawBase() => DrawBall(Part);
}

/// Crochet: sew the parts together with the yarn tails: the head onto the body, then the ears onto the head.
/// Whipstitch along each dashed seam.
public class SewPartsStep : TracingStep
{
    static readonly Vector2 Head = new Vector2(0.5f, 0.63f), Body = new Vector2(0.5f, 0.3f);
    const float HeadR = 0.2f, BodyR = 0.19f, EarR = 0.08f;

    protected override float StitchLength => 0.022f;
    protected override Color ThreadColor => Color.Lerp(Yarn, Color.black, 0.35f);

    static Vector2 On(Vector2 c, float r, float deg) => c + new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad)) * r;

    protected override void DrawPiece()
    {
        var stripe = order.target != null && order.target.palette.Count > 0 ? order.target.palette[0] : Yarn;
        Piece(Circle(Body, BodyR, 40), Yarn, 0);
        var band = new List<Vector2>();   // the stripe round, round the body's middle
        for (int i = 0; i <= 20; i++) band.Add(On(Body, BodyR * 0.98f, 200 + i * 7f));
        for (int i = 20; i >= 0; i--) band.Add(On(Body, BodyR * 0.72f, 200 + i * 7f));
        Piece(band, stripe, 1);
        foreach (var side in new[] { 125f, 55f })
            Piece(Circle(On(Head, HeadR * 0.95f, side), EarR, 24), Color.Lerp(Yarn, Color.black, 0.08f), 2);
        Piece(Circle(Head, HeadR, 40), Yarn, 3);
        foreach (var e in new[] { On(Head, 0.09f, 125f), On(Head, 0.09f, 55f) }) Dot(e, AttachEyesStep.EyeSize, AttachEyesStep.EyeColor, 4, shine: true);
    }

    protected override List<Vector2> Guide() => Guides()[0];

    protected override List<List<Vector2>> Guides()
    {
        var neck = new List<Vector2>();   // where the head sits on the body
        for (int i = 0; i <= 12; i++) neck.Add(On(Head, HeadR * 0.98f, 235f + i * 70f / 12f));
        var list = new List<List<Vector2>> { neck };
        foreach (var side in new[] { 125f, 55f })   // each ear's base, along the top of the head
        {
            var ear = new List<Vector2>();
            for (int i = 0; i <= 6; i++) ear.Add(On(Head, HeadR * 1.02f, side - 18f + i * 6f));
            list.Add(ear);
        }
        return list;
    }
}
