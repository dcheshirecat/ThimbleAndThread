using System.Collections.Generic;
using UnityEngine;

/// Runs the shop's day and night.
/// Day: customers visit and leave orders in the order book; finished orders are handed over.
/// Night: the player works on any order, can stop at any point and pick it up later (the work is saved),
/// and sets finished pieces aside for the customer to collect the next day. Nothing has a deadline.
/// Every change autosaves through GameManager.Persist.
public class ShopDay : MonoBehaviour
{
    public CustomerDef[] customers;
    public CraftItemDef[] items;
    public ShopManager shop;
    public MinigameHost minigame;

    // Not saved: what is on screen right now.
    public CustomerDef Visitor { get; private set; }     // customer in the Dialogue screen
    public Order Offer { get; private set; }             // their request, not yet accepted
    public Order Selected { get; private set; }          // order open on the Workbench
    public float LastAccuracy { get; private set; }
    public int LastStars { get; private set; }
    public int LastPrice { get; private set; }    // what the customer will pay at hand-over
    public int LastPoints { get; private set; }
    public string LastMessage { get; private set; } = "";

    GameManager GM => GameManager.Instance;
    SaveData Save => GM.Save;

    public List<Order> Orders => Save.orders;
    public int CustomersWaiting => Save.visitorsToday.Count;

    void Start() => GM.OnGameLoaded += OnGameLoaded;

    void OnDestroy()
    {
        if (GM != null) GM.OnGameLoaded -= OnGameLoaded;
    }

    void OnGameLoaded(bool isNewGame)
    {
        minigame.Close();
        Visitor = null; Offer = null; Selected = null;

        // Saves store item ids; look the definitions back up. Drop orders for items that no longer exist.
        Save.orders.RemoveAll(o =>
        {
            o.item = ItemFor(o.itemId);
            if (o.item == null) Debug.LogWarning($"Dropping saved order for unknown item '{o.itemId}'.");
            return o.item == null;
        });

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
        foreach (var c in customers)
            if (!Save.orders.Exists(o => o.customerId == c.customerId)) list.Add(c.customerId);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public CustomerDef CustomerFor(string id) => System.Array.Find(customers, c => c.customerId == id);
    CraftItemDef ItemFor(string id) => System.Array.Find(items, i => i.itemId == id);

    // ---- Shop ----

    public void GreetCustomer()
    {
        if (Save.night) return;   // the shop is closed at night
        while (Save.visitorsToday.Count > 0)
        {
            var customer = CustomerFor(Save.visitorsToday[0]);
            Save.visitorsToday.RemoveAt(0);
            if (customer == null) continue;
            var order = OrderGenerator.Generate(customer, GM.GetRelationship(customer.customerId).level, Save.day);
            if (order == null) continue;   // nothing this customer can ask for yet

            Visitor = customer;
            Offer = order;
            GM.Persist();
            GM.ChangeScreen(ScreenId.Dialogue);
            return;
        }
        GM.Persist();
        LastMessage = "No one else is waiting today. Close the shop whenever you're ready.";
        GM.ChangeScreen(ScreenId.Shop);
    }

    public void AcceptOrder()
    {
        Save.orders.Add(Offer);
        LastMessage = $"{Visitor.displayName}'s {Offer.item.itemName.ToLower()} is in your order book.";
        Visitor = null; Offer = null;
        GM.Persist();
        GM.ChangeScreen(ScreenId.Shop);
    }

    public void DeclineOrder()
    {
        LastMessage = $"{Visitor.displayName} will come back another day.";
        Visitor = null; Offer = null;
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
        GM.NextDay();
        InviteVisitors();
        GM.Persist();
        int ready = Save.orders.FindAll(o => o.finished).Count;
        LastMessage = $"Good morning! Day {Save.day}." + (ready > 0 ? $" {ready} order(s) ready to hand over." : "");
        GM.ChangeScreen(ScreenId.Shop);
    }

    // ---- Order book and workbench ----

    public void SelectOrder(string orderId)
    {
        Selected = Save.orders.Find(o => o.id == orderId);
        if (Selected != null) GM.ChangeScreen(ScreenId.Workbench);
    }

    /// Night only: crafting happens in the workroom.
    public void StartCrafting()
    {
        if (!Save.night || Selected == null) return;
        Selected.finished = false;   // reopening a set-aside piece makes it a work in progress again
        minigame.Open(Selected);
        GM.ChangeScreen(ScreenId.Minigame);
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

    /// Shows how the piece turned out and what the customer will pay.
    public void FinishCrafting()
    {
        LastAccuracy = minigame.SaveWork();
        minigame.Close();
        GM.Persist();

        int level = GM.GetRelationship(Selected.customerId).level;
        LastStars = PricingCalculator.Stars(LastAccuracy);
        LastPrice = PricingCalculator.Compute(Selected.item, LastAccuracy, shop.BestTier(Selected.item.craft), level, Selected.patternOnly);
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
        int price = PricingCalculator.Compute(Selected.item, Selected.accuracy, shop.BestTier(Selected.item.craft), level, Selected.patternOnly);
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
