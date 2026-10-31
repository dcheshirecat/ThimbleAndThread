using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Runs the shop's day and night.
/// Day: customers visit and browse the pattern shelf. Each one buys a pattern or orders an item
/// made from one (it goes in the order book); finished orders are handed over.
/// Night: the player works on any order, can stop at any point and pick it up later (the work is saved),
/// and sets finished pieces aside until the customer comes back for them (on a day the player isn't told).
/// Nothing has a deadline.
/// Every change autosaves through GameManager.Persist.
public class ShopDay : MonoBehaviour
{
    public ContentLibrary content;
    public ShopManager shop;
    public MinigameHost minigame;
    public DialogueUI dialogue;

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

    Progression progress;
    /// Skills, craft levels, the story chapter and shop upgrades.
    public Progression Progress => progress ??= new Progression(content, shop);

    public List<Order> Orders => Save.orders;
    public List<OwnedPattern> Patterns => Save.patterns;
    public List<OwnedPattern> Shelf => Save.patterns.FindAll(p => p.onShelf && ItemFor(p.itemId) != null);
    public int CustomersWaiting => Save.visitorsToday.Count;

    void Start()
    {
        GM.OnGameLoaded += OnGameLoaded;
        GM.OnScreenChanged += OnScreenChanged;
        GM.OnRelationshipLevelUp += FriendshipLevelUp;
    }

    void FriendshipLevelUp(string customerId, int level)
    {
        var who = CustomerFor(customerId);
        if (who != null) Stamps.Earn(Save, $"friend:{customerId}:{level}", who.displayName, $"Friendship level {level}", StampTheme.Friendship);
        if (who != null && level >= CommissionOfferLevel && Progress.Knows(PatternDesignSkill) && !who.shopOwner)
            Stamps.Earn(Save, "ability:offer_commission", "Commissions", $"You can offer to make something special for close friends like {who.displayName}. They'll decide.", StampTheme.Skill);
    }

    // ---- The doorbell: rings on any screen, with a buzz, until someone opens the door ----

    // Visitors come now and then, not one after another (owner, 2026-10-31: time to do other things between).
    const float BellRepeat = 45f;                          // rings again while no one answers
    const float FirstVisitorMin = 8f, FirstVisitorMax = 15f;    // the day's first visitor
    const float NextVisitorMin = 30f, NextVisitorMax = 75f;     // seconds (on any screen but a conversation) until the next
    bool firstVisitor = true;
    float bellTimer = -1f;
    public bool BellRinging { get; private set; }
    public event System.Action OnBell;

    float questTimer;

    void Update()
    {
        if (Save != null && Save.player.created && (questTimer -= Time.unscaledDeltaTime) <= 0f)
        {
            questTimer = 1f;
            Progress.CheckQuests();
            CheckShops();
        }
        if (Save == null || Save.night || CustomersWaiting == 0 || GM.Current == ScreenId.Dialogue || GM.Current == ScreenId.Title
            || GM.Current == ScreenId.SaveSelect || GM.Current == ScreenId.Customize || GM.Current == ScreenId.Tutorial || !Save.player.created)
        {
            if (Save == null || Save.night || CustomersWaiting == 0) { BellRinging = false; bellTimer = -1f; }
            return;
        }
        if (bellTimer < 0)   // someone's on their way...
        {
            bellTimer = BellRinging ? BellRepeat : firstVisitor ? Random.Range(FirstVisitorMin, FirstVisitorMax) : Random.Range(NextVisitorMin, NextVisitorMax);
            firstVisitor = false;
        }
        bellTimer -= Time.unscaledDeltaTime;
        if (bellTimer <= 0) { bellTimer = -1f; RingBell(); }
    }

    void RingBell()
    {
        BellRinging = true;
        AudioManager.Play(Sfx.Bell);
        Haptics.Buzz();
        if (GM.Current != ScreenId.Shop) Stamps.Notice("Ding-dong! Someone's at the shop door.");
        OnBell?.Invoke();
    }

    /// The door's been opened: the bell stops until the next visitor arrives.
    public void AnswerDoor() { BellRinging = false; bellTimer = -1f; }

    void OnDestroy()
    {
        if (GM == null) return;
        GM.OnGameLoaded -= OnGameLoaded;
        GM.OnScreenChanged -= OnScreenChanged;
        GM.OnRelationshipLevelUp -= FriendshipLevelUp;
    }

    void OnGameLoaded(bool isNewGame)
    {
        minigame.Close();
        Visitor = null; Offer = null; SaleOffer = null; Selected = null; SelectedPattern = null; DesigningFor = null;
        playing = false;
        if (isNewGame) Save.pendingScenes.Add("chapter_1");   // the story's opening, once the shop is on screen

        // The great-aunt's book: starter patterns for new games, and any starter pattern added in an
        // update for older saves (book patterns can't be deleted, so a missing one was never there).
        // They go on display while there's room; the rest wait in the book.
        foreach (var def in content.patterns)
            if (def != null && def.startsOnShelf && def.item != null && !Save.patterns.Exists(p => p.id == def.patternId))
                Save.patterns.Add(new OwnedPattern { id = def.patternId, name = def.displayName, itemId = def.item.itemId, price = def.price,
                                                     onShelf = false });

        // Saves store item ids; look the definitions back up. Drop orders for items that no longer exist.
        Save.orders.RemoveAll(o =>
        {
            o.item = ItemFor(o.itemId);
            if (o.item == null) Debug.LogWarning($"Dropping saved order for unknown item '{o.itemId}'.");
            return o.item == null;
        });
        foreach (var o in Save.orders) o.KeepUpWithRecipe();

        // Supplies: starter kit for new games; older saves get counted stock and threads on their patterns.
        shop.SetUpStock(isNewGame);

        // What the player has learned: everything for saves from before skills, else whatever their
        // chapter, levels, books and friends have taught them (chapter 1 opens cross stitch).
        if (Save.learnEverything) Progress.LearnEverything();
        Progress.CatchUpStamps();
        Progress.CheckUnlocks();

        // A new game opens with patterns for things the player can already make on display first.
        if (isNewGame)
        {
            foreach (var p in Save.patterns) p.onShelf = false;
            var book = new List<OwnedPattern>(Save.patterns);
            book.Sort((a, b) => Progress.CanMake(ItemFor(b.itemId)).CompareTo(Progress.CanMake(ItemFor(a.itemId))));
            foreach (var p in book) if (Shelf.Count < Save.displaySpots && Progress.CanMake(ItemFor(p.itemId))) p.onShelf = true;
        }
        // Patterns for things the player can't make yet don't stay on display (they can't be sold).
        foreach (var p in Save.patterns) if (p.onShelf && !Progress.CanMake(ItemFor(p.itemId))) p.onShelf = false;
        foreach (var o in Save.orders) Materials.EnsureThreads(o.target, content);
        foreach (var p in Save.patterns) if (p.designed) Materials.EnsureThreads(p.design, content);

        if (isNewGame)
        {
            InviteVisitors();
            LastMessage = "Welcome to Thimble & Thread!";
        }
        else LastMessage = Save.night ? $"Welcome back! Night of day {Save.day}." : $"Welcome back! Day {Save.day}.";
    }

