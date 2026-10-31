using UnityEngine;
using UnityEngine.UI;

/// The bar along the top of the shop and the workroom: the weekday (its colour and symbol), the day and
/// chapter, the coins, and the settings gear (pause: the shop waits while Settings is open).
public class TopBar : MonoBehaviour
{
    public ShopDay day;
    public Image weekdayBadge, weekdaySymbol;
    public Text weekdayName, dayText, coinText;
    public Sprite[] symbols = new Sprite[7];   // Monday first

    int shownDay = -1, shownCoins = -1;
    string shownChapter;
    bool shownNight;

    void OnEnable() => shownDay = -1;

    void Update()
    {
        var save = GameManager.Instance != null ? GameManager.Instance.Save : null;
        if (save == null) return;
        string chapter = day != null && day.Progress.Chapter is ChapterDef ch ? $"Chapter {ch.number}: {ch.title}" : "";
        if (save.day == shownDay && save.coins == shownCoins && chapter == shownChapter && save.night == shownNight) return;
        shownDay = save.day; shownCoins = save.coins; shownChapter = chapter; shownNight = save.night;

        int w = Week.Index(save.day);
        weekdayBadge.color = Week.ColorOf(save.day);
        weekdaySymbol.sprite = symbols != null && w < symbols.Length ? symbols[w] : null;
        weekdaySymbol.enabled = weekdaySymbol.sprite != null;
        weekdayName.text = Week.Names[w];
        dayText.text = $"<b>Day {save.day}</b>{(save.night ? "  ·  Night" : "")}" + (chapter.Length > 0 ? $"\n{chapter}" : "");
        coinText.text = save.coins.ToString();
    }

    public void OpenSettings() => GameManager.Instance.ChangeScreen(ScreenId.Settings);
}
