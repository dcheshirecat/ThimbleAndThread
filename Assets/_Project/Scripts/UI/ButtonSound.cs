using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// A soft click when the button on the same object is tapped (and can be pressed).
public class ButtonSound : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData e)
    {
        var b = GetComponent<Selectable>();
        if (b == null || b.IsInteractable()) AudioManager.Play(Sfx.Click);
    }
}
