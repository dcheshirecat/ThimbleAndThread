using UnityEngine;
using UnityEngine.UI;

/// The town map on Saturdays (owner, 2026-11-01; ShopDayTown.cs). Owner, 2026-11-02: an interactive map of points in town, not
/// a list of squares. The map scrolls up and down; every place is a pin on it (ShopDay.MapSpot) with its name, and the face of
/// someone you know who's there today. Tap a pin for a card about the place (who's there, birthdays, whether it's open), then
/// Go There. The pin for your own shop takes you home. The map picture is a placeholder until the owner draws one.
public class TownMapUI : MonoBehaviour
{
    public ShopDay day;
    public Text status;
    public Button[] places = new Button[0];   // the pins, in ShopDay.Places order; children Name/Text, Face/Img, Count
    public Button homePin;
    public ScrollRect scroll;
    [Header("The card for the pin you tapped")]
    public GameObject card;
    public Text cardTitle, cardBody, goLabel;
    public Button goButton;

    static readonly Color PinPlace = new Color(1f, 1f, 1f), PinShop = new Color(0.98f, 0.80f, 0.40f), PinShut = new Color(0.65f, 0.65f, 0.65f),
                          PinHome = new Color(0.90f, 0.40f, 0.45f), FaceBirthday = new Color(1f, 0.55f, 0.75f), FaceRing = new Color(1f, 0.97f, 0.9f);
    const int Home = -2;
    int selected = -1;

    SaveData Save => GameManager.Instance != null ? GameManager.Instance.Save : null;

    void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged += OnScreen;
        Refresh();
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreen;
    }

    void OnScreen(ScreenId id)
    {
        if (id != ScreenId.TownMap) return;
        CloseCard();
        Refresh();
        if (scroll != null)   // start at the seafront, where your shop is (the map hangs from its bottom edge)
        {
            scroll.StopMovement();
            scroll.content.anchoredPosition = Vector2.zero;
        }
    }

    public void Refresh()
    {
        var save = Save;
        if (save == null || day == null) return;
        int left = ShopDay.OutingsPerSaturday - day.OutingsDone;
        status.text = $"Saturday {day.SaturdayTime}  ·  {left} outing{(left == 1 ? "" : "s")} left. Shops don't take up an outing.";
        for (int i = 0; i < places.Length && i < ShopDay.Places.Length; i++)
        {
            var p = ShopDay.Places[i];
            var pin = places[i];
            var people = day.PeopleAt(p.id);
            bool shut = day.Shut(p) != null;
            pin.GetComponent<Image>().color = shut ? PinShut : p.shop != ScreenId.Title ? PinShop : PinPlace;
            pin.transform.localScale = Vector3.one * (i == selected ? 1.25f : 1f);
            var name = pin.transform.Find("Name/Text").GetComponent<Text>();
            if (name.text != p.name) name.text = p.name;
            var face = pin.transform.Find("Face");
            face.gameObject.SetActive(people.Count > 0);
            if (people.Count > 0)
            {
                var c = people[0];
                var sprite = c.GetPortrait(Expression.Neutral);
                var img = face.Find("Img").GetComponent<Image>();
                img.sprite = sprite;
                img.color = sprite != null ? Color.white : new Color(0.85f, 0.78f, 0.68f);
                face.GetComponent<Image>().color = people.Exists(x => day.IsBirthday(x, save.day)) ? FaceBirthday : FaceRing;
            }
            var count = pin.transform.Find("Count").GetComponent<Text>();
            count.text = people.Count > 1 ? $"+{people.Count - 1}" : "";
        }
        if (homePin != null)
        {
            homePin.GetComponent<Image>().color = PinHome;
            homePin.transform.localScale = Vector3.one * (selected == Home ? 1.25f : 1f);
        }
        if (selected != -1) ShowCard();
    }

    /// A pin was tapped: the card about that place.
    public void Select(int index)
    {
        if (index < 0 || index >= ShopDay.Places.Length) return;
        selected = selected == index ? -1 : index;   // tapping it again closes the card
        if (selected == -1) CloseCard(); else Refresh();
    }

    public void SelectHome()
    {
        selected = selected == Home ? -1 : Home;
        if (selected == -1) CloseCard(); else Refresh();
    }

    void ShowCard()
    {
        card.SetActive(true);
        card.transform.SetAsLastSibling();
        if (selected == Home)
        {
            cardTitle.text = "Thimble & Thread";
            cardBody.text = "Your shop, shut for Saturday. Going home ends your day out.";
            goLabel.text = "Go Home";
            goButton.interactable = true;
            return;
        }
        var p = ShopDay.Places[selected];
        var people = day.PeopleAt(p.id);
        string shut = day.Shut(p);
        string here = people.Count == 0 ? "No one you know is here just now."
            : people.Count == 1 ? $"{people[0].displayName} is here." : $"Here: {string.Join(", ", people.ConvertAll(c => c.displayName))}.";
        foreach (var c in people) if (day.IsBirthday(c, Save.day)) here += $" It's {c.displayName.Split(' ')[0]}'s birthday!";
        if (p.activity != null) here += $"\nYou could: {p.activity.ToLower()}.";
        cardTitle.text = p.name;
        if (p.id == ShopDay.HomesId && shut == null) here += "\nYou can call on friends who are in, and give close friends a present.";
        cardBody.text = $"{p.blurb}\n" + (shut ?? here);
        goLabel.text = p.shop != ScreenId.Title ? "Go Inside" : p.id == ShopDay.HomesId ? "Knock" : "Go There";
        goButton.interactable = shut == null;
    }

    public void CloseCard()
    {
        bool was = selected != -1;
        selected = -1;
        if (card != null) card.SetActive(false);
        if (was) Refresh();
    }

    /// The card's button: off you go (a conversation, the shop, or home).
    public void Go()
    {
        int index = selected;
        CloseCard();
        if (index == Home) day.GoHomeSaturday();
        else GoTo(index);
    }

    public void GoTo(int index) { if (index >= 0 && index < ShopDay.Places.Length) day.GoTo(ShopDay.Places[index].id); }
}
