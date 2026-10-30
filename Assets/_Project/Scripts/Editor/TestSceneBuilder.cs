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
    const float ReferenceHeight = 2732;   // canvas units; taller phones only add height, so fractions err on the safe side
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
        var titleButtons = Column(p, 0.5f, 0.45f);
        Button(titleButtons, "Play", router.GoToSaveSelect);
        Button(titleButtons, "How to Play", router.GoToTutorial);

        // Tutorial: shown the first time the app is opened, and from "How to Play"
        p = Panel(ScreenId.Tutorial, "", new Color(0.30f, 0.22f, 0.35f));
        var tutorial = canvasGo.AddComponent<TutorialUI>();
        tutorial.content = content;
        tutorial.square = uiSprite;
        tutorial.circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var pageGo = new GameObject("Page", typeof(RectTransform), typeof(CanvasGroup));
        var pageRt = (RectTransform)pageGo.transform;
        pageRt.SetParent(p, false);
        Stretch(pageRt);
        tutorial.page = pageGo.GetComponent<CanvasGroup>();
        tutorial.titleText = Label(pageRt, "PageTitle", new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.93f), 100, TextAnchor.MiddleCenter);
        tutorial.titleText.color = Color.white;
        var stageGo = new GameObject("Stage", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        var stageRt = (RectTransform)stageGo.transform;
        stageRt.SetParent(pageRt, false);
        stageRt.anchorMin = stageRt.anchorMax = new Vector2(0.5f, 0.635f);
        stageRt.sizeDelta = new Vector2(1700, 1150);   // fixed canvas units, so the scenes look the same on every phone
        stageGo.GetComponent<Image>().color = new Color(1, 1, 1, 0.08f);
        tutorial.stage = stageRt;
        var artGo = new GameObject("Art", typeof(RectTransform), typeof(Image));
        var artRt = (RectTransform)artGo.transform;
        artRt.SetParent(stageRt, false);
        Stretch(artRt);
        tutorial.art = artGo.GetComponent<Image>();
        tutorial.art.preserveAspect = true;
        tutorial.art.raycastTarget = false;
        tutorial.bodyText = Label(pageRt, "Body", new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.41f), 64, TextAnchor.UpperCenter);
        tutorial.bodyText.color = Color.white;
        tutorial.pageText = Label(pageRt, "Dots", new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.19f), 56, TextAnchor.MiddleCenter);
        tutorial.pageText.color = Color.white;
        var tutorialButtons = Row(p, 0.5f, 0.07f);
        tutorial.backButton = Button(tutorialButtons, "Back", tutorial.Back, 600).gameObject;
        tutorial.nextLabel = Button(tutorialButtons, "Next", tutorial.Next, 900).GetComponentInChildren<Text>();
        var skip = Button(p, "Skip", tutorial.Skip, 360);
        var skipRt = (RectTransform)skip.transform;
        skipRt.anchorMin = skipRt.anchorMax = new Vector2(0.88f, 0.965f);
        skipRt.sizeDelta = new Vector2(360, 130);

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
        ui.orderTemplate = Button(ScrollList(p, 0.14f, 0.88f), "Order Template", null, 1840);
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
        ui.workbenchText = Label(p, "Order", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.9f), 60, TextAnchor.MiddleCenter);
        b = Column(p, 0.5f, 0.24f);
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
        ui.marketStatus = Label(p, "Status", new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.92f), 60, TextAnchor.MiddleCenter);
        var shelves = ScrollList(p, 0.14f, 0.81f);
        ui.shelfTitleTemplate = ShelfTitle(shelves);
        ui.shelfTemplate = Shelf(shelves);
        ui.marketTemplate = SupplyTile(ui.shelfTemplate);
        Button(Column(p, 0.5f, 0.04f), "Back", router.GoToHub);

        p = Panel(ScreenId.Journal, "Journal", new Color(0.80f, 0.93f, 0.74f));
        var journal = canvasGo.AddComponent<JournalUI>();
        journal.day = day;
        var tabRow = Row(p, 0.5f, 0.865f);
        var tabNames = new[] { "Story", "Friends", "Skills", "Supplies", "News" };
        journal.tabs = new Image[tabNames.Length];
        for (int i = 0; i < tabNames.Length; i++)
        {
            var tab = Button(tabRow, tabNames[i], null, 360);
            tab.GetComponentInChildren<Text>().fontSize = 52;
            UnityEventTools.AddIntPersistentListener(tab.onClick, journal.ShowPage, i);
            journal.tabs[i] = tab.GetComponent<Image>();
        }
        var journalList = ScrollList(p, 0.13f, 0.82f);
        journal.pageText = MakeText(journalList, "Page", "", 54);
        journal.pageText.alignment = TextAnchor.UpperLeft;
        journal.pageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        journal.pageText.verticalOverflow = VerticalWrapMode.Overflow;
        var pageLayout = journal.pageText.gameObject.AddComponent<LayoutElement>();
        pageLayout.preferredWidth = 1800;
        Button(Column(p, 0.5f, 0.04f), "Back", router.GoToHub);

        p = Panel(ScreenId.Settings, "Settings", new Color(0.78f, 0.78f, 0.82f));
        b = Column(p, 0.5f, 0.45f);
        Button(b, "Back", router.GoToHub, 1100);
        Button(b, "Save and Quit to Title", gm.QuitToTitle, 1100);

        p = Panel(ScreenId.DaySummary, "End of Day", new Color(0.78f, 0.82f, 0.95f));
        ui.summaryText = Label(p, "Summary", new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.8f), 72, TextAnchor.MiddleCenter);
        Button(Column(p, 0.5f, 0.3f), "Go to the Workroom", day.GoToWorkroom, 1100);

        // Pattern Studio
        p = Panel(ScreenId.PatternStudio, "Pattern Studio", new Color(0.93f, 0.78f, 0.90f));
        ui.patternTemplate = Button(ScrollList(p, 0.14f, 0.88f), "Pattern Template", null, 1840);
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
        designer.swatches = Swatches(p, 0.32f, Materials.Threads(content).Count, true, 130);
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

    /// A vertical list that scrolls when it's longer than the band between y = bottom and y = top.
    /// Returns the list's content, where rows (or a row template) go.
    static Transform ScrollList(Transform panel, float bottom, float top)
    {
        var view = new GameObject("List", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect), typeof(Image));
        var vrt = (RectTransform)view.transform;
        vrt.SetParent(panel, false);
        vrt.anchorMin = new Vector2(0.03f, bottom); vrt.anchorMax = new Vector2(0.97f, top);
        vrt.offsetMin = vrt.offsetMax = Vector2.zero;
        view.GetComponent<Image>().color = new Color(1, 1, 1, 0.001f);   // catches drags for scrolling

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var crt = (RectTransform)content.transform;
        crt.SetParent(vrt, false);
        crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.offsetMin = crt.offsetMax = Vector2.zero;
        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = view.GetComponent<ScrollRect>();
        scroll.content = crt;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return crt;
    }

    // ---- Supply Market shelves (OrderFlowUI copies these templates) ----

    static Text ShelfTitle(Transform list)
    {
        var t = MakeText(list, "Shelf Title Template", "Shelf", 64);
        t.alignment = TextAnchor.LowerLeft;
        t.fontStyle = FontStyle.Bold;
        var le = t.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 1840; le.preferredHeight = 100;
        return t;
    }

    /// A wooden shelf: a grid of tiles, 3 per row.
    static RectTransform Shelf(Transform list)
    {
        var go = new GameObject("Shelf Template", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup));
        go.transform.SetParent(list, false);
        go.GetComponent<Image>().color = new Color(0.62f, 0.45f, 0.30f);
        var grid = go.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(580, 720);
        grid.spacing = new Vector2(30, 40);
        grid.padding = new RectOffset(25, 25, 30, 30);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperLeft;
        return (RectTransform)go.transform;
    }

    /// One supply on the shelf: its icon, then its name, then price and how many you have.
    static Button SupplyTile(Transform shelf)
    {
        var go = new GameObject("Supply Template", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(shelf, false);
        var img = go.GetComponent<Image>();
        img.sprite = uiSprite;
        img.type = Image.Type.Sliced;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        var irt = (RectTransform)icon.transform;
        irt.SetParent(go.transform, false);
        irt.anchorMin = new Vector2(0.1f, 0.40f); irt.anchorMax = new Vector2(0.9f, 0.96f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var iimg = icon.GetComponent<Image>();
        iimg.preserveAspect = true;
        iimg.raycastTarget = false;

        TileText(go.transform, "Name", 44, 0.21f, 0.40f, FontStyle.Bold);
        TileText(go.transform, "Info", 38, 0.02f, 0.21f, FontStyle.Normal);
        return go.GetComponent<Button>();
    }

    static void TileText(Transform tile, string name, int size, float bottom, float top, FontStyle style)
    {
        var t = MakeText(tile, name, "", size);
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var rt = (RectTransform)t.transform;
        rt.anchorMin = new Vector2(0.04f, bottom); rt.anchorMax = new Vector2(0.96f, top);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
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

    /// A grid of colour buttons (optionally with an eraser), centred at height y, with a label above it.
    /// Up to 8 per row; "cell" is the button size, smaller when there are many colours.
    static ThreadSwatches Swatches(Transform panel, float y, int count, bool eraser, float cell = 170)
    {
        const int columns = 8;
        const float spacing = 20;
        var go = new GameObject("Swatches", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        var row = (RectTransform)go.transform;
        row.SetParent(panel, false);
        row.anchorMin = row.anchorMax = new Vector2(0.5f, y);
        var grid = go.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(cell, cell);
        grid.spacing = new Vector2(spacing, spacing);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Min(columns, count + (eraser ? 1 : 0));
        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        int rows = Mathf.CeilToInt((count + (eraser ? 1 : 0)) / (float)columns);
        float top = y + (rows * cell + (rows - 1) * spacing) / 2f / ReferenceHeight;

        var ts = go.AddComponent<ThreadSwatches>();
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
        ts.label = Label(panel, "ThreadName", new Vector2(0.1f, top + 0.004f), new Vector2(0.9f, top + 0.034f), 56, TextAnchor.MiddleCenter);
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
