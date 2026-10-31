#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Temporary smoke test: knitting a sweater and a scarf as garments. Not committed.
[InitializeOnLoad]
public static class TempSmokeTest
{
    static string shotDir;
    static double startTime;
    static int frames;
    static IEnumerator routine;
    static int fails;

    static TempSmokeTest()
    {
        if (!SessionState.GetBool("smoke_running", false)) return;
        EditorApplication.delayCall += () => { if (EditorApplication.isPlaying) EditorApplication.isPlaying = false; EditorApplication.delayCall += Begin; };
    }

    public static void Run() { SessionState.SetBool("smoke_running", true); Begin(); }

    static void Begin()
    {
        shotDir = System.Environment.GetEnvironmentVariable("SMOKE_SHOTS");
        Directory.CreateDirectory(shotDir);
        PlayerPrefs.SetInt("tutorial_seen", 1);
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/TestRouter.unity");
        startTime = EditorApplication.timeSinceStartup;
        frames = 0; routine = null; fails = 0;
        EditorApplication.update -= Poll;
        EditorApplication.update += Poll;
        EditorApplication.isPlaying = true;
    }

    static void Poll()
    {
        if (EditorApplication.timeSinceStartup - startTime > 400) { Debug.Log("SMOKE FAIL timeout"); EditorApplication.Exit(2); }
        if (!EditorApplication.isPlaying) return;
        EditorApplication.isPaused = true;
        EditorApplication.Step();
        if (++frames < 10 || GameManager.Instance == null) return;
        routine ??= Steps();
        try
        {
            if (!routine.MoveNext())
            {
                EditorApplication.update -= Poll;
                SessionState.SetBool("smoke_running", false);
                Debug.Log(fails == 0 ? "SMOKE PASS" : $"SMOKE FAIL ({fails})");
                EditorApplication.Exit(fails == 0 ? 0 : 1);
            }
        }
        catch (System.Exception e) { Debug.Log($"SMOKE FAIL exception {e}"); EditorApplication.Exit(3); }
    }

