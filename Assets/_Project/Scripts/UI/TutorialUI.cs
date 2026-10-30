using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// The "How to Play" tutorial: a few pages, each with a title, a short text and a little animated
/// scene (or the page's own art once it's drawn). Shown automatically the first time the app is
/// opened, and from the title screen's "How to Play" button. The words live in Data/Tutorial.asset.
public class TutorialUI : MonoBehaviour
{
    public const string SeenKey = "tutorial_seen";   // PlayerPrefs: set once the tutorial has been finished or skipped

    public ContentLibrary content;
    public Text titleText, bodyText, pageText, nextLabel;
    public GameObject backButton;
    public Image art;                    // the page's illustration, when it has one
    public RectTransform stage;          // where the placeholder animation plays
    public CanvasGroup page;             // fades and slides in on each new page
    public Sprite square, circle;

    int index;
    float shownAt;
    readonly Dictionary<string, RectTransform> shapes = new Dictionary<string, RectTransform>();

    TutorialPage[] Pages => content != null && content.tutorial != null ? content.tutorial.pages : new TutorialPage[0];
    public int Index => index;

    public static bool Seen => PlayerPrefs.GetInt(SeenKey, 0) == 1;

    void Start()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScreenChanged += OnScreenChanged;
        if (!Seen && Pages.Length > 0) GameManager.Instance.ChangeScreen(ScreenId.Tutorial);   // first time the app is opened
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreenChanged;
    }

    void OnScreenChanged(ScreenId id)
    {
        if (id != ScreenId.Tutorial) return;
        returnTo = GameManager.Instance.Previous == ScreenId.Settings ? ScreenId.Settings : ScreenId.Title;
        index = 0;
        Show();
    }

    // ---- Buttons ----

    public void Next()
    {
        if (index >= Pages.Length - 1) { Finish(); return; }
        index++;
        Show();
    }

    public void Back()
    {
        if (index == 0) return;
        index--;
        Show();
    }

    public void Skip() => Finish();

    void Finish()
    {
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
        GameManager.Instance.ChangeScreen(returnTo);
    }

    ScreenId returnTo = ScreenId.Title;   // Settings when opened from there mid-game

    // ---- Pages ----

    void Show()
    {
        var pages = Pages;
        if (pages.Length == 0) { Finish(); return; }
        var p = pages[index];
        titleText.text = p.title;
        bodyText.text = p.text;
        pageText.text = string.Join("  ", System.Array.ConvertAll(pages, x => x == p ? "●" : "○"));
        nextLabel.text = index == pages.Length - 1 ? "Let's Begin" : "Next";
        backButton.SetActive(index > 0);
        art.sprite = p.art;
        art.enabled = p.art != null;
        shownAt = Time.unscaledTime;

        foreach (var s in shapes.Values) if (s) Destroy(s.gameObject);
        shapes.Clear();
        if (p.art == null) Build(p.animation);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.Current != ScreenId.Tutorial || Pages.Length == 0) return;
        float t = Time.unscaledTime - shownAt;
        // The page fades in and rises a little.
        float k = Mathf.Clamp01(t / 0.4f);
        page.alpha = k;
        ((RectTransform)page.transform).anchoredPosition = new Vector2(0, (1 - k) * -60f);
        var p = Pages[index];
        if (p.art != null) art.rectTransform.localScale = Vector3.one * (1f + 0.02f * Mathf.Sin(t * 1.5f));   // a gentle breath
        else Animate(p.animation, t);
    }

    // ---- Placeholder scenes (simple shapes; replaced by the page's art when it has some) ----

    static readonly Color Cream = new Color(0.98f, 0.95f, 0.88f), Wood = new Color(0.62f, 0.45f, 0.30f),
        Sea = new Color(0.40f, 0.62f, 0.80f), Ink = new Color(0.25f, 0.20f, 0.28f), Rose = new Color(0.90f, 0.45f, 0.50f),
        Gold = new Color(0.95f, 0.78f, 0.30f), Hand = new Color(1f, 1f, 1f, 0.85f);

    void Build(TutorialAnim anim)
    {
        switch (anim)
        {
            case TutorialAnim.Welcome:
                Box("sea", 0.5f, 0.1f, 1f, 0.2f, Sea);
                Box("shop", 0.5f, 0.42f, 0.5f, 0.38f, Wood);
                Box("roof", 0.5f, 0.64f, 0.62f, 0.08f, Ink);
                Box("window", 0.38f, 0.45f, 0.13f, 0.13f, Cream);
                Box("door", 0.6f, 0.34f, 0.12f, 0.22f, Ink);
                Box("sign", 0.5f, 0.74f, 0.36f, 0.08f, Cream);
                for (int i = 0; i < 3; i++) Dot($"wave{i}", 0.2f + i * 0.3f, 0.18f, 0.08f, Cream);
                break;
            case TutorialAnim.DayAndNight:
                Box("sky", 0.5f, 0.5f, 1f, 1f, Sea);
                Dot("sun", 0.5f, 0.8f, 0.14f, Gold);
                Dot("moon", 0.5f, 0.8f, 0.11f, Cream);
                Box("shop", 0.5f, 0.25f, 0.5f, 0.32f, Wood);
                Box("window", 0.5f, 0.27f, 0.14f, 0.12f, Cream);
                break;
            case TutorialAnim.Customers:
                Box("counter", 0.65f, 0.25f, 0.5f, 0.12f, Wood);
                Box("customer", 0f, 0.4f, 0.14f, 0.36f, Rose);
                Box("bubble", 0.42f, 0.78f, 0.3f, 0.12f, Cream);
                Dot("coin", 0.62f, 0.4f, 0.08f, Gold);
                break;
            case TutorialAnim.Stitching:
                for (int x = 0; x < 3; x++)
                    for (int y = 0; y < 3; y++) Box($"cell{x}{y}", 0.3f + x * 0.2f, 0.3f + y * 0.2f, 0.18f, 0.18f, Cream);
                Line("slash", Rose);
                Line("back", Rose);
                Dot("hand", 0.4f, 0.4f, 0.09f, Hand);
                break;
            case TutorialAnim.Gestures:
                for (int i = 0; i < 3; i++) Box($"panel{i}", 0.18f + i * 0.32f, 0.5f, 0.28f, 0.7f, Cream);
                Line("knit", Rose);
                Line("trace", Ink);
                Dot("loop", 0.5f, 0.55f, 0.06f, Gold);
                for (int i = 0; i < 3; i++) Dot($"hand{i}", 0.18f + i * 0.32f, 0.5f, 0.08f, Ink);
                break;
            case TutorialAnim.OrderBook:
                for (int i = 0; i < 3; i++)
                {
                    Box($"row{i}", 0.5f, 0.72f - i * 0.22f, 0.8f, 0.15f, Cream);
                    Box($"bar{i}", 0.2f, 0.72f - i * 0.22f, 0.6f, 0.04f, Sea);
                    shapes[$"bar{i}"].pivot = new Vector2(0f, 0.5f);   // fills from the left
                }
                break;
            case TutorialAnim.Market:
                Box("shelf", 0.5f, 0.45f, 0.86f, 0.7f, Wood);
                for (int i = 0; i < 6; i++) Box($"tile{i}", 0.25f + (i % 3) * 0.25f, 0.62f - (i / 3) * 0.32f, 0.2f, 0.26f, Cream);
                break;
        }
    }

    void Animate(TutorialAnim anim, float t)
    {
        switch (anim)
        {
            case TutorialAnim.Welcome:
                Rot("sign", 6f * Mathf.Sin(t * 2f));
                for (int i = 0; i < 3; i++) Move($"wave{i}", 0.2f + i * 0.3f + 0.03f * Mathf.Sin(t * 1.3f + i), 0.18f + 0.02f * Mathf.Sin(t * 2f + i * 2f));
                Tint("window", Color.Lerp(Cream, Gold, 0.5f + 0.5f * Mathf.Sin(t * 1.5f)));   // the lamp's on: it opens at dusk
                break;
            case TutorialAnim.DayAndNight:
            {
                float phase = t % 8f / 8f;            // first half day, second half night
                bool day = phase < 0.5f;
                float a = (day ? phase : phase - 0.5f) * 2f * Mathf.PI;
                var arc = new Vector2(0.5f - 0.38f * Mathf.Cos(a), 0.45f + 0.4f * Mathf.Sin(a));
                Move("sun", arc.x, day ? arc.y : -1f);
                Move("moon", arc.x, day ? -1f : arc.y);
                Tint("sky", day ? Sea : new Color(0.15f, 0.16f, 0.32f));
                Tint("window", day ? Cream : Gold);
                break;
            }
            case TutorialAnim.Customers:
            {
                float c = t % 6f;
                float x = c < 1.5f ? Mathf.SmoothStep(-0.1f, 0.3f, c / 1.5f) : c < 4.5f ? 0.3f : Mathf.SmoothStep(0.3f, -0.1f, (c - 4.5f) / 1.5f);
                Move("customer", x, 0.4f);
                Show("bubble", c > 1.6f && c < 3f);
                float coin = Mathf.Clamp01((c - 3f) / 1.2f);
                Show("coin", c > 3f && c < 4.2f);
                Move("coin", 0.62f, 0.4f + 0.25f * coin);
                break;
            }
            case TutorialAnim.Stitching:
            {
                float c = t % 3.5f;
                var a = new Vector2(0.41f, 0.41f); var b = new Vector2(0.59f, 0.59f);     // the middle square's corners
                var d = new Vector2(0.41f, 0.59f); var e = new Vector2(0.59f, 0.41f);
                float first = Mathf.Clamp01(c / 1f), second = Mathf.Clamp01((c - 1.3f) / 1f);
                Segment("slash", a, Vector2.Lerp(a, b, first), 0.025f);
                Segment("back", d, Vector2.Lerp(d, e, second), 0.025f);
                var hand = c < 1.15f ? Vector2.Lerp(a, b, first) : c < 1.3f ? Vector2.Lerp(b, d, (c - 1.15f) / 0.15f) : Vector2.Lerp(d, e, second);
                Move("hand", hand.x, hand.y);
                break;
            }
            case TutorialAnim.Gestures:
            {
                float c = t % 2.4f, k = Mathf.Clamp01(c / 1.2f);
                // Knit: swipe down.
                Move("hand0", 0.18f, Mathf.Lerp(0.7f, 0.35f, k));
                Segment("knit", new Vector2(0.18f, 0.7f), new Vector2(0.18f, Mathf.Lerp(0.7f, 0.35f, k)), 0.02f);
                // Crochet: tap, then flick.
                float pulse = c < 0.6f ? 1f + 0.5f * Mathf.Sin(c / 0.6f * Mathf.PI) : 1f;
                Move("hand1", 0.5f, c < 0.8f ? 0.5f : Mathf.Lerp(0.5f, 0.8f, Mathf.Clamp01((c - 0.8f) / 0.3f)));
                Scale("hand1", pulse);
                Show("loop", c > 0.3f && c < 1.1f);
                // Sew and embroider: trace the line.
                var from = new Vector2(0.73f, 0.3f); var to = new Vector2(0.92f, 0.72f);
                Move("hand2", Vector2.Lerp(from, to, k).x, Vector2.Lerp(from, to, k).y);
                Segment("trace", from, Vector2.Lerp(from, to, k), 0.015f);
                break;
            }
            case TutorialAnim.OrderBook:
                for (int i = 0; i < 3; i++)
                {
                    float speed = new[] { 0.25f, 0.12f, 0.06f }[i];
                    float fill = i == 1 && t % 6f > 3f ? 0.35f : Mathf.Clamp01(t * speed % 1.2f);   // the middle one is paused: no rush
                    var bar = shapes[$"bar{i}"];
                    bar.localScale = new Vector3(Mathf.Max(0.02f, fill), 1, 1);
                    Tint($"bar{i}", fill >= 1f ? Gold : Sea);
                }
                break;
            case TutorialAnim.Market:
                for (int i = 0; i < 6; i++)
                {
                    float c = (t + i * 0.15f) % 4f;                        // every few seconds the stock turns over
                    float flip = c > 3.4f ? Mathf.Abs(Mathf.Cos((c - 3.4f) / 0.6f * Mathf.PI)) : 1f;
                    shapes[$"tile{i}"].localScale = new Vector3(flip, 1, 1);
                    int round = Mathf.FloorToInt((t + i * 0.15f + 0.6f) / 4f);
                    Tint($"tile{i}", Color.HSVToRGB(((i * 0.17f) + round * 0.31f) % 1f, 0.35f, 0.95f));
                }
                break;
        }
    }

    // ---- Shape helpers: x and widths are fractions of the stage's width, y and heights of its height ----
    // (The stage has a fixed size in canvas units, so these don't change with the phone.)

    Vector2 Size => new Vector2(stage.rect.width, stage.rect.height);
    Vector2 At(float x, float y) => Vector2.Scale(new Vector2(x, y), Size);

    RectTransform Add(string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(stage, false);
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        if (sprite == square) img.type = Image.Type.Sliced;   // crisp rounded corners instead of a stretched blur
        img.color = color;
        img.raycastTarget = false;
        shapes[name] = rt;
        return rt;
    }

    void Box(string name, float x, float y, float w, float h, Color color)
    {
        var rt = Add(name, square, color);
        rt.sizeDelta = At(w, h);
        Move(name, x, y);
    }

    /// A round dot; its size is a fraction of the stage's height.
    void Dot(string name, float x, float y, float size, Color color)
    {
        var rt = Add(name, circle, color);
        rt.sizeDelta = Vector2.one * size * Size.y;
        Move(name, x, y);
    }

    void Line(string name, Color color) => Add(name, square, color).sizeDelta = Vector2.zero;

    void Move(string name, float x, float y) => shapes[name].anchoredPosition = At(x, y);
    void Rot(string name, float degrees) => shapes[name].localRotation = Quaternion.Euler(0, 0, degrees);
    void Scale(string name, float k) => shapes[name].localScale = Vector3.one * k;
    void Show(string name, bool on) => shapes[name].gameObject.SetActive(on);
    void Tint(string name, Color c) => shapes[name].GetComponent<Image>().color = c;

    /// Stretches a line shape from a to b.
    void Segment(string name, Vector2 a, Vector2 b, float thickness)
    {
        var rt = shapes[name];
        var d = At(b.x, b.y) - At(a.x, a.y);
        rt.sizeDelta = new Vector2(d.magnitude, thickness * Size.y);
        rt.anchoredPosition = At((a.x + b.x) / 2f, (a.y + b.y) / 2f);
        rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }
}
