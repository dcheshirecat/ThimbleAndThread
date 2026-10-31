using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// Before an order is started, the player gathers its supplies (owner, 2026-10-30): the cupboard scrolls
/// below, a list of what the order needs sits on top, and tapping the right things ticks them off.
/// Picking something the order doesn't need just says so. Once everything is ticked, Start Crafting.
/// Anything missing from the cupboard shows on the list with how much more to buy.
/// Knitting, crochet and sewing start with a choice: which yarn (or fabric) to make it in. That's a line on
/// the list too ("one yarn, 300 stitches"): picking one sets the order's yarn, and the yarns for the
/// pattern's colours (colourwork, stripes) join the list, matched to it.
public class GatherUI : MonoBehaviour
{
    public ShopDay day;
    public Text listText, messageText;
    public RectTransform shelf;          // the cupboard: a grid of tiles
    public Button tileTemplate;          // one supply (children Icon, Name, Info)
    public GameObject startButton;

    /// One line on the list: a material (supply and amount) or a tool (any that fits).
    class Need
    {
        public MaterialNeed material;
        public ToolNeed tool;
        public bool choice;              // the main yarn or fabric, any that fits
        public string pickedId;          // what was put on the workbench for it
        public bool Ticked => pickedId != null;
    }

    readonly List<Need> needs = new List<Need>();
    readonly List<GameObject> tiles = new List<GameObject>();
    Order order;

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
        if (id != ScreenId.Gather || day.Selected == null) return;
        if (order != day.Selected)   // a new order: start the list over (coming back to the same one keeps the ticks)
        {
            order = day.Selected;
            needs.Clear();
            BuildNeeds();
            messageText.text = "Tap the things this order needs to put them on the workbench.";
        }
        Show();
    }

    /// The list for the order as it stands; ticks already made are kept where the line is still there.
    void BuildNeeds()
    {
        var old = new List<Need>(needs);
        needs.Clear();
        if (day.MaterialToChoose(order))
            needs.Add(new Need { choice = true, pickedId = ChoiceFits(order.fabricId) ? order.fabricId : null });
        foreach (var m in day.NeedsFor(order))
            if (!(needs.Count > 0 && needs[0].choice && m.supplyId == order.fabricId)) needs.Add(new Need { material = m });   // the choice line covers it
        foreach (var t in day.ToolsFor(order)) needs.Add(new Need { tool = t });
        foreach (var n in needs)
        {
            if (n.choice) continue;
            var was = old.Find(o => o.Ticked && !o.choice && (n.material != null ? o.material != null && o.material.supplyId == n.material.supplyId : o.tool != null && o.tool.toolType == n.tool.toolType));
            if (was != null && day.content.Supply(was.pickedId) is SupplyDef s && Fits(n, s)) n.pickedId = was.pickedId;
        }
    }

    bool ChoiceFits(string supplyId) =>
        !string.IsNullOrEmpty(supplyId) && ChooseMaterialStep.Options(order, day.content, day.shop).Exists(s => s.supplyId == supplyId);

    public bool AllGathered => needs.TrueForAll(n => n.Ticked);

    // ---- Picking ----

    public void Pick(string supplyId)
    {
        var s = day.content.Supply(supplyId);
        if (s == null) return;
        var need = needs.Find(n => !n.Ticked && Fits(n, s));
        if (need != null && need.choice)   // the yarn (or fabric) it'll be made in: its colour yarns follow
        {
            order.fabricId = supplyId;
            need.pickedId = supplyId;
            BuildNeeds();
            bool more = needs.Exists(n => n.material != null && n.material.supplyId != supplyId && day.content.Supply(n.material.supplyId)?.kind == SupplyKind.Yarn);
            messageText.text = $"{s.displayName}: it'll be made in this." + (more ? " The pattern's colours need their own yarns too: they're on the list." : "");
            AudioManager.Play(Sfx.Stitch);
        }
        else if (need != null)
        {
            need.pickedId = supplyId;
            messageText.text = $"{s.displayName}: on the workbench.";
            AudioManager.Play(Sfx.Stitch);
        }
        else if (needs.Exists(n => n.pickedId == supplyId))
        {
            var picked = needs.Find(n => n.pickedId == supplyId);   // tap again to put it back
            picked.pickedId = null;
            if (picked.choice) { order.fabricId = null; BuildNeeds(); }   // choose again
            messageText.text = $"{s.displayName}: back in the cupboard.";
        }
        else
        {
            var close = needs.Find(n => !n.Ticked && Close(n, s));
            messageText.text = close != null ? $"Not that {Kind(s)}: the list wants {Label(close)}." : $"The {s.displayName.ToLower()} isn't needed for this one.";
        }
        Show();
    }

    bool Fits(Need n, SupplyDef s)
    {
        if (n.choice) return ChoiceFits(s.supplyId);
        if (n.material != null) return s.supplyId == n.material.supplyId && day.shop.Stock(s.supplyId) >= n.material.amount;
        var t = n.tool;
        return s.kind == SupplyKind.Tool && s.toolType == t.toolType && day.shop.Owns(s.supplyId)
               && (t.exactSize ? s.size == t.minSize : s.size >= t.minSize);
    }

    /// Same sort of thing as the line wants, but not the right one (another floss colour, a smaller hoop).
    bool Close(Need n, SupplyDef s)
    {
        if (n.choice) return s.kind == (order.ChooseKind() == StepKind.ChooseYarn ? SupplyKind.Yarn : SupplyKind.Fabric);
        if (n.material != null) { var want = day.content.Supply(n.material.supplyId); return want != null && want.kind == s.kind; }
        return s.kind == SupplyKind.Tool && s.toolType == n.tool.toolType;
    }

    static string Kind(SupplyDef s) => s.kind switch
    {
        SupplyKind.Thread => "floss",
        SupplyKind.Yarn => "yarn",
        SupplyKind.Fabric => "fabric",
        SupplyKind.Tool => "one",
        _ => "one",
    };

    string Label(Need n)
    {
        if (n.tool != null) return n.tool.label;
        if (n.choice)
            return n.Ticked && day.content.Supply(n.pickedId) is SupplyDef chosen ? $"{chosen.displayName} ({order.ChosenAmount()} {chosen.unit}): the main {Kind(chosen)}"
                 : $"Choose {ChooseMaterialStep.Wanted(order, day.content)}";
        var s = day.content.Supply(n.material.supplyId);
        bool colour = s != null && s.kind == SupplyKind.Yarn && s.supplyId != order.fabricId;
        return s != null ? $"{s.displayName} ({n.material.amount} {s.unit}){(colour ? ": for the pattern's colours" : "")}" : n.material.supplyId;
    }

    public void StartCrafting()
    {
        if (!AllGathered)
        {
            messageText.text = needs.Exists(n => !n.Ticked && Missing(n))
                ? "Something on the list isn't in your cupboard. Buy it at the Supply Market by day."
                : "Gather everything on the list first.";
            return;
        }
        order.gathered = true;
        // The yarn or fabric was chosen here: that's its Choose step done.
        if (order.CurrentStep.kind is StepKind.ChooseYarn or StepKind.ChooseFabric && !string.IsNullOrEmpty(order.fabricId))
        {
            var st = order.StepState(order.step);
            st.done = true; st.score = 1f; st.progress = 1f;
            order.step++;
        }
        day.StartCrafting();
    }

    bool Missing(Need n) => n.choice ? ChooseMaterialStep.Options(order, day.content, day.shop).Count == 0
                          : n.material != null ? day.shop.Stock(n.material.supplyId) < n.material.amount : !day.shop.HasTool(n.tool);

    // ---- Showing ----

    void Show()
    {
        var sb = new StringBuilder($"<b>For the {order.item.itemName.ToLower()}</b>\n");
        if (needs.Count == 0) sb.AppendLine("Nothing to gather for this one.");
        foreach (var n in needs)
        {
            string line = Label(n);
            if (n.Ticked) sb.AppendLine($"[x]  {line}");
            else if (Missing(n))
            {
                int have = n.material != null ? day.shop.Stock(n.material.supplyId) : 0;
                sb.AppendLine(n.choice ? $"[  ]  {line}  (you don't have enough of any: buy some)"
                              : n.material != null ? $"[  ]  {line}  (you have {have}: buy more)" : $"[  ]  {line}  (you don't have one: buy it)");
            }
            else sb.AppendLine($"[  ]  {line}");
        }
        listText.text = sb.ToString();
        startButton.GetComponent<Image>().color = AllGathered ? new Color(0.70f, 0.90f, 0.70f) : new Color(0.85f, 0.85f, 0.85f);

        foreach (var t in tiles) Destroy(t);
        tiles.Clear();
        tileTemplate.gameObject.SetActive(false);
        var stock = new List<SupplyStock>(GameManager.Instance.Save.stock);
        stock.Sort((a, b) => SortKey(a).CompareTo(SortKey(b)));
        foreach (var st in stock)
        {
            var s = day.content.Supply(st.supplyId);
            if (s == null || st.amount <= 0 || s.kind == SupplyKind.Book) continue;
            string id = s.supplyId;
            bool picked = needs.Exists(n => n.pickedId == id);
            var tile = Instantiate(tileTemplate, shelf);
            tile.gameObject.SetActive(true);
            tile.name = $"Cupboard_{id}";
            tile.onClick.AddListener(() => Pick(id));
            tile.GetComponent<Image>().color = picked ? new Color(0.75f, 0.92f, 0.75f) : new Color(0.98f, 0.95f, 0.88f);
            var icon = tile.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = s.icon;
            icon.color = s.icon != null ? Color.white : s.color;
            tile.transform.Find("Name").GetComponent<Text>().text = s.displayName;
            tile.transform.Find("Info").GetComponent<Text>().text = picked ? "On the workbench" : s.kind == SupplyKind.Tool ? "Tool" : $"{st.amount} {s.unit}";
            tiles.Add(tile.gameObject);
        }
    }

    int SortKey(SupplyStock st)
    {
        var s = day.content.Supply(st.supplyId);
        return s == null ? 99 : (int)ShopManager.GroupOf(s.kind);
    }
}
