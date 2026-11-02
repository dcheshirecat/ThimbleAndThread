using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Byte & Bobbin (owner, 2026-11-01: a tech shop late in the game): Ravi Sen sells automations that run parts of the shop
/// for you, each at a cost (ShopDayLate.Automations). Tap one to buy it; once yours, tap to switch it on or off.
public class TechShopUI : MonoBehaviour
{
    public ShopDay day;
    public Text statusText;
    public Button rowTemplate;
    readonly List<GameObject> rows = new List<GameObject>();

    void OnEnable() { if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged += OnScreenChanged; }
    void OnDisable() { if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreenChanged; }

    void OnScreenChanged(ScreenId id)
    {
        if (id != ScreenId.TechShop || GameManager.Instance.Save == null) return;
        day.Met(ShopDay.TechOwnerId);
        Show();
    }

    public void Chat() => day.ChatWith(ShopDay.TechOwnerId, ScreenId.TechShop);
    public void Leave() => day.LeaveShop(ShopDay.TechOwnerId);

    void Show()
    {
        var save = GameManager.Instance.Save;
        statusText.text = "Things that run the shop for you. Each one costs you something too." + (string.IsNullOrEmpty(day.LastMessage) ? "" : $"\n{day.LastMessage}");
        foreach (var r in rows) Destroy(r);
        rows.Clear();
        rowTemplate.gameObject.SetActive(false);
        foreach (var a in ShopDay.Automations)
        {
            string id = a.id;
            bool owned = day.Owns(id), on = day.AutomationOn(id);
            var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"Automation_{id}";
            row.GetComponentInChildren<Text>().text = $"<b>{a.name}</b>  ·  " + (owned ? (on ? "ON (tap to switch off)" : "OFF (tap to switch on)") : $"{a.price} coins") + $"\n{a.what}";
            row.GetComponent<Image>().color = owned ? (on ? new Color(0.75f, 0.92f, 0.75f) : new Color(0.85f, 0.85f, 0.85f)) : new Color(0.98f, 0.95f, 0.88f);
            row.onClick.AddListener(() => day.BuyOrToggleAutomation(id));
            row.interactable = owned || save.coins >= a.price;
            rows.Add(row.gameObject);
        }
    }
}
