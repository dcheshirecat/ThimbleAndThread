using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// A row of thread-colour buttons (and optionally an eraser). The chosen one is outlined,
/// its name shows in the label, and onPick is called with its index (-1 for the eraser).
public class ThreadSwatches : MonoBehaviour
{
    public Image[] swatches;
    public Image eraser;               // optional
    public Text label;                 // optional
    public UnityEvent<int> onPick = new UnityEvent<int>();

    readonly List<string> names = new List<string>();

    /// Shows one button per colour (up to the number of swatches) and picks the first.
    public void Show(IList<Color> colors, IList<string> colorNames = null)
    {
        names.Clear();
        for (int i = 0; i < swatches.Length; i++)
        {
            bool used = i < colors.Count;
            swatches[i].gameObject.SetActive(used);
            if (!used) continue;
            var c = colors[i]; c.a = 1f;
            swatches[i].color = c;
            names.Add(colorNames != null && i < colorNames.Count ? colorNames[i] : $"Thread {i + 1}");
        }
        Pick(0);
    }

    public void Pick(int index)
    {
        for (int i = 0; i < swatches.Length; i++) SetOutline(swatches[i], i == index);
        if (eraser != null) SetOutline(eraser, index < 0);
        if (label != null) label.text = index < 0 ? "Eraser" : index < names.Count ? names[index] : "";
        onPick.Invoke(index);
    }

    static void SetOutline(Image image, bool on)
    {
        var outline = image.GetComponent<Outline>();
        if (outline != null) outline.enabled = on;
    }
}