    /// Who drops by today, in a random order: everyone without an open order, and anyone whose finished
    /// order they've decided to collect today (the player only finds out when the bell rings).
    void InviteVisitors()
    {
        firstVisitor = true;
        bellTimer = -1f;
        var list = Save.visitorsToday;
        list.Clear();
        foreach (var c in content.customers)
            if (c != null && !c.shopOwner && c.firstChapter <= Save.chapter && (!Save.orders.Exists(o => o.customerId == c.customerId) || PickupFor(c) != null))
                list.Add(c.customerId);
        // A sign out front: a few of them drop by a second time.
        int extra = Mathf.Min(Progress.UpgradeAmount(UpgradeEffect.ExtraVisitor), list.Count);
        for (int i = 0; i < extra; i++) list.Add(list[i]);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public CustomerDef CustomerFor(string id) => content.Customer(id);

    // ---- Special touches: extras the player can add as a surprise (lace, buttons, ribbon...) ----

    /// Owned extras that suit this order's item, for the Result screen.
    public List<SupplyDef> ExtrasFor(Order o)
    {
        var list = new List<SupplyDef>();
        if (o == null || o.patternOnly || o.item == null) return list;
        foreach (var s in content.supplies)
            if (s != null && s.decorates != null && System.Array.IndexOf(s.decorates, o.item) >= 0
                && (shop.Stock(s.supplyId) >= s.decorateAmount || o.extras.Contains(s.supplyId)))
                list.Add(s);
        return list;
    }

    /// Adds or removes a special touch (used from the cupboard when the piece is set aside).
    public void ToggleExtra(string supplyId)
    {
        if (Selected == null) return;
        if (!Selected.extras.Remove(supplyId)) Selected.extras.Add(supplyId);
        GM.Persist();
        GM.ChangeScreen(ScreenId.Result);   // refresh
    }

    /// A finished order this customer has come back for today, or null.
    public Order PickupFor(CustomerDef c) =>
        c == null ? null : Save.orders.Find(o => o.customerId == c.customerId && o.finished && o.pickupDay > 0 && o.pickupDay <= Save.day);

    /// When a customer will come back for a finished piece: one to three days later, never said.
    static int PickupDelay() => Random.Range(1, 4);
    public CraftItemDef ItemFor(string id) => content.Item(id);

    /// The stitch grid of an owned pattern (a fresh copy, safe to hand to an order).
    public Pattern DesignFor(OwnedPattern owned)
    {
        if (owned.designed) return JsonUtility.FromJson<Pattern>(JsonUtility.ToJson(owned.design));
        var def = content.Pattern(owned.id);
        return def != null ? def.ToPattern() : PlaceholderPatterns.For(ItemFor(owned.itemId));
    }

    // ---- Shop: visitors ----

    /// How often a visitor tells you about something they need (an Ink request) rather than browsing.
    public const float RequestChance = 0.5f;

    public void GreetCustomer()
    {
        if (Save.night) return;   // the shop is closed at night
        while (Save.visitorsToday.Count > 0)
        {
            var customer = CustomerFor(Save.visitorsToday[0]);
            Save.visitorsToday.RemoveAt(0);
            if (customer == null) continue;
            Visitor = customer;
            GM.Persist();
            // A first visit: they introduce themselves (owner, 2026-10-31), then say what they're after.
            if (!Save.met.Contains(customer.customerId))
            {
                Introduce(customer, () => WhatTheyWant(customer));
                return;
            }
            // Back to collect a finished order: hand it over (their reaction plays, then any scenes).
            var pickup = PickupFor(customer);
            if (pickup != null)
            {
                // They ask for it; the player decides (owner, 2026-10-31: not automatic).
                var order = pickup;
                Say(customer, Pick($"I've come for my {order.item.itemName.ToLower()}! Is it ready?", $"Hello again! I'm here for the {order.item.itemName.ToLower()}."),
                    ($"Hand over the {order.item.itemName.ToLower()}", () => { Selected = order; HandOver(); }),
                    ("Not quite yet", () =>
                    {
                        order.pickupDay = Save.day + 1;   // they'll try again another day
                        GM.Persist();
                        Say(customer, Pick("No rush at all. I'll pop back another day.", "Oh, of course. Another day, then!"), ("Continue", () => GM.ChangeScreen(ScreenId.Shop)));
                    }));
                return;
            }
            // A friendship scene first, if one is waiting, then their hello, then what they're after.
            var knots = ScenesDue(customer);
            var hello = VisitKnot(customer);
            if (hello != null) knots.Add(hello);
            Play(customer, knots, () => WhatTheyWant(customer));
            return;
        }
        Visitor = null;
        GM.Persist();
        LastMessage = "";   // no hint that the day's visitors are done (owner, 2026-10-31): the player decides when to close
        GM.ChangeScreen(ScreenId.Shop);
    }

    /// After their hello, the player decides (owner, 2026-10-31): see what they're after, suggest any pattern
    /// on display, or (with good friends) offer to make them something special.
    void WhatTheyWant(CustomerDef c)
    {
        var options = new List<(string, System.Action)> { ("See what they're after", () => TheirIdea(c)) };
        if (Shelf.Count > 0) options.Add(("Suggest a pattern on display", () => SuggestPattern(c)));
        if (CanOfferCommission(c)) options.Add(("Offer to make something special", () => OfferCommission(c)));
        if (options.Count == 1) { TheirIdea(c); return; }
        Narrate(c, $"{c.displayName} looks around the shop.", options.ToArray());
    }

    /// From this friendship level, the player may offer a commission; the customer still decides.
    public const int CommissionOfferLevel = 2;

    bool CanOfferCommission(CustomerDef c) =>
        Progress.Knows(PatternDesignSkill) && GM.GetRelationship(c.customerId).level >= CommissionOfferLevel
        && !Save.orders.Exists(o => o.customerId == c.customerId && !string.IsNullOrEmpty(o.commissionId));

    /// They might say yes: more likely the closer you are. A story commission of theirs comes first; otherwise
    /// they pick something they'd like, and the player designs it from scratch.
    void OfferCommission(CustomerDef c)
    {
        int level = GM.GetRelationship(c.customerId).level;
        System.Action notNow = () => Say(c, Pick("That's so kind. I can't think of anything I need just now, though.",
                                                 "Ooh. Let me think about it. Not today, but ask me again sometime?",
                                                 "You'd do that? I'm flattered. Maybe another time."),
                                         ("Continue", () => Leave($"{c.displayName} wasn't after a commission today.")));
        if (Random.value > CommissionAcceptChance(level)) { notNow(); return; }
        var story = CommissionFor(c);
        if (story != null)
        {
            Offer = NewCommission(story);
            var needs = story.requiredSupply != null ? $" It must be made with {story.requiredSupply.displayName}." : "";
            Say(c, $"Actually... yes. {story.request}\n\n(A commission: you'll design a brand-new {story.item.itemName.ToLower()} pattern for it.{needs})",
                ("Take the Commission", AcceptOffer), ("Not Today", DeclineOffer));
            return;
        }
        var wants = OrderGenerator.Wants(c, level).FindAll(i => Progress.CanMake(i));
        if (wants.Count == 0) { notNow(); return; }
        var item = wants[Random.Range(0, wants.Count)];
        Offer = new Order
        {
            id = System.Guid.NewGuid().ToString("N"), customerId = c.customerId, itemId = item.itemId, item = item, dayPlaced = Save.day,
            commissionId = OwnCommissionPrefix + System.Guid.NewGuid().ToString("N"), needsDesign = true, fit = 2,
            target = new Pattern { displayName = $"{c.displayName}'s {item.itemName}", craft = item.craft, width = 8, height = 8 },
        };
        Say(c, Pick($"Something just for me? Then... a {item.itemName.ToLower()}. One nobody else has.",
                    $"Oh, I'd love that. A {item.itemName.ToLower()}, maybe? Surprise me with the design."),
            ("Take the Commission", AcceptOffer), ("Not Today", DeclineOffer));
    }

    /// How likely a friend is to say yes to a commission: 30% at level 2, 10% more each level, at most 75%
    /// (owner, 2026-10-31: never automatic, but likelier with closer friends).
    public static float CommissionAcceptChance(int level) => Mathf.Clamp(0.3f + 0.1f * (level - CommissionOfferLevel), 0.3f, 0.75f);

    /// Commissions the player offered (not story ones) have ids starting with this.
    public const string OwnCommissionPrefix = "own:";

    /// The player shows them any pattern on display: they may order it, buy the pattern, or pass.
    void SuggestPattern(CustomerDef c)
    {
        var options = new List<(string, System.Action)>();
        foreach (var p in Shelf)
        {
            var pattern = p;
            var item = ItemFor(p.itemId);
            if (!Progress.CanMake(item)) continue;
            options.Add(($"{p.name} ({item.itemName})", () => Suggested(c, pattern)));
        }
        options.Add(("Never mind", () => TheirIdea(c)));
        Narrate(c, "Which pattern do you show them?", options.ToArray());
    }

    /// How they take a suggestion follows who they are (owner, 2026-10-31: not random): something they'd want,
    /// they ask you to make; a craft they do themselves, they buy the pattern for; anything else, they turn down
    /// in their own words (ink knots name_suggest_yes / _buy / _no, with variants).
    void Suggested(CustomerDef c, OwnedPattern p)
    {
        var item = ItemFor(p.itemId);
        bool wanted = OrderGenerator.Wants(c, GM.GetRelationship(c.customerId).level).Exists(i => i.itemId == p.itemId);
        bool makesIt = item != null && c.makes != null && System.Array.IndexOf(c.makes, item.craft) >= 0;
        if (wanted && !makesIt)
        {
            Offer = OrderGenerator.FromPattern(c, item, DesignFor(p), Save.day);
            Offer.fromPatternId = p.id;
            Reaction(c, "yes", () => Say(c, $"Could you make me a {item.itemName.ToLower()} like that?", Offer.target, ("Take the Order", AcceptOffer), ("Not Today", DeclineOffer)));
        }
        else if (makesIt)
        {
            SaleOffer = p;
            Reaction(c, "buy", () => Say(c, $"Could I buy the {p.name} pattern? I'd like to make it myself.", DesignFor(p), ($"Sell It ({p.price} coins)", AcceptOffer), ("Not Today", DeclineOffer)));
        }
        else Reaction(c, "no", () => Leave($"{c.displayName} wasn't taken with the {p.name}."),
                      "It's lovely, but it's not really for me.");
    }

    /// Plays their own line for a reaction if they have one (a variant not heard lately), then carries on.
    void Reaction(CustomerDef c, string kind, System.Action then, string fallback = null)
    {
        var knot = string.IsNullOrEmpty(c.inkName) ? null : Fresh(Variants($"{c.inkName}_suggest_{kind}"));
        if (knot != null) { Play(c, new List<string> { knot }, then); return; }
        if (fallback != null) { Say(c, fallback, ("Continue", then)); return; }
        then();
    }

    /// What the customer came in for (a story commission, a request, or a browse of the display).
    void TheirIdea(CustomerDef c)
    {
        var commission = CommissionFor(c);
        if (commission != null && Random.value < CommissionChance)
        {
            Offer = NewCommission(commission);
            var needs = commission.requiredSupply != null ? $" It must be made with {commission.requiredSupply.displayName}." : "";
            Say(c, $"{commission.request}\n\n(A commission: you'll design a brand-new {commission.item.itemName.ToLower()} pattern for it.{needs})",
                ("Take the Commission", AcceptOffer), ("Not Today", DeclineOffer));
            return;
        }
        var requests = RequestsFor(c);
        if (requests.Count > 0 && Random.value < RequestChance)
        {
            var knot = Fresh(requests);
            Play(c, new List<string> { knot }, () => AskForPattern(c, knot));
            return;
        }
        Browse(c);
    }

    /// They browse the display. They can buy any pattern to make themselves, but only order a finished
    /// piece the player can make.
    void Browse(CustomerDef c)
    {
        var wants = OrderGenerator.Wants(c, GM.GetRelationship(c.customerId).level);
        var matches = Shelf.FindAll(p => wants.Exists(i => i.itemId == p.itemId) && Progress.CanMake(ItemFor(p.itemId)));
        if (matches.Count == 0)
        {
            string hoped = wants.Count > 0 ? $" I was hoping for something like a {wants[Random.Range(0, wants.Count)].itemName.ToLower()}." : "";
            Say(c, $"Nothing on display for me today.{hoped} Maybe next time!",
                ("Continue", () => Leave($"{c.displayName} browsed the display but didn't find anything.{hoped.Replace("I was", "They were")}")));
            return;
        }
        var pick = matches[Random.Range(0, matches.Count)];
        if (c.makes != null && System.Array.IndexOf(c.makes, ItemFor(pick.itemId).craft) >= 0)
        {
            SaleOffer = pick;   // they make this kind of thing themselves: they buy the pattern
            Say(c, $"Oh, I love your {pick.name} pattern! May I buy it? I'd like to make it myself.",
                DesignFor(pick), ($"Sell It ({pick.price} coins)", AcceptOffer), ("Not Today", DeclineOffer));
        }
        else
        {
            Offer = OrderGenerator.FromPattern(c, ItemFor(pick.itemId), DesignFor(pick), Save.day);
            Offer.fromPatternId = pick.id;
            Say(c, $"Your {pick.name} pattern is lovely! Could you make me a {Offer.item.itemName.ToLower()} with it?\n\n(No rush, whenever you have time.)",
                Offer.target, ("Take the Order", AcceptOffer), ("Not Today", DeclineOffer));
        }
    }

    /// After an indirect request: the player offers a pattern from the display. Several can fit; the best
    /// fit makes them happiest. "Nothing fits" sends them away to look elsewhere.
    void AskForPattern(CustomerDef c, string requestKnot)
    {
        var knot = content.Ink.Get(requestKnot);
        var fits = TagList(knot, "fits");
        var best = TagList(knot, "best");
        var options = new List<(string, System.Action)>();
        foreach (var p in Shelf)
        {
            var item = ItemFor(p.itemId);
            if (!Progress.CanMake(item)) continue;
            var pattern = p;
            options.Add(($"Offer {p.name} ({item.itemName})", () => OfferPattern(c, pattern, best.Contains(pattern.itemId) ? 2 : fits.Contains(pattern.itemId) ? 1 : 0, fits)));
        }
        options.Add(("I don't have anything for that, sorry.", () =>
        {
            string hoped = fits.Count > 0 && ItemFor(fits[0]) is CraftItemDef i ? $" Something like a {i.itemName.ToLower()} would have done." : "";
            Leave($"{c.displayName} will look elsewhere.{hoped}");
        }));
        Say(c, "What do you think? Have you got anything like that?", options.ToArray());
    }

    void OfferPattern(CustomerDef c, OwnedPattern p, int fit, List<string> fits)
    {
        if (fit == 0)
        {
            string hint = fits.Count > 0 && ItemFor(fits[0]) is CraftItemDef i ? $" (They were thinking of something like a {i.itemName.ToLower()}.)" : "";
            Say(c, "Hmm. It's lovely, but it's not really what I had in mind. Maybe another time.",
                ("Continue", () => Leave($"{c.displayName} didn't take the {p.name}.{hint}")));
            return;
        }
        Offer = OrderGenerator.FromPattern(c, ItemFor(p.itemId), DesignFor(p), Save.day);
        Offer.fromPatternId = p.id;
        Offer.fit = fit;
        Say(c, fit == 2 ? "Oh! That's exactly it. How did you know?" : "Oh, that could work! Yes, let's do that.",
            DesignFor(p), ("Continue", AcceptOffer));
    }

    void Leave(string message)
    {
        Visitor = null; Offer = null; SaleOffer = null;
        LastMessage = message;
        GM.Persist();
        GM.ChangeScreen(ScreenId.Shop);
    }

    /// Sell the pattern, or take the order into the order book.
    public void AcceptOffer()
    {
        ChapterDef next = null;
        if (SaleOffer != null)
        {
            GM.AddCoins(SaleOffer.price);
            GM.AddRelationshipPoints(Visitor.customerId, 5);
            Save.coinsToday += SaleOffer.price;
            Save.patternsSoldToday++;
            Save.totalPatternsSold++;
            foreach (int n in new[] { 1, 10, 25, 50 })
                if (Save.totalPatternsSold == n)
                    Stamps.Earn(Save, $"patterns_sold:{n}", n == 1 ? "First pattern sold" : $"{n} patterns sold", "Someone's making your pattern tonight.", StampTheme.Shop);
            SaleOffer.sold++;
            next = Progress.AdvanceChapter();
            LastMessage = $"Sold the {SaleOffer.name} pattern to {Visitor.displayName}: +{SaleOffer.price} coins.";
        }
        else
        {
            Save.orders.Add(Offer);
            var from = Save.patterns.Find(x => x.id == Offer.fromPatternId);
            if (from != null) from.ordered++;
            LastMessage = Offer.needsDesign
                ? $"{Visitor.displayName}'s commission is in your order book. Design its pattern tonight."
                : $"{Visitor.displayName}'s {Offer.item.itemName.ToLower()} is in your order book.";
        }
        if (next != null) QueueChapterScene(next);
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

    // ---- Conversations (Ink dialogue, see Dialogue/InkLite.cs) ----

    bool playing;

    /// Plays Ink knots one after another on the Dialogue screen, then calls "done".
    /// Knots that don't exist are skipped, so a character without dialogue simply carries on.
    public void Play(CustomerDef who, List<string> knots, System.Action done)
    {
        knots = knots.FindAll(content.Ink.Has);
        if (knots.Count == 0) { playing = false; done(); return; }
        playing = true;
        var runner = new DialogueRunner(content.Ink, knots[0], c => Condition(who, c), c => Command(who, c));
        knots.RemoveAt(0);
        if (GM.Current != ScreenId.Dialogue) GM.ChangeScreen(ScreenId.Dialogue);
        Beat(who, runner, null, () => Play(who, knots, done));
    }

    /// One line (or a choice). Reads the line after it before showing it, so its commands have run: a
    /// ~ met() right after someone says their name puts the name up on that very line.
    void Beat(CustomerDef who, DialogueRunner runner, DialogueRunner.Line last, System.Action done, DialogueRunner.Line ahead = null, bool read = false)
    {
        var line = read ? ahead : runner.Next();
        if (line == null) { done(); return; }
        if (line.choices != null)
        {
            var options = new List<(string, System.Action)>();
            for (int i = 0; i < line.choices.Count; i++)
            {
                int k = i;
                options.Add((line.choices[i].text, () => { runner.Choose(k); Beat(who, runner, last, done); }));
            }
            Show(who, last, null, options);
            return;
        }
        var next = runner.Next();
        Show(who, line, null, new List<(string, System.Action)> { ("Continue", () => Beat(who, runner, line, done, next, true)) });
    }

    /// A line said by a customer (outside Ink): their face, the text, and answer buttons.
    void Say(CustomerDef who, string text, params (string, System.Action)[] answers) => Say(who, text, null, answers);

    /// A line of narration (no speaker) while someone is in the shop.
    void Narrate(CustomerDef who, string text, params (string, System.Action)[] answers)
    {
        if (GM.Current != ScreenId.Dialogue) GM.ChangeScreen(ScreenId.Dialogue);
        Show(who, new DialogueRunner.Line { speaker = null, text = text, tags = new List<string>() }, null, new List<(string, System.Action)>(answers));
    }

    void Say(CustomerDef who, string text, Pattern preview, params (string, System.Action)[] answers)
    {
        if (GM.Current != ScreenId.Dialogue) GM.ChangeScreen(ScreenId.Dialogue);
        Show(who, new DialogueRunner.Line { speaker = who != null ? who.displayName : null, text = text, tags = new List<string>() },
             preview, new List<(string, System.Action)>(answers));
    }

    Texture previewTexture;

    void Show(CustomerDef who, DialogueRunner.Line line, Pattern preview, List<(string, System.Action)> options)
    {
        string speaker = line?.speaker;
        var face = FaceFor(speaker, who, line?.tags);
        if (speaker != null)   // not introduced yet: no name to put to the face
        {
            var c = who != null && Speaks(who, speaker) ? who : System.Array.Find(content.customers, x => x != null && Speaks(x, speaker));
            if (c != null && !Save.met.Contains(c.customerId)) speaker = "???";
        }
        if (previewTexture != null) Destroy(previewTexture);
        previewTexture = preview != null ? PatternPreview.Make(preview) : null;
        dialogue.Show(speaker, face, line?.text ?? "", options, previewTexture);
    }

    /// The speaker's portrait, with the expression from the line's tags (# happy, # sad...).
    Sprite FaceFor(string speaker, CustomerDef who, List<string> tags)
    {
        var c = who;
        if (speaker != null && (c == null || !Speaks(c, speaker)))
            c = System.Array.Find(content.customers, x => x != null && Speaks(x, speaker));
        if (c == null) return null;
        var mood = Expression.Neutral;
        if (tags != null)
            foreach (var t in tags)
                if (System.Enum.TryParse(t, true, out Expression e)) mood = e;
        return c.GetPortrait(mood);
    }

    static bool Speaks(CustomerDef c, string speaker) =>
        c.displayName == speaker || c.displayName.EndsWith(" " + speaker) || string.Equals(c.inkName, speaker, System.StringComparison.OrdinalIgnoreCase);

    /// Ink conditions on choices: a flag name, "romance", "level >= N", or "not ..." any of those.
    bool Condition(CustomerDef who, string condition)
    {
        condition = condition.Trim();
        if (condition.StartsWith("not ")) return !Condition(who, condition.Substring(4));
        var rel = who != null ? GM.GetRelationship(who.customerId) : null;
        if (condition == "romance") return rel != null && rel.romance;
        if (condition.StartsWith("level >=") && int.TryParse(condition.Substring(8).Trim(), out int n)) return rel != null && rel.level >= n;
        return Save.flags.Contains(condition);
    }

    /// Ink commands: ~ friendship(5), ~ romance(1) / romance(0), ~ flag(name), ~ unflag(name).
    void Command(CustomerDef who, string call)
    {
        int open = call.IndexOf('('), close = call.LastIndexOf(')');
        string name = open > 0 ? call.Substring(0, open).Trim() : call.Trim();
        string arg = open > 0 && close > open ? call.Substring(open + 1, close - open - 1).Trim().Trim('"') : "";
        var rel = who != null ? GM.GetRelationship(who.customerId) : null;
        switch (name)
        {
            case "friendship":
                if (who != null && int.TryParse(arg, out int points)) GM.AddRelationshipPoints(who.customerId, points);
                break;
            case "romance":
                if (rel != null && who.romanceable)
                {
                    rel.romance = arg != "0";
                    if (rel.romance)
                    {
                        Progress.AddNews($"You and {who.displayName} are seeing each other.");
                        Stamps.Earn(Save, $"romance:{who.customerId}", $"You and {who.displayName}", "Something more than friendship.", StampTheme.Romance);
                    }
                }
                break;
            case "note":   // ~ note(Keeps the lighthouse.): something they've told you about themselves, for the Journal
                string entry = who != null ? $"{who.customerId}|{arg}" : null;
                if (entry != null && arg.Length > 0 && !Save.notes.Contains(entry))
                {
                    Save.notes.Add(entry);
                    Progress.AddNews($"Noted in your journal about {who.displayName}: {arg}");
                }
                break;
            case "met": if (who != null) Met(who.customerId); break;   // ~ met(): they've just said their name
            case "flag": if (!Save.flags.Contains(arg)) Save.flags.Add(arg); break;
            case "unflag": Save.flags.Remove(arg); break;
            default: Debug.LogWarning($"Unknown dialogue command: {call}"); break;
        }
        GM.Persist();
    }

    /// The greeting for this friendship level: <name>_visit_N for the highest N reached, else <name>_visit.
    /// A greeting for this visit: any knot named name_visit, name_visit_N (friendship level N or more) or
    /// with a further suffix (name_visit_b, name_visit_2_rain...), preferring one not heard lately.
    string VisitKnot(CustomerDef c)
    {
        if (string.IsNullOrEmpty(c.inkName)) return null;
        int level = GM.GetRelationship(c.customerId).level;
        var options = new List<string>();
        foreach (var name in Variants($"{c.inkName}_visit"))
            if (MinLevel(name, $"{c.inkName}_visit") <= level) options.Add(name);
        return Fresh(options);
    }

    /// Knots called exactly "prefix" or "prefix_<anything>".
    List<string> Variants(string prefix)
    {
        var list = new List<string>();
        foreach (var name in content.Ink.KnotNames)
            if (name == prefix || name.StartsWith(prefix + "_")) list.Add(name);
        return list;
    }

    /// The level in a name like "mara_visit_2_rain" (2), or 0 when the first part after the prefix isn't a number.
    static int MinLevel(string name, string prefix)
    {
        if (name.Length <= prefix.Length + 1) return 0;
        var first = name.Substring(prefix.Length + 1).Split('_')[0];
        return int.TryParse(first, out int n) ? n : 0;
    }

    const int RecentMemory = 40;   // knots remembered, so a regular doesn't say the same thing twice in a row

    /// Picks one of the options, preferring ones not heard lately (else the one heard longest ago), and remembers it.
    string Fresh(List<string> options)
    {
        if (options == null || options.Count == 0) return null;
        var unheard = options.FindAll(o => !Save.recentLines.Contains(o));
        string pick;
        if (unheard.Count > 0) pick = unheard[Random.Range(0, unheard.Count)];
        else
        {
            pick = options[0];
            foreach (var o in options) if (Save.recentLines.IndexOf(o) > Save.recentLines.IndexOf(pick)) pick = o;
        }
        Save.recentLines.Remove(pick);
        Save.recentLines.Insert(0, pick);
        if (Save.recentLines.Count > RecentMemory) Save.recentLines.RemoveRange(RecentMemory, Save.recentLines.Count - RecentMemory);
        return pick;
    }

    /// The next friendship scene this customer has reached and not yet played (one at a time).
    List<string> ScenesDue(CustomerDef c)
    {
        var due = new List<string>();
        if (string.IsNullOrEmpty(c.inkName)) return due;
        int level = GM.GetRelationship(c.customerId).level;
        for (int n = 1; n <= level; n++)
        {
            string knot = $"{c.inkName}_scene_{n}";
            if (!content.Ink.Has(knot) || Save.flags.Contains("seen:" + knot)) continue;
            Save.flags.Add("seen:" + knot);
            due.Add(knot);
            break;
        }
        return due;
    }

    /// This customer's indirect requests that the player could fill with at least one item they can make.
    List<string> RequestsFor(CustomerDef c)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(c.inkName)) return list;
        foreach (var name in content.Ink.KnotNames)
            if (name.StartsWith($"{c.inkName}_request_") && TagList(content.Ink.Get(name), "fits").Exists(id => Progress.CanMake(ItemFor(id))))
                list.Add(name);
        list.Sort();
        return list;
    }

