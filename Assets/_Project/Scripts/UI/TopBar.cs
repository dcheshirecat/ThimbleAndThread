using UnityEngine;
using UnityEngine.UI;

/// The bar along the top of the shop and the workroom: your portrait, level and experience bar (top left; tap it
/// for your look and your shop), the weekday (its colour and symbol), the day, the time of day and the chapter,
/// the coins, and the settings gear (pause: the shop waits while Settings is open).
public class TopBar : MonoBehaviour
{
    public ShopDay day;
    public Image weekdayBadge, weekdaySymbol;
    public Text weekdayName, dayText, coinText, levelText;
    public Image xpFill;
    public ShopkeeperView portrait;
    public Sprite[] symbols = new Sprite[7];   // Monday first

    int shownDay = -1, shownCoins = -1, shownClock = -1, shownXp = -1;
    string shownChapter;
    bool shownNight;

    void OnEnable() => shownDay = -1;

    void Update()
    {
        var save = GameManager.Instance != null ? GameManager.Instance.Save : null;
        if (save == null) return;
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
        dayText.text = $"<b>Day {save.day}  ·  {time}</b>" + (chapter.Length > 0 ? $"\n{chapter}" : "");
        coinText.text = save.coins.ToString();
    }

    public void OpenSettings() => GameManager.Instance.ChangeScreen(ScreenId.Settings);
    public void OpenPersonal() => GameManager.Instance.ChangeScreen(ScreenId.Customize);
}
