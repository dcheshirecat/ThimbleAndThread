using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// A piece of furniture you can pick up (UI/DecorEditor.cs): a tile in the tray (decorId), or a piece out in the shop
/// (placed). In the tray a sideways drag scrolls the tray; dragging up lifts the piece. Out in the shop (owner, 2026-11-03:
/// "I tried to move the rug and it stayed put"), press and hold a piece until it pops up, then drag it anywhere; a quick
/// swipe across it still turns the shop's pages, and a tap goes through to whatever is under it (pets are stroked).
public class DecorPiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public const float HoldTime = 0.35f;

    public DecorEditor editor;
    public string decorId;
    public int slot = -1;                // the template at a DecorSlot's usual spot (never dragged itself)
    public bool placed;                  // a piece out in the shop (decorId), not a tile in the tray

    ScrollRect passTo;                   // a sideways drag in the tray goes to the tray's scrolling
    ShopSceneUI swipe;                   // a quick swipe over a piece in the shop turns the page
    float downAt = -1;                   // when the finger went down on a placed piece
    bool lifted;

    bool Held => placed && downAt >= 0 && Time.unscaledTime - downAt >= HoldTime;

    public void OnPointerDown(PointerEventData e)
    {
        if (placed) downAt = Time.unscaledTime;
    }

    public void OnPointerUp(PointerEventData e)
    {
        downAt = -1;
        Pop(false);
    }

    void Update()
    {
        if (placed && downAt >= 0 && !lifted && Held) Pop(true);   // ready to move: it lifts a little
    }

    void Pop(bool up)
    {
        lifted = up;
        transform.localScale = up ? Vector3.one * 1.1f : Vector3.one;
        if (up) AudioManager.Play(Sfx.Click);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        passTo = null;
        swipe = null;
        if (placed)
        {
            if (!editor.Editing && !Held) swipe = editor.scene;   // not held long enough: it's a swipe
        }
        else if (Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y)) passTo = GetComponentInParent<ScrollRect>();
        if (passTo != null) passTo.OnBeginDrag(e);
        else if (swipe != null) swipe.OnBeginDrag(e);
        else editor.BeginDrag(this, e.position);
    }

    public void OnDrag(PointerEventData e)
    {
        if (passTo != null) passTo.OnDrag(e);
        else if (swipe != null) swipe.OnDrag(e);
        else editor.MoveDrag(e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (passTo != null) passTo.OnEndDrag(e);
        else if (swipe != null) swipe.OnEndDrag(e);
        else editor.EndDrag(e.position);
        passTo = null;
        swipe = null;
        downAt = -1;
        Pop(false);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.dragging) return;
        if (placed && !editor.Editing)
        {
            if (editor.scene.day.Decor(decorId) is DecorDef d && d.pet) editor.scene.TapPet(this);
            else PassClick(e);
            return;
        }
        editor.Tapped(this);
    }

    /// A tap on a piece out in the shop goes to the button under it (a parcel, the door...), as if the piece weren't there.
    void PassClick(PointerEventData e)
    {
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(e, hits);
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject || h.gameObject.transform.IsChildOf(transform)) continue;
            var target = ExecuteEvents.GetEventHandler<IPointerClickHandler>(h.gameObject);
            if (target == null || target == gameObject || target.GetComponent<DecorPiece>() != null) continue;
            ExecuteEvents.Execute(target, e, ExecuteEvents.pointerClickHandler);
            return;
        }
    }
}
