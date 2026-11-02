using UnityEngine;
using UnityEngine.UI;

/// The town map on Saturdays (owner, 2026-11-01; ShopDayTown.cs): each place is a button showing who's there today.
/// The art is a placeholder: the places sit on a simple grid over the sea until the owner's map is drawn.
public class TownMapUI : MonoBehaviour
{
    public ShopDay day;
    public Text status;
    public Button[] places = new Button[0];   // in ShopDay.Places order

    void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged += OnScreen;
        Refresh();
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreen;
    }

    void OnScreen(ScreenId id) { if (id == ScreenId.TownMap) Refresh(); }

    public void Refresh()
    {
        var save = GameManager.Instance != null ? GameManager.Instance.Save : null;
        if (save == null || day == null) return;
        int left = ShopDay.OutingsPerSaturday - day.OutingsDone;
        status.text = $"Saturday {day.SaturdayTime}  ·  {left} outing{(left == 1 ? "" : "s")} left. Shops don't take up an outing.";
        for (int i = 0; i < places.Length && i < ShopDay.Places.Length; i++)
        {
            var p = ShopDay.Places[i];
            var people = day.PeopleAt(p.id);
            string who = people.Count == 0 ? "" : string.Join(", ", people.ConvertAll(c => c.displayName.Split(' ')[0] + (day.IsBirthday(c, save.day) ? " (birthday!)" : "")));
            bool shut = p.shop != ScreenId.Title && !day.ShopOpen(p.shop);
            places[i].GetComponentInChildren<Text>().text = $"<b>{p.name}</b>" + (shut ? "\n<size=56>not open yet</size>" : who.Length > 0 ? $"\n<size=56>{who}</size>" : "");
        }
    }

    public void GoTo(int index) { if (index >= 0 && index < ShopDay.Places.Length) day.GoTo(ShopDay.Places[index].id); }
}
