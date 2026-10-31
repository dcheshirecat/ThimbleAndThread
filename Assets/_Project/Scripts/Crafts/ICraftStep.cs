using UnityEngine;

/// One step of making an item, played as a small minigame (stitching, hemming, cutting, stuffing...).
/// Every step plugs in the same way: MinigameHost makes the step's GameObject, calls Begin,
/// and later SaveWork (on "Done for now" and "Finish") before destroying it.
/// To add a step kind: append it to StepKind, write a MonoBehaviour that implements this
/// (PieceStep and TracingStep do most of the work), and add it to MinigameHost.Create.
public interface ICraftStep
{
    /// Shows the step for this order, restoring the work saved earlier.
    void Begin(Order order, int stepIndex, StepArea area);

    /// Copies the work into the order so it gets saved. Returns the quality so far (0..1).
    float SaveWork();

    /// Steps back one action, if the step supports it.
    void Undo();

    /// Why the step can't be finished yet (e.g. "Choose a fabric first"), or null when it can.
    string FinishBlocker { get; }
}

/// Where a step may draw: a square in world space, plus the shared camera, material and sprite,
/// and the supply cupboard for steps that offer a choice of materials.
public struct StepArea
{
    public Camera cam;
    public Material material;
    public Sprite square;          // plain white square sprite, 1 world unit at scale 1
    public Vector2 center;
    public float size;             // side of the square, in world units
    public Color fabricColor;      // the fabric the order is made on
    public ContentLibrary content;
    public ShopManager shop;
    public float assist;           // 0..1: how much the best tool for this step helps (SupplyDef.assist)
}

/// A step that can say what mistakes are still in the work (owner, 2026-10-30: mistakes can happen anywhere,
/// can be undone, and if left in they lower the result). Finish warns about them once before going on.
public interface IMistakes
{
    /// e.g. "2 wrong stitches", or null when there are none.
    string MistakesLeft { get; }
}
