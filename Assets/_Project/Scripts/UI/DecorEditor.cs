using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Decorating the shop by hand (owner, 2026-11-02): the pencil on the right of the shop scene opens a tray of your
/// furniture and decorations along the bottom. Drag a piece up into the shop and drop it ANYWHERE on the page you're on
/// (owner, 2026-11-02 evening: move them around, not just in and out); drag pieces that are out to move them, or down
/// into the tray to put them away. Swipe the shop's pages to decorate the pickup corner or the display.
public class DecorEditor : MonoBehaviour
{
    public ShopSceneUI scene;
    public ShopDay day;
    public GameObject pencil;            // the shop edit button, on the right near the bottom
    public Image pencilBack;             // lit while editing
    public GameObject tray;              // the strip of your pieces, while editing
    public RectTransform trayContent;    // the pieces, side by side (it scrolls sideways)
    public DecorPiece pieceTemplate;     // children Icon, Name
    public Text trayInfo;
    public Image[] markers;              // one per DecorSlot: the outline of the spot, shown while editing
    public Sprite plainSprite;           // pieces without art yet: a coloured block

    static readonly Color MarkerIdle = new Color(1, 1, 1, 0.28f), MarkerLit = new Color(1f, 0.92f, 0.45f, 0.75f);
    static readonly Color PencilIdle = Color.white, PencilLit = new Color(1f, 0.85f, 0.45f);

    public bool Editing => tray != null && tray.activeSelf;
    readonly List<GameObject> pieces = new List<GameObject>();
    DecorDef held;                       // being dragged
    DecorPiece heldPiece;                // the copy out in the shop it was lifted from, or null from the tray
    RectTransform ghost;
    string note = "";

    SaveData Save => GameManager.Instance != null ? GameManager.Instance.Save : null;
    RectTransform Root => (RectTransform)transform;
    Camera Cam
    {
        get
        {
            var canvas = GetComponentInParent<Canvas>();
            return canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }
    }

