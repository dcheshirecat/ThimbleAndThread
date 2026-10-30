using System.Collections.Generic;
using UnityEngine;

/// Runs the shop's day and night.
/// Day: customers visit and browse the pattern shelf. Each one buys a pattern or orders an item
/// made from one (it goes in the order book); finished orders are handed over.
/// Night: the player works on any order, can stop at any point and pick it up later (the work is saved),
/// and sets finished pieces aside for the customer to collect the next day. Nothing has a deadline.
/// Every change autosaves through GameManager.Persist.
public class ShopDay : MonoBehaviour
{
    public ContentLibrary content;
    public ShopManager shop;
    public MinigameHost minigame;

    // Not saved: what is on screen right now.
    public CustomerDef Visitor { get; private set; }            // customer in the Dialogue screen
    public Order Offer { get; private set; }                    // their pattern order, not yet accepted
    public OwnedPattern SaleOffer { get; private set; }         // or: the shelf pattern they want to buy
    public Order Selected { get; private set; }                 // order open on the Workbench
    public OwnedPattern SelectedPattern { get; private set; }   // pattern open in the Pattern Studio
    public float LastAccuracy { get; private set; }
    public int LastStars { get; private set; }
    public int LastPrice { get; private set; }    // what the customer will pay at hand-over
    public int LastPoints { get; private set; }
    public string LastMessage { get; private set; } = "";

    GameManager GM => GameManager.Instance;
    SaveData Save => GM.Save;

    public List<Order> Orders => Save.orders;
    public List<OwnedPattern> Patterns => Save.patterns;
    public List<OwnedPattern> Shelf => Save.patterns.FindAll(p => p.onShelf && ItemFor(p.itemId) != null);
    public int CustomersWaiting => Save.visitorsToday.Count;

    void Start() => GM.OnGameLoaded += OnGameLoaded;

    void OnDestroy()
    {
        if (GM != null) GM.OnGameLoaded -= OnGameLoaded;
    }

    void OnGameLoaded(bool isNewGame)
    {
        minigame.Close();
        Visitor = null; Offer = null; SaleOffer = null; Selected = null; SelectedPattern = null;

        // The great-aunt's book: starter patterns for new games, and any starter pattern added in an
        // update for older saves (book patterns can't be deleted, so a missing one was never there).
        foreach (var def in content.patterns)
            if (def != null && def.startsOnShelf && def.item != null && !Save.patterns.Exists(p => p.id == def.patternId))
                Save.patterns.Add(new OwnedPattern { id = def.patternId, name = def.displayName, itemId = def.item.itemId, price = def.price, onShelf = true });

        // Saves store item ids; look the definitions back up. Drop orders for items that no longer exist.
        Save.orders.RemoveAll(o =>
        {
            o.item = ItemFor(o.itemId);
            if (o.item == null) Debug.LogWarning($"Dropping saved order for unknown item '{o.itemId}'.");
            return o.item == null;
        });

        // Supplies: starter kit for new games; older saves get counted stock and threads on their patterns.
        shop.SetUpStock(isNewGame);
        foreach (var o in Save.orders) Materials.EnsureThreads(o.target, content);
        foreach (var p in Save.patterns) if (p.designed) Materials.EnsureThreads(p.design, content);

        if (isNewGame)
        {
            InviteVisitors();
            LastMessage = "Welcome to Thimble & Thread!";
        }
        else LastMessage = Save.night ? $"Welcome back! Night of day {Save.day}." : $"Welcome back! Day {Save.day}.";
    }

