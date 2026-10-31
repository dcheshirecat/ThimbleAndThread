using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Trade with friends: give supplies, share a pattern, open a friend's code, and copy codes made before.
/// Codes are text to copy and send any way the player likes (see Core/Trading.cs).
public class TradeUI : MonoBehaviour
{
    public enum Tab { Give, Patterns, Open, MyCodes }

    public ShopDay day;
    public Image[] tabs;                 // tab buttons, in Tab order
    public Text infoText;                // instructions, the parcel, results
    public Button rowTemplate;           // one row of the list
    public InputField codeField;         // the code made, or where a friend's code is pasted
    public Button actionA, actionB;      // labels and jobs change with the tab
    public Text actionALabel, actionBLabel;

    Tab tab = Tab.Give;
    Trading trading;
    readonly List<SupplyStock> parcel = new List<SupplyStock>();
    readonly List<GameObject> rows = new List<GameObject>();
    string message;

    SaveData Save => GameManager.Instance.Save;
    Trading Trade => trading ??= new Trading(day.content, day.shop, day.Progress);

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
        if (id != ScreenId.Trade) return;
        parcel.Clear();
        message = null;
        ShowTab((int)Tab.Give);
    }

    public void ShowTab(int t)
    {
        tab = (Tab)t;
        message = null;
        codeField.text = "";
        codeField.readOnly = tab != Tab.Open;
        Show();
    }

    void Show()
    {
        if (Save == null) return;
        for (int i = 0; i < tabs.Length; i++) tabs[i].color = i == (int)tab ? new Color(1f, 0.92f, 0.7f) : Color.white;
        foreach (var r in rows) Destroy(r);
        rows.Clear();
        rowTemplate.gameObject.SetActive(false);

        string info;
        switch (tab)
        {
            case Tab.Give:
                info = "Tap a supply to put one pack in the parcel, then make a gift code and send it to a friend. "
                     + "The supplies leave your cupboard when the code is made.\n"
                     + (parcel.Count == 0 ? "The parcel is empty." : $"<b>In the parcel:</b> {Trade.Describe(parcel)}");
                foreach (var st in Save.stock)
                {
                    var s = day.content.Supply(st.supplyId);
                    if (!Trade.CanGive(s) || st.amount <= 0) continue;
                    int inParcel = InParcel(s.supplyId);
                    string id = s.supplyId;
                    AddRow($"{s.displayName}: you have {st.amount} {s.unit}" + (inParcel > 0 ? $"  ·  {inParcel} in the parcel" : ""), () => AddToParcel(id));
                }
                SetActions("Make Gift Code", "Empty Parcel");
                break;
            case Tab.Patterns:
                info = "Tap one of your own designs to make a code for a copy of it. You keep yours.";
                foreach (var p in Save.patterns)
                {
                    if (!p.designed || !day.PatternShown(p)) continue;
                    var item = day.ItemFor(p.itemId);
                    var pattern = p;
                    AddRow($"{p.name}  ·  {(item != null ? item.itemName : p.itemId)}", () => Share(Trade.SharePattern(pattern), "Code made and copied. Send it to a friend!"));
                }
                if (rows.Count == 0) info += "\nYou haven't designed any patterns yet. Design one in the Pattern Studio at night.";
                SetActions("Copy Code", null);
                break;
            case Tab.Open:
                info = TradeServer.Ready ? "Type or paste the six-letter code a friend sent you (or a long code), then open it."
                                         : "Paste a code a friend sent you, then open it.";
                SetActions("Paste", "Open It");
                break;
            default:
                info = "Codes you've made, newest first. Tap one to copy it again.";
                foreach (var c in Save.sentCodes)
                {
                    var code = c.code;
                    var shortCode = c.shortCode;
                    AddRow($"Day {c.day}: {c.summary}" + (string.IsNullOrEmpty(shortCode) ? "" : $"  ·  {shortCode}"),
                           () => ShowCode(string.IsNullOrEmpty(shortCode) ? code : shortCode, "Copied."));
                }
                if (rows.Count == 0) info += "\nNone yet.";
                SetActions("Copy Code", null);
                break;
        }
        infoText.text = message != null ? $"{message}\n\n{info}" : info;
    }

    void SetActions(string a, string b)
    {
        actionALabel.text = a;
        actionB.gameObject.SetActive(b != null);
        if (b != null) actionBLabel.text = b;
    }

    void AddRow(string label, UnityEngine.Events.UnityAction onClick)
    {
        var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
        row.gameObject.SetActive(true);
        row.name = $"TradeRow_{rows.Count}";
        row.GetComponentInChildren<Text>().text = label;
        row.onClick.AddListener(onClick);
        rows.Add(row.gameObject);
    }

    int InParcel(string supplyId)
    {
        var p = parcel.Find(x => x.supplyId == supplyId);
        return p != null ? p.amount : 0;
    }

    public void AddToParcel(string supplyId)
    {
        var s = day.content.Supply(supplyId);
        int left = day.shop.Stock(supplyId) - InParcel(supplyId);
        if (s == null || left <= 0) { message = "That's all you have."; Show(); return; }
        var p = parcel.Find(x => x.supplyId == supplyId);
        if (p == null) parcel.Add(p = new SupplyStock { supplyId = supplyId });
        p.amount += Mathf.Min(Mathf.Max(1, s.perPack), left);
        message = null;
        Show();
    }

    void ShowCode(string code, string msg)
    {
        codeField.text = code;
        Copy(code);
        message = msg;
        Show();
    }

    static void Copy(string code)
    {
        if (!string.IsNullOrEmpty(code)) GUIUtility.systemCopyBuffer = code;
    }

    // ---- The two action buttons ----

    public void ActionA()
    {
        switch (tab)
        {
            case Tab.Give:
                string code = Trade.MakeSupplyGift(parcel, out string error);
                if (code == null) { message = error; Show(); return; }
                parcel.Clear();
                Share(code, "Gift code made and copied. Send it to a friend! (It's also kept under My Codes.)");
                break;
            case Tab.Open:
                codeField.text = GUIUtility.systemCopyBuffer;
                break;
            default:
                if (string.IsNullOrEmpty(codeField.text)) { message = "Tap a row first."; Show(); return; }
                Copy(codeField.text);
                message = "Copied.";
                Show();
                break;
        }
    }

    bool busy;

    void Open(string code)
    {
        string got = Trade.Redeem(code, out string error);
        message = got != null ? $"You received {got}!" : error;
        if (got != null) codeField.text = "";
        Show();
    }

    /// Shows a new code: the six-character one when the online service answers, else the long one.
    void Share(string longCode, string msg)
    {
        if (longCode == null) return;
        if (!TradeServer.Ready || busy) { ShowCode(longCode, msg); return; }
        busy = true;
        message = "Making your code...";
        Show();
        StartCoroutine(TradeServer.Upload(longCode, (shortCode, err) =>
        {
            busy = false;
            if (shortCode == null) { ShowCode(longCode, $"{err} Here's the long code instead; it works just the same."); return; }
            Trade.SetShortCode(longCode, shortCode);
            ShowCode(shortCode, $"Your code is {shortCode}. It's copied: send it to a friend! It works once.");
        }));
    }

    public void ActionB()
    {
        switch (tab)
        {
            case Tab.Give:
                parcel.Clear();
                message = null;
                Show();
                break;
            case Tab.Open:
                var shortCode = TradeServer.Clean(codeField.text);
                if (shortCode != null && TradeServer.Ready)
                {
                    if (busy) return;
                    busy = true;
                    message = "Opening it...";
                    Show();
                    StartCoroutine(TradeServer.Claim(shortCode, Trade.CheckRedeem, (longCode, err) =>
                    {
                        busy = false;
                        if (longCode == null) { message = err; Show(); return; }
                        Open(longCode);
                    }));
                    return;
                }
                Open(codeField.text);
                break;
        }
    }
}
