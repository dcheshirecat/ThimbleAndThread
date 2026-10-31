using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Shows Explainers: a card over the screen with a title, a few lines and Next / Got it. It blocks the work
/// underneath until it's read. Unlock explanations wait for a calm moment (like stamps); a crafting step's
/// explanation shows at once.
public class ExplainerUI : MonoBehaviour
{
    public GameObject panel;
    public Text titleText, bodyText, nextLabel, pageLabel;

    readonly List<(string key, string[] pages, bool anywhere)> queue = new List<(string, string[], bool)>();
    (string key, string[] pages, bool anywhere) current;
    int page = -1;

    void OnEnable() => Explainers.OnExplain += Add;
    void OnDisable() => Explainers.OnExplain -= Add;

    public bool Showing => page >= 0;
    public static bool Open { get; private set; }   // a card is up: stamps wait behind it

    void Add(string key, string[] pages, bool anywhere)
    {
        if ((Showing && current.key == key) || queue.Exists(q => q.key == key)) return;
        queue.Add((key, pages, anywhere));
        Next();
    }

    void Update()
    {
        if (!Showing && queue.Count > 0) Next();
        else if (Showing && !current.anywhere && !StampPopup.Calm()) Hold();   // into a conversation or crafting: later
    }

    void Hold()
    {
        queue.Insert(0, current);
        page = -1;
        Open = false;
        panel.SetActive(false);
    }

    void Next()
    {
        if (Showing || queue.Count == 0 || GameManager.Instance == null || GameManager.Instance.Save == null) return;
        if (!GameManager.Instance.Save.player.created) return;
        int i = StampPopup.Calm() ? 0 : queue.FindIndex(q => q.anywhere);
        if (i < 0) return;
        current = queue[i];
        queue.RemoveAt(i);
        page = 0;
        ShowPage();
    }

    void ShowPage()
    {
        Open = true;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        var text = PlayerText.Fill(current.pages[page]);
        int cut = text.IndexOf('\n');
        titleText.text = cut < 0 ? text : text.Substring(0, cut);
        bodyText.text = cut < 0 ? "" : text.Substring(cut + 1).Trim();
        bool last = page == current.pages.Length - 1;
        nextLabel.text = last ? "Got it" : "Next";
        pageLabel.text = current.pages.Length > 1 ? $"{page + 1} / {current.pages.Length}" : "";
    }

    /// Next page, or done.
    public void Continue()
    {
        if (!Showing) return;
        if (page < current.pages.Length - 1) { page++; ShowPage(); return; }
        Explainers.MarkSeen(GameManager.Instance.Save, current.key);
        GameManager.Instance.Persist();
        page = -1;
        Open = false;
        panel.SetActive(false);
        Next();
    }
}
