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
    public enum Page { Orders, Story, Friends, Skills, Supplies, News, Stamps, Goals }

    public ShopDay day;
    public ScreenRouter router;
    public GameObject overlay;           // the whole journal (dim backdrop + book); hidden when closed
    public RectTransform book;           // slides in from the right
    public RectTransform page;           // flips over at its left edge (pivot x = 0)
    public Text pageText;
    public Button rowTemplate;           // an order row on the Orders page
    public Image[] tabs;                 // tab buttons, in Page order (the open one is highlighted)
    public GameObject marketLink;        // only by day
    public GameObject boutiqueLink;      // only by day
    public GameObject friendTemplate;    // a friend's card: Name, Bar/Fill, Body
    public RectTransform stampGrid;      // the Stamps page: one stamp per achievement
    public GameObject stampTemplate;     // children Ring (Image), Word, Title, Detail (Text)

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
        if (marketLink != null) marketLink.SetActive(!Save.night && day.ShopOpen(ScreenId.Market));
        if (boutiqueLink != null) boutiqueLink.SetActive(!Save.night && day.ShopOpen(ScreenId.Boutique));
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
    public void GoToMarket() { overlay.SetActive(false); day.EnterShop(ShopDay.MarketOwnerId, ScreenId.Market); }
    public void GoToTrade() { overlay.SetActive(false); router.GoToTrade(); }
    public void GoToBoutique() { overlay.SetActive(false); day.EnterShop(BoutiqueUI.OwnerId, ScreenId.Boutique); }
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
        if (stampGrid != null) stampGrid.gameObject.SetActive(current == Page.Stamps);
        if (friendTemplate != null) friendTemplate.SetActive(false);
        pageText.text = PlayerText.Fill(current switch
        {
            Page.Orders => Orders(),
            Page.Story => Story(),
            Page.Friends => Friends(),
            Page.Skills => Skills(),
            Page.Supplies => Supplies(),
            Page.Stamps => StampsPage(),
            Page.Goals => Goals(),
            _ => News(),
        });
    }

    string Orders()
    {
        if (Save.orders.Count == 0) return "<b>Orders</b>\n\nNo orders yet. Customers ring the shop bell during the day.";
        foreach (var order in Save.orders)
        {
            var customer = day.CustomerFor(order.customerId);
            string kind = order.patternOnly ? " (pattern)" : !string.IsNullOrEmpty(order.commissionId) ? " (commission)" : "";
            string state = order.finished ? "ready for pickup" : $"{Mathf.RoundToInt(order.Progress() * 100)}% done";
            if (!order.finished && day.ShortOf(order).Count > 0) state += ", missing supplies";
            string id = order.id;
            var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"JournalOrder_{id}";
            row.GetComponentInChildren<Text>().text = $"{(order.item != null ? order.item.itemName : order.itemId)}{kind} for {day.NameFor(order.customerId)}  ·  {state}";
            row.onClick.AddListener(() => { overlay.SetActive(false); day.SelectOrder(id); });
            rows.Add(row.gameObject);
        }
        return Save.night ? "<b>Orders</b>\nTap one to work on it (once you have everything for it)." : "<b>Orders</b>\nTap one for details.";
    }

    string Story()
    {
        var sb = new StringBuilder();
        var c = Progress.Chapter;
        if (c == null) return "Your story begins...";
        sb.AppendLine($"<b>Chapter {c.number}: {c.title}</b>\n");
        sb.AppendLine(c.intro).AppendLine();
        // How the story moves on is never spelled out (owner, 2026-10-31).
        if (day.content.Chapter(c.number + 1) == null) sb.AppendLine("The last chapter. The festival is coming...");
        sb.AppendLine($"\nDay {Save.day}. Orders handed over: {Save.totalDelivered}. Patterns sold: {Save.totalPatternsSold}.");
        return sb.ToString();
    }

    /// Only people you've met, and only what they've told you themselves (owner, 2026-10-31: find things
    /// out from their dialogue, not from the journal). Each has a meter towards the next friendship level.
    string Friends()
    {
        int shown = 0;
        foreach (var cust in day.content.customers)
        {
            if (cust == null) continue;
            if (!Save.met.Contains(cust.customerId))
            {
                // Someone around town you haven't met yet (owner, 2026-10-31): ??? and no face until they
                // introduce themselves. Only people who are about by now, so it doesn't hint at what's coming.
                bool about = cust.shopOwner ? (cust.customerId == ShopDay.MarketOwnerId ? day.ShopOpen(ScreenId.Market) : day.ShopOpen(ScreenId.Boutique))
                                            : cust.firstChapter <= Save.chapter;
                if (about) { FriendCard("???", null, null, "You haven't met them yet."); shown++; }
                continue;
            }
            var r = GameManager.Instance.GetRelationship(cust.customerId);
            var sb = new StringBuilder();
            foreach (var n in Save.notes)
                if (n.StartsWith(cust.customerId + "|")) sb.AppendLine("· " + n.Substring(cust.customerId.Length + 1));
            if (sb.Length == 0) sb.AppendLine("You don't know much about them yet.");
            // Likes and dislikes, only the ones found out (by adding that touch to their order).
            var likes = new List<string>();
            var dislikes = new List<string>();
            foreach (var pref in cust.preferences ?? new Preference[0])
                if (pref != null && Save.knownPrefs.Contains($"{cust.customerId}:{pref.id}")) (pref.likes ? likes : dislikes).Add(pref.text);
            if (likes.Count > 0) sb.AppendLine("Likes: " + string.Join(", ", likes));
            if (dislikes.Count > 0) sb.AppendLine("Dislikes: " + string.Join(", ", dislikes));
            if (r.romance) sb.AppendLine(Save.flags.Contains($"{cust.inkName}_together") ? "♥ Together." : "♥ You're seeing each other.");
            foreach (var s in day.content.skills)   // only once it's happened: what they'll teach is a surprise
                if (s != null && s.friend == cust && s.friendLevel > 0 && Progress.Knows(s.skillId) && r.level >= s.friendLevel)
                    sb.AppendLine($"Taught you {s.displayName}.");
            FriendCard(cust.displayName, r, cust.GetPortrait(Expression.Neutral), sb.ToString().TrimEnd());
            shown++;
        }
        return shown == 0 ? "<b>Friends</b>\n\nYou haven't met anyone yet. Customers ring the bell during the day." : "<b>Friends</b>";
    }

    void FriendCard(string name, CustomerRelationship r, Sprite face, string body)
    {
        if (friendTemplate == null) return;
        var card = Instantiate(friendTemplate, friendTemplate.transform.parent);
        card.SetActive(true);
        card.name = $"Friend_{name}";
        var portrait = card.transform.Find("Portrait");
        if (portrait != null)
        {
            var img = portrait.GetComponent<Image>();
            img.sprite = face;
            img.color = face != null ? Color.white : new Color(0.55f, 0.5f, 0.48f);   // a blank frame until you've met
            portrait.Find("Unknown").gameObject.SetActive(face == null);
        }
        if (r == null)   // not met yet
        {
            card.transform.Find("Name").GetComponent<Text>().text = $"<b>{name}</b>";
            card.transform.Find("Bar").gameObject.SetActive(false);
            card.transform.Find("Body").GetComponent<Text>().text = body;
            rows.Add(card);
            return;
        }
        int need = r.PointsForNextLevel();
        float fill = need > 0 ? Mathf.Clamp01(r.points / (float)need) : 1f;
        card.transform.Find("Name").GetComponent<Text>().text = $"<b>{name}</b>   friendship level {r.level}";
        var bar = (RectTransform)card.transform.Find("Bar/Fill");
        bar.anchorMax = new Vector2(fill, 1);
        card.transform.Find("Bar/Label").GetComponent<Text>().text = $"{r.points} / {need} to level {r.level + 1}";
        card.transform.Find("Body").GetComponent<Text>().text = PlayerText.Fill(body);
        rows.Add(card);
    }

    string Skills()
    {
        // Crafts and techniques only appear once learned: which ones are still to come, and how, is a
        // surprise (owner, 2026-10-30 and 2026-10-31).
        var sb = new StringBuilder("<b>Crafts</b>\n");
        int unknown = 0;
        foreach (CraftType craft in System.Enum.GetValues(typeof(CraftType)))
        {
            if (!Progress.CraftOpen(craft)) { unknown++; continue; }
            sb.AppendLine($"{Progression.CraftName(craft)}: level {Progress.Level(craft)} ({Progress.XpToNext(craft)} experience to the next)");
        }
        sb.AppendLine("\n<b>Techniques</b>");
        foreach (var s in day.content.skills)
        {
            if (s == null || s.isCraft) continue;
            if (Progress.Knows(s.skillId)) sb.AppendLine($"{s.displayName}: {s.description}");
            else unknown++;
        }
        if (unknown > 0) sb.AppendLine(unknown == 1 ? "There's one more thing to learn out there..." : $"There are {unknown} more things to learn out there...");
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

    string StampsPage()
    {
        if (stampTemplate == null) return "";
        stampTemplate.SetActive(false);
        for (int i = Save.stamps.Count - 1; i >= 0; i--)   // newest first
        {
            var s = Save.stamps[i];
            var go = Instantiate(stampTemplate, stampGrid);
            go.SetActive(true);
            go.name = $"Stamp_{s.id}";
            go.transform.Find("Ring").GetComponent<Image>().color = Stamps.ColorOf(s.theme);
            go.transform.Find("Ring/Word").GetComponent<Text>().text = Stamps.Word(s.theme);
            go.transform.Find("Title").GetComponent<Text>().text = s.title;
            go.transform.Find("Detail").GetComponent<Text>().text = $"{s.detail}\nDay {s.day}";
            rows.Add(go);
        }
        return Save.stamps.Count == 0 ? "<b>Stamps</b>\n\nNo stamps yet. They come as you learn, make friends and grow the shop."
                                      : $"<b>Stamps</b>  ({Save.stamps.Count})";
    }

    /// Achievement quests: open goals, claimed here for coins and experience.
    string Goals()
    {
        var quests = Progress.OpenQuests();
        foreach (var q in quests)
        {
            bool done = Progress.QuestDone(q);
            var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.name = $"Quest_{q.questId}";
            var reward = new List<string>();
            if (q.coins > 0) reward.Add($"{q.coins} coins");
            if (q.xp > 0) reward.Add($"{q.xp} experience");
            row.GetComponentInChildren<Text>().text = done
                ? $"<b>{q.title}</b>  ·  Claim {string.Join(" + ", reward)}"
                : $"{q.title}  ·  {Mathf.Min(Progress.QuestCount(q), q.target)} / {q.target}";
            row.interactable = done;
            var quest = q;
            row.onClick.AddListener(() => ClaimQuest(quest));
            rows.Add(row.gameObject);
        }
        return quests.Count == 0 ? "<b>Goals</b>\n\nNothing to aim for just now. More goals come as the shop grows."
                                 : "<b>Goals</b>\nReach one, then tap it to claim your reward.";
    }

    void ClaimQuest(QuestDef q)
    {
        string msg = Progress.Claim(q);
        if (msg == null) return;
        AudioManager.Play(Sfx.Coin);
        Stamps.Notice(msg);
        GameManager.Instance.Persist();
        Show();
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
