#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// Tools > Thimble & Thread > Build Test Scene
/// Builds a playable portrait test scene: one placeholder panel per ScreenId, switched by ScreenRouter,
/// plus the shop (day), workroom (night), order book, cross stitch minigame and Pattern Studio.
/// Build never overwrites an existing scene. Rebuild replaces it (the scene is fully generated,
/// so only use Rebuild if nobody has hand-edited it).
/// Layout: reference canvas 2048 x 2732 (portrait), scaled by width; taller phones get extra height.
/// Positions are fractions of the screen, so they hold on any portrait aspect.
public static class TestSceneBuilder
{
    const string ScenePath = "Assets/_Project/Scenes/TestRouter.unity";
    const string Root = "Assets/_Project";
    const string SpriteMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

    static readonly Color Ink = new Color(0.20f, 0.15f, 0.20f);

    static Font font;
    static Sprite uiSprite;

    [MenuItem("Tools/Thimble & Thread/Build Test Scene")]
    public static void Build()
    {
        if (File.Exists(ScenePath))
        {
            Debug.Log($"{ScenePath} already exists; not overwriting. Use Rebuild Test Scene to replace it.");
            return;
        }
        Create();
    }

    [MenuItem("Tools/Thimble & Thread/Rebuild Test Scene (replaces it)")]
    public static void Rebuild() => Create();

