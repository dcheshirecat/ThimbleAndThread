using System.Collections.Generic;
using UnityEngine;

/// Recipe step: cut the pieces out along the pattern line. The fabric is folded, so front and back
/// are cut together in one go.
public class CutPiecesStep : TracingStep
{
    protected override bool ClosedGuide => true;
    protected override float StitchLength => 0f;   // a continuous line, not stitches
    protected override Color ThreadColor => new Color(0.35f, 0.35f, 0.40f);   // the scissors' cut
    protected override Color GuideColor => new Color(0.15f, 0.15f, 0.2f, 0.8f);   // marking pen

    protected override List<Vector2> Guide() => Shape.Outline;
}
