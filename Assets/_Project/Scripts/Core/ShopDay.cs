using System.Collections.Generic;
using UnityEngine;

/// Runs the shop: customers visit each day and leave orders in the order book.
/// The player works on any order whenever they like, can stop at any point and pick it up later
/// (the work is saved), and delivers when happy. Nothing has a deadline.
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
    public int LastPrice { get; private set; }
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
        else LastMessage = $"Welcome back! Day {Save.day}.";
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
        LastMessage = "No one else is waiting today. Work on your orders, or end the day when you like.";
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

    public void EndDay() => GM.ChangeScreen(ScreenId.DaySummary);

    public void StartNextDay()
    {
        Save.deliveredToday = 0;
        Save.coinsToday = 0;
        GM.NextDay();
        InviteVisitors();
        GM.Persist();
        LastMessage = $"Day {Save.day}. The shop is open!";
        GM.ChangeScreen(ScreenId.Shop);
    }

    // ---- Order book and workbench ----

    public void SelectOrder(string orderId)
    {
        Selected = Save.orders.Find(o => o.id == orderId);
        if (Selected != null) GM.ChangeScreen(ScreenId.Workbench);
    }

    public void StartCrafting()
    {
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

    public void Deliver()
    {
        var customer = CustomerFor(Selected.customerId);
        string name = customer != null ? customer.displayName : Selected.customerId;
        int before = GM.GetRelationship(Selected.customerId).level;

        GM.AddCoins(LastPrice);
        GM.AddRelationshipPoints(Selected.customerId, LastPoints);
        Save.orders.Remove(Selected);
        Save.deliveredToday++;
        Save.coinsToday += LastPrice;
        GM.Persist();

        int after = GM.GetRelationship(Selected.customerId).level;
        LastMessage = after > before
            ? $"{name} is now a closer friend! (level {after})"
            : $"Delivered to {name}: +{LastPrice} coins, +{LastPoints} friendship.";
        Selected = null;
        GM.ChangeScreen(ScreenId.Shop);
    }
}
