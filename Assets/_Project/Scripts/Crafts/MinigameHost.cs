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
    public void Open(Order order, Color fabricColor, ContentLibrary content, ShopManager shop, float assist = 0f)
    {
        Close();
        var area = Area(fabricColor);
        area.content = content;
        area.shop = shop;
        area.assist = assist;
        stepObject = new GameObject($"Step_{order.CurrentStep.kind}");
        stepObject.transform.SetParent(transform, false);
        step = order.item != null && order.item.craft == CraftType.Crochet && order.CurrentStep.kind == StepKind.Stuff ? stepObject.AddComponent<CrochetStuffStep>()
             : order.item != null && order.item.craft == CraftType.Crochet && order.CurrentStep.kind == StepKind.AttachEyes ? stepObject.AddComponent<CrochetEyesStep>()
             : Create(order.CurrentStep.kind, stepObject);
        step.Begin(order, Mathf.Clamp(order.step, 0, order.Steps.Length - 1), area);
    }

    /// Adds the MonoBehaviour that plays a step kind. New step kinds go here.
    static ICraftStep Create(StepKind kind, GameObject go) => kind switch
    {
        StepKind.Hem => go.AddComponent<HemStep>(),
        StepKind.ChooseFabric or StepKind.ChooseYarn => go.AddComponent<ChooseMaterialStep>(),
        StepKind.CastOn => go.AddComponent<CastOnStep>(),
        StepKind.HoopFabric => go.AddComponent<HoopFabricStep>(),
        StepKind.Press => go.AddComponent<PressStep>(),
        StepKind.WeaveEnds => go.AddComponent<WeaveEndsStep>(),
        StepKind.Backstitch => go.AddComponent<BackstitchStep>(),
        StepKind.SatinFill => go.AddComponent<SatinFillStep>(),
        StepKind.FrenchKnots => go.AddComponent<FrenchKnotsStep>(),
        StepKind.KnitRows => go.AddComponent<KnitRowsStep>(),
        StepKind.CastOff => go.AddComponent<CastOffStep>(),
        StepKind.SewUp => go.AddComponent<SewUpStep>(),
        StepKind.Fringe => go.AddComponent<FringeStep>(),
        StepKind.MitreCorners => go.AddComponent<MitreCornersStep>(),
        StepKind.PinPattern => go.AddComponent<PinPatternStep>(),
        StepKind.ClipCurves => go.AddComponent<ClipCurvesStep>(),
        StepKind.BoxCorners => go.AddComponent<BoxCornersStep>(),
        StepKind.TransferDesign => go.AddComponent<TransferDesignStep>(),
        StepKind.TightenHoop => go.AddComponent<TightenHoopStep>(),
        StepKind.StemStitch => go.AddComponent<StemStitchStep>(),
        StepKind.FinishBack => go.AddComponent<FinishBackStep>(),
        StepKind.SewParts => go.AddComponent<SewPartsStep>(),
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
    public void OpenDesign(Pattern design, CraftItemDef item = null, Color? cloth = null)
    {
        Close();
        var frame = CrossStitchCanvas.FrameFor(item);
        var area = Area(Color.white);
        if (frame == CrossStitchCanvas.DesignFrame.Hoop) area.size *= 0.84f;   // room for the hoop's ring and screw
        designBoard = CrossStitchStep.MakeBoard(transform, area, design.width, design.height);
        designBoard.mode = CrossStitchCanvas.Mode.PatternFill;
        designBoard.frame = frame;
        designBoard.design = design;
        if (cloth.HasValue && frame == CrossStitchCanvas.DesignFrame.Squares) { designBoard.showCloth = true; designBoard.fabricColor = cloth.Value; }
    }

    public void SelectColour(int index)
    {
        var board = Board;
        if (board == null) return;
        board.erase = index < 0;
        if (index >= 0) board.currentColor = index;
    }

    public void Undo() => step?.Undo();

    /// The stitch picker (seam ripper) in hand, or the needle back.
    public void TogglePicker() { if (step is IStitchPicker p && p.CanPick) p.Picking = !p.Picking; }
    public bool CanPick => step is IStitchPicker p && p.CanPick;
    public bool Picking => step is IStitchPicker p && p.Picking;

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
