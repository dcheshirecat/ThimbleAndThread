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
/// Builds a playable test scene: one placeholder panel per ScreenId, switched by ScreenRouter,
/// plus the order loop (customer, order, cross stitch minigame, result, delivery).
/// Build never overwrites an existing scene. Rebuild replaces it (the scene is fully generated,
/// so only use Rebuild if nobody has hand-edited it).
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

        // ---- Order loop ----
        var shop = new GameObject("ShopManager").AddComponent<ShopManager>();
        shop.catalog = LoadAll<SupplyDef>($"{Root}/Data/Supplies");

        var minigame = new GameObject("MinigameHost").AddComponent<MinigameHost>();
        minigame.cam = cam;
        minigame.material = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);

        var day = new GameObject("ShopDay").AddComponent<ShopDay>();
        day.customers = LoadAll<CustomerDef>($"{Root}/Data/Customers");
        day.items = LoadAll<CraftItemDef>($"{Root}/Data/Items");
        day.shop = shop;
        day.minigame = minigame;

        // ---- UI ----
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2732, 2048);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        var router = canvasGo.AddComponent<ScreenRouter>();
        var ui = canvasGo.AddComponent<OrderFlowUI>();
        ui.day = day;
        var slots = canvasGo.AddComponent<SaveSlotsUI>();

        var entries = new List<ScreenRouter.Entry>();
        RectTransform Panel(ScreenId id, string title, Color color, string bgSprite = null)
        {
            var root = MakePanel(canvasGo.transform, id, title, color, bgSprite);
            entries.Add(new ScreenRouter.Entry { id = id, root = root.gameObject });
            root.gameObject.SetActive(id == ScreenId.Title);
            return root;
        }

        var p = Panel(ScreenId.Title, "Thimble & Thread", new Color(0.30f, 0.22f, 0.35f));
        p.Find("Title").GetComponent<Text>().color = Color.white;
        var b = Buttons(p, new Vector2(0.5f, 0.45f));
        Button(b, "Play", router.GoToSaveSelect);

        p = Panel(ScreenId.SaveSelect, "Choose a Save", new Color(0.88f, 0.84f, 0.92f));
        for (int i = 0; i < SaveSystem.SlotCount; i++)
        {
            float y = 0.68f - i * 0.17f;
            var row = new SaveSlotsUI.Row();
            row.summary = Label(p, $"Slot{i}", new Vector2(0.06f, y - 0.07f), new Vector2(0.52f, y + 0.07f), 56, TextAnchor.MiddleLeft);
            var play = Button(Buttons(p, new Vector2(0.64f, y)), $"Play Slot {i + 1}", null, 520);
            UnityEventTools.AddIntPersistentListener(play.onClick, slots.Play, i);
            row.playLabel = play.GetComponentInChildren<Text>();
            row.deleteButton = Button(Buttons(p, new Vector2(0.86f, y)), $"Delete Slot {i + 1}", null, 560);
            UnityEventTools.AddIntPersistentListener(row.deleteButton.onClick, slots.Delete, i);
            row.deleteLabel = row.deleteButton.GetComponentInChildren<Text>();
            slots.rows[i] = row;
        }
        Button(Buttons(p, new Vector2(0.5f, 0.08f)), "Back", router.GoToTitle);

        p = Panel(ScreenId.Shop, "Shop", new Color(0.95f, 0.88f, 0.78f), "Art/Backgrounds/bg_shop_day.png");
        ui.shopStatus = Label(p, "Status", new Vector2(0.06f, 0.20f), new Vector2(0.50f, 0.80f), 64, TextAnchor.MiddleLeft);
        b = Buttons(p, new Vector2(0.72f, 0.45f));
        ui.greetLabel = Button(b, "Greet Customer", day.GreetCustomer).GetComponentInChildren<Text>();
        Button(b, "Order Book", router.GoToOrderBook);
        Button(b, "Supply Market", router.GoToMarket);
        Button(b, "Pattern Studio", router.GoToPatternStudio);
        Button(b, "Journal", router.GoToJournal);
        Button(b, "Settings", router.GoToSettings);
        Button(b, "End Day", day.EndDay);

        p = Panel(ScreenId.OrderBook, "Order Book", new Color(0.96f, 0.92f, 0.84f));
        var list = Buttons(p, new Vector2(0.5f, 0.55f));
        ui.orderTemplate = Button(list, "Order Template", null, 1700);
        ui.orderBookEmpty = Label(p, "Empty", new Vector2(0.2f, 0.45f), new Vector2(0.8f, 0.65f), 64, TextAnchor.MiddleCenter);
        ui.orderBookEmpty.text = "No orders yet. Greet a customer in the shop.";
        Button(Buttons(p, new Vector2(0.5f, 0.08f)), "Back to Shop", router.GoToShop);

        p = Panel(ScreenId.Dialogue, "", new Color(0.80f, 0.86f, 0.93f));
        ui.dialoguePortrait = Portrait(p, new Vector2(0.27f, 0.52f), 1000);
        ui.dialogueName = Label(p, "Name", new Vector2(0.52f, 0.72f), new Vector2(0.95f, 0.85f), 96, TextAnchor.MiddleLeft);
        ui.dialogueText = Label(p, "Request", new Vector2(0.52f, 0.38f), new Vector2(0.95f, 0.72f), 64, TextAnchor.UpperLeft);
        b = Buttons(p, new Vector2(0.73f, 0.22f));
        Button(b, "Accept Order", day.AcceptOrder);
        Button(b, "Not Today", day.DeclineOrder);

        p = Panel(ScreenId.Workbench, "Workbench", new Color(0.80f, 0.66f, 0.50f), "Art/Backgrounds/bg_workbench.png");
        ui.workbenchText = Label(p, "Order", new Vector2(0.15f, 0.45f), new Vector2(0.85f, 0.80f), 72, TextAnchor.MiddleCenter);
        b = Buttons(p, new Vector2(0.5f, 0.28f));
        ui.startLabel = Button(b, "Start Crafting", day.StartCrafting).GetComponentInChildren<Text>();
        Button(b, "Back to Order Book", router.GoToOrderBook);

        // Transparent so the stitching board (world sprites) shows through.
        p = Panel(ScreenId.Minigame, "", Color.clear);
        p.GetComponent<Image>().raycastTarget = false;
        ui.minigameHint = Label(p, "Hint", new Vector2(0.02f, 0.20f), new Vector2(0.23f, 0.80f), 52, TextAnchor.MiddleLeft);
        ui.minigameHint.color = Color.white;
        b = Buttons(p, new Vector2(0.88f, 0.35f));
        Button(b, "Done for Now", day.DoneForNow, 520);
        Button(b, "Finish", day.FinishCrafting, 520);

        p = Panel(ScreenId.Result, "Result", new Color(0.97f, 0.88f, 0.65f));
        ui.resultPortrait = Portrait(p, new Vector2(0.27f, 0.48f), 900);
        ui.resultText = Label(p, "Summary", new Vector2(0.52f, 0.35f), new Vector2(0.95f, 0.80f), 72, TextAnchor.MiddleLeft);
        b = Buttons(p, new Vector2(0.73f, 0.20f));
        Button(b, "Deliver", day.Deliver);
        Button(b, "Keep Working", day.KeepWorking);

        p = Panel(ScreenId.Market, "Supply Market", new Color(0.85f, 0.70f, 0.60f));
        Button(Buttons(p, new Vector2(0.5f, 0.45f)), "Back to Shop", router.GoToShop);

        p = Panel(ScreenId.PatternStudio, "Pattern Studio", new Color(0.93f, 0.78f, 0.90f));
        Button(Buttons(p, new Vector2(0.5f, 0.45f)), "Back to Shop", router.GoToShop);

        p = Panel(ScreenId.Journal, "Journal", new Color(0.80f, 0.93f, 0.74f));
        Button(Buttons(p, new Vector2(0.5f, 0.45f)), "Back to Shop", router.GoToShop);

        p = Panel(ScreenId.Settings, "Settings", new Color(0.78f, 0.78f, 0.82f));
        b = Buttons(p, new Vector2(0.5f, 0.45f));
        Button(b, "Back to Shop", router.GoToShop);
        Button(b, "Save and Quit to Title", gm.QuitToTitle);

        p = Panel(ScreenId.DaySummary, "End of Day", new Color(0.78f, 0.82f, 0.95f));
        ui.summaryText = Label(p, "Summary", new Vector2(0.2f, 0.40f), new Vector2(0.8f, 0.80f), 72, TextAnchor.MiddleCenter);
        Button(Buttons(p, new Vector2(0.5f, 0.25f)), "Next Day", day.StartNextDay);

        router.screens = entries.ToArray();

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettingsFirst(ScenePath);
        Debug.Log($"Test scene built at {ScenePath}.");
    }

    static T[] LoadAll<T>(string folder) where T : Object
    {
        var list = new List<T>();
        foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder }))
            list.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
        return list.ToArray();
    }

    static RectTransform MakePanel(Transform parent, ScreenId id, string title, Color color, string bgSprite)
    {
        var panel = new GameObject($"Screen_{id}", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)panel.transform;
        rt.SetParent(parent, false);
        Stretch(rt);

        var img = panel.GetComponent<Image>();
        var sprite = bgSprite != null ? AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/{bgSprite}") : null;
        img.sprite = sprite;
        img.color = sprite != null ? Color.white : color;

        var label = MakeText(rt, "Title", title, 120);
        var lrt = (RectTransform)label.transform;
        lrt.anchorMin = new Vector2(0, 1); lrt.anchorMax = new Vector2(1, 1);
        lrt.pivot = new Vector2(0.5f, 1);
        lrt.anchoredPosition = new Vector2(0, -120);
        lrt.sizeDelta = new Vector2(0, 200);
        return rt;
    }

    static Transform Buttons(Transform panel, Vector2 anchor)
    {
        var buttons = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var brt = (RectTransform)buttons.transform;
        brt.SetParent(panel, false);
        brt.anchorMin = brt.anchorMax = anchor;
        brt.pivot = new Vector2(0.5f, anchor.y < 0.15f ? 0f : 0.5f);
        var layout = buttons.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 40;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = buttons.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return brt;
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
        le.preferredWidth = width; le.preferredHeight = 150;

        var text = MakeText(go.transform, "Label", label, 64);
        Stretch((RectTransform)text.transform);

        var button = go.GetComponent<Button>();
        if (onClick != null) UnityEventTools.AddPersistentListener(button.onClick, onClick);
        return button;
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
