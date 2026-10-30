using UnityEngine;

/// Opens the minigame for the step an order is on (see ICraftStep), restoring the work saved earlier,
/// and reads the work back. Also opens the cross stitch grid as a paint canvas for the Pattern Studio.
public class MinigameHost : MonoBehaviour
{
    public Camera cam;
    public Material material;                // sprite material for cells and threads

    [Header("Board placement, as fractions of the screen (portrait)")]
    [Range(0, 1)] public float bandBottom = 0.40f;   // controls go below this
    [Range(0, 1)] public float bandTop = 0.86f;      // title/hint above this
    [Range(0, 1)] public float maxWidth = 0.92f;

    ICraftStep step;
    GameObject stepObject;
    CrossStitchCanvas designBoard;
    Sprite square;

    /// The cross stitch board on screen (stitching or designing), or null.
    public CrossStitchCanvas Board => designBoard != null ? designBoard : (step as CrossStitchStep)?.Board;

    public ICraftStep Step => step;

    /// Opens the order's current step.
    public void Open(Order order, Color fabricColor, ContentLibrary content, ShopManager shop)
    {
        Close();
        var area = Area(fabricColor);
        area.content = content;
        area.shop = shop;
        stepObject = new GameObject($"Step_{order.CurrentStep.kind}");
        stepObject.transform.SetParent(transform, false);
        step = Create(order.CurrentStep.kind, stepObject);
        step.Begin(order, Mathf.Clamp(order.step, 0, order.Steps.Length - 1), area);
    }

    /// Adds the MonoBehaviour that plays a step kind. New step kinds go here.
    static ICraftStep Create(StepKind kind, GameObject go) => kind switch
    {
        StepKind.Hem => go.AddComponent<HemStep>(),
        StepKind.ChooseFabric or StepKind.ChooseYarn => go.AddComponent<ChooseMaterialStep>(),
        StepKind.CastOn => go.AddComponent<CastOnStep>(),
        StepKind.KnitRows => go.AddComponent<KnitRowsStep>(),
        StepKind.CastOff => go.AddComponent<CastOffStep>(),
        StepKind.MagicRing => go.AddComponent<MagicRingStep>(),
        StepKind.CrochetRounds => go.AddComponent<CrochetRoundsStep>(),
        StepKind.CutPieces => go.AddComponent<CutPiecesStep>(),
        StepKind.Pin => go.AddComponent<PinStep>(),
        StepKind.SewSeam => go.AddComponent<SewSeamStep>(),
        StepKind.TurnRightSideOut => go.AddComponent<TurnRightSideOutStep>(),
        StepKind.Stuff => go.AddComponent<StuffStep>(),
        StepKind.AttachEyes => go.AddComponent<AttachEyesStep>(),
        StepKind.CloseOpening => go.AddComponent<CloseOpeningStep>(),
        StepKind.SewSides => go.AddComponent<SewSidesStep>(),
        StepKind.HemTop => go.AddComponent<HemTopStep>(),
        StepKind.AttachHandles => go.AddComponent<AttachHandlesStep>(),
        _ => go.AddComponent<CrossStitchStep>(),
    };

    /// Pattern Studio: a paint grid for designing. Read the result from Board.pattern before Close().
    public void OpenDesign(Pattern design)
    {
        Close();
        designBoard = CrossStitchStep.MakeBoard(transform, Area(Color.white), design.width, design.height);
        designBoard.mode = CrossStitchCanvas.Mode.PatternFill;
        designBoard.design = design;
    }

    public void SelectColour(int index)
    {
        var board = Board;
        if (board == null) return;
        board.erase = index < 0;
        if (index >= 0) board.currentColor = index;
    }

    public void Undo() => step?.Undo();

    /// Copies the work into the order (for saving). Returns the step's quality so far (0..1).
    public float SaveWork() => step != null ? step.SaveWork() : 0f;

    public void Close()
    {
        if (stepObject != null) Destroy(stepObject);
        if (designBoard != null) Destroy(designBoard.gameObject);
        stepObject = null;
        step = null;
        designBoard = null;
    }

    /// The square band of the screen that steps draw in.
    StepArea Area(Color fabricColor)
    {
        if (square == null)
            square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        float viewH = cam.orthographicSize * 2f, viewW = viewH * cam.aspect;
        return new StepArea
        {
            cam = cam,
            material = material,
            square = square,
            size = Mathf.Min(viewW * maxWidth, viewH * (bandTop - bandBottom)),
            center = new Vector2(cam.transform.position.x, cam.transform.position.y + viewH * ((bandTop + bandBottom) / 2f - 0.5f)),
            fabricColor = fabricColor,
        };
    }
}
