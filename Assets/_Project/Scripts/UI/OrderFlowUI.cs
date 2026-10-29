using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Fills in the text, portraits and order list on the shop screens whenever the screen changes.
public class OrderFlowUI : MonoBehaviour
{
    public ShopDay day;

    [Header("Shop")] public Text shopStatus; public Text greetLabel;
    [Header("Dialogue")] public Text dialogueName, dialogueText; public Image dialoguePortrait;
    [Header("Order book")] public Button orderTemplate; public Text orderBookEmpty;
    [Header("Workbench")] public Text workbenchText; public Text startLabel;
    [Header("Minigame")] public Text minigameHint;
    [Header("Result")] public Text resultText; public Image resultPortrait;
    [Header("Day summary")] public Text summaryText;

    readonly List<GameObject> orderRows = new List<GameObject>();

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

        shopStatus.text = $"Day {save.day}    {save.coins} coins\n" +
                          $"Open orders: {save.orders.Count}\n\n{day.LastMessage}";
        greetLabel.text = day.CustomersWaiting > 0 ? $"Greet Customer ({day.CustomersWaiting})" : "No one waiting";
        summaryText.text = $"Day {save.day} is over.\n\n" +
                           $"Orders delivered: {save.deliveredToday}\nCoins earned: {save.coinsToday}\n" +
                           $"Total coins: {save.coins}\nOrders still open: {save.orders.Count}";

        if (screen == ScreenId.OrderBook) BuildOrderList();

        if (day.Offer != null && day.Visitor != null)
        {
            var offer = day.Offer;
            string item = offer.item.itemName.ToLower();
            dialogueName.text = day.Visitor.displayName;
            dialogueText.text = (offer.patternOnly
                ? $"Hello! Could you design a {item} pattern for me? I'd like to make it myself."
                : $"Hello! Could you make me a {item}?") +
                $"\n\n({char.ToUpper(CraftName(offer.item.craft)[0])}{CraftName(offer.item.craft).Substring(1)}. No rush, whenever you have time.)";
            SetPortrait(dialoguePortrait, day.Visitor.GetPortrait(Expression.Neutral));
        }

        var order = day.Selected;
        if (order == null) return;
        var customer = day.CustomerFor(order.customerId);
        string who = customer != null ? customer.displayName : order.customerId;
        int progress = Mathf.RoundToInt(order.Progress() * 100);

        workbenchText.text = $"{order.item.itemName} for {who}\n\n" +
            (order.patternOnly ? "Pattern only: fill in the squares to design it." : "Finished piece: stitch each square.") +
            $"\nProgress: {progress}%";
        startLabel.text = order.work.Count > 0 ? "Keep Working" : "Start Crafting";

        minigameHint.text = order.patternOnly
            ? "Tap or drag over the faint squares to fill in the pattern."
            : "Drag from one corner of a faint square to the opposite corner, then drag the other diagonal to finish the X.";
        minigameHint.text += "\n\nDone for now keeps your work for later.";
        if (order.item.craft != CraftType.CrossStitch)
            minigameHint.text += $"\n\n(The {CraftName(order.item.craft)} minigame is coming soon. Cross stitch stands in for now.)";

        resultText.text = $"{order.item.itemName} for {who}\n\n" +
                          $"Accuracy: {Mathf.RoundToInt(day.LastAccuracy * 100)}%\n" +
                          $"Stars: {day.LastStars} / 3\n" +
                          $"Price: {day.LastPrice} coins\n" +
                          $"Friendship: +{day.LastPoints}";
        var mood = day.LastStars >= 3 ? Expression.Happy : day.LastStars == 2 ? Expression.Thoughtful : Expression.Sad;
        SetPortrait(resultPortrait, customer != null ? customer.GetPortrait(mood) : null);
    }

    void BuildOrderList()
    {
        foreach (var row in orderRows) Destroy(row);
        orderRows.Clear();
        orderTemplate.gameObject.SetActive(false);
        orderBookEmpty.gameObject.SetActive(day.Orders.Count == 0);

        foreach (var order in day.Orders)
        {
            var row = Instantiate(orderTemplate, orderTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"Order_{order.id}";
            var customer = day.CustomerFor(order.customerId);
            string kind = order.patternOnly ? " (pattern)" : "";
            row.GetComponentInChildren<Text>().text =
                $"{order.item.itemName}{kind} for {(customer != null ? customer.displayName : order.customerId)}  ·  {Mathf.RoundToInt(order.Progress() * 100)}%";
            string id = order.id;
            row.onClick.AddListener(() => day.SelectOrder(id));
            orderRows.Add(row.gameObject);
        }
    }

    static void SetPortrait(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    static string CraftName(CraftType craft) => craft == CraftType.CrossStitch ? "cross stitch" : craft.ToString().ToLower();
}
