using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// The Sunday market stall (ShopDayWeekend): the clock, what's on the table, and the shopper in front of you
/// with what they'd like and their price. Sell, ask for more, or keep it; Pack Up ends the day.
public class StallUI : MonoBehaviour
{
    public ShopDay day;
    public Text clockText, tableText, shopperText, messageText;
    public GameObject shopperPanel, sellButton, haggleButton;
    public Text declineLabel;
    public ShopkeeperView keeper;

    ShopDay.Shopper shown;
    int shownStall = -1, shownClock = -1;
    string shownMessage;

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
        if (id != ScreenId.Stall || GameManager.Instance.Save == null) return;
        if (keeper != null) keeper.Apply(GameManager.Instance.Save.player);
        shown = null; shownStall = -1; shownClock = -1;
        Show();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.Current != ScreenId.Stall) return;
        day.StallTick();
        var save = GameManager.Instance.Save;
        if (day.CurrentShopper != shown || save.stall.Count != shownStall || save.clock != shownClock || day.LastMessage != shownMessage) Show();
    }

    public void Sell() { day.StallSell(); Show(); }
    public void Haggle() { day.StallHaggle(); shown = null; Show(); }
    public void Decline() { day.StallDecline(); Show(); }
    public void PackUp() => day.PackUpStall();

    void Show()
    {
        var save = GameManager.Instance.Save;
        if (save == null) return;
        shown = day.CurrentShopper; shownStall = save.stall.Count; shownClock = save.clock; shownMessage = day.LastMessage;
        bool over = save.clock >= ShopDay.StallCloses;
        clockText.text = over ? "Four o'clock: the market's winding down. Pack up when you're ready."
                              : $"Sunday market  ·  {ShopDay.TimeText(save.clock)}  ·  open till 4 pm";
        var counts = new Dictionary<string, int>();
        foreach (var s in save.stall) { counts.TryGetValue(s.name, out int n); counts[s.name] = n + 1; }
        var lines = new List<string>();
        foreach (var kv in counts) lines.Add(kv.Value > 1 ? $"{kv.Key} x{kv.Value}" : kv.Key);
        tableText.text = "<b>On your table</b>\n" + (lines.Count == 0 ? "No handmade pieces yet (make some for next Sunday). Copies of your patterns are on offer."
                                                                       : string.Join("\n", lines) + "\nand copies of your patterns.");
        var sh = day.CurrentShopper;
        shopperPanel.SetActive(sh != null);
        if (sh != null)
        {
            bool buying = sh.piece != null || sh.pattern != null;
            shopperText.text = $"\"{sh.line}\"";
            sellButton.SetActive(buying);
            haggleButton.SetActive(buying && !sh.haggled);
            declineLabel.text = buying ? "Keep It" : "Bye!";
        }
        messageText.text = sh == null && !over && string.IsNullOrEmpty(day.LastMessage) ? "Waiting for the next shopper..." : day.LastMessage;
    }
}
