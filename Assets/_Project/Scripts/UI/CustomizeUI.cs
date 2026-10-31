using UnityEngine;
using UnityEngine.UI;

/// The customization scene at the start of a game (and from Settings later): the player's name, the
/// pronouns everyone uses for them, and how their shopkeeper looks. Hair styles and tops beyond the free
/// ones come from the boutique; only owned ones can be picked here.
public class CustomizeUI : MonoBehaviour
{
    public ShopDay day;
    public InputField nameField;
    public Image[] pronounButtons;       // in PlayerText.Sets order
    public Text skinLabel, hairStyleLabel, hairColorLabel, topLabel, topColorLabel, madeLabel, messageText;
    public ShopkeeperView preview;

    PlayerProfile P => GameManager.Instance.Save.player;

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
        if (id != ScreenId.Customize || GameManager.Instance.Save == null) return;
        nameField.text = P.name;
        messageText.text = P.created ? "Change anything you like." : "Welcome! Who's taking over the shop?";
        Show();
    }

    public void SetPronouns(int i) { P.pronouns = Mathf.Clamp(i, 0, PlayerText.Sets.Length - 1); Show(); }

    public void NextSkin() { P.skin = (P.skin + 1) % Appearance.Skin.Length; Show(); }
    public void NextHairColor() { P.hairColor = (P.hairColor + 1) % Appearance.Hair.Length; Show(); }
    public void NextTopColor() { P.topColor = (P.topColor + 1) % Appearance.Cloth.Length; Show(); }
    public void NextHairStyle() { P.hairStyle = NextUnlocked(P.hairStyle, Appearance.HairStyles.Length, Appearance.FreeHairStyles, CosmeticSlot.Hair); Show(); }
    public void NextTop() { P.top = NextUnlocked(P.top, Appearance.Tops.Length, Appearance.FreeTops, CosmeticSlot.Top); Show(); }

    /// Steps through the things the player made for themselves (and wearing none of them).
    public void NextMade()
    {
        var list = P.made;
        int i = list.FindIndex(m => m.id == P.wornMade);
        P.wornMade = i + 1 < list.Count ? list[i + 1].id : "";
        Show();
    }

    int NextUnlocked(int current, int count, int free, CosmeticSlot slot)
    {
        for (int step = 1; step <= count; step++)
        {
            int i = (current + step) % count;
            if (Unlocked(day.content, P, slot, i, free)) return i;
        }
        return current;
    }

    /// Free styles, plus any the player bought at the boutique.
    public static bool Unlocked(ContentLibrary content, PlayerProfile p, CosmeticSlot slot, int style, int free)
    {
        if (style < free) return true;
        foreach (var c in content.cosmetics)
            if (c != null && c.slot == slot && c.styleIndex == style && p.owned.Contains(c.cosmeticId)) return true;
        return false;
    }

    public void Done()
    {
        string name = nameField.text.Trim();
        if (name.Length == 0) { messageText.text = "What should everyone call you?"; return; }
        P.name = name.Length > 20 ? name.Substring(0, 20) : name;
        bool first = !P.created;
        P.created = true;
        GameManager.Instance.Persist();
        GameManager.Instance.ChangeScreen(first ? GameManager.Instance.HubScreen : ScreenId.Settings);
    }

    void Show()
    {
        for (int i = 0; i < pronounButtons.Length; i++)
            pronounButtons[i].color = i == P.pronouns ? new Color(1f, 0.92f, 0.7f) : Color.white;
        skinLabel.text = $"Skin {P.skin + 1}";
        hairStyleLabel.text = Appearance.HairStyles[Mathf.Clamp(P.hairStyle, 0, Appearance.HairStyles.Length - 1)];
        hairColorLabel.text = $"Hair colour {P.hairColor + 1}";
        topLabel.text = Appearance.Tops[Mathf.Clamp(P.top, 0, Appearance.Tops.Length - 1)];
        topColorLabel.text = $"Colour {P.topColor + 1}";
        if (madeLabel != null)
        {
            var made = P.made.Find(m => m.id == P.wornMade);
            madeLabel.text = P.made.Count == 0 ? "Made by you: nothing yet" : made != null ? $"Wearing: {made.name}" : "Made by you: not wearing";
        }
        preview.Apply(P);
    }
}
