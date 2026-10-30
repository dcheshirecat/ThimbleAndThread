using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// The Dialogue screen: who's talking (name and portrait), what they say, and buttons to answer:
/// "Continue", or the choices. ShopDay drives it one beat at a time (see ShopDay.Play).
public class DialogueUI : MonoBehaviour
{
    public Text nameText, lineText;
    public Image portrait;
    public RawImage patternPreview;      // shows the pattern being talked about, when there is one
    public Button choiceTemplate;        // copied for each answer button

    readonly List<GameObject> buttons = new List<GameObject>();

    /// Shows one beat. "choices" are the buttons, top to bottom; each runs its action when tapped.
    public void Show(string speaker, Sprite face, string text, List<(string label, Action pick)> choices, Texture preview = null)
    {
        nameText.text = speaker ?? "";
        lineText.text = text ?? "";
        lineText.fontStyle = string.IsNullOrEmpty(speaker) ? FontStyle.Italic : FontStyle.Normal;   // narration in italics
        portrait.sprite = face;
        portrait.enabled = face != null;
        patternPreview.texture = preview;
        patternPreview.gameObject.SetActive(preview != null);

        foreach (var b in buttons) Destroy(b);
        buttons.Clear();
        choiceTemplate.gameObject.SetActive(false);
        for (int i = 0; i < choices.Count; i++)
        {
            var b = Instantiate(choiceTemplate, choiceTemplate.transform.parent);
            b.gameObject.SetActive(true);
            b.name = $"Choice_{i}";
            b.GetComponentInChildren<Text>().text = choices[i].label;
            var pick = choices[i].pick;
            b.onClick.AddListener(() => pick());
            buttons.Add(b.gameObject);
        }
    }

    /// The buttons now showing, for tests: their labels in order.
    public List<string> Choices
    {
        get
        {
            var labels = new List<string>();
            foreach (var b in buttons) labels.Add(b.GetComponentInChildren<Text>().text);
            return labels;
        }
    }

    /// Taps a button (for tests).
    public void Pick(int index) => buttons[index].GetComponent<Button>().onClick.Invoke();
}
