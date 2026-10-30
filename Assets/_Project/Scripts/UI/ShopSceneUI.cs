using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// The shop by day, as a place rather than a menu (owner, 2026-10-30). Three pages side by side:
/// left, orders waiting to be picked up; middle, the shopkeeper behind the counter and the shop door;
/// right, the pattern display. Swipe (or tap the arrows) to move between them. A coin counter sits in
/// the corner and the OPEN sign flips to close the shop for the night. When a customer arrives the
/// doorbell rings and the door shakes; tap the door to let them in, and the scene cuts to them.
/// Placeholder shapes stand in for the shopkeeper and the door until the owner's art exists.
public class ShopSceneUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ShopDay day;
    public JournalUI journal;
    public RectTransform[] pages;        // left (pickups), middle (shop), right (display)
    public Text coinText, statusText, bubbleText, pageLabel;
    public RectTransform door;           // shakes when the bell rings, swings when opened
    public GameObject bubble;            // "Someone's at the door!"
    public GameObject closeConfirm;      // "Close the shop for the night?"
    [Header("Left page")] public Button pickupTemplate; public Text pickupEmpty;
    [Header("Right page")] public Button displayTemplate; public Text displayInfo;

    public const int MiddlePage = 1;
    public int Page { get; private set; } = MiddlePage;
    public bool Ringing { get; private set; }

    float offset;                        // current horizontal shift, in page widths (animated)
    float dragStart;
    float bellTimer = -1;
    bool opening;
    readonly List<GameObject> rows = new List<GameObject>();

    SaveData Save => GameManager.Instance != null ? GameManager.Instance.Save : null;
    bool OnShop => GameManager.Instance != null && GameManager.Instance.Current == ScreenId.Shop;

    void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScreenChanged += OnScreenChanged;
        GameManager.Instance.OnCoinsChanged += OnCoins;
        // This sits on the shop panel, which the router switches on while the screen change is being
        // announced, too late to hear it: so set up here.
        if (GameManager.Instance.Current == ScreenId.Shop) OnScreenChanged(ScreenId.Shop);
    }

    void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScreenChanged -= OnScreenChanged;
        GameManager.Instance.OnCoinsChanged -= OnCoins;
    }

    void OnCoins(int coins) => ShowCoins();

    void OnScreenChanged(ScreenId id)
    {
        if (id != ScreenId.Shop || Save == null) return;
        Ringing = false;
        opening = false;
        bellTimer = Random.Range(1.5f, 3.5f);   // someone's on their way...
        if (closeConfirm != null) closeConfirm.SetActive(false);
        door.localRotation = Quaternion.identity;
        door.localScale = Vector3.one;
        Refresh();
    }

    public void Refresh()
    {
        if (Save == null) return;
        ShowCoins();
        BuildPickups();
        BuildDisplay();
        bool someoneComing = day.CustomersWaiting > 0;
        bubble.SetActive(Ringing || !someoneComing);
        bubbleText.text = Ringing ? "Ding-dong! Someone's at the door." : someoneComing ? "" : "It's gone quiet. Flip the sign to close up when you're ready.";
        statusText.text = $"Day {Save.day}" + (day.Progress.Chapter is ChapterDef ch ? $"  ·  Chapter {ch.number}: {ch.title}" : "") +
                          (string.IsNullOrEmpty(day.LastMessage) ? "" : $"\n{day.LastMessage}");
        ShowPage(Page, instant: true);
    }

    void ShowCoins()
    {
        if (Save != null && coinText != null) coinText.text = Save.coins.ToString();
    }

    // ---- The doorbell ----

    void Update()
    {
        if (!OnShop || Save == null) return;
        if (!Ringing && bellTimer > 0 && day.CustomersWaiting > 0 && !journal.IsOpen)
        {
            bellTimer -= Time.unscaledDeltaTime;
            if (bellTimer <= 0) Ring();
        }
        if (Ringing && !opening)   // the door rattles now and then until it's answered
        {
            float t = Time.unscaledTime % 1.6f;
            door.localRotation = Quaternion.Euler(0, 0, t < 0.35f ? Mathf.Sin(t * 60f) * 3f * (1 - t / 0.35f) : 0);
        }
        // Ease the pages toward the one chosen.
        float target = -Page;
        if (Mathf.Abs(offset - target) > 0.0005f && !dragging)
        {
            offset = Mathf.Lerp(offset, target, 1 - Mathf.Exp(-14f * Time.unscaledDeltaTime));
            Place();
        }
    }

    /// The bell rings: someone is at the door.
    public void Ring()
    {
        if (day.CustomersWaiting == 0) return;
        Ringing = true;
        AudioManager.Play(Sfx.Bell);
        if (Page != MiddlePage) ShowPage(MiddlePage);
        Refresh();
    }

    /// Tapping the door: lets the customer in (the scene cuts to them), or says no one's there.
    public void TapDoor()
    {
        if (opening) return;
        if (!Ringing)
        {
            bubble.SetActive(true);
            bubbleText.text = day.CustomersWaiting > 0 ? "No one yet... they'll ring when they arrive." : "It's gone quiet. Flip the sign to close up when you're ready.";
            return;
        }
        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor()
    {
        opening = true;
        Ringing = false;
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.35f)   // the door swings open (squashes toward its hinge)
        {
            door.localRotation = Quaternion.identity;
            door.localScale = new Vector3(Mathf.Lerp(1, 0.15f, t * t), 1, 1);
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.1f);
        opening = false;
        day.GreetCustomer();   // the dialogue: the customer takes the shopkeeper's place on screen
    }

    // ---- The OPEN sign ----

    public void TapSign() => closeConfirm.SetActive(true);
    public void KeepOpen() => closeConfirm.SetActive(false);
    public void CloseShop()
    {
        closeConfirm.SetActive(false);
        day.CloseShop();
    }

    public void OpenJournal() => journal.Open();

    // ---- Pages and swiping ----

    public void Left() => ShowPage(Mathf.Max(0, Page - 1));
    public void Right() => ShowPage(Mathf.Min(pages.Length - 1, Page + 1));

    public void ShowPage(int p, bool instant = false)
    {
        Page = Mathf.Clamp(p, 0, pages.Length - 1);
        if (instant) { offset = -Page; Place(); }
        pageLabel.text = string.Join("  ", new[] { Page == 0 ? "●" : "○", Page == 1 ? "●" : "○", Page == 2 ? "●" : "○" });   // page dots
    }

    bool dragging;

    public void OnBeginDrag(PointerEventData e) { dragging = true; dragStart = offset; }

    public void OnDrag(PointerEventData e)
    {
        float width = ((RectTransform)transform).rect.width;
        var canvas = GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1f;
        offset = Mathf.Clamp(dragStart + (e.position.x - e.pressPosition.x) / scale / Mathf.Max(1, width), -(pages.Length - 1) - 0.2f, 0.2f);
        Place();
    }

    public void OnEndDrag(PointerEventData e)
    {
        dragging = false;
        float moved = offset - dragStart;   // positive = dragged right = toward the left page
        if (moved > 0.15f) Left();
        else if (moved < -0.15f) Right();
        else ShowPage(Page);
    }

    /// Lays the pages side by side, each one screen wide, shifted by the offset. Done with anchors (which
    /// may go past 0..1), so it holds whatever the screen size and before the layout has been measured.
    void Place()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].anchorMin = new Vector2(i + offset, 0);
            pages[i].anchorMax = new Vector2(i + offset + 1, 1);
            pages[i].offsetMin = pages[i].offsetMax = Vector2.zero;
        }
    }

    // ---- Left page: orders waiting to be picked up ----

    void BuildPickups()
    {
        ClearRows();
        pickupTemplate.gameObject.SetActive(false);
        displayTemplate.gameObject.SetActive(false);
        int ready = 0;
        foreach (var order in Save.orders)
        {
            if (!order.finished) continue;
            ready++;
            var c = day.CustomerFor(order.customerId);
            string id = order.id;
            rows.Add(Row(pickupTemplate, $"Pickup_{id}", $"{order.item.itemName} for {(c != null ? c.displayName : order.customerId)}\nWrapped and waiting", () => day.SelectOrder(id)));
        }
        int working = Save.orders.Count - ready;
        pickupEmpty.text = ready == 0
            ? "Nothing is waiting to be picked up yet." + (working > 0 ? $"\n{working} order{(working == 1 ? " is" : "s are")} still being made: work on them tonight." : "")
            : "Wrapped orders, waiting for their customers.";
    }

    // ---- Right page: the pattern display ----

    void BuildDisplay()
    {
        var shelf = day.Shelf;
        displayInfo.text = $"{shelf.Count} of {Save.displaySpots} spots filled. Tap a pattern to change its price or take it down; " +
                           "tap an empty spot to choose one from your book.";
        for (int i = 0; i < Save.displaySpots; i++)
        {
            if (i < shelf.Count)
            {
                var p = shelf[i];
                var item = day.ItemFor(p.itemId);
                string id = p.id;
                var row = Row(displayTemplate, $"Display_{i}", $"{p.name}\n{(item != null ? item.itemName : p.itemId)} · {p.price} coins · sold {p.sold}, ordered {p.ordered}", () => day.SelectPattern(id));
                var preview = row.GetComponentInChildren<RawImage>(true);
                if (preview != null) { preview.texture = PatternPreview.Make(day.DesignFor(p)); preview.gameObject.SetActive(true); }
                rows.Add(row);
            }
            else
            {
                var row = Row(displayTemplate, $"Display_{i}", "Empty spot\nTap to choose a pattern", () => GameManager.Instance.ChangeScreen(ScreenId.PatternStudio));
                var preview = row.GetComponentInChildren<RawImage>(true);
                if (preview != null) preview.gameObject.SetActive(false);
                rows.Add(row);
            }
        }
    }

    static GameObject Row(Button template, string name, string label, UnityEngine.Events.UnityAction onClick)
    {
        var row = Instantiate(template, template.transform.parent);
        row.gameObject.SetActive(true);
        row.name = name;
        row.GetComponentInChildren<Text>().text = label;
        row.onClick.AddListener(onClick);
        return row.gameObject;
    }

    void ClearRows()
    {
        foreach (var r in rows)
        {
            if (r == null) continue;
            var img = r.GetComponentInChildren<RawImage>(true);
            if (img != null && img.texture != null) Destroy(img.texture);
            Destroy(r);
        }
        rows.Clear();
    }

    void OnDestroy() => ClearRows();
}