    static IEnumerator Steps()
    {
        var gm = GameManager.Instance;
        var day = Object.FindAnyObjectByType<ShopDay>();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 5;

        gm.StartNewGame(2);
        yield return null;
        gm.Save.player.created = true;
        day.Progress.LearnEverything();
        gm.Save.night = true;
        day.shop.Add("acrylic_yarn", 3000);
        day.shop.Add("knitting_needles_5mm", 1);
        day.shop.Add("tapestry_needle", 1);
        day.shop.Add("knitting_needles_bamboo_5mm", 1);
        day.shop.Add("tapestry_needle_bent", 1);

        foreach (var itemId in new[] { "sweater" })
        {
            // A two-colour heart motif
            var pat = new Pattern { width = 6, height = 5 };
            pat.palette.Add(new Color(0.9f, 0.2f, 0.3f));
            pat.palette.Add(new Color(1f, 1f, 1f));
            string[] rows = { ".X..X.", "XXXXXX", "XXOXXX", ".XXXX.", "..XX.." };   // top row first
            for (int r = 0; r < rows.Length; r++)
                for (int x = 0; x < 6; x++)
                    if (rows[r][x] != '.') pat.cells.Add(new StitchCell { x = x, y = rows.Length - 1 - r, colorIndex = rows[r][x] == 'O' ? 1 : 0 });
            var item = day.content.Item(itemId);
            var order = OrderGenerator.FromPattern(day.content.Customer("juno"), item, pat, gm.Save.day);
            order.gathered = true;
            gm.Save.orders.Add(order);
            day.SelectOrder(order.id);
            day.StartCrafting();
            yield return null;
            Expect(gm.Current == ScreenId.Minigame, $"{itemId}: crafting starts ({gm.Current}; {day.LastMessage})");
            int guard = 0;
            while (gm.Current == ScreenId.Minigame && guard++ < 20)
            {
                var step = order.CurrentStep;
                var piece = Object.FindAnyObjectByType<PieceStep>();
                Debug.Log($"SMOKE step {order.step}: {step.label} ({step.kind}, piece '{step.piece}') -> {piece?.GetType().Name}");
                yield return null;
                Debug.Log($"SMOKE   message: {day.LastMessage}");
                if (step.kind == StepKind.KnitRows) Expect(day.LastMessage.Contains("Bamboo"), "bamboo needles help knitting");
                if (step.kind == StepKind.ChooseYarn) order.fabricId = "acrylic_yarn";
                else if (step.kind == StepKind.CastOn)
                {
                    for (int i = 0; i < 40; i++) Call(piece, "OnPress", new Vector2(0.5f, 0.5f));
                }
                else if (step.kind == StepKind.KnitRows)
                {
                    var knit = (KnitLayout)typeof(PieceStep).GetProperty("Knit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(piece);
                    int colourwork = 0;
                    for (int i = 0; i < knit.Stitches; i++)
                    {
                        if (knit.stitches[i].color >= 0) colourwork++;
                        if (itemId == "sweater" && step.piece == "front" && i == knit.Stitches / 2) { for (int f = 0; f < 4; f++) yield return null; Shot($"{itemId}_{order.step}_half"); }
                        Call(piece, "OnPress", new Vector2(0.5f, 0.5f));
                        bool goWrong = step.piece == "back" && i == 30;
                        Call(piece, "OnRelease", new Vector2(0.5f, knit.Purl(i) != goWrong ? 0.6f : 0.4f));
                    }
                    Debug.Log($"SMOKE   {knit.Stitches} stitches ({knit.gridWidth} x {knit.gridHeight}), colourwork {colourwork}, mistakes {order.CurrentStep.label} {order.StepState(order.step).mistakes}");
                    if (step.piece == "front") Expect(colourwork > 0, "the front carries the pattern in colour");
                    if (step.piece == "back") Expect(colourwork == 0, "the back is plain");
                }
                else if (piece is TracingStep tracing)
                {
                    var guides = (System.Collections.Generic.List<System.Collections.Generic.List<Vector2>>)typeof(TracingStep).GetMethod("Guides", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tracing, null);
                    foreach (var g in guides)
                    {
                        var pts = PieceShape.Resample(g, false, 60);
                        Call(piece, "OnPress", pts[0]);
                        foreach (var p in pts) Call(piece, "OnDrag", p + (step.kind == StepKind.SewUp ? new Vector2(0.045f, 0.045f) * 0.7f : Vector2.zero));
                        Call(piece, "OnRelease", pts[pts.Count - 1]);
                        if (step.kind == StepKind.SewUp) { for (int f = 0; f < 3; f++) yield return null; Shot("sewup_needle"); }
                    }
                    Debug.Log($"SMOKE   traced: coverage {tracing.Coverage:0.00}, score {tracing.Score():0.00}");
                    if (step.kind == StepKind.SewUp) Expect(guides.Count == 2 && tracing.Score() > 0.7f, $"seams sewn a little off the line still count with the bent-tip needle ({tracing.Score():0.00})");
                }
                yield return null;
                Shot($"{itemId}_{order.step}_{step.kind}");
                int stepBefore = order.step;
                day.FinishCrafting();
                if (step.piece == "back" && step.kind == StepKind.KnitRows)
                {
                    Expect(order.step == stepBefore && day.LastMessage.Contains("1 wrong stitch"), $"Finish warns: {day.LastMessage}");
                    Shot("wrong_stitch");
                    day.FinishCrafting();
                    Expect(order.step == stepBefore + 1, $"second Finish goes on (score {order.steps[stepBefore].score:0.00})");
                }
                yield return null;
            }
            Expect(gm.Current == ScreenId.Result, $"{itemId}: finished -> Result ({gm.Current}; {day.LastMessage})");
            Debug.Log($"SMOKE {itemId} quality {order.accuracy:0.00}; step scores {string.Join(", ", order.steps.ConvertAll(s => s.score.ToString("0.00")))}");
            gm.Save.night = true;
        }
    }

    static void Call(object o, string method, object arg) =>
        typeof(PieceStep).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, new[] { arg });

    static void Shot(string name)
    {
        Canvas.ForceUpdateCanvases();
        var cam = Camera.main;
        var rt = new RenderTexture(1080, 2400, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(shotDir, name + ".png"), tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
    }

    static void Expect(bool ok, string what)
    {
        if (!ok) fails++;
        Debug.Log($"SMOKE {(ok ? "ok  " : "FAIL")} {what}");
    }
}
#endif