    static void Create()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.32f, 0.28f, 0.35f);   // behind the stitching board
        camGo.transform.position = new Vector3(0, 0, -10);

        var gm = new GameObject("GameManager").AddComponent<GameManager>();

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // ---- Game systems ----
        var content = ContentLibraryRefresher.Refresh();
        var shop = new GameObject("ShopManager").AddComponent<ShopManager>();
        shop.content = content;

        var minigame = new GameObject("MinigameHost").AddComponent<MinigameHost>();
        minigame.cam = cam;
        minigame.material = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);

        var day = new GameObject("ShopDay").AddComponent<ShopDay>();
        day.content = content;
        day.shop = shop;
        day.minigame = minigame;

        // ---- UI ----
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2048, 2732);
        scaler.matchWidthOrHeight = 0f;   // match width: everything keeps its size across the screen
        canvasGo.AddComponent<GraphicRaycaster>();
        var router = canvasGo.AddComponent<ScreenRouter>();
        var ui = canvasGo.AddComponent<OrderFlowUI>();
        ui.day = day;
        var slots = canvasGo.AddComponent<SaveSlotsUI>();
        var designer = canvasGo.AddComponent<PatternDesignerUI>();
        designer.day = day;
        designer.host = minigame;

        var entries = new List<ScreenRouter.Entry>();
        RectTransform Panel(ScreenId id, string title, Color color, string bgSprite = null)
        {
            var root = MakePanel(canvasGo.transform, id, title, color, bgSprite);
            entries.Add(new ScreenRouter.Entry { id = id, root = root.gameObject });
            root.gameObject.SetActive(id == ScreenId.Title);
            return root;
        }

        // Title
        var p = Panel(ScreenId.Title, "Thimble & Thread", new Color(0.30f, 0.22f, 0.35f));
        p.Find("Title").GetComponent<Text>().color = Color.white;
        Button(Column(p, 0.5f, 0.45f), "Play", router.GoToSaveSelect);

        // Save select
        p = Panel(ScreenId.SaveSelect, "Choose a Save", new Color(0.88f, 0.84f, 0.92f));
        for (int i = 0; i < SaveSystem.SlotCount; i++)
        {
            float y = 0.76f - i * 0.21f;
            var row = new SaveSlotsUI.Row();
            row.summary = Label(p, $"Slot{i}", new Vector2(0.08f, y - 0.02f), new Vector2(0.92f, y + 0.08f), 64, TextAnchor.MiddleLeft);
            var buttons = Row(p, 0.5f, y - 0.06f);
            var play = Button(buttons, $"Play Slot {i + 1}", null, 860);
            UnityEventTools.AddIntPersistentListener(play.onClick, slots.Play, i);
            row.playLabel = play.GetComponentInChildren<Text>();
            row.deleteButton = Button(buttons, $"Delete Slot {i + 1}", null, 860);
            UnityEventTools.AddIntPersistentListener(row.deleteButton.onClick, slots.Delete, i);
            row.deleteLabel = row.deleteButton.GetComponentInChildren<Text>();
            slots.rows[i] = row;
        }
        Button(Column(p, 0.5f, 0.06f), "Back", router.GoToTitle);

        // Shop (day)
        p = Panel(ScreenId.Shop, "Shop", new Color(0.95f, 0.88f, 0.78f), "Art/Backgrounds/bg_shop_day.png");
        ui.shopStatus = Label(p, "Status", new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.88f), 68, TextAnchor.MiddleLeft);
        var b = Column(p, 0.5f, 0.33f);
        ui.greetLabel = Button(b, "Greet Customer", day.GreetCustomer, 1100).GetComponentInChildren<Text>();
        Button(b, "Order Book", router.GoToOrderBook, 1100);
        Button(b, "Supply Market", router.GoToMarket, 1100);
        Button(b, "Journal", router.GoToJournal, 1100);
        Button(b, "Settings", router.GoToSettings, 1100);
        Button(b, "Close the Shop", day.CloseShop, 1100);

        // Workroom (night)
        p = Panel(ScreenId.Workroom, "Workroom", new Color(0.22f, 0.24f, 0.40f), "Art/Backgrounds/bg_workroom_night.png");
        p.Find("Title").GetComponent<Text>().color = Color.white;
        ui.workroomStatus = Label(p, "Status", new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.88f), 68, TextAnchor.MiddleLeft);
        ui.workroomStatus.color = Color.white;
        b = Column(p, 0.5f, 0.35f);
        Button(b, "Order Book", router.GoToOrderBook, 1100);
        Button(b, "Pattern Studio", router.GoToPatternStudio, 1100);
        Button(b, "Journal", router.GoToJournal, 1100);
        Button(b, "Settings", router.GoToSettings, 1100);
        Button(b, "Go to Bed", day.GoToBed, 1100);

        // Order book
        p = Panel(ScreenId.OrderBook, "Order Book", new Color(0.96f, 0.92f, 0.84f));
        ui.orderTemplate = Button(Column(p, 0.5f, 0.6f), "Order Template", null, 1840);
        ui.orderBookEmpty = Label(p, "Empty", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.7f), 64, TextAnchor.MiddleCenter);
        ui.orderBookEmpty.text = "No orders yet. Customers visit the shop during the day.";
        Button(Column(p, 0.5f, 0.06f), "Back", router.GoToHub);

        // Dialogue
        p = Panel(ScreenId.Dialogue, "", new Color(0.80f, 0.86f, 0.93f));
        ui.dialoguePortrait = Portrait(p, new Vector2(0.5f, 0.75f), 960);
        ui.dialogueName = Label(p, "Name", new Vector2(0.08f, 0.53f), new Vector2(0.92f, 0.58f), 96, TextAnchor.MiddleLeft);
        ui.dialogueText = Label(p, "Request", new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.53f), 68, TextAnchor.UpperLeft);
        ui.dialoguePattern = Preview(p, new Vector2(0.22f, 0.2f), 420);
        b = Column(p, 0.66f, 0.2f);
        ui.acceptLabel = Button(b, "Accept", day.AcceptOffer, 960).GetComponentInChildren<Text>();
        Button(b, "Not Today", day.DeclineOffer, 960);

        // Workbench
        p = Panel(ScreenId.Workbench, "Workbench", new Color(0.80f, 0.66f, 0.50f), "Art/Backgrounds/bg_workbench.png");
        ui.workbenchText = Label(p, "Order", new Vector2(0.08f, 0.5f), new Vector2(0.92f, 0.84f), 72, TextAnchor.MiddleCenter);
        b = Column(p, 0.5f, 0.3f);
        var start = Button(b, "Start Crafting", day.StartCrafting, 1100);
        ui.startLabel = start.GetComponentInChildren<Text>();
        ui.startButton = start.gameObject;
        ui.handOverButton = Button(b, "Hand Over", day.HandOver, 1100).gameObject;
        Button(b, "Back to Order Book", router.GoToOrderBook, 1100);

        // Minigame: transparent so the stitching board (world sprites, upper half) shows through
        p = Panel(ScreenId.Minigame, "", Color.clear);
        p.GetComponent<Image>().raycastTarget = false;
        ui.minigameHint = Label(p, "Hint", new Vector2(0.06f, 0.87f), new Vector2(0.94f, 0.98f), 56, TextAnchor.MiddleCenter);
        ui.minigameHint.color = Color.white;
        ui.minigameThreads = Swatches(p, 0.31f, 8, false);
        ui.minigameThreads.label.color = Color.white;
        UnityEventTools.AddPersistentListener(ui.minigameThreads.onPick, minigame.SelectColour);
        b = Row(p, 0.5f, 0.19f);
        Button(b, "Undo", minigame.Undo, 520);
        Button(b, "Done for Now", day.DoneForNow, 600);
        Button(b, "Finish", day.FinishCrafting, 520);

        // Result
        p = Panel(ScreenId.Result, "Result", new Color(0.97f, 0.88f, 0.65f));
        ui.resultPortrait = Portrait(p, new Vector2(0.5f, 0.71f), 840);
        ui.resultText = Label(p, "Summary", new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.55f), 72, TextAnchor.MiddleCenter);
        b = Column(p, 0.5f, 0.18f);
        Button(b, "Set Aside", day.SetAside, 1100);
        Button(b, "Keep Working", day.KeepWorking, 1100);

        // Side screens
        p = Panel(ScreenId.Market, "Supply Market", new Color(0.85f, 0.70f, 0.60f));
        Button(Column(p, 0.5f, 0.45f), "Back", router.GoToHub);

        p = Panel(ScreenId.Journal, "Journal", new Color(0.80f, 0.93f, 0.74f));
        Button(Column(p, 0.5f, 0.45f), "Back", router.GoToHub);

        p = Panel(ScreenId.Settings, "Settings", new Color(0.78f, 0.78f, 0.82f));
        b = Column(p, 0.5f, 0.45f);
        Button(b, "Back", router.GoToHub, 1100);
        Button(b, "Save and Quit to Title", gm.QuitToTitle, 1100);

        p = Panel(ScreenId.DaySummary, "End of Day", new Color(0.78f, 0.82f, 0.95f));
        ui.summaryText = Label(p, "Summary", new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.8f), 72, TextAnchor.MiddleCenter);
        Button(Column(p, 0.5f, 0.3f), "Go to the Workroom", day.GoToWorkroom, 1100);

        // Pattern Studio
        p = Panel(ScreenId.PatternStudio, "Pattern Studio", new Color(0.93f, 0.78f, 0.90f));
        ui.patternTemplate = Button(Column(p, 0.5f, 0.6f), "Pattern Template", null, 1840);
        b = Row(p, 0.5f, 0.06f);
        Button(b, "New Pattern", day.NewPattern, 860);
        Button(b, "Back", router.GoToHub, 860);

        p = Panel(ScreenId.PatternEdit, "Pattern", new Color(0.93f, 0.78f, 0.90f));
        ui.patternPreview = Preview(p, new Vector2(0.5f, 0.71f), 860);
        ui.patternText = Label(p, "Details", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.55f), 64, TextAnchor.MiddleLeft);
        b = Column(p, 0.28f, 0.18f);
        Button(b, "Price Up", day.PriceUp, 860);
        Button(b, "Price Down", day.PriceDown, 860);
        ui.editDesignButton = Button(b, "Edit Design", day.EditDesign, 860).gameObject;
        b = Column(p, 0.72f, 0.18f);
        ui.shelfLabel = Button(b, "Shelf", day.ToggleShelf, 860).GetComponentInChildren<Text>();
        var delete = Button(b, "Delete", day.DeletePattern, 860);
        ui.deleteButton = delete.gameObject;
        ui.deleteLabel = delete.GetComponentInChildren<Text>();
        Button(b, "Back to Studio", router.GoToPatternStudio, 860);

        // Pattern designer: transparent so the paint grid shows through
        p = Panel(ScreenId.PatternDesigner, "", Color.clear);
        p.GetComponent<Image>().raycastTarget = false;
        designer.nameField = NameField(p, new Vector2(0.5f, 0.925f));
        designer.swatches = Swatches(p, 0.315f, ShopDay.DesignerPalette.Length, true);
        designer.swatches.label.color = Color.white;
        UnityEventTools.AddPersistentListener(designer.swatches.onPick, designer.SelectColour);
        b = Row(p, 0.5f, 0.235f);
        designer.makesLabel = Button(b, "Makes", designer.NextItem, 900).GetComponentInChildren<Text>();
        designer.sizeLabel = Button(b, "Size", designer.NextSize, 760).GetComponentInChildren<Text>();
        b = Row(p, 0.5f, 0.155f);
        Button(b, "Price Down", designer.PriceDown, 520).GetComponentInChildren<Text>().text = "Price -";
        designer.priceLabel = RowLabel(b, 480);
        Button(b, "Price Up", designer.PriceUp, 520).GetComponentInChildren<Text>().text = "Price +";
        b = Row(p, 0.5f, 0.06f);
        Button(b, "Undo", designer.Undo, 440);
        Button(b, "Clear", designer.Clear, 440);
        Button(b, "Save", designer.Save, 440);
        Button(b, "Cancel", designer.Cancel, 440);

        router.screens = entries.ToArray();

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettingsFirst(ScenePath);
        Debug.Log($"Test scene built at {ScenePath}.");
    }

    // ---------- building blocks ----------

    static RectTransform MakePanel(Transform parent, ScreenId id, string title, Color color, string bgSprite)
    {
        var panel = new GameObject($"Screen_{id}", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)panel.transform;
        rt.SetParent(parent, false);
        Stretch(rt);

        panel.GetComponent<Image>().color = color;
        var sprite = bgSprite != null ? AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/{bgSprite}") : null;
        if (sprite != null)
        {
            // Fill the screen without stretching: keep the art's shape and trim the edges that don't fit.
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            bg.transform.SetParent(rt, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = sprite;
            bgImg.raycastTarget = false;
            var fitter = bg.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        var label = MakeText(rt, "Title", title, 120);
        var lrt = (RectTransform)label.transform;
        lrt.anchorMin = new Vector2(0, 1); lrt.anchorMax = new Vector2(1, 1);
        lrt.pivot = new Vector2(0.5f, 1);
        lrt.anchoredPosition = new Vector2(0, -120);
        lrt.sizeDelta = new Vector2(0, 200);
        return rt;
    }

    /// A vertical stack of buttons centred on (x, y); near the bottom edge it grows upwards instead.
    static Transform Column(Transform panel, float x, float y) => Group<VerticalLayoutGroup>(panel, x, y, 40);

    /// A horizontal row of buttons centred on (x, y).
    static Transform Row(Transform panel, float x, float y) => Group<HorizontalLayoutGroup>(panel, x, y, 30);

    static Transform Group<T>(Transform panel, float x, float y, float spacing) where T : HorizontalOrVerticalLayoutGroup
    {
        var go = new GameObject("Buttons", typeof(RectTransform), typeof(T), typeof(ContentSizeFitter));
        var rt = (RectTransform)go.transform;
        rt.SetParent(panel, false);
        rt.anchorMin = rt.anchorMax = new Vector2(x, y);
        rt.pivot = new Vector2(0.5f, y < 0.1f ? 0f : 0.5f);
        var layout = go.GetComponent<T>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rt;
    }

    /// onClick may be null when the caller wires its own listener (e.g. one with a slot number).
    static Button Button(Transform parent, string label, UnityAction onClick, float width = 800)
    {
        var go = new GameObject($"Button_{label.Replace(" ", "")}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = uiSprite;
        img.type = Image.Type.Sliced;
        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = width; le.preferredHeight = 160;

        var text = MakeText(go.transform, "Label", label, 64);
        Stretch((RectTransform)text.transform);

        var button = go.GetComponent<Button>();
        if (onClick != null) UnityEventTools.AddPersistentListener(button.onClick, onClick);
        return button;
    }

    /// A plain text cell inside a Row, e.g. the price between the - and + buttons.
    static Text RowLabel(Transform row, float width)
    {
        var t = MakeText(row, "Value", "", 64);
        var le = t.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = width; le.preferredHeight = 160;
        t.color = Color.white;
        return t;
    }

    /// A row of colour buttons (optionally with an eraser) and a label above it, all at height y.
    static ThreadSwatches Swatches(Transform panel, float y, int count, bool eraser)
    {
        var row = Row(panel, 0.5f, y);
        var ts = row.gameObject.AddComponent<ThreadSwatches>();
        ts.swatches = new Image[count];
        for (int i = 0; i < count; i++)
        {
            var b = Swatch(row, $"Swatch_{i}", Color.white);
            UnityEventTools.AddIntPersistentListener(b.onClick, ts.Pick, i);
            ts.swatches[i] = b.GetComponent<Image>();
        }
        if (eraser)
        {
            var b = Swatch(row, "Swatch_Eraser", new Color(0.95f, 0.95f, 0.95f));
            var t = MakeText(b.transform, "Label", "Erase", 40);
            Stretch((RectTransform)t.transform);
            UnityEventTools.AddIntPersistentListener(b.onClick, ts.Pick, -1);
            ts.eraser = b.GetComponent<Image>();
        }
        ts.label = Label(panel, "ThreadName", new Vector2(0.1f, y + 0.035f), new Vector2(0.9f, y + 0.07f), 56, TextAnchor.MiddleCenter);
        return ts;
    }

    static Button Swatch(Transform row, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(Outline));
        go.transform.SetParent(row, false);
        var img = go.GetComponent<Image>();
        img.sprite = uiSprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = le.preferredHeight = 170;
        var outline = go.GetComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(12, -12);
        outline.enabled = false;
        return go.GetComponent<Button>();
    }

    static InputField NameField(Transform panel, Vector2 anchor)
    {
        var go = new GameObject("NameField", typeof(RectTransform), typeof(Image), typeof(InputField));
        var rt = (RectTransform)go.transform;
        rt.SetParent(panel, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(1700, 160);
        var img = go.GetComponent<Image>();
        img.sprite = uiSprite;
        img.type = Image.Type.Sliced;

        var text = MakeText(go.transform, "Text", "", 72);
        text.supportRichText = false;
        var trt = (RectTransform)text.transform;
        Stretch(trt);
        trt.offsetMin = new Vector2(40, 0); trt.offsetMax = new Vector2(-40, 0);
        var hint = MakeText(go.transform, "Placeholder", "Name your pattern", 72);
        hint.fontStyle = FontStyle.Italic;
        hint.color = new Color(Ink.r, Ink.g, Ink.b, 0.5f);
        var hrt = (RectTransform)hint.transform;
        Stretch(hrt);
        hrt.offsetMin = trt.offsetMin; hrt.offsetMax = trt.offsetMax;

        var field = go.GetComponent<InputField>();
        field.textComponent = text;
        field.placeholder = hint;
        field.characterLimit = 30;
        return field;
    }

    static Text Label(Transform panel, string name, Vector2 min, Vector2 max, int size, TextAnchor align)
    {
        var t = MakeText(panel, name, "", size);
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var rt = (RectTransform)t.transform;
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }

    static RawImage Preview(Transform panel, Vector2 anchor, float size)
    {
        var go = new GameObject("PatternPreview", typeof(RectTransform), typeof(RawImage));
        var rt = (RectTransform)go.transform;
        rt.SetParent(panel, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(size, size);
        go.GetComponent<RawImage>().raycastTarget = false;
        return go.GetComponent<RawImage>();
    }

    static Image Portrait(Transform panel, Vector2 anchor, float size)
    {
        var go = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(panel, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(size, size);
        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    static Text MakeText(Transform parent, string name, string value, int size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.text = value;
        t.font = font;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Ink;
        t.raycastTarget = false;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void AddToBuildSettingsFirst(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(s => s.path == path);
        scenes.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
