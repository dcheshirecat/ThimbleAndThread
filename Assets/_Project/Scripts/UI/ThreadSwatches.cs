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
    readonly List<bool> locked = new List<bool>();

    /// Shows one button per colour (up to the number of swatches) and picks the first that isn't locked.
    /// Locked colours (e.g. floss not owned yet) are greyed out and can't be picked; tapping one says why.
    public void Show(IList<Color> colors, IList<string> colorNames = null, IList<bool> lockedColors = null)
    {
        names.Clear();
        locked.Clear();
        for (int i = 0; i < swatches.Length; i++)
        {
            bool used = i < colors.Count;
            swatches[i].gameObject.SetActive(used);
            if (!used) continue;
            bool isLocked = lockedColors != null && i < lockedColors.Count && lockedColors[i];
            var c = colors[i]; c.a = 1f;
            if (isLocked) c = Color.Lerp(new Color(c.grayscale, c.grayscale, c.grayscale), new Color(0.75f, 0.75f, 0.75f), 0.5f);
            swatches[i].color = c;
            names.Add(colorNames != null && i < colorNames.Count ? colorNames[i] : $"Thread {i + 1}");
            locked.Add(isLocked);
        }
        int first = locked.IndexOf(false);
        Pick(first >= 0 ? first : (eraser != null ? -1 : 0));
    }

    public void Pick(int index)
    {
        if (index >= 0 && index < locked.Count && locked[index])
        {
            if (label != null) label.text = names[index];   // says it isn't owned yet; the choice stays as it was
            return;
        }
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
