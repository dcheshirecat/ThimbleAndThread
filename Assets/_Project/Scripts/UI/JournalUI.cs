using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// The Journal: pages for the story so far, friendships, crafts and skills, the supply cupboard,
/// and recent news. Open from either hub; the tabs along the top switch pages.
public class JournalUI : MonoBehaviour
{
    public enum Page { Story, Friends, Skills, Supplies, News }

    public ShopDay day;
    public Text pageText;
    public Image[] tabs;                 // tab buttons, in Page order (the open one is highlighted)

    Page page = Page.Story;

    SaveData Save => GameManager.Instance.Save;
    Progression Progress => day.Progress;

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
        if (id == ScreenId.Journal) Show();
    }

    public void ShowPage(int p)
    {
        page = (Page)p;
        Show();
    }

    void Show()
    {
        if (Save == null) return;
        for (int i = 0; i < tabs.Length; i++) tabs[i].color = i == (int)page ? new Color(1f, 0.92f, 0.7f) : Color.white;
        pageText.text = page switch
        {
            Page.Story => Story(),
            Page.Friends => Friends(),
            Page.Skills => Skills(),
            Page.Supplies => Supplies(),
            _ => News(),
        };
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
