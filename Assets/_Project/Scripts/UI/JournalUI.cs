using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// The Journal: a storybook that slides in from the side over whatever screen is showing (owner,
/// 2026-10-30: the old hub menu became this side journal). Tabs along the top turn to a page: orders,
/// the story so far, friendships, crafts and skills, the supply cupboard, recent news. Links at the
/// bottom go to the Supply Market (by day), Trade and Settings. Turning a page flips it over at the spine.
public class JournalUI : MonoBehaviour
{
    public enum Page { Orders, Story, Friends, Skills, Supplies, News }

    public ShopDay day;
    public ScreenRouter router;
    public GameObject overlay;           // the whole journal (dim backdrop + book); hidden when closed
    public RectTransform book;           // slides in from the right
    public RectTransform page;           // flips over at its left edge (pivot x = 0)
    public Text pageText;
    public Button rowTemplate;           // an order row on the Orders page
    public Image[] tabs;                 // tab buttons, in Page order (the open one is highlighted)
    public GameObject marketLink;        // only by day

    Page current = Page.Orders;
    bool animating;
    readonly List<GameObject> rows = new List<GameObject>();

    SaveData Save => GameManager.Instance.Save;
    Progression Progress => day.Progress;
    public bool IsOpen => overlay != null && overlay.activeSelf;

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
        if (id == ScreenId.Journal) Open();                  // old links still work
        else if (IsOpen && !animating) overlay.SetActive(false);   // another screen took over
    }

    // ---- Opening, closing, turning pages ----

    public void Open()
    {
        if (Save == null) return;
        overlay.SetActive(true);
        overlay.transform.SetAsLastSibling();
        if (marketLink != null) marketLink.SetActive(!Save.night);
        Show();
        StartCoroutine(Slide(true));
        AudioManager.Play(Sfx.Click);
    }

    public void Close()
    {
        if (!IsOpen || animating) return;
        StartCoroutine(Slide(false));
    }

    public void ShowPage(int p)
    {
        if (animating || (Page)p == current) return;
        StartCoroutine(Flip((Page)p));
    }

    // Links: close the book, then go.
    public void GoToMarket() { overlay.SetActive(false); router.GoToMarket(); }
    public void GoToTrade() { overlay.SetActive(false); router.GoToTrade(); }
    public void GoToSettings() { overlay.SetActive(false); router.GoToSettings(); }

    IEnumerator Slide(bool opening)
    {
        animating = true;
        float width = ((RectTransform)book.parent).rect.width;
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.3f)
        {
            float e = 1 - (1 - t) * (1 - t);   // ease out
            float x = opening ? Mathf.Lerp(width, 0, e) : Mathf.Lerp(0, width, e);
            book.anchoredPosition = new Vector2(x, 0);
            book.localRotation = Quaternion.Euler(0, 0, (opening ? 1 - e : e) * -4f);
            yield return null;
        }
        book.anchoredPosition = opening ? Vector2.zero : new Vector2(width, 0);
        book.localRotation = Quaternion.identity;
        if (!opening) overlay.SetActive(false);
        animating = false;
    }

    IEnumerator Flip(Page to)
    {
        animating = true;
        AudioManager.Play(Sfx.Click);
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.14f)   // the page folds over at the spine
        {
            page.localScale = new Vector3(Mathf.Cos(t * Mathf.PI / 2), 1, 1);
            yield return null;
        }
        current = to;
        Show();
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.18f)   // and the next one opens out
        {
            page.localScale = new Vector3(Mathf.Sin(t * Mathf.PI / 2), 1, 1);
            yield return null;
        }
        page.localScale = Vector3.one;
        animating = false;
    }

    void Show()
    {
        if (Save == null) return;
        for (int i = 0; i < tabs.Length; i++) tabs[i].color = i == (int)current ? new Color(1f, 0.92f, 0.7f) : new Color(0.93f, 0.88f, 0.80f);
        foreach (var r in rows) Destroy(r);
        rows.Clear();
        rowTemplate.gameObject.SetActive(false);
        pageText.text = current switch
        {
            Page.Orders => Orders(),
            Page.Story => Story(),
            Page.Friends => Friends(),
            Page.Skills => Skills(),
            Page.Supplies => Supplies(),
            _ => News(),
        };
    }

    string Orders()
    {
        if (Save.orders.Count == 0) return "<b>Orders</b>\n\nNo orders yet. Customers ring the shop bell during the day.";
        foreach (var order in Save.orders)
        {
            var customer = day.CustomerFor(order.customerId);
            string kind = order.patternOnly ? " (pattern)" : !string.IsNullOrEmpty(order.commissionId) ? " (commission)" : "";
            string state = order.finished ? "ready for pickup" : $"{Mathf.RoundToInt(order.Progress() * 100)}% done";
            string id = order.id;
            var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"JournalOrder_{id}";
            row.GetComponentInChildren<Text>().text = $"{(order.item != null ? order.item.itemName : order.itemId)}{kind} for {(customer != null ? customer.displayName : order.customerId)}  ·  {state}";
            row.onClick.AddListener(() => { overlay.SetActive(false); day.SelectOrder(id); });
            rows.Add(row.gameObject);
        }
        return Save.night ? "<b>Orders</b>\nTap one to work on it." : "<b>Orders</b>\nYou can work on these tonight. Tap one for details.";
    }

    string Story()
    {
        var sb = new StringBuilder();
        var c = Progress.Chapter;
        if (c == null) return "Your story begins...";
        sb.AppendLine($"<b>Chapter {c.number}: {c.title}</b>\n");
        sb.AppendLine(c.intro).AppendLine();
        if (day.content.Chapter(c.number + 1) is ChapterDef next)
        {
            sb.AppendLine("<b>To reach the next chapter</b>");
            sb.AppendLine(c.goalText);
            sb.AppendLine(Progress.GoalProgress(c));
        }
        else sb.AppendLine("The last chapter. The festival is coming...");
        sb.AppendLine($"\nDay {Save.day}. Orders handed over: {Save.totalDelivered}. Patterns sold: {Save.totalPatternsSold}.");
        return sb.ToString();
    }

    string Friends()
    {
        var sb = new StringBuilder();
        foreach (var cust in day.content.customers)
        {
            if (cust == null || cust.firstChapter > Save.chapter) continue;
            var r = GameManager.Instance.GetRelationship(cust.customerId);
            sb.AppendLine($"<b>{cust.displayName}</b>  ·  friendship level {r.level} ({r.points} of {r.PointsForNextLevel()} to the next)");
            if (!string.IsNullOrEmpty(cust.bio)) sb.AppendLine(cust.bio);
            if (r.romance) sb.AppendLine(Save.flags.Contains($"{cust.inkName}_together") ? "♥ Together." : "♥ You're seeing each other.");
            else if (cust.romanceable && r.level < 3) sb.AppendLine("You sense there could be more to this friendship...");
            foreach (var s in day.content.skills)
                if (s != null && s.friend == cust && s.friendLevel > 0)
                    sb.AppendLine($"Can teach you {s.displayName} at friendship level {s.friendLevel}" + (Progress.Knows(s.skillId) ? " (you know it already)." : "."));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    string Skills()
    {
        var sb = new StringBuilder("<b>Crafts</b>\n");
        foreach (CraftType craft in System.Enum.GetValues(typeof(CraftType)))
        {
            var skill = Progress.CraftSkill(craft);
            string name = Progression.CraftName(craft);
            sb.AppendLine(Progress.CraftOpen(craft)
                ? $"{name}: level {Progress.Level(craft)} ({Progress.XpToNext(craft)} experience to the next)"
                : $"{name}: not yet. Comes with {Progress.HowToLearn(skill)}.");
        }
        sb.AppendLine("\n<b>Techniques</b>");
        foreach (var s in day.content.skills)
        {
            if (s == null || s.isCraft) continue;
            sb.AppendLine(Progress.Knows(s.skillId)
                ? $"{s.displayName}: learned. {s.description}"
                : $"{s.displayName}: not yet. Learn it from {Progress.HowToLearn(s)}.");
        }
        return sb.ToString();
    }

    string Supplies()
    {
        var sb = new StringBuilder();
        var groups = new SortedDictionary<string, List<string>>();   // by shelf name, so notions share one
        foreach (var st in Save.stock)
        {
            var s = day.content.Supply(st.supplyId);
            if (s == null || st.amount <= 0) continue;
            if (!groups.TryGetValue(KindName(s.kind), out var list)) groups[KindName(s.kind)] = list = new List<string>();
            list.Add(s.kind == SupplyKind.Tool || s.kind == SupplyKind.Book ? s.displayName : $"{s.displayName}: {st.amount} {s.unit}");
        }
        if (groups.Count == 0) return "Your cupboard is empty. Visit the Supply Market by day.";
        foreach (var kv in groups)
        {
            sb.AppendLine($"<b>{kv.Key}</b>");
            foreach (var line in kv.Value) sb.AppendLine(line);
            sb.AppendLine();
        }
        sb.AppendLine($"Coins: {Save.coins}");
        return sb.ToString();
    }

    string News() => Save.journalNews.Count == 0 ? "Nothing new yet." : string.Join("\n", Save.journalNews);

    static string KindName(SupplyKind k) => k switch
    {
        SupplyKind.Thread => "Embroidery floss",
        SupplyKind.Stuffing or SupplyKind.Eyes or SupplyKind.Notion => "Notions",
        SupplyKind.Book => "Books",
        SupplyKind.Tool => "Tools",
        _ => k.ToString(),
    };
}
