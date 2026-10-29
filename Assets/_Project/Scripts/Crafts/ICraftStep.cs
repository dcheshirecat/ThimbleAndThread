using UnityEngine;

/// One step of making an item, played as a small minigame (stitching, hemming, cutting, stuffing...).
/// Every step plugs in the same way: MinigameHost makes the step's GameObject, calls Begin,
/// and later SaveWork (on "Done for now" and "Finish") before destroying it.
/// To add a step kind: append it to StepKind, write a MonoBehaviour that implements this,
/// and add it to MinigameHost.Create.
public interface ICraftStep
{
    /// Shows the step for this order, restoring the work saved earlier.
    void Begin(Order order, int stepIndex, StepArea area);

    /// Copies the work into the order so it gets saved. Returns the quality so far (0..1).
    float SaveWork();

    /// Steps back one action, if the step supports it.
    void Undo();
}

/// Where a step may draw: a square in world space, plus the shared camera, material and sprite.
public struct StepArea
{
    public Camera cam;
    public Material material;
    public Sprite square;          // plain white square sprite, 1 world unit at scale 1
    public Vector2 center;
    public float size;             // side of the square, in world units
    public Color fabricColor;      // the fabric the order is made on
}