    /// A knot tag like "fits: scarf, sweater" as a list of ids.
    static List<string> TagList(InkLite.Knot knot, string key)
    {
        var list = new List<string>();
        if (knot == null) return list;
        foreach (var t in knot.tags)
            if (t.StartsWith(key + ":"))
                foreach (var id in t.Substring(key.Length + 1).Split(','))
                    if (id.Trim().Length > 0) list.Add(id.Trim());
        return list;
    }

    /// A story scene to play next time the shop or workroom is on screen.
    void QueueChapterScene(ChapterDef chapter)
    {
        string knot = !string.IsNullOrEmpty(chapter.inkScene) ? chapter.inkScene : $"chapter_{chapter.number}";
        if (!Save.pendingScenes.Contains(knot)) Save.pendingScenes.Add(knot);
        LastMessage += $"\n\nChapter {chapter.number}: {chapter.title}";
    }

    /// Plays waiting story scenes when a hub screen comes up.
    void OnScreenChanged(ScreenId id)
    {
        if (playing || Save == null || Save.pendingScenes.Count == 0 || (id != ScreenId.Shop && id != ScreenId.Workroom)) return;
        var knot = Save.pendingScenes[0];
        Save.pendingScenes.RemoveAt(0);
        GM.Persist();
        Play(null, new List<string> { knot }, () => GM.ChangeScreen(GM.HubScreen));
    }

