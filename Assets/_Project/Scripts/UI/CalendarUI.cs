using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// The calendar (owner, 2026-11-01): tap the weekday on the top bar. It shows a season's four weeks (arrows flip through all four), today, the
/// weekends, and the birthdays of everyone you've met.
public class CalendarUI : MonoBehaviour
{
    public ShopDay day;
    public GameObject layer;
    public Text heading, footer;
    public Text[] cells = new Text[Week.SeasonDays];       // Monday of the first week first
    public Image[] cellBacks = new Image[Week.SeasonDays];

    int shown;   // which season is showing (0 spring .. 3 winter); opens on today's

    public void Toggle()
    {
        var save = GameManager.Instance != null ? GameManager.Instance.Save : null;
        if (save == null) return;
        if (layer.activeSelf) { layer.SetActive(false); return; }
        shown = (Week.DayOfYear(save.day) - 1) / Week.SeasonDays;
        Render();
        layer.transform.SetAsLastSibling();
        layer.SetActive(true);
        AudioManager.Play(Sfx.Click);
    }

    /// Flip through the year (owner, 2026-11-03): any season, any time.
    public void Next() { shown = (shown + 1) % 4; Render(); AudioManager.Play(Sfx.Click); }
    public void Previous() { shown = (shown + 3) % 4; Render(); AudioManager.Play(Sfx.Click); }

    void Render()
    {
        var save = GameManager.Instance.Save;
        int yearStart = save.day - (Week.DayOfYear(save.day) - 1);
        int first = yearStart + shown * Week.SeasonDays;   // this year's days for the season shown
        bool thisSeason = save.day >= first && save.day < first + Week.SeasonDays;
        heading.text = Week.SeasonName(first);   // no year number (owner, 2026-11-03)
        var upcoming = new StringBuilder();
        for (int i = 0; i < Week.SeasonDays; i++)
        {
            int d = first + i;
            var who = day.BirthdaysOn(d);
            var sb = new StringBuilder($"<b>{Week.DayOfSeason(d)}</b>");
            foreach (var c in who) sb.Append($"\n<size=56>{c.displayName.Split(' ')[0]}'s birthday</size>");
            if (day.FestivalOn(d) is ShopDay.FestivalDef fest) sb.Append($"\n<size=56><b>{fest.name}</b></size>");
            cells[i].text = sb.ToString();
            bool today = d == save.day, weekend = Week.IsSaturday(d) || Week.IsSunday(d);
            cellBacks[i].color = today ? new Color(1f, 0.85f, 0.45f) : day.FestivalOn(d) != null ? new Color(0.80f, 0.92f, 0.80f) : who.Count > 0 ? new Color(0.98f, 0.80f, 0.85f)
                               : weekend ? new Color(0.90f, 0.88f, 0.95f) : new Color(1f, 0.98f, 0.94f);
            if (today) foreach (var c in who) upcoming.Append($"Today is {c.displayName}'s birthday! ");
        }
        footer.text = upcoming.Length > 0 ? upcoming + "If they come in, wish them a happy one."
                    : thisSeason ? "Birthdays of the people you've met, and festivals, show here. Tap the arrows to see the rest of the year."
                    : "Tap the arrows to flip through the year.";
    }
}
