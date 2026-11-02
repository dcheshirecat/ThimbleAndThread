using UnityEngine;
using UnityEngine.UI;

/// The bar along the top of the shop and the workroom: your portrait, level and experience bar (top left; tap it
/// for your look and your shop), the weekday (its colour and symbol), the day, the time of day and the chapter,
/// the coins, the notices bell (owner, round 7: a round icon with a count badge that opens a list of what's happened:
/// new stamps and goals ready to claim), and the settings gear (pause: the shop waits while Settings is open).
public class TopBar : MonoBehaviour
{
    public ShopDay day;
    public Image weekdayBadge, weekdaySymbol;
    public Text weekdayName, dayText, coinText, levelText;
    public Image xpFill;
    public ShopkeeperView portrait;
    public Sprite[] symbols = new Sprite[7];   // Monday first
    public GameObject bellBadge, noticesLayer, goalsButton, skillsLayer;
    public Text skillsText;
    public Text bellCount, noticesText;
    int shownNotices = -1;

    int shownDay = -1, shownCoins = -1, shownClock = -1, shownXp = -1;
    string shownChapter;
    bool shownNight;

    void OnEnable() => shownDay = -1;

    void Update()
    {
        var save = GameManager.Instance != null ? GameManager.Instance.Save : null;
        if (save == null) return;
        int notices = Unseen(save);
        if (notices != shownNotices && bellBadge != null)
        {
            shownNotices = notices;
            bellBadge.SetActive(notices > 0);
            bellCount.text = notices > 9 ? "9+" : notices.ToString();
        }
        string chapter = day != null && day.Progress.Chapter is ChapterDef ch ? $"Chapter {ch.number}: {ch.title}" : "";
        int xp = day != null ? day.Progress.TotalXp : 0;
        if (save.day == shownDay && save.coins == shownCoins && chapter == shownChapter && save.night == shownNight && save.clock == shownClock && xp == shownXp) return;
        shownDay = save.day; shownCoins = save.coins; shownChapter = chapter; shownNight = save.night; shownClock = save.clock; shownXp = xp;

        if (levelText != null && day != null)
        {
            int level = day.Progress.PlayerLevel;
            levelText.text = $"<b>{save.player.name}</b>  ·  Level {level}";
            xpFill.rectTransform.anchorMax = new Vector2(Mathf.Max(0.01f, day.Progress.LevelProgress), 1);
        }

        int w = Week.Index(save.day);
        weekdayBadge.color = Week.ColorOf(save.day);
        weekdaySymbol.sprite = symbols != null && w < symbols.Length ? symbols[w] : null;
        weekdaySymbol.enabled = weekdaySymbol.sprite != null;
        weekdayName.text = Week.Names[w];
        string time = save.night ? "Night" : save.clock >= ShopDay.ClosesAt ? "Closing time" : ShopDay.TimeText(Mathf.Max(ShopDay.OpensAt, save.clock));
        dayText.text = $"<b>Day {save.day}  ·  {time}</b>  ·  {ShopDay.WeatherWord(day.Today)}";   // the day's weather (owner, 2026-11-02)   // the chapter is in the Journal's Story page (owner, 2026-11-01)
        coinText.text = save.coins.ToString();
    }

    public void OpenSettings() => GameManager.Instance.ChangeScreen(ScreenId.Settings);

    /// New stamps since the list was last looked at, and goals reached but not claimed.
    static int Unseen(SaveData save) => Mathf.Max(0, save.stamps.Count - save.noticesSeen) + save.questsReady.Count;

    /// The bell: opens (or closes) the list of what's happened. Opening it marks the stamps as seen.
    public void ToggleNotices()
    {
        var save = GameManager.Instance.Save;
        if (noticesLayer == null || save == null) return;
        if (noticesLayer.activeSelf) { noticesLayer.SetActive(false); return; }
        var sb = new System.Text.StringBuilder();
        if (save.questsReady.Count > 0)
            sb.Append(save.questsReady.Count == 1 ? "<b>A goal is ready to claim</b> in the Journal's Goals page.\n\n" : $"<b>{save.questsReady.Count} goals are ready to claim</b> in the Journal's Goals page.\n\n");
        int newFrom = save.noticesSeen;
        int shown = 0;
        for (int i = save.stamps.Count - 1; i >= 0 && shown < 8; i--, shown++)
        {
            var s = save.stamps[i];
            sb.Append(i >= newFrom ? "<b>NEW</b>  " : "").Append($"<b>{s.title}</b>  (day {s.day})\n");
            if (!string.IsNullOrEmpty(s.detail)) sb.Append(s.detail).Append("\n");
            sb.Append("\n");
        }
        if (sb.Length == 0) sb.Append("Nothing new yet. Stamps and goals you reach will show up here.");
        noticesText.text = sb.ToString().TrimEnd();
        if (goalsButton != null) goalsButton.SetActive(save.questsReady.Count > 0);
        save.noticesSeen = save.stamps.Count;
        GameManager.Instance.Persist();
        noticesLayer.transform.SetAsLastSibling();
        noticesLayer.SetActive(true);
        AudioManager.Play(Sfx.Click);
    }

    /// From the notices list: the Journal, open at its Goals page.
    public void OpenGoals()
    {
        if (noticesLayer != null) noticesLayer.SetActive(false);
        var journal = FindAnyObjectByType<JournalUI>(FindObjectsInactive.Include);
        if (journal != null) journal.OpenAt(JournalUI.Page.Goals);
    }
    public void OpenPersonal() => GameManager.Instance.ChangeScreen(ScreenId.Customize);

    /// The weekday badge: the calendar, with birthdays.
    public void OpenCalendar() { var c = FindAnyObjectByType<CalendarUI>(); if (c != null) c.Toggle(); }

    /// The level and experience bar: how far to the next level, and your crafts and techniques (owner, 2026-11-01).
    public void ToggleSkills()
    {
        if (skillsLayer == null || day == null) return;
        if (skillsLayer.activeSelf) { skillsLayer.SetActive(false); return; }
        skillsText.text = PlayerText.Fill(day.Progress.SkillsReport());
        skillsLayer.transform.SetAsLastSibling();
        skillsLayer.SetActive(true);
        AudioManager.Play(Sfx.Click);
    }
}
