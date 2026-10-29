using UnityEngine;
using UnityEngine.UI;

/// Pattern Studio designer: paint a pattern on the grid, name it, choose what it makes,
/// its size and price, then save it to the pattern book.
public class PatternDesignerUI : MonoBehaviour
{
    public ShopDay day;
    public MinigameHost host;

    public InputField nameField;
    public Text makesLabel, sizeLabel, priceLabel;
    public ThreadSwatches swatches;   // its onPick calls SelectColour

    static readonly int[] Sizes = { 8, 10, 12, 16 };
    static readonly string[] ColourNames = { "Red", "Pink", "Gold", "Green", "Sky Blue", "Navy", "Purple", "Charcoal" };

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
        itemIndex = Mathf.Max(0, System.Array.FindIndex(day.content.items, i => i != null && i.itemId == p.itemId));
        price = p.price;
        OpenBoard(Copy(p.design, p.design.width, p.design.height));
        open = true;
        swatches.Show(ShopDay.DesignerPalette, ColourNames);   // picks the first colour
        Refresh();
    }

    /// The design as it is on the board right now.
    Pattern Current()
    {
        var b = host.Board;
        return b != null && b.Ready ? b.pattern : opened;
    }

    void OpenBoard(Pattern design)
    {
        opened = design;
        host.OpenDesign(design);
    }

    static Pattern Copy(Pattern src, int width, int height)
    {
        var p = new Pattern { id = src.id, displayName = src.displayName, craft = src.craft, width = width, height = height };
        p.palette.AddRange(src.palette.Count > 0 ? src.palette : new System.Collections.Generic.List<Color>(ShopDay.DesignerPalette));
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
        if (!erasing) colour = Mathf.Clamp(index, 0, ShopDay.DesignerPalette.Length - 1);
        ApplyTool();
    }

    public void Undo() => host.Undo();

    public void NextItem()
    {
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

    public void Save()
    {
        var d = Current();
        day.SaveDesign(Copy(d, d.width, d.height), nameField.text, day.content.items[itemIndex].itemId, price);
    }

    public void Cancel() => day.CancelDesign();
}
