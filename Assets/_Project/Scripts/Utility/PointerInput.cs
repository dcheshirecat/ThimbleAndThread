using UnityEngine;
using UnityEngine.EventSystems;

/// All press/drag/release input for the crafting boards goes through here (CLAUDE.md: "keep all input in one wrapper
/// so PC mouse support comes cheaply"). One pointer: the first finger on a phone, the left mouse button on a PC.
/// With the legacy Input Manager Unity already maps the first touch to the mouse, so both work today; switching to the
/// new Input System, or adding PC-only extras (right-click = undo...), only needs changes in this file.
public static class PointerInput
{
    /// The pointer went down this frame.
    public static bool Down => Input.GetMouseButtonDown(0);
    /// The pointer is held.
    public static bool Held => Input.GetMouseButton(0);
    /// The pointer came up this frame.
    public static bool Up => Input.GetMouseButtonUp(0);
    /// Where the pointer is, in screen pixels.
    public static Vector3 Position => Input.mousePosition;

    /// True when the pointer is over a UI element (a button, a panel). Checks touches by finger id too, since on phones
    /// IsPointerOverGameObject() with no id only knows about the mouse.
    public static bool OverUI()
    {
        var es = EventSystem.current;
        if (es == null) return false;
        if (es.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
            if (es.IsPointerOverGameObject(Input.GetTouch(i).fingerId)) return true;
        return false;
    }
}
