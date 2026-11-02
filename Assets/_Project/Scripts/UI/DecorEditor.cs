using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Decorating the shop by hand (owner, 2026-11-02): the pencil on the right of the shop scene opens a tray of your
/// furniture and decorations along the bottom. Drag a piece up into the shop: the spot it fits glows, and dropping it
/// there puts it out (swapping whatever was there back into the tray). Drag a piece out of the shop and down into the
/// tray to put it away. Pieces for the pickup corner switch to that page as you pick them up.
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
    int heldFrom = -1;                   // the spot it came from, or -1 from the tray
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
        heldFrom = -1;
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
            bool placed = day.Placed(d.slot) == d;
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
        string help = "Drag a piece up to its spot in the shop. Drag one out of the shop and down here to put it away.";
        trayInfo.text = $"<b>Charm {day.Charm}/{ShopDay.MaxCharm}</b>  ·  " + (note != "" ? note : help);
    }

    /// Spot outlines while editing; the one a piece fits glows (lit = that slot, or none lit).
    void ShowMarkers(DecorDef lit)
    {
        if (markers == null) return;
        for (int i = 0; i < markers.Length; i++)
        {
            if (markers[i] == null) continue;
            markers[i].gameObject.SetActive(Editing);
            markers[i].color = lit != null && (int)lit.slot == i ? MarkerLit : MarkerIdle;
        }
    }

    /// Pieces already out in the shop can be picked up while editing (and only then: otherwise taps pass through).
    void SetSpotsDraggable(bool on)
    {
        if (scene == null || scene.decorSpots == null) return;
        foreach (var spot in scene.decorSpots) if (spot != null) spot.raycastTarget = on;
    }

    static int PageOf(DecorSlot slot) => slot == DecorSlot.PickupWall || slot == DecorSlot.PickupShelf ? 0 : ShopSceneUI.MiddlePage;

    /// Tapping a piece in the tray: what it is and where it goes.
    public void Tapped(DecorPiece piece)
    {
        var d = piece.slot >= 0 ? day.Placed((DecorSlot)piece.slot) : day.Decor(piece.decorId);
        if (d == null || !Editing) return;
        scene.ShowPage(PageOf(d.slot));
        note = $"{d.displayName}: goes {FurnitureUI.SpotName(d.slot)}, +{d.charm} charm" + BoostText(d) + ".";
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
        held = piece.slot >= 0 ? day.Placed((DecorSlot)piece.slot) : day.Decor(piece.decorId);
        heldFrom = piece.slot;
        if (held == null) return;
        scene.ShowPage(PageOf(held.slot));
        if (heldFrom >= 0) scene.decorSpots[heldFrom].enabled = false;   // lifted off its spot
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
        note = $"Bring the {held.displayName.ToLower()} to the glowing spot.";
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

    public void EndDrag(Vector2 screenPos) => Drop(OverTray(screenPos), NearSpot(screenPos));

    /// Where a dragged piece lands: on its spot (put out), on the tray (put away), or anywhere else (back where it was).
    public void Drop(bool onTray, bool onSpot)
    {
        var d = held;
        int from = heldFrom;
        DropGhost();
        held = null;
        heldFrom = -1;
        if (d == null) return;
        if (onSpot)
        {
            var was = day.Placed(d.slot);
            day.Place(d.slot, d.decorId);
            note = was != null && was != d ? $"The {d.displayName.ToLower()} is out; the {was.displayName.ToLower()} is back in the tray."
                                           : $"The {d.displayName.ToLower()} is out in the shop.";
        }
        else if (onTray && from >= 0)
        {
            day.Place(d.slot, "");
            note = $"Put the {d.displayName.ToLower()} away.";
        }
        else note = from >= 0 ? "" : $"That goes {FurnitureUI.SpotName(d.slot)}: drop it on the glowing spot.";
        scene.Refresh();
        Build();
    }

    void DropGhost()
    {
        if (ghost != null) Destroy(ghost.gameObject);
        ghost = null;
        if (scene != null && scene.decorSpots != null) foreach (var spot in scene.decorSpots) if (spot != null) spot.enabled = true;
    }

    bool OverTray(Vector2 screenPos) => RectTransformUtility.RectangleContainsScreenPoint((RectTransform)tray.transform, screenPos, Cam);

    /// Close enough to the held piece's spot: inside its outline, or within a finger's width of it.
    bool NearSpot(Vector2 screenPos)
    {
        if (held == null || markers == null || (int)held.slot >= markers.Length || OverTray(screenPos)) return false;
        var marker = (RectTransform)markers[(int)held.slot].transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(marker, screenPos, Cam, out var local)) return false;
        var r = marker.rect;
        const float slack = 160;
        return local.x > r.xMin - slack && local.x < r.xMax + slack && local.y > r.yMin - slack && local.y < r.yMax + slack;
    }

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
