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
        var audio = gm.gameObject.AddComponent<AudioManager>();
        audio.musicDay = Clip("Music/music_shop_day");
        audio.musicNight = Clip("Music/music_workroom_night");
        audio.click = Clip("SFX/sfx_click");
        audio.coin = Clip("SFX/sfx_coin");
        audio.stitch = Clip("SFX/sfx_stitch");
        audio.stepDone = Clip("SFX/sfx_step_done");
        audio.bell = Clip("SFX/sfx_shop_bell");

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

        // Shop (day): a scene with three pages (pickups | shopkeeper and door | display), see ShopSceneUI
        p = Panel(ScreenId.Shop, "", new Color(0.95f, 0.88f, 0.78f), "Art/Backgrounds/bg_shop_day.png");
        var shopScene = p.gameObject.AddComponent<ShopSceneUI>();
        shopScene.day = day;
        var pageNames = new[] { "Page_Pickups", "Page_Shop", "Page_Display" };
        shopScene.pages = new RectTransform[3];
        for (int i = 0; i < 3; i++)
        {
            var pg = new GameObject(pageNames[i], typeof(RectTransform));
            var prt = (RectTransform)pg.transform;
            prt.SetParent(p, false);
            Stretch(prt);
            shopScene.pages[i] = prt;
        }
        // Middle: the shopkeeper behind the counter, and the door (placeholder shapes)
        var mid = shopScene.pages[1];
        Keeper(mid, new Vector2(0.36f, 0.5f), content);
        var counter = Shape(mid, "Counter", new Vector2(0.4f, 0.38f), new Vector2(1500, 280), new Color(0.55f, 0.36f, 0.22f), uiSprite);
        Shape(counter, "CounterTop", new Vector2(0.5f, 1f), new Vector2(1560, 50), new Color(0.68f, 0.47f, 0.30f), uiSprite);
        var doorButton = Button(mid, "Door", shopScene.TapDoor, 360);
        doorButton.GetComponentInChildren<Text>().text = "";
        var doorRt = (RectTransform)doorButton.transform;
        Object.DestroyImmediate(doorButton.GetComponent<LayoutElement>());
        doorRt.anchorMin = doorRt.anchorMax = new Vector2(0.8f, 0.6f);
        doorRt.pivot = new Vector2(0, 0.5f);   // hinged on its left edge
        doorRt.anchoredPosition = new Vector2(-180, 0);
        doorRt.sizeDelta = new Vector2(360, 680);
        doorButton.GetComponent<Image>().color = new Color(0.45f, 0.30f, 0.20f);
        Shape(doorRt, "Window", new Vector2(0.5f, 0.75f), new Vector2(220, 200), new Color(0.75f, 0.88f, 0.95f), uiSprite);
        Shape(doorRt, "Knob", new Vector2(0.82f, 0.45f), new Vector2(50, 50), new Color(0.95f, 0.80f, 0.35f), Knob());
        shopScene.door = doorRt;
        var bubble = Shape(mid, "Bubble", new Vector2(0.5f, 0.745f), new Vector2(1500, 170), Color.white, uiSprite);
        shopScene.bubble = bubble.gameObject;
        shopScene.bubbleText = MakeText(bubble, "Text", "", 56);
        Stretch((RectTransform)shopScene.bubbleText.transform);
        shopScene.statusText = Label(mid, "Status", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.27f), 52, TextAnchor.UpperCenter);
        // Left: orders waiting to be picked up
        var left = shopScene.pages[0];
        Label(left, "Heading", new Vector2(0.06f, 0.71f), new Vector2(0.94f, 0.77f), 76, TextAnchor.MiddleCenter).text = "Waiting for pickup";
        shopScene.pickupEmpty = Label(left, "Info", new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.71f), 52, TextAnchor.UpperCenter);
        shopScene.pickupTemplate = Button(StackList(left, 0.63f), "Pickup Template", null, 1800);
        TallRow(shopScene.pickupTemplate, 230, 54);
        // Right: the pattern display
        var right = shopScene.pages[2];
        Label(right, "Heading", new Vector2(0.06f, 0.71f), new Vector2(0.94f, 0.77f), 76, TextAnchor.MiddleCenter).text = "The pattern display";
        shopScene.displayInfo = Label(right, "Info", new Vector2(0.08f, 0.63f), new Vector2(0.92f, 0.71f), 50, TextAnchor.UpperCenter);
        shopScene.displayTemplate = Button(StackList(right, 0.62f), "Display Template", null, 1800);
        TallRow(shopScene.displayTemplate, 260, 52);
        var dtText = shopScene.displayTemplate.GetComponentInChildren<Text>();
        ((RectTransform)dtText.transform).offsetMin = new Vector2(300, 0);
        dtText.alignment = TextAnchor.MiddleLeft;
        var dtPreview = Preview(shopScene.displayTemplate.transform, new Vector2(0, 0.5f), 220);
        ((RectTransform)dtPreview.transform).anchoredPosition = new Vector2(150, 0);
        // On top of all pages: the OPEN sign, the coins, the page name, arrows and the journal
        var sign = Button(p, "Open Sign", shopScene.TapSign, 560);
        sign.GetComponentInChildren<Text>().text = "OPEN";
        sign.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        sign.GetComponent<Image>().color = new Color(0.55f, 0.80f, 0.55f);
        Place(sign, new Vector2(0.115f, 0.893f), new Vector2(380, 140));
        shopScene.coinText = TopBar(p, day, 0.375f, Ink, content).coinText;
        var shops = Button(p, "Shops", shopScene.ToggleShops, 380);
        Place(shops, new Vector2(0.82f, 0.838f), new Vector2(420, 110));
        var shopsColors = shops.colors;
        shopsColors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.55f);   // greyed out until a shop opens
        shops.colors = shopsColors;
        shops.GetComponentInChildren<Text>().fontSize = 56;
        shopScene.shopsButton = shops.gameObject;
        var shopsLayer = new GameObject("Shops Layer", typeof(RectTransform), typeof(Image), typeof(Button));
        shopsLayer.transform.SetParent(p, false);
        Stretch((RectTransform)shopsLayer.transform);
        shopsLayer.GetComponent<Image>().color = new Color(0, 0, 0, 0.001f);   // invisible, but catches taps outside the menu
        UnityEventTools.AddPersistentListener(shopsLayer.GetComponent<Button>().onClick, shopScene.ToggleShops);
        var shopsMenu = Shape(shopsLayer.transform, "Shops Menu", new Vector2(0.72f, 0.64f), new Vector2(900, 760), new Color(1, 0.97f, 0.9f), uiSprite);
        shopsMenu.GetComponent<Image>().raycastTarget = true;
        shopsMenu.gameObject.AddComponent<ClickBlocker>();
        var sm = Column(shopsMenu, 0.5f, 0.5f);
        var menuMarket = Button(sm, "Menu Market", shopScene.GoToMarket, 800);
        menuMarket.GetComponentInChildren<Text>().text = "Supply Market";
        shopScene.marketButton = menuMarket.gameObject;
        var menuBoutique = Button(sm, "Menu Boutique", shopScene.GoToBoutique, 800);
        menuBoutique.GetComponentInChildren<Text>().text = "The Boutique";
        shopScene.boutiqueButton = menuBoutique.gameObject;
        var menuPatterns = Button(sm, "Menu Patterns", shopScene.GoToPatternShop, 800);
        menuPatterns.GetComponentInChildren<Text>().text = "The Pattern Shop";
        shopScene.patternShopButton = menuPatterns.gameObject;
        Button(sm, "Menu Close", shopScene.ToggleShops, 800).GetComponentInChildren<Text>().text = "Close";
        shopsLayer.SetActive(false);
        shopScene.shopsMenu = shopsLayer;
        shopScene.pageLabel = Label(p, "PageName", new Vector2(0.2f, 0.778f), new Vector2(0.8f, 0.812f), 52, TextAnchor.MiddleCenter);
        shopScene.pageLabel.raycastTarget = false;   // the Shops button sits beside it
        var b = Row(p, 0.5f, 0.05f);
        Button(b, "Pickups Arrow", shopScene.Left, 420).GetComponentInChildren<Text>().text = "< Pickups";
        Button(b, "Journal", shopScene.OpenJournal, 600);
        Button(b, "Display Arrow", shopScene.Right, 420).GetComponentInChildren<Text>().text = "Display >";
        // "Close the shop for the night?"
        var confirm = Shape(p, "Close Confirm", new Vector2(0.5f, 0.5f), new Vector2(1700, 560), new Color(1, 0.97f, 0.9f), uiSprite);
        confirm.GetComponent<Image>().raycastTarget = true;
        var question = Label(confirm, "Question", new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.95f), 60, TextAnchor.MiddleCenter);
        question.text = "Flip the sign to CLOSED and end the day?\nAnyone who hasn't come in yet will visit another day.";
        b = Row(confirm, 0.5f, 0.25f);
        Button(b, "Close the Shop", shopScene.CloseShop, 640);
        Button(b, "Stay Open", shopScene.KeepOpen, 640);
        confirm.gameObject.SetActive(false);
        shopScene.closeConfirm = confirm.gameObject;

        // Workroom (night)
        p = Panel(ScreenId.Workroom, "Workroom", new Color(0.22f, 0.24f, 0.40f), "Art/Backgrounds/bg_workroom_night.png");
        p.Find("Title").GetComponent<Text>().color = Color.white;
        var wTitle = (RectTransform)p.Find("Title");   // below the top bar, at a fraction of the screen so it clears it on any phone
        wTitle.anchorMin = new Vector2(0, 0.79f); wTitle.anchorMax = new Vector2(1, 0.79f);
        wTitle.anchoredPosition = Vector2.zero;
        TopBar(p, day, 0.2f, Color.white, content);
        ui.workroomStatus = Label(p, "Status", new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.82f), 68, TextAnchor.MiddleLeft);
        ui.workroomStatus.color = Color.white;
        b = Column(p, 0.5f, 0.35f);
        Button(b, "Order Book", router.GoToOrderBook, 1100);
        ui.patternStudioButton = Button(b, "Pattern Studio", router.GoToPatternStudio, 1100);
        var psColors = ui.patternStudioButton.colors;
        psColors.disabledColor = new Color(0.55f, 0.55f, 0.6f, 0.6f);
        ui.patternStudioButton.colors = psColors;
        var workroomJournal = Button(b, "Journal", null, 1100);   // wired to the journal overlay below
        Button(b, "Go to Bed", day.GoToBed, 1100);

        // Order book
        p = Panel(ScreenId.OrderBook, "Order Book", new Color(0.96f, 0.92f, 0.84f));
        ui.orderTemplate = Button(ScrollList(p, 0.14f, 0.83f), "Order Template", null, 1840);
        TallRow(ui.orderTemplate, 250, 56);
        ui.orderBookMessage = Label(p, "Message", new Vector2(0.06f, 0.835f), new Vector2(0.94f, 0.92f), 50, TextAnchor.MiddleCenter);
        ui.orderBookEmpty = Label(p, "Empty", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.7f), 64, TextAnchor.MiddleCenter);
        ui.orderBookEmpty.text = "No orders yet. Customers visit the shop during the day.";
        Button(Column(p, 0.5f, 0.06f), "Back", router.GoToHub);

        // Dialogue
        p = Panel(ScreenId.Dialogue, "", new Color(0.80f, 0.86f, 0.93f));
        var dialogue = canvasGo.AddComponent<DialogueUI>();
        day.dialogue = dialogue;
        dialogue.portrait = Portrait(p, new Vector2(0.5f, 0.76f), 900);
        dialogue.patternPreview = Preview(p, new Vector2(0.85f, 0.62f), 300);
        dialogue.nameText = Label(p, "Name", new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.6f), 96, TextAnchor.MiddleLeft);
        dialogue.lineText = Label(p, "Line", new Vector2(0.08f, 0.37f), new Vector2(0.92f, 0.55f), 66, TextAnchor.UpperLeft);
        dialogue.choiceTemplate = Button(Column(p, 0.5f, 0.04f), "Choice", null, 1750);
        dialogue.choiceTemplate.GetComponentInChildren<Text>().fontSize = 56;

        // Workbench
        p = Panel(ScreenId.Workbench, "Workbench", new Color(0.80f, 0.66f, 0.50f), "Art/Backgrounds/bg_workbench.png");
        ui.workbenchText = Label(p, "Order", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.9f), 60, TextAnchor.MiddleCenter);
        b = Column(p, 0.5f, 0.24f);
        var start = Button(b, "Start Crafting", day.StartCrafting, 1100);
        ui.startLabel = start.GetComponentInChildren<Text>();
        ui.startButton = start.gameObject;
        ui.handOverButton = Button(b, "Hand Over", day.HandOver, 1100).gameObject;
        Button(b, "Back to Order Book", router.GoToOrderBook, 1100);

        // Gather supplies: pick what the order needs from the cupboard, ticking off its list (GatherUI)
        p = Panel(ScreenId.Gather, "Gather Supplies", new Color(0.88f, 0.80f, 0.68f));
        var gather = canvasGo.AddComponent<GatherUI>();
        gather.day = day;
        gather.listText = Label(p, "List", new Vector2(0.06f, 0.63f), new Vector2(0.94f, 0.88f), 52, TextAnchor.UpperLeft);
        gather.messageText = Label(p, "Message", new Vector2(0.06f, 0.57f), new Vector2(0.94f, 0.63f), 50, TextAnchor.MiddleCenter);
        var cupboard = ScrollList(p, 0.13f, 0.565f);
        gather.shelf = Shelf(cupboard);
        gather.shelf.name = "Cupboard";
        gather.tileTemplate = SupplyTile(gather.shelf);
        gather.tileTemplate.name = "Cupboard Template";
        b = Row(p, 0.5f, 0.04f);
        gather.startButton = Button(b, "Start Crafting", gather.StartCrafting, 860).gameObject;
        Button(b, "Back", router.GoToWorkbench, 860);

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
        ui.pickerButton = Button(p, "Stitch Picker", minigame.TogglePicker, 700);   // a seam ripper: unpick stitches one by one
        Place(ui.pickerButton, new Vector2(0.5f, 0.115f), new Vector2(760, 120));
        // Finish's pop-up: mistakes left in (fix them, or finish anyway), or why the step can't finish yet
        var finishPanel = Shape(p, "FinishPanel", new Vector2(0.5f, 0.55f), new Vector2(1700, 900), new Color(1f, 0.97f, 0.9f), uiSprite);
        finishPanel.GetComponent<Image>().raycastTarget = true;   // the board underneath can't be touched meanwhile
        ui.finishText = Label(finishPanel, "Text", new Vector2(0.06f, 0.3f), new Vector2(0.94f, 0.94f), 56, TextAnchor.MiddleCenter);
        b = Row(finishPanel, 0.5f, 0.15f);
        ui.fixLabel = Button(b, "Fix It", day.FixMistakes, 700).GetComponentInChildren<Text>();
        ui.finishAnywayButton = Button(b, "Finish Anyway", day.FinishAnyway, 700).gameObject;
        ui.finishPanel = finishPanel.gameObject;
        finishPanel.gameObject.SetActive(false);

        // Result
        p = Panel(ScreenId.Result, "Result", new Color(0.97f, 0.88f, 0.65f));
        ui.resultPortrait = Portrait(p, new Vector2(0.5f, 0.71f), 840);
        ui.resultText = Label(p, "Summary", new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.55f), 72, TextAnchor.MiddleCenter);
        b = Column(p, 0.5f, 0.18f);
        Button(b, "Set Aside", day.SetAside, 1100);
        Button(b, "Keep Working", day.KeepWorking, 1100);
        b = Row(p, 0.5f, 0.275f);   // special touches (filled in by OrderFlowUI)
        ui.extraButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            ui.extraButtons[i] = Button(b, $"Extra {i + 1}", null, 620);
            ui.extraButtons[i].GetComponentInChildren<Text>().fontSize = 48;
        }

        // Side screens
        p = Panel(ScreenId.Market, "Supply Market", new Color(0.85f, 0.70f, 0.60f));
        ui.marketStatus = Label(p, "Status", new Vector2(0.22f, 0.82f), new Vector2(0.96f, 0.92f), 56, TextAnchor.MiddleCenter);
        OwnerPortrait(p, content.Customer(ShopDay.MarketOwnerId), day.ChatWithMarketOwner);
        var shelves = ScrollList(p, 0.14f, 0.81f);
        ui.shelfTitleTemplate = ShelfTitle(shelves);
        ui.shelfTemplate = Shelf(shelves);
        ui.marketTemplate = SupplyTile(ui.shelfTemplate);
        b = Row(p, 0.5f, 0.04f);
        Button(b, "Shopping List", ui.ToggleShoppingList, 500);
        ui.orderLabel = Button(b, "Special Order", ui.ToggleOrdering, 500).GetComponentInChildren<Text>();
        Button(b, "Chat with Bram", day.ChatWithMarketOwner, 400).GetComponentInChildren<Text>().text = "Chat";
        Button(b, "Back", day.LeaveMarket, 360);
        var list = Shape(p, "ShoppingListPanel", new Vector2(0.5f, 0.48f), new Vector2(1840, 1700), new Color(1f, 0.98f, 0.92f), uiSprite);
        list.GetComponent<Image>().raycastTarget = true;
        ui.shoppingList = list.gameObject;
        var listScroll = ScrollList(list, 0.12f, 0.97f);
        ui.shoppingListText = MakeText(listScroll, "ListText", "", 52);
        ui.shoppingListText.alignment = TextAnchor.UpperLeft;
        ui.shoppingListText.horizontalOverflow = HorizontalWrapMode.Wrap;
        ui.shoppingListText.verticalOverflow = VerticalWrapMode.Overflow;
        ui.shoppingListText.color = Ink;
        ui.shoppingListText.gameObject.AddComponent<LayoutElement>().preferredWidth = 1700;
        Button(Column(list, 0.5f, 0.02f), "Close List", ui.ToggleShoppingList, 600).GetComponentInChildren<Text>().text = "Close";
        list.gameObject.SetActive(false);

        // Trade with friends (share codes)
        p = Panel(ScreenId.Trade, "Trade with Friends", new Color(0.78f, 0.88f, 0.95f));
        var trade = canvasGo.AddComponent<TradeUI>();
        trade.day = day;
        var tabRow = Row(p, 0.5f, 0.865f);
        var tabNames = new[] { "Give", "Patterns", "Open", "My Codes" };
        trade.tabs = new Image[tabNames.Length];
        for (int i = 0; i < tabNames.Length; i++)
        {
            var tab = Button(tabRow, tabNames[i], null, 440);
            tab.GetComponentInChildren<Text>().fontSize = 56;
            UnityEventTools.AddIntPersistentListener(tab.onClick, trade.ShowTab, i);
            trade.tabs[i] = tab.GetComponent<Image>();
        }
        trade.infoText = Label(p, "Info", new Vector2(0.06f, 0.69f), new Vector2(0.94f, 0.83f), 52, TextAnchor.UpperLeft);
        trade.rowTemplate = Button(ScrollList(p, 0.33f, 0.68f), "Trade Row Template", null, 1840);
        trade.rowTemplate.GetComponentInChildren<Text>().fontSize = 54;
        trade.codeField = CodeField(p, new Vector2(0.5f, 0.235f));
        b = Row(p, 0.5f, 0.12f);
        trade.actionA = Button(b, "Trade Action A", trade.ActionA, 860);
        trade.actionALabel = trade.actionA.GetComponentInChildren<Text>();
        trade.actionB = Button(b, "Trade Action B", trade.ActionB, 860);
        trade.actionBLabel = trade.actionB.GetComponentInChildren<Text>();
        Button(Column(p, 0.5f, 0.04f), "Back", router.GoToHub);

        p = Panel(ScreenId.Settings, "Settings", new Color(0.78f, 0.78f, 0.82f));
        var settings = canvasGo.AddComponent<SettingsUI>();
        Label(p, "Paused", new Vector2(0.08f, 0.8f), new Vector2(0.92f, 0.86f), 52, TextAnchor.MiddleCenter).text =
            "Paused: the shop waits while you're here. Your game is saved.";
        b = Row(p, 0.5f, 0.74f);
        Button(b, "Music Down", settings.MusicDown, 300).GetComponentInChildren<Text>().text = "-";
        settings.musicLabel = RowLabel(b, 700);
        Button(b, "Music Up", settings.MusicUp, 300).GetComponentInChildren<Text>().text = "+";
        b = Row(p, 0.5f, 0.66f);
        Button(b, "Sound Down", settings.SoundDown, 300).GetComponentInChildren<Text>().text = "-";
        settings.soundLabel = RowLabel(b, 700);
        Button(b, "Sound Up", settings.SoundUp, 300).GetComponentInChildren<Text>().text = "+";
        settings.musicLabel.color = settings.soundLabel.color = Ink;
        var vib = Button(Column(p, 0.5f, 0.585f), "Vibration", settings.ToggleHaptics, 1000);
        settings.hapticsLabel = vib.GetComponentInChildren<Text>();
        b = Column(p, 0.5f, 0.4f);
        Button(b, "How to Play", router.GoToTutorial, 1100);
        Button(b, "Save and Quit to Title", gm.QuitToTitle, 1100);
        Button(b, "Back", router.GoToHub, 1100);

        // Customization: name, pronouns, look (CustomizeUI)
        p = Panel(ScreenId.Customize, "Your Shopkeeper", new Color(0.93f, 0.85f, 0.80f));
        var custom = canvasGo.AddComponent<CustomizeUI>();
        custom.day = day;
        custom.nameField = NameField(p, new Vector2(0.5f, 0.87f));
        custom.nameField.name = "PlayerNameField";
        custom.nameField.characterLimit = 20;
        ((Text)custom.nameField.placeholder).text = "Your name";
        b = Row(p, 0.5f, 0.795f);
        custom.pronounButtons = new Image[PlayerText.Sets.Length];
        for (int i = 0; i < PlayerText.Sets.Length; i++)
        {
            var pb = Button(b, $"Pronouns {i}", null, 520);
            pb.GetComponentInChildren<Text>().text = PlayerText.Sets[i].label;
            UnityEventTools.AddIntPersistentListener(pb.onClick, custom.SetPronouns, i);
            custom.pronounButtons[i] = pb.GetComponent<Image>();
        }
        custom.preview = Keeper(p, new Vector2(0.5f, 0.53f), content);
        custom.preview.transform.localScale = Vector3.one * 0.85f;
        b = Group<VerticalLayoutGroup>(p, 0.5f, 0.27f, 22);
        custom.skinLabel = OptionRow(b, "Skin", custom.NextSkin);
        custom.hairStyleLabel = OptionRow(b, "Hair Style", custom.NextHairStyle);
        custom.hairColorLabel = OptionRow(b, "Hair Colour", custom.NextHairColor);
        custom.topLabel = OptionRow(b, "Top", custom.NextTop);
        custom.topColorLabel = OptionRow(b, "Top Colour", custom.NextTopColor);
        custom.madeLabel = OptionRow(b, "Your Makes", custom.NextMade);
        custom.messageText = Label(p, "Message", new Vector2(0.06f, 0.075f), new Vector2(0.94f, 0.1f), 52, TextAnchor.MiddleCenter);
        Button(Column(p, 0.5f, 0.01f), "Done", custom.Done, 800);

        // The Sunday market stall (StallUI): the table, the shopkeeper behind it, and the shopper in front
        p = Panel(ScreenId.Stall, "", new Color(0.78f, 0.88f, 0.95f));
        var stall = canvasGo.AddComponent<StallUI>();
        stall.day = day;
        stall.clockText = Label(p, "Clock", new Vector2(0.05f, 0.9f), new Vector2(0.95f, 0.97f), 58, TextAnchor.MiddleCenter);
        var awning = Shape(p, "Awning", new Vector2(0.5f, 0.84f), new Vector2(1900, 160), new Color(0.90f, 0.45f, 0.45f), uiSprite);
        Shape(awning, "Stripe", new Vector2(0.5f, 0.5f), new Vector2(1900, 50), new Color(1f, 0.95f, 0.9f), uiSprite);
        stall.keeper = Keeper(p, new Vector2(0.5f, 0.66f), content);
        stall.keeper.transform.localScale = Vector3.one * 0.7f;
        var table = Shape(p, "Table", new Vector2(0.5f, 0.5f), new Vector2(1800, 480), new Color(0.62f, 0.45f, 0.30f), uiSprite);
        stall.tableText = Label(table, "OnTable", new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.94f), 46, TextAnchor.UpperLeft);
        stall.tableText.color = Color.white;
        var shopperCard = Shape(p, "Shopper", new Vector2(0.5f, 0.27f), new Vector2(1840, 560), new Color(1f, 0.98f, 0.92f), uiSprite);
        stall.shopperText = Label(shopperCard, "Line", new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.95f), 54, TextAnchor.MiddleCenter);
        b = Row(shopperCard, 0.5f, 0.2f);
        stall.sellButton = Button(b, "Stall Sell", stall.Sell, 520).gameObject;
        stall.sellButton.GetComponentInChildren<Text>().text = "Sell";
        stall.haggleButton = Button(b, "Stall Haggle", stall.Haggle, 560).gameObject;
        stall.haggleButton.GetComponentInChildren<Text>().text = "Ask for More";
        stall.declineLabel = Button(b, "Stall Decline", stall.Decline, 520).GetComponentInChildren<Text>();
        stall.shopperPanel = shopperCard.gameObject;
        stall.messageText = Label(p, "Message", new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.16f), 50, TextAnchor.MiddleCenter);
        Button(Column(p, 0.5f, 0.05f), "Pack Up the Stall", stall.PackUp, 900);

        // Finch's Pattern Parlour: ready-made patterns to sell (PatternShopUI)
        p = Panel(ScreenId.PatternShop, "Finch's Pattern Parlour", new Color(0.80f, 0.86f, 0.78f));
        var pshop = canvasGo.AddComponent<PatternShopUI>();
        pshop.day = day;
        pshop.statusText = Label(p, "Status", new Vector2(0.22f, 0.79f), new Vector2(0.96f, 0.92f), 52, TextAnchor.MiddleCenter);
        OwnerPortrait(p, content.Customer(ShopDay.PatternShopOwnerId), pshop.Chat);
        pshop.rowTemplate = Button(ScrollList(p, 0.12f, 0.77f), "PatternShop Template", null, 1840);
        TallRow(pshop.rowTemplate, 280, 48);
        var psText = pshop.rowTemplate.GetComponentInChildren<Text>();
        ((RectTransform)psText.transform).offsetMin = new Vector2(300, 0);
        psText.alignment = TextAnchor.MiddleLeft;
        var psPreview = Preview(pshop.rowTemplate.transform, new Vector2(0, 0.5f), 230);
        ((RectTransform)psPreview.transform).anchoredPosition = new Vector2(150, 0);
        b = Row(p, 0.5f, 0.04f);
        pshop.orderLabel = Button(b, "Order a Pattern", pshop.ToggleOrdering, 640).GetComponentInChildren<Text>();
        Button(b, "Chat with Ada", pshop.Chat, 520).GetComponentInChildren<Text>().text = "Chat";
        Button(b, "Back", pshop.Leave, 520);

        // Moreau's boutique: hair, tops, accessories (BoutiqueUI)
        p = Panel(ScreenId.Boutique, "Moreau's Boutique", new Color(0.92f, 0.82f, 0.88f));
        var boutique = canvasGo.AddComponent<BoutiqueUI>();
        boutique.day = day;
        boutique.statusText = Label(p, "Status", new Vector2(0.22f, 0.82f), new Vector2(0.96f, 0.92f), 56, TextAnchor.MiddleCenter);
        OwnerPortrait(p, content.Customer(BoutiqueUI.OwnerId), boutique.Chat);
        boutique.preview = Keeper(p, new Vector2(0.5f, 0.64f), content);
        boutique.preview.transform.localScale = Vector3.one * 0.7f;
        var rails = ScrollList(p, 0.13f, 0.47f);
        boutique.shelf = Shelf(rails);
        boutique.shelf.name = "Rail";
        boutique.tileTemplate = SupplyTile(boutique.shelf);
        boutique.tileTemplate.name = "Cosmetic Template";
        // Trying something on: the mirror shows it; buy it or put it back
        var tryRow = Shape(p, "TryOn", new Vector2(0.5f, 0.505f), new Vector2(1840, 300), new Color(1f, 0.95f, 0.98f), uiSprite);
        boutique.tryText = Label(tryRow, "Text", new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.95f), 50, TextAnchor.MiddleCenter);
        b = Row(tryRow, 0.5f, 0.3f);
        Button(b, "Buy It", boutique.BuyTried, 640).GetComponent<LayoutElement>().preferredHeight = 130;
        Button(b, "Put It Back", boutique.PutBack, 640).GetComponent<LayoutElement>().preferredHeight = 130;
        boutique.tryPanel = tryRow.gameObject;
        tryRow.gameObject.SetActive(false);
        b = Row(p, 0.5f, 0.04f);
        Button(b, "Chat with Celeste", boutique.Chat, 860);
        Button(b, "Back", boutique.Leave, 860);

        p = Panel(ScreenId.DaySummary, "End of Day", new Color(0.78f, 0.82f, 0.95f));
        ui.summaryText = Label(p, "Summary", new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.88f), 60, TextAnchor.MiddleCenter);
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
        b = Column(p, 0.28f, 0.2f);
        Button(b, "Price Up", day.PriceUp, 860);
        Button(b, "Price Down", day.PriceDown, 860);
        ui.editDesignButton = Button(b, "Edit Design", day.EditDesign, 860).gameObject;
        ui.makeForMeButton = Button(b, "Make One for Me", day.MakeForMyself, 860).gameObject;
        ui.makeForStallButton = Button(b, "Make One for the Stall", day.MakeForStall, 860).gameObject;
        b = Column(p, 0.72f, 0.2f);
        ui.shelfLabel = Button(b, "Shelf", day.ToggleShelf, 860).GetComponentInChildren<Text>();
        var delete = Button(b, "Delete", day.DeletePattern, 860);
        ui.deleteButton = delete.gameObject;
        ui.deleteLabel = delete.GetComponentInChildren<Text>();
        Button(b, "Back to Studio", router.GoToPatternStudio, 860);

        // Pattern designer: transparent so the paint grid shows through
        p = Panel(ScreenId.PatternDesigner, "", Color.clear);
        p.GetComponent<Image>().raycastTarget = false;
        // Naming pop-up, when the design is done
        var namePop = Shape(p, "Name Popup", new Vector2(0.5f, 0.6f), new Vector2(1800, 760), new Color(1, 0.97f, 0.9f), uiSprite);
        namePop.GetComponent<Image>().raycastTarget = true;
        Label(namePop, "Question", new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.95f), 64, TextAnchor.MiddleCenter).text = "Name your pattern";
        designer.nameField = NameField(namePop, new Vector2(0.5f, 0.55f));
        var nb = Row(namePop, 0.5f, 0.2f);
        Button(nb, "Save to Book", designer.ConfirmSave, 700);
        Button(nb, "Keep Designing", designer.KeepDesigning, 700);
        designer.namePopup = namePop.gameObject;
        namePop.gameObject.SetActive(false);
        designer.swatches = Swatches(p, 0.31f, Materials.Threads(content).Count, true, 96, 12);   // 12 a row: room for 35 (3 rows)
        designer.swatches.label.color = Color.white;
        UnityEventTools.AddPersistentListener(designer.swatches.onPick, designer.SelectColour);
        b = Row(p, 0.5f, 0.218f);
        designer.makesLabel = Button(b, "Makes", designer.NextItem, 900).GetComponentInChildren<Text>();
        designer.sizeLabel = Button(b, "Size", designer.NextSize, 760).GetComponentInChildren<Text>();
        b = Row(p, 0.5f, 0.155f);
        Button(b, "Price Down", designer.PriceDown, 330).GetComponentInChildren<Text>().text = "Price -";
        designer.priceLabel = RowLabel(b, 360);
        Button(b, "Price Up", designer.PriceUp, 330).GetComponentInChildren<Text>().text = "Price +";
        designer.clothLabel = Button(b, "Cloth", designer.NextCloth, 780).GetComponentInChildren<Text>();   // cross stitch: the cloth it's on
        designer.clothLabel.fontSize = 46;
        b = Row(p, 0.5f, 0.06f);
        Button(b, "Undo", designer.Undo, 360);
        Button(b, "Clear", designer.Clear, 360);
        designer.halfLabel = Button(b, "Half", designer.ToggleHalf, 360).GetComponentInChildren<Text>();
        designer.halfLabel.text = "Half: off";
        Button(b, "Done", designer.Save, 360);
        Button(b, "Cancel", designer.Cancel, 360);

        // The journal: a storybook overlay that slides in over any screen (JournalUI)
        var journal = canvasGo.AddComponent<JournalUI>();
        journal.day = day;
        journal.router = router;
        var overlay = new GameObject("JournalOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        var ort = (RectTransform)overlay.transform;
        ort.SetParent(canvasGo.transform, false);
        Stretch(ort);
        overlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.45f);
        UnityEventTools.AddPersistentListener(overlay.GetComponent<Button>().onClick, journal.Close);   // tap outside the book
        journal.overlay = overlay;
        var bookGo = new GameObject("Book", typeof(RectTransform), typeof(Image));
        var bookRt = (RectTransform)bookGo.transform;
        bookRt.SetParent(ort, false);
        bookRt.anchorMin = new Vector2(0.04f, 0.03f); bookRt.anchorMax = new Vector2(0.99f, 0.97f);
        bookRt.offsetMin = bookRt.offsetMax = Vector2.zero;
        var cover = bookGo.GetComponent<Image>();
        cover.sprite = uiSprite;
        cover.type = Image.Type.Sliced;
        cover.color = new Color(0.55f, 0.32f, 0.25f);   // the cover
        bookGo.AddComponent<ClickBlocker>();   // taps on the book itself never close it
        journal.book = bookRt;
        var paper = Shape(bookRt, "Paper", new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0.98f, 0.95f, 0.87f), uiSprite);
        paper.anchorMin = new Vector2(0.035f, 0.012f); paper.anchorMax = new Vector2(0.985f, 0.988f);
        paper.offsetMin = paper.offsetMax = Vector2.zero;
        var spine = Shape(bookRt, "Spine", new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0.42f, 0.24f, 0.18f), uiSprite);
        spine.anchorMin = new Vector2(0, 0); spine.anchorMax = new Vector2(0.03f, 1);
        spine.offsetMin = spine.offsetMax = Vector2.zero;
        Label(paper, "BookTitle", new Vector2(0.05f, 0.93f), new Vector2(0.95f, 0.985f), 84, TextAnchor.MiddleCenter).text = "Journal";
        var jTabRows = new[] { Row(paper, 0.5f, 0.9f), Row(paper, 0.5f, 0.853f) };   // two rows (four, then three), inside the page
        var jTabNames = new[] { "Orders", "Story", "Friends", "Skills", "Supplies", "Stamps", "Goals" };
        journal.tabs = new Image[jTabNames.Length];
        for (int i = 0; i < jTabNames.Length; i++)
        {
            var tab = Button(jTabRows[i / 4], "Tab " + jTabNames[i], null, 390);
            tab.GetComponent<LayoutElement>().preferredHeight = 110;
            tab.GetComponentInChildren<Text>().text = jTabNames[i];
            tab.GetComponentInChildren<Text>().fontSize = 46;
            UnityEventTools.AddIntPersistentListener(tab.onClick, journal.ShowPage, i);
            journal.tabs[i] = tab.GetComponent<Image>();
        }
        var jPageGo = new GameObject("Page", typeof(RectTransform), typeof(Image));
        var jPage = (RectTransform)jPageGo.transform;
        jPage.SetParent(paper, false);
        jPage.anchorMin = new Vector2(0.02f, 0.1f); jPage.anchorMax = new Vector2(0.98f, 0.82f);
        jPage.offsetMin = jPage.offsetMax = Vector2.zero;
        jPage.pivot = new Vector2(0, 0.5f);   // turns over at the spine
        jPageGo.GetComponent<Image>().color = new Color(1f, 0.985f, 0.94f);
        journal.page = jPage;
        var journalList = ScrollList(jPage, 0.02f, 0.98f);
        journal.pageText = MakeText(journalList, "PageText", "", 52);
        journal.pageText.alignment = TextAnchor.UpperLeft;
        journal.pageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        journal.pageText.verticalOverflow = VerticalWrapMode.Overflow;
        journal.pageText.gameObject.AddComponent<LayoutElement>().preferredWidth = 1650;
        journal.rowTemplate = Button(journalList, "Journal Row Template", null, 1650);
        journal.rowTemplate.GetComponentInChildren<Text>().fontSize = 50;
        var rowFace = Shape(journal.rowTemplate.transform, "Portrait", new Vector2(0, 0.5f), new Vector2(120, 120), new Color(0.85f, 0.78f, 0.68f), Knob());
        rowFace.pivot = new Vector2(0, 0.5f);
        rowFace.anchoredPosition = new Vector2(24, 0);
        rowFace.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        Shape(rowFace, "Face", new Vector2(0.5f, 0.5f), new Vector2(112, 112), Color.white, null).GetComponent<Image>().preserveAspect = true;
        rowFace.gameObject.SetActive(false);   // shown on rows about a person (JournalUI.WithFace)
        // A friend's card: name, a meter towards the next level, what you know about them
        var friendCard = new GameObject("Friend Template", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        friendCard.transform.SetParent(journalList, false);
        friendCard.GetComponent<Image>().color = new Color(0.97f, 0.93f, 0.85f);
        var fv = friendCard.GetComponent<VerticalLayoutGroup>();
        fv.padding = new RectOffset(270, 30, 24, 24);   // room on the left for their portrait
        fv.spacing = 14;
        fv.childControlWidth = fv.childControlHeight = true;
        fv.childForceExpandWidth = true; fv.childForceExpandHeight = false;
        friendCard.GetComponent<LayoutElement>().preferredWidth = 1650;
        friendCard.GetComponent<LayoutElement>().minHeight = 260;
        // A round portrait left of the name: a circle that masks the face inside it
        var fFrame = Shape(friendCard.transform, "Portrait", new Vector2(0, 1), new Vector2(210, 210), new Color(0.85f, 0.78f, 0.68f), Knob());
        fFrame.pivot = new Vector2(0, 1);
        fFrame.anchoredPosition = new Vector2(30, -24);
        fFrame.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        fFrame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        var fFace = Shape(fFrame, "Face", new Vector2(0.5f, 0.5f), new Vector2(196, 196), Color.white, null);
        fFace.GetComponent<Image>().preserveAspect = true;
        var fUnknown = MakeText(fFrame, "Unknown", "?", 130);
        Stretch((RectTransform)fUnknown.transform);
        fUnknown.color = new Color(0.97f, 0.93f, 0.85f);
        var fName = MakeText(friendCard.transform, "Name", "", 52);
        fName.alignment = TextAnchor.MiddleLeft; fName.color = Ink;
        var fBar = new GameObject("Bar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        fBar.transform.SetParent(friendCard.transform, false);
        fBar.GetComponent<Image>().color = new Color(0.85f, 0.80f, 0.72f);
        fBar.GetComponent<LayoutElement>().preferredHeight = 46;
        var fFill = Shape(fBar.transform, "Fill", Vector2.zero, Vector2.zero, new Color(0.88f, 0.58f, 0.25f), uiSprite);
        fFill.anchorMin = Vector2.zero; fFill.anchorMax = new Vector2(0.5f, 1); fFill.offsetMin = fFill.offsetMax = Vector2.zero;
        var fLabel = MakeText(fBar.transform, "Label", "", 34);
        Stretch((RectTransform)fLabel.transform);
        fLabel.color = Ink;
        var fBody = MakeText(friendCard.transform, "Body", "", 44);
        fBody.alignment = TextAnchor.UpperLeft; fBody.color = Ink;
        fBody.horizontalOverflow = HorizontalWrapMode.Wrap; fBody.verticalOverflow = VerticalWrapMode.Overflow;
        journal.friendTemplate = friendCard;
        friendCard.SetActive(false);
        var gridGo = new GameObject("StampGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        gridGo.transform.SetParent(journalList, false);
        var grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(380, 470);
        grid.spacing = new Vector2(30, 30);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.UpperCenter;
        gridGo.GetComponent<LayoutElement>().preferredWidth = 1650;
        var gridFit = gridGo.AddComponent<ContentSizeFitter>();
        gridFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        journal.stampGrid = (RectTransform)gridGo.transform;
        var stampT = new GameObject("Stamp Template", typeof(RectTransform));
        stampT.transform.SetParent(gridGo.transform, false);
        var ringOuter = Shape(stampT.transform, "Ring", new Vector2(0.5f, 0.72f), new Vector2(250, 250), Color.gray, Knob());
        Shape(ringOuter, "Inner", new Vector2(0.5f, 0.5f), new Vector2(200, 200), new Color(1, 1, 1, 0.25f), Knob());
        var word = MakeText(ringOuter, "Word", "", 46);
        Stretch((RectTransform)word.transform);
        word.color = Color.white;
        word.fontStyle = FontStyle.Bold;
        var st = Label(stampT.transform, "Title", new Vector2(0f, 0.26f), new Vector2(1f, 0.44f), 40, TextAnchor.MiddleCenter);
        st.fontStyle = FontStyle.Bold;
        st.color = Ink;
        var sd = Label(stampT.transform, "Detail", new Vector2(0f, 0f), new Vector2(1f, 0.26f), 32, TextAnchor.UpperCenter);
        sd.color = Ink;
        journal.stampTemplate = stampT;
        stampT.SetActive(false);
        gridGo.SetActive(false);
        b = Row(paper, 0.5f, 0.05f);
        // The stores open from the Shops menu by the coins (owner, 2026-10-31), not from the journal.
        Button(b, "Journal Trade", journal.GoToTrade, 330).GetComponentInChildren<Text>().text = "Trade";
        Button(b, "Journal Close", journal.Close, 330).GetComponentInChildren<Text>().text = "Close";
        foreach (var t in b.GetComponentsInChildren<Text>()) t.fontSize = 50;
        overlay.SetActive(false);
        shopScene.journal = journal;
        UnityEventTools.AddPersistentListener(workroomJournal.onClick, journal.Open);

        // Stamp pop-up (achievements) and notices like the doorbell (StampPopup)
        var popup = canvasGo.AddComponent<StampPopup>();
        var popPanel = new GameObject("StampPopup", typeof(RectTransform), typeof(Image), typeof(Button));
        var popRt = (RectTransform)popPanel.transform;
        popRt.SetParent(canvasGo.transform, false);
        Stretch(popRt);
        popPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.25f);
        UnityEventTools.AddPersistentListener(popPanel.GetComponent<Button>().onClick, popup.Dismiss);   // tap anywhere
        var popCard = Shape(popRt, "Card", new Vector2(0.5f, 0.62f), new Vector2(1500, 560), new Color(1f, 0.97f, 0.9f), uiSprite);
        popup.panel = popPanel;
        popup.card = popCard;
        popup.stamp = Shape(popCard, "Stamp", new Vector2(0.18f, 0.5f), new Vector2(300, 300), Color.gray, Knob()).GetComponent<Image>();
        Shape(popup.stamp.transform, "Inner", new Vector2(0.5f, 0.5f), new Vector2(240, 240), new Color(1, 1, 1, 0.25f), Knob());
        popup.stampWord = MakeText(popup.stamp.transform, "Word", "", 52);
        Stretch((RectTransform)popup.stampWord.transform);
        popup.stampWord.color = Color.white;
        popup.stampWord.fontStyle = FontStyle.Bold;
        popup.titleText = Label(popCard, "Title", new Vector2(0.36f, 0.5f), new Vector2(0.96f, 0.92f), 64, TextAnchor.MiddleLeft);
        popup.titleText.fontStyle = FontStyle.Bold;
        popup.titleText.color = Ink;
        popup.detailText = Label(popCard, "Detail", new Vector2(0.36f, 0.08f), new Vector2(0.96f, 0.5f), 48, TextAnchor.UpperLeft);
        popup.detailText.color = Ink;
        popPanel.SetActive(false);

        // Explainers: a card for things seen the first time (a step, a shop, a craft), read before going on
        var explainer = canvasGo.AddComponent<ExplainerUI>();
        var exPanel = new GameObject("ExplainerPanel", typeof(RectTransform), typeof(Image));
        var exRt = (RectTransform)exPanel.transform;
        exRt.SetParent(canvasGo.transform, false);
        Stretch(exRt);
        exPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.45f);   // blocks the work underneath
        var exCard = Shape(exRt, "Card", new Vector2(0.5f, 0.55f), new Vector2(1760, 1100), new Color(1f, 0.97f, 0.9f), uiSprite);
        explainer.titleText = Label(exCard, "Title", new Vector2(0.06f, 0.8f), new Vector2(0.94f, 0.94f), 66, TextAnchor.MiddleCenter);
        explainer.titleText.fontStyle = FontStyle.Bold;
        explainer.bodyText = Label(exCard, "Body", new Vector2(0.07f, 0.24f), new Vector2(0.93f, 0.78f), 52, TextAnchor.UpperLeft);
        explainer.pageLabel = Label(exCard, "PageNumber", new Vector2(0.06f, 0.05f), new Vector2(0.3f, 0.18f), 44, TextAnchor.MiddleLeft);
        var exNext = Button(exCard, "Explainer Next", explainer.Continue, 560);
        Place(exNext, new Vector2(0.72f, 0.11f), new Vector2(560, 150));
        explainer.nextLabel = exNext.GetComponentInChildren<Text>();
        explainer.panel = exPanel;
        exPanel.SetActive(false);

        router.screens = entries.ToArray();

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettingsFirst(ScenePath);
        Debug.Log($"Test scene built at {ScenePath}.");
    }

    // ---------- building blocks ----------

    /// The player's shopkeeper (placeholder shapes, coloured by ShopkeeperView from the save).
    static ShopkeeperView Keeper(Transform parent, Vector2 anchor, ContentLibrary content)
    {
        var body = Shape(parent, "Shopkeeper", anchor, new Vector2(460, 600), new Color(0.45f, 0.55f, 0.75f), uiSprite);
        var view = body.gameObject.AddComponent<ShopkeeperView>();
        view.content = content;
        view.body = body.GetComponent<Image>();
        view.hairBack = Shape(body, "HairBack", new Vector2(0.5f, 1.2f), new Vector2(360, 300), Color.black, Knob()).GetComponent<Image>();
        view.apron = Shape(body, "Apron", new Vector2(0.5f, 0.55f), new Vector2(260, 420), Color.white, uiSprite).GetComponent<Image>();
        view.head = Shape(body, "Head", new Vector2(0.5f, 1.25f), new Vector2(300, 300), new Color(0.96f, 0.82f, 0.70f), Knob()).GetComponent<Image>();
        view.hair = Shape(body, "Hair", new Vector2(0.5f, 1.42f), new Vector2(320, 150), new Color(0.40f, 0.26f, 0.18f), Knob()).GetComponent<Image>();
        view.accessory = Shape(body, "Accessory", new Vector2(0.5f, 1.27f), new Vector2(230, 50), Color.black, uiSprite).GetComponent<Image>();
        foreach (var img in body.GetComponentsInChildren<Image>()) img.raycastTarget = false;
        return view;
    }

    /// "Skin 2  [>]": a label and a button that steps to the next option.
    static Text OptionRow(Transform column, string name, UnityAction next)
    {
        var row = new GameObject($"Option_{name.Replace(" ", "")}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(column, false);
        var h = row.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 30; h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = false;
        var label = RowLabel(row.transform, 900);
        label.color = Ink;
        label.GetComponent<LayoutElement>().preferredHeight = 120;
        var nextButton = Button(row.transform, $"Next {name}", next, 300);
        nextButton.GetComponentInChildren<Text>().text = ">";
        nextButton.GetComponent<LayoutElement>().preferredHeight = 120;
        return label;
    }

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

        go.AddComponent<ButtonSound>();
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
    /// Up to "columns" per row (8 by default); "cell" is the button size, smaller when there are many colours.
    static ThreadSwatches Swatches(Transform panel, float y, int count, bool eraser, float cell = 170, int columns = 8)
    {
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
            var t = MakeText(b.transform, "Label", "Erase", cell < 120 ? 28 : 40);
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
        go.AddComponent<ButtonSound>();
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

    /// A tall text box for share codes: wraps long codes, selectable, no length limit.
    static InputField CodeField(Transform panel, Vector2 anchor)
    {
        var field = NameField(panel, anchor);
        field.name = "CodeField";
        ((RectTransform)field.transform).sizeDelta = new Vector2(1840, 300);
        field.characterLimit = 0;
        field.lineType = InputField.LineType.MultiLineSubmit;
        foreach (var t in field.GetComponentsInChildren<Text>())
        {
            t.fontSize = 40;
            t.alignment = TextAnchor.UpperLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
        }
        ((Text)field.placeholder).text = "A friend's code goes here";
        return field;
    }

    /// An audio file from Assets/_Project/Audio by name without extension (.ogg, .wav or .mp3), or null.
    static AudioClip Clip(string path)
    {
        foreach (var ext in new[] { ".ogg", ".wav", ".mp3" })
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{Root}/Audio/{path}{ext}");
            if (clip != null) return clip;
        }
        return null;
    }

    /// A plain coloured shape (placeholder art), centred at a fraction of its parent.
    /// The shop owner behind their counter, top left of their shop: tap them to chat.
    static void OwnerPortrait(Transform parent, CustomerDef owner, UnityAction chat)
    {
        var face = owner != null ? owner.GetPortrait(Expression.Neutral) : null;
        var rt = Shape(parent, "Owner", new Vector2(0.115f, 0.87f), new Vector2(330, 330), face != null ? Color.white : new Color(0.6f, 0.5f, 0.45f), face != null ? face : uiSprite);
        rt.GetComponent<Image>().preserveAspect = true;
        rt.GetComponent<Image>().raycastTarget = true;
        UnityEventTools.AddPersistentListener(rt.gameObject.AddComponent<Button>().onClick, chat);
    }

    static RectTransform Shape(Transform parent, string name, Vector2 anchor, Vector2 size, Color color, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = sprite == uiSprite ? Image.Type.Sliced : Image.Type.Simple;
        img.color = color;
        img.raycastTarget = false;
        return rt;
    }

    /// The top bar (UI/TopBar.cs) along a hub screen: the weekday badge (centred at x = weekdayX), coins, the
    /// settings gear at the top right, and the day and chapter beneath.
    static TopBar TopBar(Transform panel, ShopDay day, float weekdayX, Color textColor, ContentLibrary content)
    {
        var bar = panel.gameObject.AddComponent<TopBar>();
        bar.day = day;
        const float top = 0.958f, y = 0.893f;   // the personal area and the gear on top; the day and coins below
        // Top left (owner, 2026-10-31): your portrait, your level and an experience bar. Tap = your look and your shop.
        var me = Button(panel, "Personal", bar.OpenPersonal, 760);
        Place(me, new Vector2(0.215f, top), new Vector2(780, 170));
        me.GetComponent<Image>().color = new Color(1, 1, 1, 0.85f);
        var meText = me.GetComponentInChildren<Text>();
        meText.alignment = TextAnchor.UpperLeft;
        meText.fontSize = 52;
        var meTextRt = (RectTransform)meText.transform;
        meTextRt.anchorMin = Vector2.zero; meTextRt.anchorMax = Vector2.one;
        meTextRt.offsetMin = new Vector2(200, 60); meTextRt.offsetMax = new Vector2(-20, -10);
        bar.levelText = meText;
        var frame = Shape(me.transform, "Portrait", new Vector2(0, 0.5f), new Vector2(160, 160), new Color(0.85f, 0.78f, 0.68f), Knob());
        frame.anchoredPosition = new Vector2(95, 0);
        frame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        frame.GetComponent<Image>().raycastTarget = false;
        bar.portrait = Keeper(frame, new Vector2(0.5f, 0.5f), content);
        bar.portrait.transform.localScale = Vector3.one * 0.32f;
        ((RectTransform)bar.portrait.transform).anchoredPosition = new Vector2(0, -150);
        var track = Shape(me.transform, "XpBar", new Vector2(0, 0), new Vector2(540, 44), new Color(0.80f, 0.74f, 0.66f), uiSprite);
        track.pivot = new Vector2(0, 0.5f);
        track.anchoredPosition = new Vector2(200, 46);
        var fill = Shape(track, "Fill", new Vector2(0.5f, 0.5f), new Vector2(540, 44), new Color(0.45f, 0.70f, 0.50f), null);
        bar.xpFill = fill.GetComponent<Image>();
        fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;   // TopBar narrows it to the progress
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        track.GetComponent<Image>().raycastTarget = bar.xpFill.raycastTarget = false;
        var week = Shape(panel, "Weekday", new Vector2(weekdayX, y), new Vector2(470, 150), new Color(1, 1, 1, 0.85f), uiSprite);
        var badge = Shape(week, "Badge", new Vector2(0.16f, 0.5f), new Vector2(130, 130), Color.white, Knob());
        bar.weekdayBadge = badge.GetComponent<Image>();
        bar.weekdaySymbol = Shape(badge, "Symbol", new Vector2(0.5f, 0.5f), new Vector2(92, 92), Color.white, null).GetComponent<Image>();
        bar.weekdayName = MakeText(week, "Name", "", 54);
        Stretch((RectTransform)bar.weekdayName.transform);
        ((RectTransform)bar.weekdayName.transform).offsetMin = new Vector2(140, 0);
        bar.symbols = new Sprite[7];
        for (int i = 0; i < 7; i++) bar.symbols[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/Art/UI/Weekdays/{Week.SymbolFile(i)}");
        var coins = Shape(panel, "Coins", new Vector2(0.665f, y), new Vector2(420, 150), new Color(1, 1, 1, 0.85f), uiSprite);
        Shape(coins, "CoinIcon", new Vector2(0.18f, 0.5f), new Vector2(105, 105), new Color(0.98f, 0.80f, 0.25f), Knob());
        bar.coinText = MakeText(coins, "CoinText", "0", 70);
        Stretch((RectTransform)bar.coinText.transform);
        ((RectTransform)bar.coinText.transform).offsetMin = new Vector2(140, 0);
        var gear = Button(panel, "Settings Gear", bar.OpenSettings, 150);
        Place(gear, new Vector2(0.915f, top), new Vector2(150, 150));
        gear.GetComponentInChildren<Text>().text = "";
        var gearIcon = Shape(gear.transform, "Gear", new Vector2(0.5f, 0.5f), new Vector2(118, 118), Ink, AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/Art/UI/icon_gear.png"));
        if (gearIcon.GetComponent<Image>().sprite == null) Object.DestroyImmediate(gearIcon.gameObject);
        if (gearIcon == null) gear.GetComponentInChildren<Text>().text = "Set";
        bar.dayText = Label(panel, "DayChapter", new Vector2(0.04f, 0.812f), new Vector2(0.68f, 0.862f), 54, TextAnchor.MiddleLeft);
        bar.dayText.color = textColor;
        return bar;
    }

    static Sprite Knob() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

    /// Moves a laid-out element (e.g. a Button) to a fixed spot and size of its own.
    static void Place(Component c, Vector2 anchor, Vector2 size)
    {
        var le = c.GetComponent<LayoutElement>();
        if (le != null) Object.DestroyImmediate(le);
        var rt = (RectTransform)c.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
    }

    /// A column of rows hanging from height "top" (no scrolling, so sideways swipes still turn pages).
    static Transform StackList(Transform panel, float top)
    {
        var go = new GameObject("Rows", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var rt = (RectTransform)go.transform;
        rt.SetParent(panel, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, top);
        rt.pivot = new Vector2(0.5f, 1);
        var layout = go.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rt;
    }

    static void TallRow(Button row, float height, int fontSize)
    {
        row.GetComponent<LayoutElement>().preferredHeight = height;
        row.GetComponentInChildren<Text>().fontSize = fontSize;
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