    // ---- Commissions ----

    /// The skill that lets the player design patterns (the Pattern Studio, and commissions).
    public const string PatternDesignSkill = "pattern_design";

    /// How often a visitor with a commission in mind asks for it instead of browsing.
    public const float CommissionChance = 0.3f;

    /// A commission this customer could ask for now: friendly enough, not delivered, not already in the book.
    public CommissionDef CommissionFor(CustomerDef customer)
    {
        int level = GM.GetRelationship(customer.customerId).level;
        foreach (var c in content.commissions)
            if (c != null && c.customer == customer && c.item != null && c.minRelationshipLevel <= level
                && Progress.Knows(PatternDesignSkill) && Progress.CanMake(c.item)
                && !Save.commissionsDone.Contains(c.commissionId) && !Save.orders.Exists(o => o.commissionId == c.commissionId))
                return c;
        return null;
    }

    /// The order for a commission: no pattern yet, the player designs it first.
    public Order NewCommission(CommissionDef c) => new Order
    {
        id = System.Guid.NewGuid().ToString("N"),
        customerId = c.customer.customerId,
        itemId = c.item.itemId,
        item = c.item,
        dayPlaced = Save.day,
        commissionId = c.commissionId,
        needsDesign = true,
        requiredSupplyId = c.requiredSupply != null ? c.requiredSupply.supplyId : "",
        target = new Pattern { displayName = $"{c.customer.displayName}'s {c.item.itemName}", craft = c.item.craft, width = 8, height = 8 },
    };

