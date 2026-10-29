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
/// Builds a click-through scene: one placeholder panel per ScreenId, switched by ScreenRouter.
/// Never overwrites an existing scene; delete or rename it first to rebuild.
public static class TestSceneBuilder
{
    const string ScenePath = "Assets/_Project/Scenes/TestRouter.unity";
    const string ArtRoot = "Assets/_Project/Art";

    static Font font;
    static Sprite uiSprite;

    [MenuItem("Tools/Thimble & Thread/Build Test Scene")]
    public static void Build()
    {
        if (File.Exists(ScenePath))
        {
            Debug.Log($"{ScenePath} already exists; not overwriting.");
            return;
        }

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.15f, 0.13f, 0.18f);
        camGo.transform.position = new Vector3(0, 0, -10);

        var gm = new GameObject("GameManager").AddComponent<GameManager>();

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2732, 2048);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        var router = canvasGo.AddComponent<ScreenRouter>();

        var entries = new List<ScreenRouter.Entry>();
        Transform Panel(ScreenId id, string title, Color color, string bgSprite = null)
        {
            var root = MakePanel(canvasGo.transform, id, title, color, bgSprite);
            entries.Add(new ScreenRouter.Entry { id = id, root = root.gameObject });
            root.gameObject.SetActive(id == ScreenId.Title);
            return root.Find("Buttons");
        }

        var b = Panel(ScreenId.Title, "Thimble & Thread", new Color(0.30f, 0.22f, 0.35f));
        Button(b, "New Game", gm.StartNewGame);
        Button(b, "Continue", gm.Continue);

        b = Panel(ScreenId.Shop, "Shop", new Color(0.95f, 0.88f, 0.78f), "Backgrounds/bg_shop_day.png");
        Button(b, "Next Customer", router.GoToDialogue);
        Button(b, "Supply Market", router.GoToMarket);
        Button(b, "Pattern Studio", router.GoToPatternStudio);
        Button(b, "Journal", router.GoToJournal);
        Button(b, "Settings", router.GoToSettings);
        Button(b, "End Day", router.GoToDaySummary);

        b = Panel(ScreenId.Dialogue, "Dialogue", new Color(0.55f, 0.65f, 0.80f));
        Button(b, "Accept Order", router.GoToWorkbench);

        b = Panel(ScreenId.Workbench, "Workbench", new Color(0.80f, 0.66f, 0.50f), "Backgrounds/bg_workbench.png");
        Button(b, "Start Crafting", router.GoToMinigame);

        b = Panel(ScreenId.Minigame, "Minigame", new Color(0.60f, 0.85f, 0.60f));
        Button(b, "Finish", router.GoToResult);

        b = Panel(ScreenId.Result, "Result", new Color(0.95f, 0.80f, 0.45f));
        Button(b, "Deliver", router.GoToShop);

        b = Panel(ScreenId.Market, "Supply Market", new Color(0.75f, 0.55f, 0.45f));
        Button(b, "Back to Shop", router.GoToShop);

        b = Panel(ScreenId.PatternStudio, "Pattern Studio", new Color(0.90f, 0.70f, 0.85f));
        Button(b, "Back to Shop", router.GoToShop);

        b = Panel(ScreenId.Journal, "Journal", new Color(0.70f, 0.90f, 0.60f));
        Button(b, "Back to Shop", router.GoToShop);

        b = Panel(ScreenId.Settings, "Settings", new Color(0.60f, 0.60f, 0.65f));
        Button(b, "Back to Shop", router.GoToShop);
        Button(b, "Back to Title", router.GoToTitle);

        b = Panel(ScreenId.DaySummary, "End of Day", new Color(0.35f, 0.40f, 0.60f));
        var next = Button(b, "Next Day", gm.NextDay);
        UnityEventTools.AddPersistentListener(next.onClick, router.GoToShop);

        router.screens = entries.ToArray();

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettingsFirst(ScenePath);
        Debug.Log($"Test scene built at {ScenePath}.");
    }

    static RectTransform MakePanel(Transform parent, ScreenId id, string title, Color color, string bgSprite)
    {
        var panel = new GameObject($"Screen_{id}", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)panel.transform;
        rt.SetParent(parent, false);
        Stretch(rt);

        var img = panel.GetComponent<Image>();
        var sprite = bgSprite != null ? AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/{bgSprite}") : null;
        img.sprite = sprite;
        img.color = sprite != null ? Color.white : color;

        var label = MakeText(rt, "Title", title, 120);
        var lrt = (RectTransform)label.transform;
        lrt.anchorMin = new Vector2(0, 1); lrt.anchorMax = new Vector2(1, 1);
        lrt.pivot = new Vector2(0.5f, 1);
        lrt.anchoredPosition = new Vector2(0, -120);
        lrt.sizeDelta = new Vector2(0, 200);

        var buttons = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var brt = (RectTransform)buttons.transform;
        brt.SetParent(rt, false);
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 0.45f);
        var layout = buttons.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 40;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = buttons.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rt;
    }

    static Button Button(Transform parent, string label, UnityAction onClick)
    {
        var go = new GameObject($"Button_{label.Replace(" ", "")}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = uiSprite;
        img.type = Image.Type.Sliced;
        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = 800; le.preferredHeight = 150;

        var text = MakeText(go.transform, "Label", label, 64);
        text.color = new Color(0.2f, 0.15f, 0.2f);
        Stretch((RectTransform)text.transform);

        var button = go.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, onClick);
        return button;
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
        t.color = Color.white;
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
