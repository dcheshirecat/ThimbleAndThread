using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// A piece of furniture you can pick up while decorating (UI/DecorEditor.cs): a tile in the tray (decorId), or a piece
/// out in the shop (slot). In the tray a sideways drag scrolls the tray; dragging up lifts the piece.
public class DecorPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public DecorEditor editor;
    public string decorId;
    public int slot = -1;                // a DecorSlot when it's out in the shop

    ScrollRect passTo;                   // a sideways drag in the tray goes to the tray's scrolling

    public void OnBeginDrag(PointerEventData e)
    {
        passTo = null;
        if (slot < 0 && Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y)) passTo = GetComponentInParent<ScrollRect>();
        if (passTo != null) passTo.OnBeginDrag(e);
        else editor.BeginDrag(this, e.position);
    }

    public void OnDrag(PointerEventData e)
    {
        if (passTo != null) passTo.OnDrag(e);
        else editor.MoveDrag(e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (passTo != null) passTo.OnEndDrag(e);
        else editor.EndDrag(e.position);
        passTo = null;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (!e.dragging) editor.Tapped(this);
    }
}
