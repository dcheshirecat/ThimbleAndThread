using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Pattern Studio designer: paint a pattern on the grid, name it, choose what it makes,
/// its size and price, then save it to the pattern book.
public class PatternDesignerUI : MonoBehaviour
{
    public ShopDay day;
    public MinigameHost host;

    public InputField nameField;      // in the naming pop-up
    public GameObject namePopup;      // "Name your pattern", shown when the design is done
    public Text makesLabel, sizeLabel, priceLabel, clothLabel;
    public ThreadSwatches swatches;   // its onPick calls SelectColour

    static readonly int[] Sizes = { 8, 10, 12, 16 };

    // The designer shows every floss the market sells, in catalogue order; only owned ones can be used
    // (owner's choice, 2026-10-30), the rest are greyed out. A design opened for editing keeps any
    // squares already in a colour that's no longer owned.
    List<SupplyDef> threads = new List<SupplyDef>();

    int itemIndex, price, colour;
    bool erasing, open;
    Pattern opened;   // the design last put on the board (used until the board has built itself)

    void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged += OnScreenChanged;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreenChanged;
    }

    void OnScreenChanged(ScreenId id)
    {
        if (id == ScreenId.PatternDesigner) Begin();
        else if (open) { host.Close(); open = false; }
    }

    void Begin()
    {
        var p = day.SelectedPattern;
        nameField.text = p.name;
        if (namePopup != null) namePopup.SetActive(false);
        itemIndex = Mathf.Max(0, System.Array.FindIndex(day.content.items, i => i != null && i.itemId == p.itemId));
        if (day.DesigningFor == null && !Allowed(itemIndex)) StepItem();   // only things you've learned to make
        price = p.price;
        threads = Materials.Threads(day.content);
        cloth = p.design.fabricId;
        OpenBoard(ToCatalogue(p.design));
        open = true;

        var colours = new List<Color>();
        var names = new List<string>();
        var locked = new List<bool>();
        foreach (var t in threads)
        {
            colours.Add(t.color);
            int have = day.shop.Stock(t.supplyId);
            names.Add(have > 0 ? $"{t.displayName} (you have {have} {t.unit})" : $"{t.displayName}: not owned yet. Buy it at the Supply Market to design with it.");
            locked.Add(have <= 0);
        }
        swatches.Show(colours, names, locked);   // picks the first owned colour
        Refresh();
    }

    /// The design with the full thread catalogue as its palette, so every market colour can be painted.
    Pattern ToCatalogue(Pattern design)
    {
        Materials.EnsureThreads(design, day.content);
        var p = new Pattern { id = design.id, displayName = design.displayName, craft = design.craft, width = design.width, height = design.height, fabricId = design.fabricId };
        foreach (var t in threads) { p.palette.Add(t.color); p.threads.Add(t.supplyId); }
        foreach (var c in design.cells)
        {
            int i = c.colorIndex < design.threads.Count ? threads.FindIndex(t => t.supplyId == design.threads[c.colorIndex]) : -1;
            if (i >= 0) p.cells.Add(new StitchCell { x = c.x, y = c.y, colorIndex = i });
        }
        return p;
    }

    /// The design as it is on the board right now (catalogue palette and threads).
    Pattern Current()
    {
        var b = host.Board;
        if (b == null || !b.Ready) return opened;
        var p = b.pattern;
        if (p.threads.Count != threads.Count) { p.threads.Clear(); foreach (var t in threads) p.threads.Add(t.supplyId); }
        return p;
    }

    void OpenBoard(Pattern design)
    {
        opened = design;
        design.fabricId = cloth;
        var c = Cloth;
        host.OpenDesign(design, day.content.items[itemIndex], c != null ? c.color : (Color?)null);   // shown the way the item is made (hoop, rounds, knitted)
    }

    // ---- The cloth (owner, 2026-10-31: design the fabric in the background too) ----

    string cloth;   // supplyId of the cloth, or empty for the plain white Aida

    /// Cloths a cross stitch design can go on: the ones you own (like floss, you buy before you design with it).
    List<SupplyDef> Cloths()
    {
        var list = new List<SupplyDef>();
        if (day.content.items[itemIndex].craft != CraftType.CrossStitch) return list;
        foreach (var s in day.content.supplies)
            if (s != null && s.kind == SupplyKind.Fabric && s.craft == CraftType.CrossStitch && day.shop.Stock(s.supplyId) > 0) list.Add(s);
        return list;
    }

    SupplyDef Cloth
    {
        get
        {
            if (day.content.items[itemIndex].craft != CraftType.CrossStitch) return null;
            var s = string.IsNullOrEmpty(cloth) ? null : day.content.Supply(cloth);
            return s ?? day.content.Supply("aida_14ct");
        }
    }

    public void NextCloth()
    {
        var list = Cloths();
        if (list.Count == 0) return;
        int i = list.FindIndex(s => s.supplyId == Cloth?.supplyId);
        cloth = list[(i + 1) % list.Count].supplyId;
        var d = Current();
        OpenBoard(Copy(d, d.width, d.height));
        ApplyTool();
        Refresh();
    }

    static Pattern Copy(Pattern src, int width, int height)
    {
        var p = new Pattern { id = src.id, displayName = src.displayName, craft = src.craft, width = width, height = height, fabricId = src.fabricId };
        p.palette.AddRange(src.palette);
        p.threads.AddRange(src.threads);
        foreach (var c in src.cells)
            if (c.x < width && c.y < height) p.cells.Add(new StitchCell { x = c.x, y = c.y, colorIndex = c.colorIndex });
        return p;
    }

    void ApplyTool()
    {
        var b = host.Board;
        if (b == null) return;
        b.erase = erasing;
        b.currentColor = colour;
    }

    void Refresh()
    {
        var item = day.content.items[itemIndex];
        var d = Current();
        makesLabel.text = $"Makes: {item.itemName}";
        sizeLabel.text = $"Size: {d.width} x {d.height}";
        if (clothLabel != null)
        {
            var c = Cloth;
            clothLabel.transform.parent.gameObject.SetActive(c != null);   // only cross stitch has a choice of cloth so far
            if (c != null) clothLabel.text = $"Cloth: {c.displayName}";
        }
        priceLabel.text = $"{price} coins";
    }

    // ---- Buttons ----

    public void SelectColour(int index)
    {
        erasing = index < 0;
        if (!erasing) colour = Mathf.Clamp(index, 0, threads.Count - 1);
        ApplyTool();
    }

    public void Undo() => host.Undo();

    public void NextItem()
    {
        if (day.DesigningFor != null) return;   // a commission decides what it makes
        var before = CrossStitchCanvas.FrameFor(day.content.items[itemIndex]);
        StepItem();
        if (CrossStitchCanvas.FrameFor(day.content.items[itemIndex]) != before) { OpenBoard(Copy(Current(), Current().width, Current().height)); ApplyTool(); }
        Refresh();
    }

    bool Allowed(int i) => day.content.items[i] != null && day.Progress.CanMake(day.content.items[i]);

    /// On to the next item the player can make (a craft not learned yet can't be designed for).
    void StepItem()
    {
        int n = day.content.items.Length;
        for (int k = 1; k <= n; k++)
            if (Allowed((itemIndex + k) % n)) { itemIndex = (itemIndex + k) % n; return; }
    }

    /// Steps through 8, 10, 12 and 16 squares; shrinking crops squares that fall outside.
    public void NextSize()
    {
        var d = Current();
        int i = System.Array.IndexOf(Sizes, d.width);
        int size = Sizes[(i + 1) % Sizes.Length];
        OpenBoard(Copy(d, size, size));
        ApplyTool();
        Refresh();
    }

    public void PriceUp() { price = Mathf.Min(999, price + 1); Refresh(); }
    public void PriceDown() { price = Mathf.Max(1, price - 1); Refresh(); }

    public void Clear()
    {
        var d = Current();
        var empty = Copy(d, d.width, d.height);
        empty.cells.Clear();
        OpenBoard(empty);
        ApplyTool();
        Refresh();
    }

    /// Done designing: ask for its name (owner, 2026-10-31: a pop-up when you're done).
    public void Save()
    {
        var d = Current();
        if (day.DesigningFor != null && d.cells.Count == 0)
        {
            swatches.label.text = "Fill in some squares first: this is the commission's new pattern.";
            return;
        }
        if (namePopup == null) { ConfirmSave(); return; }
        namePopup.SetActive(true);
        namePopup.transform.SetAsLastSibling();
    }

    public void ConfirmSave()
    {
        var d = Current();
        if (namePopup != null) namePopup.SetActive(false);
        var saved = Copy(d, d.width, d.height);
        saved.fabricId = Cloth != null ? Cloth.supplyId : "";
        day.SaveDesign(saved, nameField.text, day.content.items[itemIndex].itemId, price);
    }

    public void KeepDesigning() { if (namePopup != null) namePopup.SetActive(false); }

    public void Cancel() => day.CancelDesign();

    public Text halfLabel;
    /// Half stitches: just the "/" leg, for smooth diagonal edges.
    public void ToggleHalf()
    {
        var b = host.Board;
        if (b == null) return;
        b.paintHalf = !b.paintHalf;
        if (halfLabel != null) halfLabel.text = b.paintHalf ? "Half: on" : "Half: off";
    }
}
