using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// A list that scrolls up and down inside something that swipes sideways (the shop scene's pages): a drag that
/// starts mostly sideways is handed on to the page instead of scrolling the list.
public class NestedScroll : ScrollRect
{
    bool passOn;

    public override void OnBeginDrag(PointerEventData e)
    {
        passOn = Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y);
        if (passOn) ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, e, ExecuteEvents.beginDragHandler);
        else base.OnBeginDrag(e);
    }

    public override void OnDrag(PointerEventData e)
    {
        if (passOn) ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, e, ExecuteEvents.dragHandler);
        else base.OnDrag(e);
    }

    public override void OnEndDrag(PointerEventData e)
    {
        if (passOn) ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, e, ExecuteEvents.endDragHandler);
        else base.OnEndDrag(e);
        passOn = false;
    }
}
