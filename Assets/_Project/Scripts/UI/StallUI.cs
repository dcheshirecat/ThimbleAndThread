using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// The Sunday market stall (ShopDayWeekend): the clock, what's on the table, and the shopper in front of you
/// with what they'd like and their price. Sell, ask for more, or keep it; Pack Up ends the day.
public class StallUI : MonoBehaviour
{
    public ShopDay day;
    public Text clockText, tableText, shopperText, messageText;
    public Text goalsText;               // the square market's goals (ShopDayFair); empty at the fair
    public GameObject shopperPanel, sellButton, haggleButton;
    public Text declineLabel;
    public ShopkeeperView keeper;
    public Image shopperFace;            // a friend's portrait, or a stranger's placeholder
    public Text shopperName, shopperInitial;
    public GameObject chatButton, arrangeLayer;
    public Button arrangeTemplate;
    public Text arrangeInfo;
    readonly List<GameObject> arrangeRows = new List<GameObject>();
    Sprite circle;

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
    public void Chat() { day.StallChat(); shown = null; Show(); }

    /// Which patterns are on show at the stall (copies for sale): tap to put one out or take it back.
    public void ToggleArrange()
    {
        bool show = !arrangeLayer.activeSelf;
        arrangeLayer.SetActive(show);
        if (show) { arrangeLayer.transform.SetAsLastSibling(); BuildArrange(); }
    }

    void BuildArrange()
    {
        var save = GameManager.Instance.Save;
        foreach (var r in arrangeRows) Destroy(r);
        arrangeRows.Clear();
        arrangeTemplate.gameObject.SetActive(false);
        arrangeInfo.text = $"Patterns on show: {save.stallPatterns.Count} of {day.StallPatternSpots}. Shoppers buy copies; you always keep one. " +
                           "More copies at Finch's Pattern Parlour.";
        foreach (var p in save.patterns)
        {
            if (!day.PatternShown(p) || day.IsMystery(p)) continue;
            string id = p.id;
            bool on = save.stallPatterns.Contains(id);
            var row = Instantiate(arrangeTemplate, arrangeTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"Arrange_{id}";
            row.GetComponentInChildren<Text>().text = $"{(on ? "On show: " : "")}<b>{p.name}</b>  ·  {ShopDay.CopiesToSell(p)} to sell";
            row.GetComponent<Image>().color = on ? new Color(1f, 0.9f, 0.6f) : Color.white;
            row.onClick.AddListener(() => { day.ToggleStallPattern(id); BuildArrange(); });
            arrangeRows.Add(row.gameObject);
        }
    }

    void Show()
    {
        var save = GameManager.Instance.Save;
        if (save == null) return;
        shown = day.CurrentShopper; shownStall = save.stall.Count; shownClock = save.clock; shownMessage = day.LastMessage;
        bool over = save.clock >= ShopDay.StallCloses;
        clockText.text = over ? "Four o'clock: the market's winding down. Pack up when you're ready."
                              : $"{day.MarketName}  ·  {ShopDay.TimeText(save.clock)}  ·  open till 4 pm";
        var counts = new Dictionary<string, int>();
        foreach (var s in save.stall) { counts.TryGetValue(s.name, out int n); counts[s.name] = n + 1; }
        var lines = new List<string>();
        foreach (var kv in counts) lines.Add(kv.Value > 1 ? $"{kv.Key} x{kv.Value}" : kv.Key);
        var pats = day.StallShown().ConvertAll(x => $"{x.name} ({ShopDay.CopiesToSell(x)})");
        tableText.text = "<b>On your table</b>\n" + (lines.Count == 0 ? "No handmade pieces yet (make some at night: Craft for the Stall)." : string.Join("\n", lines)) +
                         "\n<b>Patterns</b>: " + (pats.Count == 0 ? "none on show (Arrange the Stall)" : string.Join(", ", pats));
        if (goalsText != null)   // the square's goals: meet them all for a pitch at a bigger market
            goalsText.text = day.AtFair ? "" : "<b>Market goals</b> (meet them all, and a bigger market wants you)\n" + string.Join("  ·  ", day.MarketGoals().ConvertAll(g =>
                g.have >= g.need ? $"<color=#2E7D32>{g.name}: done</color>" : $"{g.name}: {g.have}/{g.need}"));
        var sh = day.CurrentShopper;
        shopperPanel.SetActive(sh != null);
        if (sh != null)
        {
            bool buying = sh.piece != null || sh.pattern != null;
            shopperText.text = $"\"{sh.line}\"";
            shopperName.text = sh.name;
            var face = sh.friend != null ? sh.friend.GetPortrait(Expression.Happy) : null;
            if (circle == null) circle = shopperFace.sprite;
            shopperFace.sprite = face != null ? face : circle;   // a stranger: a round placeholder face
            shopperFace.color = face != null ? Color.white : sh.face;
            shopperInitial.text = face != null || string.IsNullOrEmpty(sh.name) ? "" : sh.regular != null ? sh.name.Substring(0, 1) : sh.name.Split(' ')[sh.name.Split(' ').Length - 1].Substring(0, 1).ToUpper();
            chatButton.SetActive(!sh.chatted);
            sellButton.SetActive(buying);
            haggleButton.SetActive(buying && !sh.haggled);
            declineLabel.text = buying ? "Keep It" : "Bye!";
        }
        messageText.text = sh == null && !over && string.IsNullOrEmpty(day.LastMessage) ? "Waiting for the next shopper..." : day.LastMessage;
    }
}