    void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged += OnScreenChanged;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreenChanged;
        Stop();
    }

    void OnScreenChanged(ScreenId id)
    {
        if (id != ScreenId.Shop) Stop();
    }

    /// The pencil shows once there's something to decorate with: the workshop has opened, or you own a piece.
    public bool CanEdit => Save != null && !Save.night && (day.ShopOpen(ScreenId.Furniture) || Save.decor.Count > 0);

    void Update()
    {
        if (Save == null || pencil == null) return;
        bool show = CanEdit && GameManager.Instance.Current == ScreenId.Shop;
        if (pencil.activeSelf != show) pencil.SetActive(show);
        if (!show && Editing) Stop();
    }

    /// The pencil: start or stop decorating.
    public void Toggle()
    {
        if (Editing) Stop();
        else Begin();
    }

    public void Begin()
    {
        if (!CanEdit) return;
        tray.SetActive(true);
        tray.transform.SetAsLastSibling();
        pencil.transform.SetAsLastSibling();
        if (pencilBack != null) pencilBack.color = PencilLit;
        note = "";
        Build();
    }

    public void Stop()
    {
        DropGhost();
        held = null;
        heldPiece = null;
        if (tray != null) tray.SetActive(false);
        if (pencilBack != null) pencilBack.color = PencilIdle;
        ShowMarkers(null);
        SetSpotsDraggable(false);
    }

    /// The tray: every piece you own, placed ones marked as out in the shop.
    void Build()
    {
        foreach (var p in pieces) Destroy(p);
        pieces.Clear();
        pieceTemplate.gameObject.SetActive(false);
        foreach (var d in day.content.decor)
        {
            if (d == null || !Save.decor.Contains(d.decorId)) continue;
            var piece = Instantiate(pieceTemplate, trayContent);
            piece.gameObject.SetActive(true);
            piece.name = $"Piece_{d.decorId}";
            piece.editor = this;
            piece.decorId = d.decorId;
            piece.slot = -1;
            var icon = piece.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = d.icon != null ? d.icon : plainSprite;
            icon.color = d.icon != null ? Color.white : d.color;
            bool placed = day.IsOut(d.decorId);
            piece.transform.Find("Name").GetComponent<Text>().text = d.displayName + (placed ? "\n<i>(in the shop)</i>" : "");
            piece.GetComponent<Image>().color = placed ? new Color(0.85f, 0.95f, 0.85f) : Color.white;
            pieces.Add(piece.gameObject);
        }
        if (pieces.Count == 0) note = "Nothing to put out yet: Marsh's Workshop sells furniture and decorations.";
        ShowInfo();
        ShowMarkers(null);
        SetSpotsDraggable(true);
    }

    void ShowInfo()
    {
        string help = "Drag a piece up and drop it anywhere in the shop. Drag pieces about to move them, or down here to put them away.";
        trayInfo.text = $"<b>Charm {day.Charm}/{ShopDay.MaxCharm}</b>  ·  " + (note != "" ? note : help);
    }

    /// Spot outlines while editing; the one a piece fits glows (lit = that slot, or none lit).
    void ShowMarkers(DecorDef lit)
    {
        if (markers == null) return;
        for (int i = 0; i < markers.Length; i++)
        {
            if (markers[i] == null) continue;
            markers[i].gameObject.SetActive(false);   // pieces go anywhere now: no spots to show
        }
    }

    /// Pieces already out in the shop can be picked up while editing (and only then: otherwise taps pass through).
    void SetSpotsDraggable(bool on)
    {
        if (scene == null) return;
        foreach (var piece in scene.DecorShown) if (piece != null) piece.GetComponent<Image>().raycastTarget = on;
    }


    /// Tapping a piece in the tray: what it is and where it goes.
    public void Tapped(DecorPiece piece)
    {
        var d = day.Decor(piece.decorId);
        if (d == null || !Editing) return;
        note = $"{d.displayName}: +{d.charm} charm" + BoostText(d) + (day.IsOut(d.decorId) ? ". Drag it to move it." : ". Drag it into the shop.");
        ShowInfo();
        ShowMarkers(d);
    }

    string BoostText(DecorDef d) => d.boost switch
    {
        DecorBoost.Experience => $", +{d.boostAmount}% experience",
        DecorBoost.Visitors => $", +{d.boostAmount}% visitors {d.boostFrom}:00-{d.boostTo}:00",
        DecorBoost.Friendship => $", +{d.boostAmount}% friendship" + (d.boostFriend != null ? $" with {day.NameFor(d.boostFriend.customerId)}" : ""),
        _ => "",
    };

    // ---- Dragging ----

    public void BeginDrag(DecorPiece piece, Vector2 screenPos)
    {
        if (!Editing) return;
        held = day.Decor(piece.decorId);
        heldPiece = piece.placed ? piece : null;
        if (held == null) return;
        if (heldPiece != null) heldPiece.GetComponent<Image>().enabled = false;   // lifted off the floor
        var go = new GameObject("Held Piece", typeof(RectTransform), typeof(Image));
        ghost = (RectTransform)go.transform;
        ghost.SetParent(Root, false);
        ghost.anchorMin = ghost.anchorMax = new Vector2(0.5f, 0.5f);
        ghost.sizeDelta = new Vector2(280, 280);
        var img = go.GetComponent<Image>();
        img.sprite = held.icon != null ? held.icon : plainSprite;
        img.color = held.icon != null ? new Color(1, 1, 1, 0.9f) : new Color(held.color.r, held.color.g, held.color.b, 0.9f);
        img.preserveAspect = true;
        img.raycastTarget = false;
        ghost.SetAsLastSibling();
        note = $"Drop the {held.displayName.ToLower()} where you'd like it.";
        ShowInfo();
        ShowMarkers(held);
        MoveDrag(screenPos);
    }

    public void MoveDrag(Vector2 screenPos)
    {
        if (ghost == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Root, screenPos, Cam, out var local))
            ghost.anchoredPosition = local - Root.rect.center;
    }

    public void EndDrag(Vector2 screenPos)
    {
        bool onTray = OverTray(screenPos);
        bool onPage = scene.PagePoint(screenPos, Cam, out int page, out var pos);
        Drop(onTray, onPage, page, pos);
    }

    /// Where a dragged piece lands: anywhere on the page (put out, or moved there), on the tray (put away), or off the
    /// shop altogether (back where it was).
    public void Drop(bool onTray, bool onPage, int page, Vector2 pos)
    {
        var d = held;
        bool wasOut = heldPiece != null;
        DropGhost();
        held = null;
        heldPiece = null;
        if (d == null) return;
        if (onTray)
        {
            if (wasOut) { day.PutAway(d.decorId); note = $"Put the {d.displayName.ToLower()} away."; }
            else note = "";
        }
        else if (onPage)
        {
            day.PutOut(d.decorId, page, pos);
            note = wasOut ? $"Moved the {d.displayName.ToLower()}." : $"The {d.displayName.ToLower()} is out in the shop.";
        }
        else note = "";
        scene.RefreshDecor();
        Build();
    }

    void DropGhost()
    {
        if (ghost != null) Destroy(ghost.gameObject);
        ghost = null;
        if (scene != null) foreach (var piece in scene.DecorShown) if (piece != null) piece.GetComponent<Image>().enabled = true;
    }

    bool OverTray(Vector2 screenPos) => RectTransformUtility.RectangleContainsScreenPoint((RectTransform)tray.transform, screenPos, Cam);

    /// For tests: the screen point at the middle of a spot.
    public Vector2 SpotCentre(DecorSlot slot)
    {
        var marker = (RectTransform)markers[(int)slot].transform;
        return RectTransformUtility.WorldToScreenPoint(Cam, marker.TransformPoint(marker.rect.center));
    }

    public Vector2 TrayCentre()
    {
        var t = (RectTransform)tray.transform;
        return RectTransformUtility.WorldToScreenPoint(Cam, t.TransformPoint(t.rect.center));
    }
}
