using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// The pop-up for a new stamp (an achievement), and for short notices like the doorbell ringing while
/// the player is on another screen. One at a time, in order; tap to dismiss, or it goes by itself.
/// Stamps and goals wait for a calm moment (a hub or shop screen, not mid-conversation or mid-craft);
/// the doorbell shows straight away.
public class StampPopup : MonoBehaviour
{
    public GameObject panel;
    public RectTransform card;
    public Image stamp;              // the round stamp in its theme colour
    public Text stampWord, titleText, detailText;

    readonly List<(Stamp stamp, string notice, bool calm)> queue = new List<(Stamp, string, bool)>();
    bool showing;
    (Stamp stamp, string notice, bool calm) current;   // the one on screen

    void OnEnable()
    {
        Stamps.OnEarned += Earned;
        Stamps.OnNotice += Notice;
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged += ScreenChanged;
    }

    void ScreenChanged(ScreenId id)
    {
        if (showing && current.calm && !Calm())   // into a conversation or crafting: put it away until later
        {
            queue.Insert(0, current);
            StopAllCoroutines();
            panel.SetActive(false);
            showing = false;
        }
        Next();   // held ones show once the player is somewhere calm
    }

    /// Screens where nothing is going on: a stamp or a goal can show there.
    static bool Calm()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.Save == null) return false;
        return gm.Current is ScreenId.Shop or ScreenId.Workroom or ScreenId.OrderBook or ScreenId.Workbench or ScreenId.Result
            or ScreenId.DaySummary or ScreenId.Market or ScreenId.Boutique or ScreenId.PatternStudio;
    }

    void Update() { if (!showing && queue.Count > 0) Next(); }   // a calm moment can come without a screen change

    /// Not over the title, save slots, the tutorial or making the shopkeeper: they wait until after.
    static bool Quiet()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.Save == null || !gm.Save.player.created) return true;
        return gm.Current == ScreenId.Title || gm.Current == ScreenId.SaveSelect || gm.Current == ScreenId.Customize || gm.Current == ScreenId.Tutorial;
    }

    void OnDisable()
    {
        Stamps.OnEarned -= Earned;
        Stamps.OnNotice -= Notice;
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= ScreenChanged;
    }

    void Earned(Stamp s) { queue.Add((s, null, true)); Next(); }
    void Notice(string text, bool calm) { queue.Add((null, text, calm)); Next(); }

    public void Dismiss()
    {
        StopAllCoroutines();
        panel.SetActive(false);
        showing = false;
        Next();
    }

    void Next()
    {
        if (showing || queue.Count == 0 || !isActiveAndEnabled || Quiet()) return;
        int i = Calm() ? 0 : queue.FindIndex(q => !q.calm);   // the first one that may show now
        if (i < 0) return;
        current = queue[i];
        var (s, notice, _) = current;
        queue.RemoveAt(i);
        showing = true;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        stamp.gameObject.SetActive(s != null);
        if (s != null)
        {
            stamp.color = Stamps.ColorOf(s.theme);
            stampWord.text = Stamps.Word(s.theme);
            titleText.text = s.title;
            detailText.text = s.detail + "\n<size=40>Stamped in your journal.</size>";
            AudioManager.Play(Sfx.StepDone);
        }
        else
        {
            titleText.text = notice;
            detailText.text = "";
        }
        StartCoroutine(Show(s != null ? 3.5f : 2.5f));
    }

    IEnumerator Show(float seconds)
    {
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.25f)   // the stamp thumps down
        {
            float e = 1 + 0.25f * Mathf.Sin(t * Mathf.PI) * (1 - t);
            card.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, t) * e;
            yield return null;
        }
        card.localScale = Vector3.one;
        yield return new WaitForSecondsRealtime(seconds);
        Dismiss();
    }
}
