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
    public Text makesLabel, sizeLabel, priceLabel;
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
        price = p.price;
        threads = Materials.Threads(day.content);
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
        var p = new Pattern { id = design.id, displayName = design.displayName, craft = design.craft, width = design.width, height = design.height };
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
        host.OpenDesign(design);
    }

    static Pattern Copy(Pattern src, int width, int height)
    {
        var p = new Pattern { id = src.id, displayName = src.displayName, craft = src.craft, width = width, height = height };
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
        itemIndex = (itemIndex + 1) % day.content.items.Length;
        Refresh();
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
        day.SaveDesign(Copy(d, d.width, d.height), nameField.text, day.content.items[itemIndex].itemId, price);
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
