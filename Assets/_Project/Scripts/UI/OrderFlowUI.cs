using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Fills in the text, portraits, lists and pattern previews on the shop screens whenever the screen changes.
public class OrderFlowUI : MonoBehaviour
{
    public ShopDay day;

    [Header("Shop")] public Text shopStatus; public Text greetLabel;   // optional: the old menu hub (the shop scene has its own)
    [Header("Workroom")] public Text workroomStatus;
    [Header("Order book")] public Button orderTemplate; public Text orderBookEmpty;
    [Header("Workbench")] public Text workbenchText; public Text startLabel; public GameObject startButton, handOverButton;
    [Header("Minigame")] public Text minigameHint; public ThreadSwatches minigameThreads;
    [Header("Result")] public Text resultText; public Image resultPortrait;
    public Button[] extraButtons;        // special touches that suit the piece (lace, buttons...), toggled on and off
    [Header("Day summary")] public Text summaryText;
    [Header("Pattern Studio")] public Button patternTemplate;
    [Header("Supply Market")] public Text marketStatus;
    public Text shelfTitleTemplate;       // a shelf's name, e.g. "Yarn"
    public RectTransform shelfTemplate;   // a grid of supply tiles
    public Button marketTemplate;         // one supply tile (children Icon, Name, Info), inside shelfTemplate
    [Header("Pattern edit")] public Text patternText, shelfLabel, deleteLabel; public RawImage patternPreview;
    public GameObject editDesignButton, deleteButton;

    readonly List<GameObject> orderRows = new List<GameObject>();
    readonly List<GameObject> patternRows = new List<GameObject>();

