using UnityEngine;
using UnityEngine.EventSystems;

/// Catches taps on a panel's background so they don't reach a "tap outside to close" button behind it.
/// Unity passes a click up to the nearest parent that handles clicks: without this, a tap anywhere on the
/// journal's pages or the shops menu reached the overlay and closed it (owner, 2026-10-31).
public class ClickBlocker : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData) { }
}
