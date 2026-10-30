using System.Collections.Generic;
using UnityEngine;

/// Crochet recipe step: make a magic ring, the adjustable loop amigurumi starts from.
/// Wrap the yarn round by dragging along the dashed circle.
public class MagicRingStep : TracingStep
{
    const float Radius = 0.15f;

    protected override bool ClosedGuide => true;
    protected override Color ThreadColor => Color.Lerp(area.fabricColor, Color.black, 0.15f);

    protected override void DrawPiece() { }   // nothing yet: just the yarn

    protected override List<Vector2> Guide()
    {
        var circle = new List<Vector2>();
        for (int i = 0; i < 24; i++)
        {
            float a = i / 24f * Mathf.PI * 2f;
            circle.Add(new Vector2(0.5f, 0.5f) + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Radius);
        }
        return circle;
    }
}
