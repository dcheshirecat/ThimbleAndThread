using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// The pop-up for a new stamp (an achievement), and for short notices like the doorbell ringing while
/// the player is on another screen. One at a time, in order; tap to dismiss, or it goes by itself.
public class StampPopup : MonoBehaviour
{
    public GameObject panel;
    public RectTransform card;
    public Image stamp;              // the round stamp in its theme colour
    public Text stampWord, titleText, detailText;

    readonly Queue<(Stamp stamp, string notice)> queue = new Queue<(Stamp, string)>();
    bool showing;

    void OnEnable()
    {
        Stamps.OnEarned += Earned;
        Stamps.OnNotice += Notice;
    }

    void OnDisable()
    {
        Stamps.OnEarned -= Earned;
        Stamps.OnNotice -= Notice;
    }

    void Earned(Stamp s) { queue.Enqueue((s, null)); Next(); }
    void Notice(string text) { queue.Enqueue((null, text)); Next(); }

    public void Dismiss()
    {
        StopAllCoroutines();
        panel.SetActive(false);
        showing = false;
        Next();
    }

    void Next()
    {
        if (showing || queue.Count == 0 || !isActiveAndEnabled) return;
        var (s, notice) = queue.Dequeue();
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