    /// Everyone without an open order drops by today, in a random order.
    void InviteVisitors()
    {
        var list = Save.visitorsToday;
        list.Clear();
        foreach (var c in content.customers)
            if (c != null && !Save.orders.Exists(o => o.customerId == c.customerId)) list.Add(c.customerId);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public CustomerDef CustomerFor(string id) => content.Customer(id);
    public CraftItemDef ItemFor(string id) => content.Item(id);

    /// The stitch grid of an owned pattern (a fresh copy, safe to hand to an order).
    public Pattern DesignFor(OwnedPattern owned)
    {
        if (owned.designed) return JsonUtility.FromJson<Pattern>(JsonUtility.ToJson(owned.design));
        var def = content.Pattern(owned.id);
        return def != null ? def.ToPattern() : PlaceholderPatterns.For(ItemFor(owned.itemId));
    }

    // ---- Shop ----

    public void GreetCustomer()
    {
        if (Save.night) return;   // the shop is closed at night
        string browsed = null;
        while (Save.visitorsToday.Count > 0)
        {
            var customer = CustomerFor(Save.visitorsToday[0]);
            Save.visitorsToday.RemoveAt(0);
            if (customer == null) continue;

            // They browse the shelf for patterns of things they'd like.
            var wants = OrderGenerator.Wants(customer, GM.GetRelationship(customer.customerId).level);
            var matches = Shelf.FindAll(p => wants.Exists(i => i.itemId == p.itemId));
            if (matches.Count == 0)
            {
                browsed = $"{customer.displayName} browsed the shelf but didn't find anything today.";
                continue;
            }

            var pick = matches[Random.Range(0, matches.Count)];
            Visitor = customer;
            if (Random.value < 0.35f) SaleOffer = pick;   // buys the pattern to make it themselves
            else Offer = OrderGenerator.FromPattern(customer, ItemFor(pick.itemId), DesignFor(pick), Save.day);
            GM.Persist();
            GM.ChangeScreen(ScreenId.Dialogue);
            return;
        }
        GM.Persist();
        LastMessage = browsed ?? "No one else is waiting today. Close the shop whenever you're ready.";
        GM.ChangeScreen(ScreenId.Shop);
    }

    /// Sell the pattern, or take the order into the order book.
    public void AcceptOffer()
    {
        if (SaleOffer != null)
        {
            GM.AddCoins(SaleOffer.price);
            GM.AddRelationshipPoints(Visitor.customerId, 5);
            Save.coinsToday += SaleOffer.price;
            Save.patternsSoldToday++;
            LastMessage = $"Sold the {SaleOffer.name} pattern to {Visitor.displayName}: +{SaleOffer.price} coins.";
        }
        else
        {
            Save.orders.Add(Offer);
            LastMessage = $"{Visitor.displayName}'s {Offer.item.itemName.ToLower()} is in your order book.";
        }
        Visitor = null; Offer = null; SaleOffer = null;
        GM.Persist();
        GM.ChangeScreen(ScreenId.Shop);
    }

    public void DeclineOffer()
    {
        LastMessage = $"{Visitor.displayName} will come back another day.";
        Visitor = null; Offer = null; SaleOffer = null;
        GM.ChangeScreen(ScreenId.Shop);
    }

    /// Day to night: ends the day's trading and shows the summary.
    public void CloseShop()
    {
        Save.night = true;
        Save.visitorsToday.Clear();   // anyone still waiting comes back another day
        GM.Persist();
        GM.ChangeScreen(ScreenId.DaySummary);
    }

    public void GoToWorkroom()
    {
        int toDo = Save.orders.FindAll(o => !o.finished).Count;
        LastMessage = toDo > 0 ? $"The shop is quiet. {toDo} order(s) to work on." : "The shop is quiet. Time to design some patterns?";
        GM.ChangeScreen(ScreenId.Workroom);
    }

    /// Night to the next morning.
    public void GoToBed()
    {
        Save.night = false;
        Save.deliveredToday = 0;
        Save.coinsToday = 0;
        Save.patternsSoldToday = 0;
        GM.NextDay();
        InviteVisitors();
        GM.Persist();
        int ready = Save.orders.FindAll(o => o.finished).Count;
        LastMessage = $"Good morning! Day {Save.day}." + (ready > 0 ? $" {ready} order(s) ready to hand over." : "");
        GM.ChangeScreen(ScreenId.Shop);
    }

    // ---- Pattern Studio (night): price and shelf ----

    public void SelectPattern(string patternId)
    {
        SelectedPattern = Save.patterns.Find(p => p.id == patternId);
        ConfirmingDelete = false;
        LastMessage = "";
        if (SelectedPattern != null) GM.ChangeScreen(ScreenId.PatternEdit);
    }

    public void PriceUp() => SetPrice(SelectedPattern.price + 1);
    public void PriceDown() => SetPrice(SelectedPattern.price - 1);

    void SetPrice(int price)
    {
        SelectedPattern.price = Mathf.Clamp(price, 1, 999);
        ConfirmingDelete = false;
        GM.Persist();
        GM.ChangeScreen(ScreenId.PatternEdit);   // refresh the screen
    }

    public void ToggleShelf()
    {
        if (!SelectedPattern.onShelf && SelectedPattern.designed && SelectedPattern.design.cells.Count == 0)
        {
            LastMessage = "Draw something first: an empty pattern can't go on the shelf.";
            GM.ChangeScreen(ScreenId.PatternEdit);
            return;
        }
        SelectedPattern.onShelf = !SelectedPattern.onShelf;
        LastMessage = "";
        GM.Persist();
        GM.ChangeScreen(ScreenId.PatternEdit);
    }

    // ---- Pattern Studio (night): designing ----

    public bool ConfirmingDelete { get; private set; }

    public void NewPattern()
    {
        var item = System.Array.Find(content.items, i => i != null && i.craft == CraftType.CrossStitch) ?? content.items[0];
        int n = Save.patterns.FindAll(p => p.designed).Count + 1;
        var design = new Pattern { displayName = $"My Pattern {n}", craft = item.craft, width = 8, height = 8 };
        SelectedPattern = new OwnedPattern
        {
            id = System.Guid.NewGuid().ToString("N"), name = design.displayName, itemId = item.itemId,
            price = Mathf.Max(1, item.patternPrice), designed = true, design = design,
        };
        // Not added to the book until the player saves it.
        GM.ChangeScreen(ScreenId.PatternDesigner);
    }

    public void EditDesign()
    {
        if (SelectedPattern != null && SelectedPattern.designed) GM.ChangeScreen(ScreenId.PatternDesigner);
    }

    /// Called by the designer's Save button with what's on the board.
    public void SaveDesign(Pattern design, string name, string itemId, int price)
    {
        var p = SelectedPattern;
        p.name = string.IsNullOrWhiteSpace(name) ? p.name : name.Trim();
        p.itemId = itemId;
        p.price = Mathf.Clamp(price, 1, 999);
        design.displayName = p.name;
        design.craft = ItemFor(itemId).craft;
        p.design = Compact(design);
        if (design.cells.Count == 0) p.onShelf = false;
        if (!Save.patterns.Contains(p)) Save.patterns.Add(p);
        GM.Persist();
        LastMessage = $"Saved \"{p.name}\" to your pattern book.";
        GM.ChangeScreen(ScreenId.PatternEdit);
    }

    /// Designer's Cancel: back without saving (a brand-new pattern is simply dropped).
    public void CancelDesign()
    {
        bool isNew = !Save.patterns.Contains(SelectedPattern);
        if (isNew) SelectedPattern = null;
        GM.ChangeScreen(isNew ? ScreenId.PatternStudio : ScreenId.PatternEdit);
    }

    /// Your own designs only. First tap asks, second tap deletes.
    public void DeletePattern()
    {
        if (SelectedPattern == null || !SelectedPattern.designed) return;
        if (!ConfirmingDelete)
        {
            ConfirmingDelete = true;
            GM.ChangeScreen(ScreenId.PatternEdit);
            return;
        }
        ConfirmingDelete = false;
        Save.patterns.Remove(SelectedPattern);
        LastMessage = $"Deleted \"{SelectedPattern.name}\".";
        SelectedPattern = null;
        GM.Persist();
        GM.ChangeScreen(ScreenId.PatternStudio);
    }

    /// Keeps only the threads a design actually uses, so its swatches and shopping list stay short.
    static Pattern Compact(Pattern d)
    {
        var used = new List<int>();
        foreach (var c in d.cells) if (!used.Contains(c.colorIndex)) used.Add(c.colorIndex);
        used.Sort();
        var p = new Pattern { id = d.id, displayName = d.displayName, craft = d.craft, width = d.width, height = d.height };
        foreach (var i in used)
        {
            p.palette.Add(d.palette[i]);
            p.threads.Add(i < d.threads.Count ? d.threads[i] : "");
        }
        foreach (var c in d.cells) p.cells.Add(new StitchCell { x = c.x, y = c.y, colorIndex = used.IndexOf(c.colorIndex) });
        return p;
    }

    // ---- Supply Market (day) ----

    public void BuyPack(string supplyId)
    {
        var s = content.Supply(supplyId);
        if (s == null || Save.night || !shop.IsOnSale(s)) return;
        LastMessage = !shop.Buy(s) ? $"Not enough coins for {s.displayName} ({s.price} coins)."
            : s.kind == SupplyKind.Tool ? $"Bought {s.displayName}. Tools are yours to keep."
            : $"Bought {s.displayName}: +{s.perPack} {s.unit} (you have {shop.Stock(supplyId)}).";
        GM.ChangeScreen(ScreenId.Market);   // refresh the list
    }

    // ---- Order book and workbench ----

    public void SelectOrder(string orderId)
    {
        Selected = Save.orders.Find(o => o.id == orderId);
        LastMessage = "";
        if (Selected != null) GM.ChangeScreen(ScreenId.Workbench);
    }

    /// Supplies one recipe step uses up, taken from the cupboard when the step is started (nothing once
    /// taken, and nothing for designing). Cross stitch: its fabric and floss. Other steps: their own list,
    /// plus their share of the fabric or yarn chosen in the Choose step.
    public List<MaterialNeed> NeedsForStep(Order o, int index)
    {
        var needs = new List<MaterialNeed>();
        if (o.patternOnly || StepMaterialsUsed(o, index)) return needs;
        var step = o.Steps[index];
        if (step.kind == StepKind.CrossStitch) return Materials.For(o.target, Materials.FabricFor(o.target, content, shop), content);
        if (step.materials != null)
            foreach (var m in step.materials) needs.Add(new MaterialNeed { supplyId = m.supplyId, amount = m.amount });
        if (step.fabricPieces > 0 && !string.IsNullOrEmpty(o.fabricId))
            needs.Add(new MaterialNeed { supplyId = o.fabricId, amount = step.ChosenAmount(o.target) });
        return needs;
    }

    bool StepMaterialsUsed(Order o, int index) =>
        o.Steps[index].kind == StepKind.CrossStitch ? o.materialsUsed : o.StepState(index).materialsUsed;

    /// Everything the steps still to do will use, added up per supply.
    public List<MaterialNeed> NeedsFor(Order o)
    {
        var all = new List<MaterialNeed>();
        for (int i = Mathf.Max(0, o.step); i < o.Steps.Length; i++)
            foreach (var n in NeedsForStep(o, i))
            {
                var same = all.Find(x => x.supplyId == n.supplyId);
                if (same != null) same.amount += n.amount;
                else all.Add(n);
            }
        return all;
    }

    public List<MaterialNeed> MissingFor(Order o) => shop.Missing(NeedsFor(o));

    /// True while the order's fabric or yarn is still to be chosen in a later (or the current) step.
    public bool MaterialToChoose(Order o)
    {
        int choose = System.Array.FindIndex(o.Steps, s => s.kind == StepKind.ChooseFabric || s.kind == StepKind.ChooseYarn);
        return choose >= 0 && choose >= o.step && !o.patternOnly;
    }

    /// Tools one recipe step needs: a cross stitch step needs a hoop that fits and a tapestry needle,
    /// plus whatever the step itself lists. Designing (pattern-only orders) needs none.
    public List<ToolNeed> ToolsForStep(Order o, int index)
    {
        var tools = new List<ToolNeed>();
        if (o.patternOnly) return tools;
        var step = o.Steps[Mathf.Clamp(index, 0, o.Steps.Length - 1)];
        if (step.kind == StepKind.CrossStitch) tools.AddRange(Materials.ToolsFor(o.target));
        if (step.tools != null)
            foreach (var t in step.tools) tools.Add(t.matchYarn ? ForYarn(o, t) : t);
        return tools;
    }

    /// Needles or a hook to match the chosen yarn's weight (any size until a yarn is chosen).
    ToolNeed ForYarn(Order o, ToolNeed t)
    {
        var yarn = content.Supply(o.fabricId);
        if (yarn == null || yarn.kind != SupplyKind.Yarn) return new ToolNeed { toolType = t.toolType, label = $"{t.label} to match your yarn" };
        return new ToolNeed { toolType = t.toolType, minSize = yarn.size, exactSize = true, label = $"{t.label} for {Materials.WeightName(yarn.size)} yarn" };
    }

    /// Tools for the steps still to do, each type once (the largest size needed).
    public List<ToolNeed> ToolsFor(Order o)
    {
        var tools = new List<ToolNeed>();
        for (int i = Mathf.Max(0, o.step); i < o.Steps.Length; i++)
            foreach (var t in ToolsForStep(o, i))
            {
                int same = tools.FindIndex(x => x.toolType == t.toolType);
                if (same < 0) tools.Add(t);
                else if (t.minSize > tools[same].minSize) tools[same] = t;
            }
        return tools;
    }

    public List<ToolNeed> MissingToolsFor(Order o) => shop.MissingTools(ToolsFor(o));

    /// Night only: crafting happens in the workroom, one recipe step at a time. Each step needs its
    /// own tools (kept once owned) and uses its own materials, taken from the cupboard the first time
    /// the step is started (e.g. the Aida is cut, the stuffing bag opened), so they must all be there.
    public void StartCrafting()
    {
        if (!Save.night || Selected == null) return;
        int index = Mathf.Clamp(Selected.step, 0, Selected.Steps.Length - 1);
        var kind = Selected.Steps[index].kind;
        string blocked = null;
        if (shop.MissingTools(ToolsForStep(Selected, index)).Count > 0)
            blocked = "You're missing a tool for this step. Look for it at the Supply Market.";
        else if ((kind == StepKind.ChooseFabric || kind == StepKind.ChooseYarn) && ChooseMaterialStep.Options(Selected, content, shop).Count == 0)
            blocked = $"You need {ChooseMaterialStep.Wanted(Selected)}. Look for some at the Supply Market.";
        else if (shop.Missing(NeedsForStep(Selected, index)).Count > 0)
            blocked = "You don't have all the materials for this step yet. Buy them at the Supply Market tomorrow.";
        if (blocked != null)
        {
            LastMessage = blocked;
            GM.ChangeScreen(ScreenId.Workbench);
            return;
        }
        if (!StepMaterialsUsed(Selected, index))
        {
            var needs = NeedsForStep(Selected, index);
            if (kind == StepKind.CrossStitch) Selected.fabricId = Materials.FabricFor(Selected.target, content, shop)?.supplyId;
            shop.Use(needs);
            if (kind == StepKind.CrossStitch) Selected.materialsUsed = true;
            else Selected.StepState(index).materialsUsed = true;
            GM.Persist();
        }
        LastMessage = "";
        Selected.finished = false;   // reopening a set-aside piece makes it a work in progress again
        var fabric = content.Supply(Selected.fabricId);
        minigame.Open(Selected, fabric != null ? fabric.color : new Color(0.95f, 0.95f, 0.90f), content, shop);
        GM.ChangeScreen(ScreenId.Minigame);
    }

    /// Material tier for pricing: the fabric the piece was stitched on.
    int TierOf(Order o)
    {
        var fabric = content.Supply(o.fabricId);
        return fabric != null ? fabric.tier : 1;
    }

    /// Stop for now: the work is kept in the order and saved.
    public void DoneForNow()
    {
        minigame.SaveWork();
        minigame.Close();
        GM.Persist();
        LastMessage = $"Saved your work on the {Selected.item.itemName.ToLower()}.";
        GM.ChangeScreen(ScreenId.OrderBook);
    }

    /// Finishes the step being worked on. If more steps follow, it's back to the Workbench with the
    /// next one ready (the player can stop between steps). After the last step, shows how the piece
    /// turned out and what the customer will pay: the quality is the average over all steps.
    public void FinishCrafting()
    {
        var blocker = minigame.Step?.FinishBlocker;
        if (blocker != null)
        {
            LastMessage = blocker;
            GM.ChangeScreen(ScreenId.Minigame);   // shows the message with the step's hint
            return;
        }
        var state = Selected.StepState(Selected.step);
        state.score = minigame.SaveWork();
        state.done = true;
        state.progress = 1f;
        minigame.Close();
        if (!Selected.IsLastStep)
        {
            string done = Selected.CurrentStep.label;
            Selected.step++;
            GM.Persist();
            LastMessage = $"{done}: done! Next: {Selected.CurrentStep.label}.";
            GM.ChangeScreen(ScreenId.Workbench);
            return;
        }
        LastAccuracy = Selected.RecipeScore();
        GM.Persist();

        int level = GM.GetRelationship(Selected.customerId).level;
        LastStars = PricingCalculator.Stars(LastAccuracy);
        LastPrice = PricingCalculator.Compute(Selected.item, LastAccuracy, TierOf(Selected), level, Selected.patternOnly);
        LastPoints = PricingCalculator.RelationshipReward(LastAccuracy);
        GM.ChangeScreen(ScreenId.Result);
    }

    public void KeepWorking() => StartCrafting();

    /// Night: the piece is done and waits for the customer to collect it tomorrow.
    public void SetAside()
    {
        Selected.finished = true;
        Selected.accuracy = LastAccuracy;
        GM.Persist();
        var customer = CustomerFor(Selected.customerId);
        LastMessage = $"The {Selected.item.itemName.ToLower()} is set aside for {(customer != null ? customer.displayName : "its owner")} to collect tomorrow.";
        Selected = null;
        GM.ChangeScreen(ScreenId.OrderBook);
    }

    /// Day: give a finished order to its customer for coins and friendship.
    public void HandOver()
    {
        if (Selected == null || !Selected.finished || Save.night) return;
        var customer = CustomerFor(Selected.customerId);
        string name = customer != null ? customer.displayName : Selected.customerId;
        int level = GM.GetRelationship(Selected.customerId).level;
        int price = PricingCalculator.Compute(Selected.item, Selected.accuracy, TierOf(Selected), level, Selected.patternOnly);
        int points = PricingCalculator.RelationshipReward(Selected.accuracy);

        GM.AddCoins(price);
        GM.AddRelationshipPoints(Selected.customerId, points);
        Save.orders.Remove(Selected);
        Save.deliveredToday++;
        Save.coinsToday += price;
        GM.Persist();

        int after = GM.GetRelationship(Selected.customerId).level;
        LastMessage = after > level
            ? $"{name} loves it and is now a closer friend! (level {after})"
            : $"Handed over to {name}: +{price} coins, +{points} friendship.";
        Selected = null;
        GM.ChangeScreen(ScreenId.Shop);
    }
}
