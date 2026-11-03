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
    public bool Ringing => day.BellRinging;

    float offset;                        // current horizontal shift, in page widths (animated)
    float dragStart;
    bool opening;
    readonly List<GameObject> rows = new List<GameObject>();

    SaveData Save => GameManager.Instance != null ? GameManager.Instance.Save : null;
    bool OnShop => GameManager.Instance != null && GameManager.Instance.Current == ScreenId.Shop;

    void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScreenChanged += OnScreenChanged;
        GameManager.Instance.OnCoinsChanged += OnCoins;
        day.OnBell += Ring;
        // This sits on the shop panel, which the router switches on while the screen change is being
        // announced, too late to hear it: so set up here.
        if (GameManager.Instance.Current == ScreenId.Shop) OnScreenChanged(ScreenId.Shop);
    }

    void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScreenChanged -= OnScreenChanged;
        GameManager.Instance.OnCoinsChanged -= OnCoins;
        day.OnBell -= Ring;
    }

    void OnCoins(int coins) => ShowCoins();

    void OnScreenChanged(ScreenId id)
    {
        if (id != ScreenId.Shop || Save == null) return;
        opening = false;
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
        ShowDecor();
        bool someoneComing = day.CustomersWaiting > 0;
        bubble.SetActive(Ringing);
        bubbleText.text = Ringing ? "Ding-dong! Someone's at the door." : "";
        statusText.text = "";   // owner, 2026-11-01: no running commentary on the shop screen (only chore instructions)
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
        if (day.DayOver && closeConfirm != null && !closeConfirm.activeSelf && !Ringing) ShowClose(true);   // sunset: the day is done
        ShowShops();
        ShowDecor();   // only rebuilds when what's out changes
        ShowWindow();
        if (pastimeButton != null) ShowPastimes();
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
        if (!OnShop || day.CustomersWaiting == 0) return;   // ShopDay rang it (sound and buzz): show it here
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
            bubbleText.text = "No one at the door just now.";
            return;
        }
        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor()
    {
        opening = true;
        day.AnswerDoor();
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

    public void TapSign() => ShowClose(false);
    public void KeepOpen() { if (!day.DayOver) closeConfirm.SetActive(false); }

    /// The OPEN sign's question, or at sunset (owner, 2026-11-03) the game saying the day is done, with only Close Up.
    void ShowClose(bool sunset)
    {
        var question = closeConfirm.transform.Find("Question")?.GetComponent<Text>();
        if (question != null)
            question.text = sunset ? "The sun's going down, and that was everyone for today.\nTime to flip the sign and close up."
                                   : "Flip the sign to CLOSED and end the day?\nAnyone who hasn't come in yet will visit another day.";
        foreach (var b in closeConfirm.GetComponentsInChildren<Button>(true))
        {
            if (b.name == "Button_StayOpen") b.gameObject.SetActive(!sunset);
            if (b.name == "Button_ClosetheShop") { var t = b.GetComponentInChildren<Text>(); if (t != null) t.text = sunset ? "Close Up" : "Close the Shop"; }
        }
        closeConfirm.transform.SetAsLastSibling();
        closeConfirm.SetActive(true);
    }
    public void CloseShop()
    {
        closeConfirm.SetActive(false);
        day.CloseShop();
    }

    public void OpenJournal() => journal.Open();

    // ---- The shops menu (by the coins): the market, the boutique ----

    public GameObject shopsMenu;
    public void ToggleShops() { shopsMenu.SetActive(!shopsMenu.activeSelf); shopsMenu.transform.SetAsLastSibling(); }
    public GameObject shopsButton, marketButton, boutiqueButton;   // only the shops that have opened show
    public void GoToMarket() { shopsMenu.SetActive(false); day.EnterShop(ShopDay.MarketOwnerId, ScreenId.Market); }
    public void GoToBoutique() { shopsMenu.SetActive(false); day.EnterShop(BoutiqueUI.OwnerId, ScreenId.Boutique); }
    public void GoToPatternShop() { shopsMenu.SetActive(false); day.EnterShop(ShopDay.PatternShopOwnerId, ScreenId.PatternShop); }
    public void GoToFurniture() { shopsMenu.SetActive(false); day.EnterShop(ShopDay.FurnitureOwnerId, ScreenId.Furniture); }
    public GameObject patternShopButton, furnitureButton, curiosButton, techButton;
    public void GoToCurios() { shopsMenu.SetActive(false); day.EnterShop(ShopDay.CuriosOwnerId, ScreenId.Curios); }
    public void GoToTech() { shopsMenu.SetActive(false); day.EnterShop(ShopDay.TechOwnerId, ScreenId.TechShop); }
    public GameObject petShopButton;
    public void GoToPetShop() { shopsMenu.SetActive(false); day.EnterShop(ShopDay.PetOwnerId, ScreenId.PetShop); }

    // ---- Shop pets (ShopDayPets): tap to stroke; the first time, name them ----
    public GameObject petNameLayer;
    public InputField petNameField;
    public Text petNameTitle;
    string namingPet;

    public void TapPet(DecorPiece piece)
    {
        var pet = piece.GetComponent<DecorPet>();
        if (pet != null) pet.Wake();
        if (day.PetName(piece.decorId) == null)
        {
            namingPet = piece.decorId;
            petNameTitle.text = $"What will you call your {day.Decor(piece.decorId)?.displayName.ToLower()}?";
            petNameField.text = "";
            petNameLayer.SetActive(true);
            petNameLayer.transform.SetAsLastSibling();
            return;
        }
        Stamps.Notice(day.StrokePet(piece.decorId));
        AudioManager.Play(Sfx.Click);
    }

    public void ConfirmPetName()
    {
        if (namingPet != null && !string.IsNullOrWhiteSpace(petNameField.text)) day.SetPetName(namingPet, petNameField.text);
        petNameLayer.SetActive(false);
        if (namingPet != null && day.PetName(namingPet) != null) Stamps.Notice(day.StrokePet(namingPet));
        namingPet = null;
    }

    // ---- Things to do between customers (owner, 2026-11-02: real things, not minigames): read (decorating: DecorEditor) ----
    public GameObject pastimeButton;      // the row of them in the computer's panel
    public Button readButton;
    public GameObject onlineButton, computer, onlineLayer;
    public Button atticButton, basementButton, onlineTemplate;
    public Text onlineInfo;
    readonly List<GameObject> onlineRows = new List<GameObject>();

    public void SortAttic() { day.SortThrough("attic"); Refresh(); }
    public void SortBasement() { day.SortThrough("basement"); Refresh(); }

    /// The computer on the counter: read, online orders, the attic and basement (owner, 2026-11-02).
    public void ToggleComputer()
    {
        bool show = !onlineLayer.activeSelf;
        onlineLayer.SetActive(show);
        if (!show) return;
        onlineLayer.transform.SetAsLastSibling();
        if (day.OnlineOpen) BuildOnline();
        else ClearOnline("Read a chapter while it's quiet, or tidy up.");
    }

    public void ShowOnline() => BuildOnline();

    void ClearOnline(string info)
    {
        foreach (var r in onlineRows) Destroy(r);
        onlineRows.Clear();
        onlineTemplate.gameObject.SetActive(false);
        onlineInfo.text = info;
    }

    void BuildOnline()
    {
        ClearOnline("");
        int waiting = Save.orders.FindAll(o => o.customerId == ShopDay.OnlineId && o.finished).Count;
        onlineInfo.text = $"Today's online orders. The courier comes on Fridays" + (waiting > 0 ? $" ({waiting} parcel{(waiting == 1 ? "" : "s")} packed)." : ".");
        for (int i = 0; i < Save.onlineOffers.Count; i++)
        {
            var o = Save.onlineOffers[i];
            var p = Save.patterns.Find(x => x.id == o.patternId);
            var item = day.ItemFor(o.itemId);
            int index = i;
            var row = Instantiate(onlineTemplate, onlineTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"Online_{i}";
            row.GetComponentInChildren<Text>().text = $"<b>{(item != null ? item.itemName : o.itemId)}</b> ({(p != null ? p.name : "?")})  ·  pays about {o.price} coins\nTap to accept";
            row.onClick.AddListener(() => { day.AcceptOnline(index); BuildOnline(); });
            onlineRows.Add(row.gameObject);
        }
        if (Save.onlineOffers.Count == 0) onlineInfo.text += "\nNothing new today: more come tomorrow morning.";
    }

    public void Read() { day.ReadChapter(); Refresh(); }

    void ShowPastimes()
    {
        if (!onlineLayer.activeSelf) return;   // the computer's menu, while it's open
        readButton.interactable = day.CanRead;
        var label = readButton.GetComponentInChildren<Text>();
        if (label.text != day.ReadLabel) label.text = day.ReadLabel;
        onlineButton.SetActive(day.OnlineOpen);
        atticButton.gameObject.SetActive(day.HasExpansion("attic"));
        atticButton.interactable = day.CanSort("attic");
        basementButton.gameObject.SetActive(day.HasExpansion("basement"));
        basementButton.interactable = day.CanSort("basement");
    }

    // ---- The shop window (owner, 2026-11-02): up to three things on show ----
    public GameObject windowLayer;
    public Text windowInfo;
    public Button windowTemplate;
    public Image[] windowSlots;          // the little things shown in the window on the shop page
    readonly List<GameObject> windowRows = new List<GameObject>();
    string windowShownFor;

    public void ToggleShopWindow()
    {
        bool show = !windowLayer.activeSelf;
        windowLayer.SetActive(show);
        if (show) { windowLayer.transform.SetAsLastSibling(); BuildWindow(); }
    }

    void BuildWindow()
    {
        foreach (var r in windowRows) Destroy(r);
        windowRows.Clear();
        windowTemplate.gameObject.SetActive(false);
        var shown = day.WindowShown();
        string state = shown.Count == 0 ? "The window's empty." : day.WindowFresh ? $"Dressed on day {Save.windowDay}: passers-by are stopping to look."
                     : $"Dressed on day {Save.windowDay}: it's looking a bit tired. Change something to freshen it up.";
        windowInfo.text = $"Put up to {ShopDay.WindowMax} things in your window: pieces waiting for the stall, or patterns. "
                        + $"A window dressed in the last week draws people in.\n<b>{state}</b>"
                        + (string.IsNullOrEmpty(day.LastMessage) ? "" : $"\n{day.LastMessage}");
        foreach (var e in day.WindowChoices())
        {
            var entry = e;
            var row = Instantiate(windowTemplate, windowTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"WindowRow_{windowRows.Count}";
            bool inWindow = shown.Contains(entry);
            row.GetComponentInChildren<Text>().text = day.WindowLabel(entry) + (inWindow ? "  ·  <b>in the window</b>" : "");
            row.GetComponent<Image>().color = inWindow ? new Color(0.85f, 0.95f, 0.85f) : Color.white;
            row.onClick.AddListener(() => { day.ToggleWindow(entry); BuildWindow(); });
            windowRows.Add(row.gameObject);
        }
        if (windowRows.Count == 0) windowInfo.text += "\nNothing to show yet: make something for the stall, or get a pattern you can make.";
        windowShownFor = null;
    }

    /// The window on the shop page shows what's in it.
    void ShowWindow()
    {
        if (windowSlots == null) return;
        string sig = string.Join(",", Save.window) + Save.stall.Count;
        if (sig == windowShownFor) return;
        windowShownFor = sig;
        var shown = day.WindowShown();
        for (int i = 0; i < windowSlots.Length; i++)
        {
            var item = i < shown.Count ? day.ItemInWindow(shown[i]) : null;
            windowSlots[i].gameObject.SetActive(item != null);
            if (item == null) continue;
            windowSlots[i].sprite = item.icon;
            windowSlots[i].color = item.icon != null ? Color.white : new Color(0.9f, 0.7f, 0.6f);
        }
    }

    // ---- Your furniture and decorations (owner, 2026-11-02: anywhere in the shop) ----
    // decorSpots are the per-kind templates at their usual spots (DecorSlot order). Each piece that's out is a copy:
    // at its usual spot, or wherever it was dropped (on that page's Decor Layer).
    public Image[] decorSpots;
    public readonly List<DecorPiece> DecorShown = new List<DecorPiece>();
    string decorShownFor;

    void ShowDecor()
    {
        if (decorSpots == null) return;
        string sig = string.Join(";", Save.decorPlaced) + "|" + Save.decor.Count;
        if (sig == decorShownFor) return;
        decorShownFor = sig;
        foreach (var p in DecorShown) if (p != null) Destroy(p.gameObject);
        DecorShown.Clear();
        foreach (var spot in decorSpots) if (spot != null) spot.gameObject.SetActive(false);
        foreach (var p in day.PlacedPieces())
        {
            var template = decorSpots[(int)p.def.slot];
            if (template == null) continue;
            Image img;
            if (!p.free) img = Instantiate(template, template.transform.parent);
            else
            {
                var page = pages[Mathf.Clamp(p.page, 0, pages.Length - 1)];
                img = Instantiate(template, DecorLayer(page));
                var rt = (RectTransform)img.transform;
                rt.anchorMin = rt.anchorMax = p.pos;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
            img.gameObject.SetActive(true);
            img.name = $"Decor_{p.def.decorId}";
            img.sprite = p.def.icon;
            img.color = p.def.icon != null ? Color.white : p.def.color;
            img.enabled = true;
            var piece = img.GetComponent<DecorPiece>();
            piece.decorId = p.def.decorId;
            piece.slot = -1;
            piece.placed = true;
            img.raycastTarget = true;   // press and hold to move it; pets are stroked; taps and swipes pass through (DecorPiece)
            if (p.def.pet && img.GetComponent<DecorPet>() == null) img.gameObject.AddComponent<DecorPet>().scene = this;
            DecorShown.Add(piece);
        }
    }

    public DecorEditor decorEditor;

    /// Free-placed pieces sit on a layer that fills the page, above the furniture and below the buttons' clicks
    /// (they only catch touches while decorating).
    static RectTransform DecorLayer(RectTransform page)
    {
        var layer = page.Find("Decor Layer") as RectTransform;
        if (layer == null)
        {
            layer = new GameObject("Decor Layer", typeof(RectTransform)).GetComponent<RectTransform>();
            layer.SetParent(page, false);
            layer.anchorMin = Vector2.zero; layer.anchorMax = Vector2.one;
            layer.offsetMin = layer.offsetMax = Vector2.zero;
        }
        layer.SetAsLastSibling();
        return layer;
    }

    /// Where on the page under a screen point (0..1), for dropping a piece.
    public bool PagePoint(Vector2 screenPos, Camera cam, out int page, out Vector2 pos)
    {
        page = Page;
        pos = Vector2.zero;
        var rt = pages[Page];
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, cam, out var local)) return false;
        var r = rt.rect;
        pos = new Vector2((local.x - r.xMin) / r.width, (local.y - r.yMin) / r.height);
        return pos.x >= 0 && pos.x <= 1 && pos.y >= 0 && pos.y <= 1;
    }

    public void RefreshDecor() { decorShownFor = null; ShowDecor(); }

    /// A shop in the menu: there once it has opened; its hours under its name, greyed out while it's shut.
    void ShopEntry(GameObject entry, ScreenId shop, string name)
    {
        if (entry == null) return;
        entry.SetActive(day.ShopOpen(shop));
        if (!entry.activeSelf) return;
        bool now = day.ShopOpenNow(shop);
        var label = entry.GetComponentInChildren<Text>();
        string text = $"{name}\n<size=68>{day.HoursText(shop)}</size>";
        if (label.text != text) label.text = text;
        entry.GetComponent<Button>().interactable = now;
    }

    void ShowShops()
    {
        if (shopsButton != null)   // there from the start, greyed out until a shop opens
        {
            var button = shopsButton.GetComponent<Button>();
            if (button.interactable != day.AnyShopOpen)
            {
                button.interactable = day.AnyShopOpen;
                button.GetComponentInChildren<Text>().color = day.AnyShopOpen ? new Color(0.2f, 0.15f, 0.2f) : new Color(0.2f, 0.15f, 0.2f, 0.35f);
            }
        }
        ShopEntry(marketButton, ScreenId.Market, "Supply Market");
        ShopEntry(boutiqueButton, ScreenId.Boutique, "The Boutique");
        ShopEntry(patternShopButton, ScreenId.PatternShop, "The Pattern Shop");
        ShopEntry(furnitureButton, ScreenId.Furniture, "The Workshop");
        ShopEntry(curiosButton, ScreenId.Curios, "The Curiosity Shop");
        ShopEntry(techButton, ScreenId.TechShop, "The Repair Shop");
        ShopEntry(petShopButton, ScreenId.PetShop, "The Pet Shop");
        if (!day.AnyShopOpen && shopsMenu.activeSelf) shopsMenu.SetActive(false);
    }

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
            bool online = order.customerId == ShopDay.OnlineId;
            rows.Add(Row(pickupTemplate, $"Pickup_{id}", $"{order.item.itemName} for {day.NameFor(order.customerId)}\n" + (online ? "Packed for Friday's courier" : "Wrapped and waiting"), () => day.SelectOrder(id)));
        }
        int working = Save.orders.Count - ready;
        pickupEmpty.text = ready == 0
            ? "Nothing is waiting to be picked up yet." + (working > 0 ? $"\n{working} order{(working == 1 ? " is" : "s are")} still being made: work on them tonight." : "")
            : "Wrapped orders, waiting for their customers.";
    }

    // ---- Right page: the pattern display ----

    bool choosing;   // the display page is showing the pattern book, to fill an empty spot
    string selected; // a displayed pattern tapped: its actions show under it
    string swapping; // choosing from the book to replace this displayed pattern

    /// The pattern book, to put one on display (not the Pattern Studio, which is unlocked later).
    void BuildChooser()
    {
        displayInfo.text = swapping != null ? "Choose the pattern to put up in its place. The other one goes back in your book."
                                            : "Choose a pattern from your book to put on display. Only things you can make can be sold.";
        foreach (var p in Save.patterns)
        {
            if (p.onShelf || (p.designed && p.design.cells.Count == 0) || !day.PatternShown(p)) continue;
            var item = day.ItemFor(p.itemId);
            bool canMake = day.Progress.CanMake(item);
            string id = p.id;
            bool mystery = day.IsMystery(p);
            var row = Row(displayTemplate, $"Choose_{id}", mystery ? "???\nyou can't make sense of this one yet"
                          : $"{p.name}\n{(item != null ? item.itemName : p.itemId)} · {p.price} coins" + (canMake ? "" : " · you can't make this yet"),
                          () => ChooseFromBook(id));
            row.GetComponent<Button>().interactable = canMake;
            var preview = row.GetComponentInChildren<RawImage>(true);
            if (preview != null) { preview.texture = mystery ? null : PatternPreview.Make(day.DesignFor(p)); preview.gameObject.SetActive(!mystery); }
            rows.Add(row);
        }
        var back = Row(displayTemplate, "Choose_Back", "Back to the display", () => { choosing = false; swapping = null; Refresh(); });
        var backPreview = back.GetComponentInChildren<RawImage>(true);
        if (backPreview != null) backPreview.gameObject.SetActive(false);
        rows.Add(back);
    }

    /// A pattern picked from the book: onto an empty spot, or in place of the one being swapped (which goes back in the book).
    void ChooseFromBook(string id)
    {
        if (swapping != null) day.TakeDown(swapping);
        if (day.PutOnDisplay(id)) { choosing = false; swapping = null; selected = null; }
        Refresh();
    }

    /// Under a tapped display pattern: what can be done with it, right here.
    void Actions(OwnedPattern p)
    {
        string id = p.id;
        AddAction("Action_PutAway", "Put it away in the book", () => { day.TakeDown(id); selected = null; Refresh(); });
        AddAction("Action_Swap", "Swap it for another from the book", () => { swapping = id; choosing = true; Refresh(); });
        AddAction("Action_PriceUp", $"Price up (now {p.price} coins)", () => { day.NudgePrice(id, 1); Refresh(); });
        AddAction("Action_PriceDown", "Price down", () => { day.NudgePrice(id, -1); Refresh(); });
        if (day.Progress.Knows(ShopDay.PatternDesignSkill) && p.designed)
            AddAction("Action_Edit", "Open it in the Pattern Studio", () => { selected = null; day.SelectPattern(id); });
        AddAction("Action_Done", "Done", () => { selected = null; Refresh(); });
    }

    void AddAction(string name, string label, UnityEngine.Events.UnityAction onClick)
    {
        var row = Row(displayTemplate, name, label, onClick);
        row.GetComponent<Image>().color = new Color(0.93f, 0.88f, 0.98f);
        var preview = row.GetComponentInChildren<RawImage>(true);
        if (preview != null) preview.gameObject.SetActive(false);
        rows.Add(row);
    }

    void BuildDisplay()
    {
        if (choosing) { BuildChooser(); return; }
        var shelf = day.Shelf;
        displayInfo.text = $"{shelf.Count} of {Save.displaySpots} spots filled. Tap a pattern to put it away, swap it or change its price; " +
                           "tap an empty spot to choose one from your book.";
        for (int i = 0; i < Save.displaySpots; i++)
        {
            if (i < shelf.Count)
            {
                var p = shelf[i];
                var item = day.ItemFor(p.itemId);
                string id = p.id;
                var row = Row(displayTemplate, $"Display_{i}", $"{p.name}\n{(item != null ? item.itemName : p.itemId)} · {p.price} coins · sold {p.sold}, ordered {p.ordered}", () => { selected = selected == id ? null : id; Refresh(); });
                var preview = row.GetComponentInChildren<RawImage>(true);
                if (preview != null) { preview.texture = PatternPreview.Make(day.DesignFor(p)); preview.gameObject.SetActive(true); }
                rows.Add(row);
                if (selected == id) Actions(p);
            }
            else
            {
                var row = Row(displayTemplate, $"Display_{i}", "Empty spot\nTap to choose a pattern", () => { choosing = true; Refresh(); });
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
