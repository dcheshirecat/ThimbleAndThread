using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Finch's Pattern Parlour (owner, 2026-10-31), the third shop on the square: Ada Finch sells ready-made
/// patterns to put on the shop's display. Each shows what it makes, how involved it is (stars), how rare,
/// and its price; rarer ones cost more and sell for more. The stock changes every 4 days (ShopDay.PatternsForSale).
public class PatternShopUI : MonoBehaviour
{
    public ShopDay day;
    public Text statusText;
    public Button rowTemplate;           // a pattern for sale: text and a RawImage preview

    readonly List<GameObject> rows = new List<GameObject>();

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
        if (id != ScreenId.PatternShop || GameManager.Instance.Save == null) return;
        day.Met(ShopDay.PatternShopOwnerId);
        Show();
    }

    public void Chat() => day.ChatWithPatternShopOwner();
    public void Leave() => day.LeavePatternShop();

    static string Stars(int n) => $"{n}/5";

    static string RarityName(MarketSlot r) => r == MarketSlot.Rare ? "Rare" : r == MarketSlot.Rotating ? "Uncommon" : "Common";

    void Show()
    {
        var save = GameManager.Instance.Save;
        var owner = day.content.Customer(ShopDay.PatternShopOwnerId);
        var rel = GameManager.Instance.GetRelationship(ShopDay.PatternShopOwnerId);
        int restock = day.PatternShopRestock;
        statusText.text = $"{(owner != null ? owner.displayName : "The owner")} · friendship level {rel.level}{(rel.romance ? " ♥" : "")}  ·  " +
                          (restock == 1 ? "New patterns tomorrow" : $"New patterns in {restock} days") +
                          (string.IsNullOrEmpty(day.LastMessage) ? "\nPatterns go in your book; put them on display to sell." : $"\n{day.LastMessage}");
        foreach (var r in rows)
        {
            var img = r.GetComponentInChildren<RawImage>(true);
            if (img != null && img.texture != null) Destroy(img.texture);
            Destroy(r);
        }
        rows.Clear();
        rowTemplate.gameObject.SetActive(false);
        var stock = day.PatternsForSale();
        if (stock.Count == 0)
        {
            var empty = Row("PatternShop_Empty", "Nothing new for you this time. Ada gets more in every few days.");
            empty.GetComponent<Button>().interactable = false;
            return;
        }
        foreach (var d in stock)
        {
            var pattern = d.ToPattern();
            string id = d.patternId;
            bool afford = save.coins >= d.shopPrice;
            var row = Row($"ForSale_{id}",
                $"<b>{d.displayName}</b>  ·  {d.item.itemName}\n{RarityName(d.rarity)}  ·  complexity {Stars(ShopDay.Complexity(pattern))}  ·  " +
                $"{d.shopPrice} coins (sells for about {d.price})");
            row.GetComponent<Button>().onClick.AddListener(() => day.BuyPattern(id));
            row.GetComponent<Image>().color = d.rarity == MarketSlot.Rare ? new Color(1f, 0.92f, 0.62f)
                : d.rarity == MarketSlot.Rotating ? new Color(0.88f, 0.93f, 1f) : new Color(0.98f, 0.95f, 0.88f);
            row.GetComponent<Button>().interactable = afford;
            var preview = row.GetComponentInChildren<RawImage>(true);
            if (preview != null) { preview.texture = PatternPreview.Make(pattern); preview.gameObject.SetActive(true); }
        }
    }

    GameObject Row(string name, string label)
    {
        var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
        row.gameObject.SetActive(true);
        row.name = name;
        row.GetComponentInChildren<Text>().text = label;
        rows.Add(row.gameObject);
        return row.gameObject;
    }
}
