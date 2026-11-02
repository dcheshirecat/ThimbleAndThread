using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Trade with other players, simplified (owner, 2026-11-02): three tabs. Send = fill a parcel (supplies), set a price
/// (Free = a gift) and make one code; your designs share with a tap; codes you've made are listed below. Open = paste a
/// code. Surprise = swap a parcel with a stranger. Codes are text to copy and send any way the player
/// likes (see Core/Trading.cs); six-character ones go through the trading service (Core/TradeServer.cs).
/// Selling supplies happens here, with other players, not at the market (owner, 2026-10-31).
public class TradeUI : MonoBehaviour
{
    public enum Tab { Send, Open, Surprise }

    public ShopDay day;
    public Image[] tabs;                 // tab buttons, in Tab order
    public Text infoText;                // instructions, the parcel, results
    public Button rowTemplate;           // one row of the list
    public InputField codeField;         // the code made, or where a friend's code is pasted
    public Button actionA, actionB;      // labels and jobs change with the tab
    public Text actionALabel, actionBLabel;
    public GameObject priceRow;          // Sell: the asking price, with - and +
    public Text priceLabel;

    Tab tab = Tab.Send;
    Trading trading;
    readonly List<SupplyStock> parcel = new List<SupplyStock>();
    readonly List<GameObject> rows = new List<GameObject>();
    string message;
    int price;                           // 0 = a gift; more = for sale

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
        price = 0;
        message = null;
        ShowTab((int)Tab.Send);
        if (TradeServer.Ready && !busy) StartCoroutine(CollectSales());
    }

    public void ShowTab(int t)
    {
        tab = (Tab)t;
        message = null;
        codeField.text = "";
        codeField.readOnly = tab != Tab.Open;
        if (codeField.placeholder is Text hint) hint.text = tab == Tab.Open ? "A friend's code goes here" : "Your code will appear here";
        Show();
    }

    void Show()
    {
        if (Save == null) return;
        for (int i = 0; i < tabs.Length; i++) tabs[i].color = i == (int)tab ? new Color(1f, 0.92f, 0.7f) : Color.white;
        foreach (var r in rows) Destroy(r);
        rows.Clear();
        rowTemplate.gameObject.SetActive(false);
        if (priceRow != null) priceRow.SetActive(tab == Tab.Send);

        string info;
        string inParcel = parcel.Count == 0 ? "The parcel is empty." : $"<b>In the parcel:</b> {Trade.Describe(parcel)}";
        switch (tab)
        {
            case Tab.Send:
                if (priceLabel != null) priceLabel.text = price > 0 ? $"Price: {price} coins" : "Price: Free (a gift)";
                info = "Tap supplies to put them in the parcel, choose a price (Free makes it a gift), then Make Code and send "
                     + "the code to a friend.\n" + inParcel;
                if (price > 0 && !TradeServer.Ready) info += "\nSelling needs the online trading service: set the price to Free to give it.";
                Cupboard();
                foreach (var p in Save.patterns)   // your designs: a tap shares a copy (you keep yours)
                {
                    if (!p.designed || !day.PatternShown(p)) continue;
                    var item = day.ItemFor(p.itemId);
                    var pattern = p;
                    AddRow($"<b>Share your pattern:</b> {p.name}  ·  {(item != null ? item.itemName : p.itemId)}",
                           () => Share(Trade.SharePattern(pattern), "Pattern code made and copied. Send it to a friend!"));
                }
                if (Save.sentCodes.Count > 0) AddRow("<b>Codes you've made</b> (tap one to copy it again)", () => { });
                foreach (var c in Save.sentCodes)
                {
                    var code = c.code;
                    var shortCode = c.shortCode;
                    string status = c.price > 0 ? (c.paid ? "  ·  paid" : "  ·  waiting for a buyer") : "";
                    AddRow($"Day {c.day}: {c.summary}" + (string.IsNullOrEmpty(shortCode) ? "" : $"  ·  {shortCode}") + status,
                           () => ShowCode(string.IsNullOrEmpty(shortCode) ? code : shortCode, "Copied."));
                }
                SetActions("Make Code", parcel.Count > 0 ? "Empty Parcel" : null);
                break;
            case Tab.Surprise:
                info = "Surprise Trade: send a parcel off to the trading post, and get back whatever another player sent. "
                     + "You won't know what it is until it arrives!\n" + inParcel
                     + (Save.surpriseOwed > 0 ? $"\n<b>{Save.surpriseOwed} surprise{(Save.surpriseOwed == 1 ? "" : "s")} still to come:</b> check the trading post." : "");
                Cupboard();
                SetActions("Send It Off", Save.surpriseOwed > 0 ? "Check the Post" : "Empty Parcel");
                break;
            default:
                info = TradeServer.Ready ? "Type or paste the code someone sent you, then open it. Supplies for sale are paid for when you open them."
                                         : "Paste a code a friend sent you, then open it.";
                SetActions("Paste", "Open It");
                break;
        }
        infoText.text = message != null ? $"{message}\n\n{info}" : info;
    }

    /// The cupboard as rows: tap to add a pack to the parcel.
    void Cupboard()
    {
        foreach (var st in Save.stock)
        {
            var s = day.content.Supply(st.supplyId);
            if (!Trade.CanGive(s) || st.amount <= 0) continue;
            int n = InParcel(s.supplyId);
            string id = s.supplyId;
            int keep = day.Reserved(id);
            AddRow($"{s.displayName}: you have {st.amount} {s.unit}" + (keep > 0 ? $" ({keep} for your orders)" : "") + (n > 0 ? $"  ·  {n} in the parcel" : ""), () => AddToParcel(id));
        }
    }

    /// A fair asking price: what the parcel would cost at the market.
    int Suggested()
    {
        int coins = 0;
        foreach (var p in parcel)
            if (day.content.Supply(p.supplyId) is SupplyDef s) coins += Mathf.CeilToInt(s.price * p.amount / (float)Mathf.Max(1, s.perPack));
        return Mathf.Max(1, coins);
    }

    public void PriceUp() { price = price == 0 ? Suggested() : Mathf.Min(9999, price + (price >= 50 ? 5 : 1)); Show(); }
    public void PriceDown() { price = Mathf.Max(0, price - (price > 50 ? 5 : 1)); Show(); }   // down to 0 = Free

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
        if (busy) return;
        switch (tab)
        {
            case Tab.Send when price == 0:
            {
                string code = Trade.MakeSupplyGift(parcel, out string error);
                if (code == null) { message = error; Show(); return; }
                parcel.Clear();
                Share(code, "Gift code made and copied. Send it to a friend!");
                break;
            }
            case Tab.Send:
            {
                if (!TradeServer.Ready) { message = "Selling needs the online trading service."; Show(); return; }
                string code = Trade.MakeSupplyGift(parcel, out string error, price);
                if (code == null) { message = error; Show(); return; }
                parcel.Clear();
                busy = true;
                message = "Putting it up for sale...";
                Show();
                StartCoroutine(TradeServer.Upload(code, (shortCode, err) =>
                {
                    busy = false;
                    price = 0;
                    if (shortCode == null) { Trade.TakeBack(code); message = $"{err} Your supplies are back in the cupboard."; Show(); return; }
                    Trade.SetShortCode(code, shortCode);
                    ShowCode(shortCode, $"For sale! The code is {shortCode} (copied): send it to the buyer. You'll get the coins once they open it.");
                }));
                break;
            }
            case Tab.Surprise:
            {
                if (!TradeServer.Ready) { message = "Surprise Trades need the online trading service."; Show(); return; }
                string code = Trade.MakeSupplyGift(parcel, out string error, 0, "Surprise trade");
                if (code == null) { message = error; Show(); return; }
                parcel.Clear();
                StartCoroutine(SurpriseTrade(code));
                break;
            }
            default:
                codeField.text = GUIUtility.systemCopyBuffer;
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
        if (busy) return;
        switch (tab)
        {
            case Tab.Send:
                parcel.Clear();
                price = 0;
                message = null;
                Show();
                break;
            case Tab.Surprise:
                if (Save.surpriseOwed > 0) { StartCoroutine(CheckPost()); return; }
                parcel.Clear();
                message = null;
                Show();
                break;
            case Tab.Open:
                var shortCode = TradeServer.Clean(codeField.text);
                if (shortCode != null && TradeServer.Ready)
                {
                    busy = true;
                    message = "Opening it...";
                    Show();
                    StartCoroutine(TradeServer.Claim(shortCode, Trade.CheckRedeem, (longCode, err) =>
                    {
                        busy = false;
                        if (longCode == null) { message = err; Show(); return; }
                        Open(longCode);
                    }, Trade.ReceiptFor, Trading.ReceiptCode(shortCode)));
                    return;
                }
                Open(codeField.text);
                break;
        }
    }

    // ---- Sales: picking up payments ----

    /// Looks for receipts left by buyers of your sales, and collects the coins.
    IEnumerator CollectSales()
    {
        busy = true;
        int collected = 0;
        foreach (var sale in Save.sentCodes.ToArray())
        {
            if (sale.price <= 0 || sale.paid || string.IsNullOrEmpty(sale.shortCode)) continue;
            string receipt = null, etag = null, err = null;
            string where = Trading.ReceiptCode(sale.shortCode);
            yield return TradeServer.Peek(where, (r, e, x) => { receipt = r; etag = e; err = x; });
            if (err != null) break;   // offline: try again next time
            if (receipt == null) continue;
            string taken = null;
            yield return TradeServer.Take(where, etag, x => taken = x);
            if (taken != null) continue;
            Trade.CollectSale(sale);
            collected += sale.price;
        }
        busy = false;
        if (collected > 0) { message = $"Someone bought your supplies! +{collected} coins."; Show(); }
    }

    // ---- Surprise Trade ----

    /// Takes a parcel someone else sent to the trading post, if there is one. done(what came, or null).
    IEnumerator TakeSurprise(System.Action<string> done)
    {
        var order = new List<int>();
        for (int i = 0; i < TradeServer.SurpriseSlots; i++) order.Insert(Random.Range(0, order.Count + 1), i);
        foreach (int i in order)
        {
            string slot = TradeServer.SurpriseSlot(i), packet = null, etag = null, err = null;
            yield return TradeServer.Peek(slot, (p, e, x) => { packet = p; etag = e; err = x; });
            if (err != null) { done(null); yield break; }
            if (packet == null || Trade.CheckRedeem(packet) != null) continue;   // empty, yours, or one you've had
            string taken = null;
            yield return TradeServer.Take(slot, etag, x => taken = x);
            if (taken != null) continue;
            done(Trade.Redeem(packet, out _));
            yield break;
        }
        done(null);
    }

    /// Sends the parcel to the trading post and takes one back (or one's owed until someone sends one).
    IEnumerator SurpriseTrade(string code)
    {
        busy = true;
        message = "Off to the trading post...";
        Show();
        string got = null;
        yield return TakeSurprise(g => got = g);
        bool sent = false;
        string error = null;
        var order = new List<int>();
        for (int i = 0; i < TradeServer.SurpriseSlots; i++) order.Insert(Random.Range(0, order.Count + 1), i);
        foreach (int i in order)
        {
            string err = null;
            yield return TradeServer.PutAt(TradeServer.SurpriseSlot(i), code, e => err = e);
            if (err == null) { sent = true; break; }
            if (err != "taken") { error = err; break; }
        }
        busy = false;
        if (!sent)
        {
            Trade.TakeBack(code);
            message = (error ?? "The trading post is full just now. Try again later.") + " Your parcel is back in your cupboard.";
        }
        else if (got != null) message = $"Your parcel's on its way to someone. In return, you got {got}!";
        else
        {
            Save.surpriseOwed++;
            GameManager.Instance.Persist();
            message = "Your parcel's at the trading post. Nobody else has sent one yet: check back later for yours.";
        }
        Show();
    }

    /// Picks up surprises still owed from earlier trades.
    IEnumerator CheckPost()
    {
        busy = true;
        message = "Checking the trading post...";
        Show();
        var got = new List<string>();
        while (Save.surpriseOwed > 0)
        {
            string g = null;
            yield return TakeSurprise(x => g = x);
            if (g == null) break;
            got.Add(g);
            Save.surpriseOwed--;
        }
        GameManager.Instance.Persist();
        busy = false;
        message = got.Count > 0 ? $"A surprise was waiting: {string.Join(", ", got)}!" : "Nothing new at the trading post yet. Try again later.";
        Show();
    }
}
