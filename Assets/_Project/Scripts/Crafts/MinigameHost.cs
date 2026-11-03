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
        ResetView();
        var area = Area(fabricColor);
        area.content = content;
        area.shop = shop;
        area.assist = assist;
        stepObject = new GameObject($"Step_{order.CurrentStep.kind}");
        stepObject.transform.SetParent(transform, false);
        step = order.item != null && order.item.craft == CraftType.Crochet && order.CurrentStep.kind == StepKind.Stuff ? stepObject.AddComponent<CrochetStuffStep>()
             : order.item != null && order.item.craft == CraftType.Crochet && order.CurrentStep.kind == StepKind.AttachEyes ? stepObject.AddComponent<CrochetEyesStep>()
             : HemFoldStep.Applies(order) ? stepObject.AddComponent<HemFoldStep>()   // the tablecloth's hem is folded, then stitched
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
        StepKind.MountCanvas => go.AddComponent<MountCanvasStep>(),
        StepKind.CutPatches => go.AddComponent<CutPatchesStep>(),
        StepKind.SewRows => go.AddComponent<SewRowsStep>(),
        StepKind.QuiltSandwich => go.AddComponent<QuiltSandwichStep>(),
        StepKind.QuiltIt => go.AddComponent<QuiltItStep>(),
        StepKind.WarpLoom => go.AddComponent<WarpLoomStep>(),
        StepKind.WeaveRows => go.AddComponent<WeaveRowsStep>(),
        StepKind.Darn => go.AddComponent<DarnStep>(),
        StepKind.HangWeaving => go.AddComponent<HangWeavingStep>(),
        StepKind.PressSeams => go.AddComponent<PressSeamsStep>(),
        StepKind.BindCanvas => go.AddComponent<BindCanvasStep>(),
        StepKind.LayoutPatches => go.AddComponent<LayoutPatchesStep>(),
        StepKind.PressQuiltSeams => go.AddComponent<QuiltPressStep>(),
        StepKind.TieOffWarps => go.AddComponent<TieOffWarpsStep>(),
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
        ResetView();
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
        if (index >= 0)
        {
            board.currentColor = index;
            board.NewThread();   // picking up a colour (even the same one again): a fresh length of thread
        }
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
        ResetView();
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
        RestoreCamera();
        float viewH = cam.orthographicSize * 2f, viewW = viewH * cam.aspect;
        workSize = Mathf.Min(viewW * maxWidth, viewH * (bandTop - bandBottom));
        workCentre = new Vector2(cam.transform.position.x, cam.transform.position.y + viewH * ((bandTop + bandBottom) / 2f - 0.5f));
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

    // ---- Looking closer (owner, 2026-11-02): zoom in and out of the work, and turn it round ----
    // The camera moves, not the work, so every step's touch handling (ScreenToWorldPoint) keeps working unchanged.
    // Two fingers: pinch to zoom, twist to turn, drag to move about. Buttons do the same in steps; the mouse wheel zooms.

    public const float MaxZoom = 4f, ZoomStep = 1.5f;
    float zoom = 1f, angle;
    Vector2 pan;                 // the point of the work at the centre of the band, from the work's centre (world units)
    Vector2 workCentre;
    float workSize;
    Vector3 basePos;
    float baseSize;
    bool moved;                  // the camera is away from its usual place

    public float Zoom => zoom;
    public float Angle => angle;
    bool WorkOpen => step != null || designBoard != null;

    public void ZoomIn() => SetView(zoom * ZoomStep, angle, pan);
    public void ZoomOut() => SetView(zoom / ZoomStep, angle, pan);
    /// A quarter turn clockwise.
    public void Turn() => SetView(zoom, angle + 90f, pan);
    public void ResetView() { zoom = 1f; angle = 0f; pan = Vector2.zero; RestoreCamera(); }

    void RestoreCamera()
    {
        if (!moved || cam == null) return;
        cam.transform.position = basePos;
        cam.transform.rotation = Quaternion.identity;
        cam.orthographicSize = baseSize;
        moved = false;
    }

    void SetView(float z, float a, Vector2 p)
    {
        if (cam == null || !WorkOpen) return;
        if (!moved) { basePos = cam.transform.position; baseSize = cam.orthographicSize; }
        zoom = Mathf.Clamp(z, 1f, MaxZoom);
        angle = Mathf.Repeat(a, 360f);
        float limit = workSize * 0.5f * (1f - 1f / zoom);   // zoomed in, you can move about the work but not off it
        pan = new Vector2(Mathf.Clamp(p.x, -limit, limit), Mathf.Clamp(p.y, -limit, limit));
        if (zoom <= 1.001f && angle < 0.01f) { ResetView(); return; }
        // Keep the work's band where it is on screen: the point "focus" shows where the work's centre used to.
        var rot = Quaternion.Euler(0, 0, angle);
        Vector2 focus = workCentre + pan;
        Vector2 centreOffset = workCentre - (Vector2)basePos;
        Vector2 pos = focus - (Vector2)(rot * centreOffset) / zoom;
        cam.transform.position = new Vector3(pos.x, pos.y, basePos.z);
        cam.transform.rotation = rot;
        cam.orthographicSize = baseSize / zoom;
        moved = true;
    }

    void Update()
    {
        if (!WorkOpen || cam == null) return;
        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0.01f) SetView(zoom * (wheel > 0 ? 1.15f : 1f / 1.15f), angle, pan);
        if (Input.touchCount != 2) return;
        Touch a = Input.GetTouch(0), b = Input.GetTouch(1);
        Vector2 a0 = a.position - a.deltaPosition, b0 = b.position - b.deltaPosition;
        float d0 = (a0 - b0).magnitude, d1 = (a.position - b.position).magnitude;
        if (d0 < 1f || d1 < 1f) return;
        float twist = Vector2.SignedAngle(a0 - b0, a.position - b.position);   // fingers turning anticlockwise...
        Vector2 move = (a.position + b.position - a0 - b0) / 2f;
        float worldPerPixel = (moved ? baseSize : cam.orthographicSize) * 2f / Screen.height / zoom;
        var rot = Quaternion.Euler(0, 0, angle);
        Vector2 newPan = pan - (Vector2)(rot * (move * worldPerPixel));        // the work follows the fingers
        SetView(zoom * d1 / d0, angle - twist, newPan);                         // ...turn the work anticlockwise
    }
}