    /// The commission an order is for, or null.
    public CommissionDef CommissionOf(Order o) => string.IsNullOrEmpty(o.commissionId) ? null : content.Commission(o.commissionId);

    /// The order whose pattern is being designed in the Pattern Studio designer (a commission), or null.
    public Order DesigningFor { get; private set; }

    /// Opens the designer for a commission's brand-new pattern. It isn't judged.
    void DesignCommission(Order o)
    {
        DesigningFor = o;
        SelectedPattern = new OwnedPattern
        {
            id = System.Guid.NewGuid().ToString("N"), name = o.target.displayName, itemId = o.itemId,
            price = Mathf.Max(1, o.item.patternPrice), designed = true, design = o.target,
        };
        LastMessage = "";
        GM.ChangeScreen(ScreenId.PatternDesigner);
    }

    /// Day to night: ends the day's trading and shows the summary.
    public void CloseShop()
    {
        Save.night = true;
        Save.visitorsToday.Clear();   // anyone still waiting comes back another day
        GM.Persist();
        GM.ChangeScreen(ScreenId.DaySummary);
    }

    /// For the end-of-day summary: what's coming, so there's something to look forward to. Never how the story
    /// moves on or who's still to come (owner, 2026-10-31): that stays a surprise.
    public string LookingAhead()
    {
        var lines = new List<string>();
        foreach (var cust in content.customers)
        {
            if (cust == null || cust.firstChapter > Save.chapter || string.IsNullOrEmpty(cust.inkName)) continue;
            int level = GM.GetRelationship(cust.customerId).level;
            for (int n = 1; n <= level; n++)
                if (content.Ink.Has($"{cust.inkName}_scene_{n}") && !Save.flags.Contains($"seen:{cust.inkName}_scene_{n}"))
                {
                    lines.Add($"{cust.displayName} has something to tell you.");
                    break;
                }
        }
        int restock = shop.DaysUntilRestock;
        lines.Add(restock == 1 ? "The Supply Market gets new stock tomorrow." : $"New stock at the Supply Market in {restock} days.");
        var ready = Save.orders.FindAll(o => o.finished).Count;
        if (ready > 0) lines.Add($"{ready} order(s) set aside to hand over tomorrow.");
        return string.Join("\n", lines.Distinct());
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
        LastMessage = $"Good morning! Day {Save.day}." + (ready > 0 ? $" {ready} wrapped order{(ready == 1 ? "" : "s")} waiting to be picked up." : "");
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

    /// Who an order is for, as the player reads it ("you" for things made for yourself).
    public string NameFor(string customerId) =>
        customerId == PlayerProfile.SelfId ? "you" : CustomerFor(customerId) is CustomerDef c ? c.displayName : customerId;

    /// Pattern Studio: puts an order for yourself in the book, to wear once it's made (sweaters, scarves).
    public void MakeForMyself()
    {
        var p = SelectedPattern;
        var item = p != null ? content.Item(p.itemId) : null;
        if (item == null || !PlayerProfile.Wearable(item.itemId)) return;
        if (!Progress.CanMake(item)) { LastMessage = $"You haven't learned {Progression.CraftName(item.craft)} yet."; GM.ChangeScreen(ScreenId.PatternEdit); return; }
        var order = new Order
        {
            id = System.Guid.NewGuid().ToString("N"), customerId = PlayerProfile.SelfId, itemId = item.itemId, item = item,
            dayPlaced = Save.day, target = DesignFor(p),
        };
        Save.orders.Add(order);
        LastMessage = $"A {item.itemName.ToLower()} for you is in the order book. Work on it at night.";
        GM.Persist();
        GM.ChangeScreen(ScreenId.PatternEdit);
    }

    /// The shop's display page: puts a pattern from the book on display, if there's room and it can be made.
    public bool PutOnDisplay(string patternId)
    {
        var p = Save.patterns.Find(x => x.id == patternId);
        if (p == null || p.onShelf || Shelf.Count >= Save.displaySpots || !Progress.CanMake(ItemFor(p.itemId))) return false;
        p.onShelf = true;
        GM.Persist();
        return true;
    }

    public void ToggleShelf()
    {
        if (!SelectedPattern.onShelf && SelectedPattern.designed && SelectedPattern.design.cells.Count == 0)
        {
            LastMessage = "Draw something first: an empty pattern can't go on the shelf.";
            GM.ChangeScreen(ScreenId.PatternEdit);
            return;
        }
        var shelfItem = ItemFor(SelectedPattern.itemId);
        if (!SelectedPattern.onShelf && !Progress.CanMake(shelfItem))   // owner, 2026-10-31: never sell what you can't make
        {
            LastMessage = $"You can't make a {(shelfItem != null ? shelfItem.itemName.ToLower() : "piece like this")} yet, so its pattern can't go on display.";
            GM.ChangeScreen(ScreenId.PatternEdit);
            return;
        }
        if (!SelectedPattern.onShelf && Shelf.Count >= Save.displaySpots)
        {
            LastMessage = $"Your display holds {Save.displaySpots} patterns. Take one off first: keep the ones that sell.";
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
        if (!Progress.Knows(PatternDesignSkill))
        {
            var s = content.Skill(PatternDesignSkill);
            LastMessage = "You haven't learned to design patterns yet. Keep making things; who knows what you'll pick up.";
            GM.ChangeScreen(ScreenId.PatternStudio);
            return;
        }
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
        if (DesigningFor != null) { SaveCommissionDesign(design, name); return; }
        var p = SelectedPattern;
        p.name = string.IsNullOrWhiteSpace(name) ? p.name : name.Trim();
        p.itemId = itemId;
        p.price = Mathf.Clamp(price, 1, 999);
        design.displayName = p.name;
        design.craft = ItemFor(itemId).craft;
        p.design = Compact(design);
        if (design.cells.Count == 0) p.onShelf = false;
        if (!Save.patterns.Contains(p)) Save.patterns.Add(p);
        if (design.cells.Count > 0) Stamps.Earn(Save, "design:first", "Your first design", $"\"{p.name}\"", StampTheme.Craft);
        GM.Persist();
        LastMessage = $"Saved \"{p.name}\" to your pattern book.";
        GM.ChangeScreen(ScreenId.PatternEdit);
    }

    /// A commission's design becomes the order's pattern; then it's made like any other order.
    void SaveCommissionDesign(Pattern design, string name)
    {
        if (design.cells.Count == 0) return;   // the designer asks for some squares first
        var o = DesigningFor;
        design.displayName = string.IsNullOrWhiteSpace(name) ? o.target.displayName : name.Trim();
        design.craft = o.item.craft;
        o.target = Compact(design);
        o.needsDesign = false;
        DesigningFor = null;
        SelectedPattern = null;
        Selected = o;
        GM.Persist();
        LastMessage = $"\"{o.target.displayName}\" is designed. Now make it!";
        GM.ChangeScreen(ScreenId.Workbench);
    }

    /// Designer's Cancel: back without saving (a brand-new pattern is simply dropped).
    public void CancelDesign()
    {
        if (DesigningFor != null)
        {
            Selected = DesigningFor;
            DesigningFor = null;
            SelectedPattern = null;
            GM.ChangeScreen(ScreenId.Workbench);
            return;
        }
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
        LastMessage = shop.PacksLeft(s) <= 0 ? $"{s.displayName} is sold out. It's back in {shop.DaysUntilBack(s)} days."
            : (s.kind == SupplyKind.Tool || s.kind == SupplyKind.Book) && shop.Owns(supplyId) ? $"You already have {s.displayName}."
            : !shop.Buy(s) ? $"Not enough coins for {s.displayName} ({s.price} coins)."
            : s.kind == SupplyKind.Tool ? $"Bought {s.displayName}. Tools are yours to keep."
            : s.kind == SupplyKind.Book ? $"Bought {s.displayName}."
            : $"Bought {s.displayName}: +{s.perPack} {s.unit} (you have {shop.Stock(supplyId)}).";
        if (LastMessage.StartsWith("Bought"))
        {
            GM.AddRelationshipPoints(MarketOwnerId, 2);   // a regular becomes a friend
            boughtThisVisit = true;
            if (s.market == MarketSlot.Rare) Stamps.Earn(Save, "market:rare", "A rare find", s.displayName, StampTheme.Shop);
            if (s.kind == SupplyKind.Tool && s.assist > 0) Stamps.Earn(Save, "tool:better", "Better tools", s.displayName, StampTheme.Craft);
        }
        foreach (var learned in Progress.CheckUnlocks()) LastMessage += $" You learned {learned.displayName}!";
        GM.Persist();
        GM.ChangeScreen(ScreenId.Market);   // refresh the list
    }

    /// Someone the player has met shows up on the Journal's Friends page.
    public void Met(string customerId)
    {
        if (!string.IsNullOrEmpty(customerId) && !Save.met.Contains(customerId)) Save.met.Add(customerId);
    }

    /// Bram Okafor runs the Supply Market (owner, 2026-10-30): someone to befriend, or more.
    public const string MarketOwnerId = "bram";

    /// A chat with a shop owner in their own shop (once a day): their greeting, or a friendship scene
    /// when one is due. Then back to their shop.
    public void ChatWithMarketOwner() => ChatWith(MarketOwnerId, ScreenId.Market);

    // ---- Shops on the square (owner, 2026-10-31): they open over time, with no warning beforehand. The market
    // opens on day 2, the boutique after three hand-overs (or chapter 2). Their owners greet you at the door
    // and see you off (ink knots name_greet*, name_bye*, name_thanks* after buying something). ----

    public const string MarketFlag = "shop:market", BoutiqueFlag = "shop:boutique";
    public const int MarketOpensDay = 2, BoutiqueDeliveries = 3;
    bool boughtThisVisit;

    public bool ShopOpen(ScreenId shop) => Save != null && Save.flags.Contains(shop == ScreenId.Market ? MarketFlag : BoutiqueFlag);
    public bool AnyShopOpen => ShopOpen(ScreenId.Market) || ShopOpen(ScreenId.Boutique);

    public void CheckShops()
    {
        if (Save.day >= MarketOpensDay)
            OpenShop(MarketFlag, "Bram's Supply Market", "Fabric, thread, yarn and tools, just across the square.");
        if (Save.totalDelivered >= BoutiqueDeliveries || Save.chapter >= 2)
            OpenShop(BoutiqueFlag, "Moreau's Boutique", "Clothes and hair, across the square. Celeste has opened her doors.");
    }

    void OpenShop(string flag, string name, string detail)
    {
        if (Save.flags.Contains(flag)) return;
        Save.flags.Add(flag);
        Stamps.Earn(Save, flag, name, detail, StampTheme.Shop);
        Progress.AddNews($"{name} is open. Find it under Shops.");
        GM.Persist();
    }

    public bool CanChat(string customerId) => !Save.flags.Contains($"chat:{customerId}:{Save.day}");

    /// Walking into a shop: its owner says hello, then you browse (or stop for a chat first).
    public void EnterShop(string ownerId, ScreenId shop)
    {
        if (Save == null || Save.night || !ShopOpen(shop)) return;
        boughtThisVisit = false;
        LastMessage = "";
        var who = CustomerFor(ownerId);
        bool first = !Save.met.Contains(ownerId);   // the first time in: they introduce themselves
        var options = new List<(string, System.Action)> { ("Have a look around", () => { Met(ownerId); GM.ChangeScreen(shop); }) };
        if (CanChat(ownerId) && !first) options.Add(("Stop for a chat", () => ChatWith(ownerId, shop)));
        string knot = first ? OwnerKnot(who, "intro") : null;
        if (!SayKnot(who, knot ?? OwnerKnot(who, "greet"), options)) { Met(ownerId); GM.ChangeScreen(shop); }
    }

    /// Leaving a shop: the owner sees you off (thanking you if you bought something), then back to your shop.
    public void LeaveShop(string ownerId)
    {
        var who = CustomerFor(ownerId);
        var options = new List<(string, System.Action)> { ("Goodbye!", () => GM.ChangeScreen(GM.HubScreen)) };
        if (CanChat(ownerId)) options.Add(("Chat a while first", () => ChatWith(ownerId, GM.HubScreen)));
        string knot = boughtThisVisit ? OwnerKnot(who, "thanks") : null;
        if (!SayKnot(who, knot ?? OwnerKnot(who, "bye"), options)) GM.ChangeScreen(GM.HubScreen);
        boughtThisVisit = false;
    }

    public void BoughtInShop() => boughtThisVisit = true;
    public void LeaveMarket() => LeaveShop(MarketOwnerId);

    /// One of a shop owner's lines of this kind (greet, bye, thanks) for the friendship level reached.
    string OwnerKnot(CustomerDef who, string kind)
    {
        if (who == null || string.IsNullOrEmpty(who.inkName)) return null;
        string prefix = $"{who.inkName}_{kind}";
        int level = GM.GetRelationship(who.customerId).level;
        return Fresh(Variants(prefix).FindAll(k => MinLevel(k, prefix) <= level));
    }

    /// Plays a short knot line by line, ending on the given answers instead of "Continue". False if there's no such knot.
    bool SayKnot(CustomerDef who, string knot, List<(string, System.Action)> answers)
    {
        if (who == null || knot == null || !content.Ink.Has(knot)) return false;
        var runner = new DialogueRunner(content.Ink, knot, c => Condition(who, c), c => Command(who, c));
        var first = runner.Next();
        if (first == null || first.choices != null) return false;
        if (GM.Current != ScreenId.Dialogue) GM.ChangeScreen(ScreenId.Dialogue);
        ShowLines(who, runner, first, answers, 0);
        return true;
    }

    /// Shows a line, reading one ahead to know whether it's the last (which gets the answers). Reading
    /// ahead runs the next line's commands, so a ~ met() just after someone says their name shows it at once.
    void ShowLines(CustomerDef who, DialogueRunner runner, DialogueRunner.Line line, List<(string, System.Action)> answers, int count)
    {
        var next = count < 20 ? runner.Next() : null;
        if (next == null || next.choices != null) { Show(who, line, null, answers); return; }
        Show(who, line, null, new List<(string, System.Action)> { ("Continue", () => ShowLines(who, runner, next, answers, count + 1)) });
    }

    /// Someone's first visit (owner, 2026-10-31): their introduction (name_intro*), after which they're met
    /// (their name and portrait go in the Journal). Without an intro knot, a plain hello.
    void Introduce(CustomerDef who, System.Action then)
    {
        System.Action done = () => { Met(who.customerId); GM.Persist(); then(); };
        string knot = string.IsNullOrEmpty(who.inkName) ? null : Fresh(Variants($"{who.inkName}_intro"));
        if (knot != null) { Play(who, new List<string> { knot }, done); return; }
        Met(who.customerId);
        Say(who, $"Hello! I'm {who.displayName}. I don't think we've met.", ("Continue", done));
    }

    public void ChatWith(string customerId, ScreenId back)
    {
        var who = CustomerFor(customerId);
        if (who == null) return;
        string flag = $"chat:{customerId}:{Save.day}";
        if (Save.flags.Contains(flag))
        {
            Say(who, Pick("Busy day! Come back tomorrow and I'll put the kettle on.", "I'd love to chat, but there's a queue. Tomorrow?"), ("Continue", () => GM.ChangeScreen(back)));
            return;
        }
        Save.flags.Add(flag);
        Met(customerId);
        GM.AddRelationshipPoints(customerId, 3);
        var knots = ScenesDue(who);
        if (knots.Count == 0) { var hello = VisitKnot(who); if (hello != null) knots.Add(hello); }
        GM.Persist();
        Play(who, knots, () => GM.ChangeScreen(back));
    }

    /// Buys a shop upgrade (once each): more display space, a sign for more visitors, a better lamp.
    public void BuyUpgrade(string upgradeId)
    {
        var u = content.Upgrade(upgradeId);
        if (u == null || Save.night || Progress.Owns(u) || u.chapter > Save.chapter) return;
        if (!GM.TrySpend(u.price)) { LastMessage = $"Not enough coins for the {u.displayName} ({u.price} coins)."; GM.ChangeScreen(ScreenId.Market); return; }
        Save.upgrades.Add(u.upgradeId);
        Stamps.Earn(Save, $"upgrade:{u.upgradeId}", u.displayName, "The shop grows a little.", StampTheme.Shop);
        if (u.effect == UpgradeEffect.DisplaySpot) Save.displaySpots += u.amount;
        Progress.AddNews($"The shop has a new {u.displayName}.");
        LastMessage = $"The {u.displayName} is in! {u.description}";
        GM.Persist();
        GM.ChangeScreen(ScreenId.Market);
    }

    // ---- Order book and workbench ----

    /// What an unfinished order still needs that the player doesn't have: supplies, tools, a fabric or yarn to
    /// choose. Empty when everything's in the cupboard (or there's nothing to make yet, e.g. a design first).
    public List<string> ShortOf(Order o)
    {
        var list = new List<string>();
        if (o == null || o.patternOnly || o.finished || o.needsDesign || o.item == null) return list;
        if (MaterialToChoose(o) && ChooseMaterialStep.Options(o, content, shop).Count == 0) list.Add(ChooseMaterialStep.Wanted(o, content));
        foreach (var n in shop.Missing(NeedsFor(o)))
        {
            var s = content.Supply(n.supplyId);
            list.Add($"{(s != null ? s.displayName : n.supplyId)} ({n.amount - shop.Stock(n.supplyId)} more {(s != null ? s.unit : "")})".Replace(" )", ")"));
        }
        foreach (var t in shop.MissingTools(ToolsFor(o))) list.Add(t.label);
        return list;
    }

    /// Whether a missing tool need would be met by this supply (for the market's shopping list).
    public static bool Fits(ToolNeed need, SupplyDef s) =>
        s != null && s.kind == SupplyKind.Tool && s.toolType == need.toolType && (need.exactSize ? s.size == need.minSize : s.size >= need.minSize);

    public void SelectOrder(string orderId)
    {
        Selected = Save.orders.Find(o => o.id == orderId);
        LastMessage = "";
        // At night, an order only opens once everything for it is in the cupboard (owner, 2026-10-31).
        var missing = ShortOf(Selected);
        if (Save.night && Selected != null && missing.Count > 0)
        {
            LastMessage = $"You can't start on the {Selected.item.itemName.ToLower()} yet. Missing: {string.Join(", ", missing)}. The Supply Market's shopping list has it all.";
            Selected = null;
            GM.ChangeScreen(ScreenId.OrderBook);
            return;
        }
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
        if (step.kind == StepKind.CrossStitch) return Materials.For(o.target, Materials.FabricFor(o.target, content, shop, o.requiredSupplyId), content);
        if (step.materials != null)
            foreach (var m in step.materials) needs.Add(new MaterialNeed { supplyId = m.supplyId, amount = m.amount });
        if (step.threadAmount > 0 && o.target != null)
        {
            Materials.EnsureThreads(o.target, content);
            if (o.target.threads.Count > 0)
            {
                string floss = o.target.threads[Mathf.Clamp(step.patternThread, 0, o.target.threads.Count - 1)];
                if (!string.IsNullOrEmpty(floss)) needs.Add(new MaterialNeed { supplyId = floss, amount = step.threadAmount });
            }
        }
        if (step.fabricPieces > 0 && !string.IsNullOrEmpty(o.fabricId))
            needs.Add(new MaterialNeed { supplyId = o.fabricId, amount = step.ChosenAmount(o.target) });
        // Colourwork: each of the pattern's colours is knitted in a yarn of that colour, one stitch each.
        if (step.kind == StepKind.KnitRows && o.target != null && !string.IsNullOrEmpty(o.fabricId))
        {
            var knit = KnitLayout.For(o, step.piece);
            var main = content.Supply(o.fabricId);
            var perColour = new Dictionary<int, int>();
            foreach (var st in knit.stitches)
                if (st.color >= 0) { perColour.TryGetValue(st.color, out int n); perColour[st.color] = n + 1; }
            foreach (var kv in perColour)
            {
                if (kv.Key >= o.target.palette.Count) continue;
                var yarn = Materials.ColourworkYarn(o.target.palette[kv.Key], main, content, shop);
                if (yarn == null) continue;
                var same = needs.Find(x => x.supplyId == yarn.supplyId);
                if (same != null) same.amount += kv.Value;
                else needs.Add(new MaterialNeed { supplyId = yarn.supplyId, amount = kv.Value });
            }
        }
        // A crocheted stripe: its rounds are worked in a yarn of that colour.
        if (step.kind == StepKind.CrochetRounds && o.target != null && !string.IsNullOrEmpty(o.fabricId))
        {
            var main = content.Supply(o.fabricId);
            foreach (var r in CrochetWork.For(step.piece).rounds)
            {
                if (r.color < 0 || r.color >= o.target.palette.Count) continue;
                var yarn = Materials.ColourworkYarn(o.target.palette[r.color], main, content, shop);
                if (yarn == null) continue;
                var same = needs.Find(x => x.supplyId == yarn.supplyId);
                if (same != null) same.amount += r.Stitches;
                else needs.Add(new MaterialNeed { supplyId = yarn.supplyId, amount = r.Stitches });
            }
        }
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

    /// The best owned tool helping with a step: the one with the highest assist among those the step uses
    /// (a basic tool doesn't count), or null.
    public SupplyDef HelpingTool(Order o, int index)
    {
        SupplyDef best = null;
        foreach (var need in ToolsForStep(o, index))
        {
            var t = shop.BestFor(need);
            if (t != null && t.assist > 0f && (best == null || t.assist > best.assist)) best = t;
        }
        return best;
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
        var notYet = Progress.MissingFor(Selected.item);
        if (notYet.Count > 0)
        {
            LastMessage = $"You haven't learned {notYet[0].displayName} yet.";
            GM.ChangeScreen(ScreenId.Workbench);
            return;
        }
        if (Selected.needsDesign) { DesignCommission(Selected); return; }
        // First time: gather the supplies from the cupboard, ticking them off the order's list (GatherUI).
        if (!Selected.gathered && !Selected.patternOnly) { LastMessage = ""; GM.ChangeScreen(ScreenId.Gather); return; }
        int index =Mathf.Clamp(Selected.step, 0, Selected.Steps.Length - 1);
        var kind = Selected.Steps[index].kind;
        string blocked = null;
        if (shop.MissingTools(ToolsForStep(Selected, index)).Count > 0)
            blocked = "You're missing a tool for this step. Look for it at the Supply Market.";
        else if ((kind == StepKind.ChooseFabric || kind == StepKind.ChooseYarn) && ChooseMaterialStep.Options(Selected, content, shop).Count == 0)
            blocked = $"You need {ChooseMaterialStep.Wanted(Selected, content)}. Look for some at the Supply Market.";
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
            if (kind == StepKind.CrossStitch) Selected.fabricId = Materials.FabricFor(Selected.target, content, shop, Selected.requiredSupplyId)?.supplyId;
            shop.Use(needs);
            if (kind == StepKind.CrossStitch) Selected.materialsUsed = true;
            else Selected.StepState(index).materialsUsed = true;
            GM.Persist();
        }
        LastMessage = "";
        Selected.finished = false;   // reopening a set-aside piece makes it a work in progress again
        var fabric = content.Supply(Selected.fabricId);
        var helper = HelpingTool(Selected, index);
        if (helper != null) LastMessage = $"Your {helper.displayName} make{(helper.displayName.EndsWith("s") ? "" : "s")} this easier.";
        minigame.Open(Selected, fabric != null ? fabric.color : new Color(0.95f, 0.95f, 0.90f), content, shop, helper != null ? helper.assist : 0f);
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
    string warnedMistakes;   // "orderId:step" already warned about its mistakes (the second Finish goes on)

    public void FinishCrafting()
    {
        // Mistakes left in: say so once; Undo fixes them, Finish again keeps them (and the lower quality).
        if (minigame.Step is IMistakes m && m.MistakesLeft is string left && warnedMistakes != $"{Selected.id}:{Selected.step}")
        {
            warnedMistakes = $"{Selected.id}:{Selected.step}";
            LastMessage = $"Still in there: {left}. Undo to fix it, or tap Finish again to leave it in.";
            GM.ChangeScreen(ScreenId.Minigame);
            return;
        }
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
        string learned = GainExperience(Selected, state.score);
        if (!Selected.IsLastStep)
        {
            // Carry straight on to the next step. Only if it needs something the player doesn't
            // have (a tool, a material) does StartCrafting stop at the Workbench to say what.
            string done = Selected.CurrentStep.label;
            Selected.step++;
            GM.Persist();
            StartCrafting();
            if (GM.Current == ScreenId.Minigame)
            {
                LastMessage = ($"{done}: done!{learned} " + LastMessage).Trim();   // keeps "your shears make this easier"
                GM.ChangeScreen(ScreenId.Minigame);   // shows it with the new step's hint
            }
            else LastMessage = $"{done}: done!{learned} {LastMessage}";
            return;
        }
        LastAccuracy = Selected.RecipeScore();
        GM.Persist();

        int level = GM.GetRelationship(Selected.customerId).level;
        LastStars = PricingCalculator.Stars(LastAccuracy);
        LastPrice = PricingCalculator.Compute(Selected.item, LastAccuracy, TierOf(Selected), level, Selected.patternOnly);
        LastPoints = PricingCalculator.RelationshipReward(LastAccuracy);
        var commission = CommissionOf(Selected);
        if (commission != null) { LastPrice += commission.bonusCoins; LastPoints += commission.bonusFriendship; }
        LastLearned = learned.Trim();
        GM.ChangeScreen(ScreenId.Result);
    }

    /// What the last finished piece taught the player (a level, a new skill), for the Result screen.
    public string LastLearned { get; private set; } = "";

    /// Experience for a finished step (more for neater work), and anything a new level unlocks.
    /// Returns a short message to show, or "" if nothing changed.
    string GainExperience(Order o, float score)
    {
        if (o.patternOnly || !o.CurrentStep.Judged) return "";
        var news = new List<string>();
        string level = Progress.AddXp(o.item.craft, 10 + Mathf.RoundToInt(10 * score));
        if (level != null) news.Add(level);
        foreach (var s in Progress.CheckUnlocks()) news.Add($"You learned {s.displayName}!");
        return news.Count > 0 ? " " + string.Join(" ", news) : "";
    }

    public void KeepWorking() => StartCrafting();

    /// Night: the piece is done, wrapped and set aside until its customer comes back for it. They don't
    /// say when: the doorbell will tell.
    public void SetAside()
    {
        if (!Selected.finished && Selected.item != null)   // one more thing made in this craft
        {
            var made = Save.itemsMade.Find(x => x.craft == Selected.item.craft);
            if (made == null) Save.itemsMade.Add(made = new CraftCount { craft = Selected.item.craft });
            made.count++;
        }
        Selected.finished = true;
        Selected.accuracy = LastAccuracy;
        foreach (var id in new List<string>(Selected.extras))   // the special touches go on now
        {
            var s = content.Supply(id);
            if (s == null || shop.Stock(id) < s.decorateAmount) { Selected.extras.Remove(id); continue; }
            shop.Use(new List<MaterialNeed> { new MaterialNeed { supplyId = id, amount = s.decorateAmount } });
        }
        if (Selected.customerId == PlayerProfile.SelfId)   // made for yourself: into the wardrobe
        {
            var yarn = content.Supply(Selected.fabricId);
            var made = new MadeItem
            {
                id = Selected.id, itemId = Selected.itemId, name = $"{Selected.target.displayName} {Selected.item.itemName.ToLower()}".Trim(),
                yarn = yarn != null ? yarn.color : Color.white,
                motif = Selected.target.palette.Count > 0 ? Selected.target.palette[0] : Color.white,
            };
            Save.player.made.Add(made);
            Stamps.Earn(Save, "made:first", "Made for me", $"Your {made.name}.", StampTheme.Craft);
            Save.player.wornMade = made.id;   // put it straight on
            Save.orders.Remove(Selected);
            GM.Persist();
            LastMessage = $"You're wearing your new {Selected.item.itemName.ToLower()}! Change what you wear in Settings > Change Your Look.";
            Selected = null;
            GM.ChangeScreen(ScreenId.OrderBook);
            return;
        }
        if (Selected.pickupDay <= Save.day) Selected.pickupDay = Save.day + PickupDelay();
        GM.Persist();
        var customer = CustomerFor(Selected.customerId);
        LastMessage = $"The {Selected.item.itemName.ToLower()} is wrapped and waiting for {(customer != null ? customer.displayName : "its owner")}. They didn't say when they'd come by.";
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
        if (Selected.fit == 2) points = Mathf.RoundToInt(points * 1.5f);   // exactly what they hoped for
        var commission = CommissionOf(Selected);
        if (commission != null)
        {
            price += commission.bonusCoins;
            points += commission.bonusFriendship;
            Save.commissionsDone.Add(commission.commissionId);
            // Its pattern goes into the book, ready to be shelved.
            Save.patterns.Add(new OwnedPattern
            {
                id = System.Guid.NewGuid().ToString("N"), name = Selected.target.displayName, itemId = Selected.itemId,
                price = Mathf.Max(1, Selected.item.patternPrice), designed = true, design = Selected.target,
            });
        }

        if (commission == null && Selected.commissionId != null && Selected.commissionId.StartsWith(OwnCommissionPrefix))
        {
            points += 10;   // something made just for them
            Save.patterns.Add(new OwnedPattern
            {
                id = System.Guid.NewGuid().ToString("N"), name = Selected.target.displayName, itemId = Selected.itemId,
                price = Mathf.Max(1, Selected.item.patternPrice), designed = true, design = Selected.target,
            });
            Stamps.Earn(Save, "commission:own", "Made to order", $"Your own design for {name}.", StampTheme.Craft);
        }

        // Special touches: a gamble. If they like the surprise it's worth more; if not, it costs a little goodwill.
        var surprises = new List<string>();
        foreach (var id in Selected.extras)
        {
            var s = content.Supply(id);
            if (s == null) continue;
            // A preference about this very thing decides it; otherwise it's their taste for surprises. Preferences are
            // never mentioned in dialogue (owner, 2026-10-31): this reaction is how the player finds out, and the Journal notes it.
            var known = customer != null && customer.preferences != null ? System.Array.Find(customer.preferences, x => x.supply == s) : null;
            bool liked = known != null ? known.likes : Random.value < (customer != null ? customer.likesSurprises : 0.65f);
            if (known != null && !Save.knownPrefs.Contains($"{customer.customerId}:{known.id}"))
            {
                Save.knownPrefs.Add($"{customer.customerId}:{known.id}");
                Progress.AddNews($"Noted in your journal: {customer.displayName} {(known.likes ? "likes" : "doesn't like")} {known.text}.");
            }
            string what = s.displayName.ToLower();
            if (liked)
            {
                price += Mathf.Max(1, Mathf.RoundToInt(price * s.decorateBonus));
                points += 5;
                surprises.Add(Pick($"And {what}! Oh, what a lovely surprise.", $"You added {what}? That's exactly the sort of thing I'd never think of. I love it."));
            }
            else
            {
                points = Mathf.Max(0, points - 4);
                surprises.Add(Pick($"Oh. {char.ToUpper(what[0]) + what.Substring(1)}. I... didn't ask for that, but never mind.", $"Hm. The {what}. Not really my sort of thing, to be honest."));
            }
        }

        GM.AddCoins(price);
        GM.AddRelationshipPoints(Selected.customerId, points);
        Save.orders.Remove(Selected);
        Save.deliveredToday++;
        Save.totalDelivered++;
        if (commission != null || Selected.commissionId != null && Selected.commissionId.StartsWith(OwnCommissionPrefix)) Save.totalCommissions++;
        foreach (int n in new[] { 1, 5, 10, 25, 50, 100 })
            if (Save.totalDelivered == n)
                Stamps.Earn(Save, $"delivered:{n}", n == 1 ? "First order delivered" : $"{n} orders delivered", n == 1 ? "Your first happy customer." : "The town keeps coming back.", StampTheme.Shop);
        if (commission != null) Stamps.Earn(Save, $"commission:{commission.commissionId}", "Commission complete", $"\"{Selected.target.displayName}\" for {name}.", StampTheme.Story);
        Save.coinsToday += price;
        var taught = Progress.CheckUnlocks();   // a closer friend may teach something
        var chapter = Progress.AdvanceChapter();
        GM.Persist();

        int after = GM.GetRelationship(Selected.customerId).level;
        LastMessage = after > level
            ? $"{name} paid you {price} coins, and is now a closer friend! (level {after})"
            : $"{name} paid you {price} coins. (+{points} friendship)";
        if (commission != null)
            LastMessage += $" The \"{Selected.target.displayName}\" pattern is in your book: put it on the shelf in the Pattern Studio.";
        foreach (var s in taught) LastMessage += s.friend != null ? $" {s.friend.displayName} taught you {s.displayName}!" : $" You learned {s.displayName}!";
        if (chapter != null) QueueChapterScene(chapter);
        var stars = PricingCalculator.Stars(Selected.accuracy);
        Selected = null;
        GM.Persist();
        // They react, and a friendship scene plays if this brought them to a new level.
        var knots = new List<string>();
        if (customer != null && !string.IsNullOrEmpty(customer.inkName))
        {
            var reaction = Fresh(Variants($"{customer.inkName}_handover_{(stars >= 3 ? "good" : stars == 2 ? "okay" : "poor")}"));
            if (reaction != null) knots.Add(reaction);
            knots.AddRange(ScenesDue(customer));
        }
        // What they paid is only known now, after they've seen it and reacted (owner, 2026-10-31).
        string paid = LastMessage;
        System.Action payment = () => Narrate(customer, paid, ("Continue", () => GM.ChangeScreen(ScreenId.Shop)));
        if (surprises.Count > 0 && customer != null)
            Play(customer, knots, () => Say(customer, string.Join("\n", surprises), ("Continue", payment)));
        else Play(customer, knots, payment);
    }

    static string Pick(params string[] lines) => lines[Random.Range(0, lines.Length)];
}