    void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScreenChanged += Refresh;
        Refresh(GameManager.Instance.Current);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= Refresh;
    }

    void Refresh(ScreenId screen)
    {
        var save = GameManager.Instance.Save;
        if (save == null) return;   // title and save select: no game loaded yet

        int ready = save.orders.FindAll(o => o.finished).Count;
        int toDo = save.orders.Count - ready;
        if (shopStatus != null) shopStatus.text = $"Day {save.day}    {save.coins} coins\n" +
                          (day.Progress.Chapter is ChapterDef ch ? $"Chapter {ch.number}: {ch.title}\n" : "") +
                          $"On display: {day.Shelf.Count} of {save.displaySpots} patterns\n" +
                          $"Ready to hand over: {ready}\nIn progress: {toDo}\n\n{day.LastMessage}";
        if (greetLabel != null) greetLabel.text = day.CustomersWaiting > 0 ? $"Greet Customer ({day.CustomersWaiting})" : "No one waiting";
        workroomStatus.text = $"Night of day {save.day}    {save.coins} coins\n" +
                              $"Orders to work on: {toDo}\nSet aside for tomorrow: {ready}\n\n{day.LastMessage}";
        summaryText.text = "The shop is closed for the night.\n\n" +
                           $"Patterns sold: {save.patternsSoldToday}\nOrders handed over: {save.deliveredToday}\n" +
                           $"Coins earned today: {save.coinsToday}\nTotal coins: {save.coins}\n\n" +
                           $"<b>Looking ahead</b>\n{day.LookingAhead()}";

        if (screen == ScreenId.OrderBook) BuildOrderList();
        if (screen == ScreenId.Market) BuildMarketList(save);
        if (screen == ScreenId.PatternStudio) BuildPatternList();
        if (screen == ScreenId.PatternEdit && day.SelectedPattern != null) ShowPattern(day.SelectedPattern);

        var order = day.Selected;
        if (order == null) return;
        var customer = day.CustomerFor(order.customerId);
        string who = customer != null ? customer.displayName : order.customerId;
        int progress = Mathf.RoundToInt(order.Progress() * 100);

        bool night = save.night;
        workbenchText.text = $"{order.item.itemName} for {who}\n\n" +
            CommissionText(order) +
            (order.patternOnly ? "Pattern only: fill in the squares to design it." : RecipeText(order)) +
            (order.finished ? $"\nFinished ({Mathf.RoundToInt(order.accuracy * 100)}% accuracy)." : $"\nProgress: {progress}%") +
            MaterialsText(order) +
            (night ? "" : order.finished ? $"\n\nWrapped and waiting. {who} will come by for it; you never quite know when." : "\n\nYou can work on this tonight, in the workroom.");
        if (!string.IsNullOrEmpty(day.LastMessage) && screen == ScreenId.Workbench) workbenchText.text += $"\n\n{day.LastMessage}";
        bool started = order.work.Count > 0 || order.step > 0;
        startLabel.text = order.needsDesign ? "Design the Pattern"
            : order.finished ? "Rework It"
            : order.Steps.Length > 1 ? (started ? "Keep Working: " : "Start: ") + order.CurrentStep.label
            : started ? "Keep Working" : "Start Crafting";
        startButton.SetActive(night);
        handOverButton.SetActive(false);   // customers collect when they come back (ShopDay.GreetCustomer)

        var step = order.CurrentStep;
        bool stitching = step.kind == StepKind.CrossStitch;
        minigameHint.text = order.patternOnly ? "Tap or drag over the faint squares to fill them in."
            : stitching ? "Stitch / first (bottom left to top right), then cross over it with \\ to finish the X. Every X should cross the same way: the other way round is a mistake (Undo unpicks it). Match each square's thread colour."
            : StepHint(step.kind);
        if (order.Steps.Length > 1) minigameHint.text = $"{step.label}. {minigameHint.text}";
        if (screen == ScreenId.Minigame && !string.IsNullOrEmpty(day.LastMessage)) minigameHint.text += $"\n{day.LastMessage}";
        if (stitching && order.item.craft != CraftType.CrossStitch && order.Steps.Length == 1)
            minigameHint.text += $" (The {CraftName(order.item.craft)} minigame is coming soon; cross stitch stands in.)";
        minigameThreads.gameObject.SetActive(stitching);
        minigameThreads.label.gameObject.SetActive(stitching);
        if (screen == ScreenId.Minigame && stitching)
        {
            var names = new List<string>();
            foreach (var id in order.target.threads) { var t = day.content.Supply(id); names.Add(t != null ? t.displayName : "Thread"); }
            minigameThreads.Show(order.target.palette, names);
        }

        resultText.text = $"{order.item.itemName} for {who}\n\n" +
                          $"Accuracy: {Mathf.RoundToInt(day.LastAccuracy * 100)}%\n" +
                          $"Stars: {day.LastStars} / 3\n" +
                          $"{who} will pay {day.LastPrice} coins\n" +
                          $"Friendship: +{day.LastPoints}\n\n" +
                          (string.IsNullOrEmpty(day.LastLearned) ? "" : $"{day.LastLearned}\n\n") +
                          "Set it aside: they'll come by for it one of these days.";
        var extras = day.ExtrasFor(order);
        if (extras.Count > 0)
            resultText.text += "\n\nAdd a special touch? They might love the surprise... or not.";
        for (int i = 0; i < extraButtons.Length; i++)
        {
            bool used = i < extras.Count;
            extraButtons[i].gameObject.SetActive(used);
            if (!used) continue;
            var s = extras[i];
            string id = s.supplyId;
            bool on = order.extras.Contains(id);
            extraButtons[i].GetComponentInChildren<Text>().text = on ? $"Remove {s.displayName}" : $"Add {s.displayName}";
            extraButtons[i].GetComponent<Image>().color = on ? new Color(0.75f, 0.92f, 0.75f) : Color.white;
            extraButtons[i].onClick.RemoveAllListeners();
            extraButtons[i].onClick.AddListener(() => day.ToggleExtra(id));
        }
        var mood = day.LastStars >= 3 ? Expression.Happy : day.LastStars == 2 ? Expression.Thoughtful : Expression.Sad;
        SetPortrait(resultPortrait, customer != null ? customer.GetPortrait(mood) : null);
    }

    void ShowPattern(OwnedPattern p)
    {
        var item = day.ItemFor(p.itemId);
        patternText.text = $"{p.name}\n\nMakes: {(item != null ? item.itemName : p.itemId)}\n" +
                           $"Price: {p.price} coins\n" +
                           (p.designed ? "Your own design" : "From your great-aunt's pattern book") + "\n\n" +
                           $"Sold {p.sold} {(p.sold == 1 ? "copy" : "copies")}, {p.ordered} {(p.ordered == 1 ? "order" : "orders")} so far.\n\n" +
                           (p.onShelf ? $"On display ({day.Shelf.Count} of {GameManager.Instance.Save.displaySpots}): customers can buy it or order the finished piece."
                                      : "Not on the shelf: customers won't see it.");
        if (!string.IsNullOrEmpty(day.LastMessage)) patternText.text += $"\n\n{day.LastMessage}";
        shelfLabel.text = p.onShelf ? "Take Off Display" : "Put on Display";
        editDesignButton.SetActive(p.designed);
        deleteButton.SetActive(p.designed);
        deleteLabel.text = day.ConfirmingDelete ? "Tap Again to Delete" : "Delete";
        SetPreview(patternPreview, day.DesignFor(p));
    }

    void BuildOrderList()
    {
        foreach (var row in orderRows) Destroy(row);
        orderRows.Clear();
        orderTemplate.gameObject.SetActive(false);
        orderBookEmpty.gameObject.SetActive(day.Orders.Count == 0);

        foreach (var order in day.Orders)
        {
            var customer = day.CustomerFor(order.customerId);
            string kind = order.patternOnly ? " (pattern)" : !string.IsNullOrEmpty(order.commissionId) ? " (commission)" : "";
            string state = order.finished ? "Ready to hand over" : $"{Mathf.RoundToInt(order.Progress() * 100)}%";
            string id = order.id;
            orderRows.Add(AddRow(orderTemplate, $"Order_{id}",
                $"{order.item.itemName}{kind} for {(customer != null ? customer.displayName : order.customerId)}  ·  {state}",
                () => day.SelectOrder(id)));
        }
    }

    void BuildPatternList()
    {
        foreach (var row in patternRows) Destroy(row);
        patternRows.Clear();
        patternTemplate.gameObject.SetActive(false);

        foreach (var p in day.Patterns)
        {
            var item = day.ItemFor(p.itemId);
            string id = p.id;
            patternRows.Add(AddRow(patternTemplate, $"Pattern_{id}",
                $"{p.name}  ·  {(item != null ? item.itemName : p.itemId)}  ·  {p.price} coins  ·  {(p.onShelf ? "On display" : "In the book")}  ·  sold {p.sold}, ordered {p.ordered}",
                () => day.SelectPattern(id)));
        }
    }

    readonly List<GameObject> marketRows = new List<GameObject>();

    // The market's shelves, top to bottom, and the kinds of supply each one holds.
    static readonly (string title, SupplyKind[] kinds)[] Shelves =
    {
        ("Fabric", new[] { SupplyKind.Fabric }),
        ("Embroidery Floss", new[] { SupplyKind.Thread }),
        ("Yarn", new[] { SupplyKind.Yarn }),
        ("Notions", new[] { SupplyKind.Stuffing, SupplyKind.Eyes, SupplyKind.Notion, SupplyKind.Other }),
        ("Tools", new[] { SupplyKind.Tool }),
        ("Books", new[] { SupplyKind.Book }),
    };

    // Tiles by rarity: common, uncommon, rare; sold-out tiles are greyed.
    static readonly Color TileColor = new Color(0.98f, 0.95f, 0.88f), UncommonTileColor = new Color(0.80f, 0.90f, 1f),
        RareTileColor = new Color(1f, 0.88f, 0.55f), SoldOutTileColor = new Color(0.7f, 0.68f, 0.66f);

    /// The Supply Market as shelves: one per kind of supply, each a grid of tiles showing the supply's icon.
    void BuildMarketList(SaveData save)
    {
        foreach (var row in marketRows) Destroy(row);
        marketRows.Clear();
        shelfTitleTemplate.gameObject.SetActive(false);
        shelfTemplate.gameObject.SetActive(false);
        marketTemplate.gameObject.SetActive(false);
        int restock = day.shop.DaysUntilRestock;
        marketStatus.text = $"{save.coins} coins. Tap something to buy it.  " +
                            (restock == 1 ? "New stock tomorrow." : $"New stock in {restock} days.") + $"\n{day.LastMessage}";

        var sale = day.shop.OnSale();
        foreach (var (title, kinds) in Shelves)
        {
            var onShelf = sale.FindAll(s => System.Array.IndexOf(kinds, s.kind) >= 0);
            if (onShelf.Count == 0) continue;

            var shelf = NewShelf(title);
            foreach (var s in onShelf) AddTile(shelf, s);
        }

        var upgrades = day.Progress.UpgradesOnOffer();
        if (upgrades.Count > 0)
        {
            var shelf = NewShelf("For the Shop");
            foreach (var u in upgrades) AddUpgradeTile(shelf, u);
        }
    }

    RectTransform NewShelf(string title)
    {
        var heading = Instantiate(shelfTitleTemplate, shelfTitleTemplate.transform.parent);
        heading.gameObject.SetActive(true);
        heading.text = title;
        marketRows.Add(heading.gameObject);
        var shelf = Instantiate(shelfTemplate, shelfTemplate.parent);
        shelf.gameObject.SetActive(true);
        shelf.name = $"Shelf_{title.Replace(" ", "")}";
        marketRows.Add(shelf.gameObject);
        return shelf;
    }

    /// A shop upgrade on the "For the Shop" shelf: bought once.
    void AddUpgradeTile(RectTransform shelf, UpgradeDef u)
    {
        string id = u.upgradeId;
        var tile = Instantiate(marketTemplate, shelf);
        tile.gameObject.SetActive(true);
        tile.name = $"Upgrade_{id}";
        tile.onClick.AddListener(() => day.BuyUpgrade(id));
        tile.GetComponent<Image>().color = new Color(0.90f, 0.85f, 0.95f);
        var icon = tile.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = u.icon;
        icon.color = u.icon != null ? Color.white : new Color(0.55f, 0.45f, 0.70f);
        tile.transform.Find("Name").GetComponent<Text>().text = $"Shop upgrade\n{u.displayName}";
        tile.transform.Find("Info").GetComponent<Text>().text = $"{u.price} coins\n{u.description}";
    }

    void AddTile(RectTransform shelf, SupplyDef s)
    {
        string id = s.supplyId;
        var tile = Instantiate(marketTemplate, shelf);
        tile.gameObject.SetActive(true);
        tile.name = $"Supply_{id}";
        tile.onClick.AddListener(() => day.BuyPack(id));
        int left = day.shop.PacksLeft(s);
        tile.GetComponent<Image>().color = left <= 0 ? SoldOutTileColor
            : s.market == MarketSlot.Rare ? RareTileColor : s.market == MarketSlot.Rotating ? UncommonTileColor : TileColor;

        var icon = tile.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = s.icon;
        icon.color = s.icon != null ? Color.white : s.color;   // no art yet: a swatch of its colour
        if (left <= 0) icon.color *= new Color(0.6f, 0.6f, 0.6f, 0.6f);

        tile.transform.Find("Name").GetComponent<Text>().text = $"{ShopManager.Rarity(s)}\n{s.displayName}";
        string stock = left <= 0 ? $"Sold out: back in {day.shop.DaysUntilBack(s)} days" : $"{left} left";
        var teaches = s.kind == SupplyKind.Book ? System.Array.Find(day.content.skills, k => k != null && k.book == s) : null;
        tile.transform.Find("Info").GetComponent<Text>().text = s.kind == SupplyKind.Book
            ? $"{s.price} coins  ·  {(day.shop.Owns(id) ? "Read" : stock)}\nTeaches {(teaches != null ? teaches.displayName : "something new")}"
            : s.kind == SupplyKind.Tool
            ? $"{s.price} coins  ·  {(day.shop.Owns(id) ? "Owned" : stock)}"
            : $"{s.price} coins for {s.perPack} {s.unit}\n{stock}  ·  you have {day.shop.Stock(id)}";
    }

    /// A commission's request, and whether its pattern still needs designing.
    string CommissionText(Order order)
    {
        var c = day.CommissionOf(order);
        if (c == null) return "";
        var text = $"Commission: \"{c.request}\"\n";
        if (c.requiredSupply != null) text += $"Must be made with {c.requiredSupply.displayName}.\n";
        if (order.needsDesign) text += "First, design a brand-new pattern for it (it isn't judged).\n";
        return text + "\n";
    }

    /// The recipe as a checklist: steps done, the one being worked on, and the ones still to come.
    static string RecipeText(Order order)
    {
        var steps = order.Steps;
        if (steps.Length == 1) return "Finished piece: stitch each square.";
        if (order.finished) return $"All {steps.Length} steps done.";
        var text = $"Step {order.step + 1} of {steps.Length}";
        if (order.step > 0) text += $" (done: {string.Join(", ", System.Array.ConvertAll(steps[..order.step], s => s.label))})";
        text += $"\nNow: {steps[order.step].label}";
        if (order.step + 1 < steps.Length)
            text += $"\nThen: {string.Join(", ", System.Array.ConvertAll(steps[(order.step + 1)..], s => s.label))}";
        return text;
    }

    static string StepHint(StepKind kind) => kind switch
    {
        StepKind.Hem => "Drag along the dashed line to sew the hem all the way round. Lift your finger and carry on wherever you like.",
        StepKind.ChooseFabric => "Tap the fabric to make it from. Only fabrics you have enough of are shown.",
        StepKind.CutPieces => "Cut along the marked line. The fabric is folded, so front and back are cut together.",
        StepKind.Pin => "Tap each white mark around the edge to pin the two pieces together.",
        StepKind.SewSeam => "Hold anywhere to run the machine; slide up to go faster, down to slow. Ease off at corners or the stitches skid off the line. It stops at the gap left for stuffing.",
        StepKind.TurnRightSideOut => "Drag from inside the piece out through the gap at the bottom to turn it right side out.",
        StepKind.Stuff => "Tap all over the piece to push stuffing in. Fill it evenly; too much in one spot makes it lumpy.",
        StepKind.AttachEyes => "Tap where each eye goes. The faint marks show where the pattern puts them.",
        StepKind.CloseOpening => "Sew the gap shut by hand: drag along the dashed line.",
        StepKind.SewSides => "Hold anywhere to run the machine down one side, along the bottom and up the other; slide up to go faster. Slow down at the corners.",
        StepKind.HemTop => "Fold the top edge over and sew along the dashed line.",
        StepKind.AttachHandles => "Tap where each end of the handle goes, on the faint marks just below the top.",
        StepKind.Press => "Drag the iron all over the piece until the creases are gone.",
        StepKind.WeaveEnds => "Drag the loose tail along the dashed path to weave it into the back of the stitches.",
        StepKind.HoopFabric => "Press the outer ring down all the way round so the fabric is drum-tight.",
        StepKind.Backstitch => "Backstitch along the printed line. The closer you follow it, the neater it looks.",
        StepKind.SatinFill => "Drag back and forth across the shape to fill it with satin stitch. Try to stay inside the outline.",
        StepKind.FrenchKnots => "On each mark: put your finger down, twirl round twice to wrap the thread, then let go.",
        StepKind.ChooseYarn => "Tap the yarn to make it from. Only yarns you have enough of are shown.",
        StepKind.CastOn => "Tap once for each stitch to cast it onto the needle.",
        StepKind.KnitRows => "Swipe down for a knit stitch, up for a purl (the bar marks). Keep a steady rhythm for even stitches.",
        StepKind.CastOff => "Drag along the top edge to cast off, so it can't unravel.",
        StepKind.MagicRing => "Wrap the yarn round: drag along the dashed circle to make a magic ring.",
        StepKind.CrochetRounds => "Tap the bright spot to hook a loop, then flick anywhere to pull it through. Round and round!",
        _ => "",
    };

    /// The materials an order needs, and what's still missing before work can start.
    string MaterialsText(Order order) => ToolsText(order) + NeedsText(order);

    /// The tools an order needs, and which ones the player doesn't own yet.
    string ToolsText(Order order)
    {
        var tools = day.ToolsFor(order);
        if (tools.Count == 0) return "";
        var missing = day.shop.MissingTools(tools);   // same list, so Contains below matches
        var text = "\n\nTools:";
        foreach (var t in tools) text += $"\n{(missing.Contains(t) ? "Missing" : "Have")}: {t.label}";
        if (missing.Count > 0) text += "\nLook for missing tools at the Supply Market (its stock changes every 4 days).";
        return text;
    }

    string NeedsText(Order order)
    {
        if (order.patternOnly) return "";
        var needs = day.NeedsFor(order);
        bool choose = day.MaterialToChoose(order);
        if (needs.Count == 0 && !choose)
        {
            var fabric = day.content.Supply(order.fabricId);
            return $"\nMaterials: ready{(fabric != null ? $" (on {fabric.displayName})" : "")}.";
        }
        var text = "\n\nMaterials:";
        if (choose)
        {
            var options = ChooseMaterialStep.Options(order, day.content, day.shop);
            text += options.Count > 0
                ? $"\nHave: {ChooseMaterialStep.Wanted(order, day.content)} (you'll choose: {string.Join(", ", options.ConvertAll(s => s.displayName))})"
                : $"\nMissing: {ChooseMaterialStep.Wanted(order, day.content)}";
        }
        var missing = day.shop.Missing(needs);
        foreach (var n in needs) text += $"\n{(missing.Contains(n) ? "Missing" : "Have")}: {Materials.Describe(n, day.content, day.shop)}";
        if (missing.Count > 0)
        {
            text += "\nTo buy at the Supply Market:";
            foreach (var n in missing)
            {
                var s = day.content.Supply(n.supplyId);
                int packs = day.shop.PacksFor(n);
                text += $"\n  {packs} x {(s != null ? s.displayName : n.supplyId)} ({packs * (s != null ? s.price : 0)} coins)" +
                        (s != null && !day.shop.IsOnSale(s) ? ", not in stock right now" : "");
            }
        }
        return text;
    }

    static GameObject AddRow(Button template, string name, string label, UnityEngine.Events.UnityAction onClick)
    {
        var row = Instantiate(template, template.transform.parent);
        row.gameObject.SetActive(true);
        row.name = name;
        row.GetComponentInChildren<Text>().text = label;
        row.onClick.AddListener(onClick);
        return row.gameObject;
    }

    /// Draws a pattern as a small pixel image: one pixel per square, on cream "fabric".
    static void SetPreview(RawImage image, Pattern pattern)
    {
        if (image.texture != null) Destroy(image.texture);
        image.texture = PatternPreview.Make(pattern);
    }

    static void SetPortrait(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    static string CraftName(CraftType craft) => craft == CraftType.CrossStitch ? "cross stitch" : craft.ToString().ToLower();
}
