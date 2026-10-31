using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Moreau's, the boutique across the square (owner, 2026-10-30): hairstyles, tops and accessories for the
/// shopkeeper, run by Celeste Moreau, someone the player can befriend (and romance). Buying something
/// puts it on straight away; tap an owned piece to wear it or take it off. Open by day, from the Journal.
public class BoutiqueUI : MonoBehaviour
{
    public const string OwnerId = "celeste";

    public ShopDay day;
    public Text statusText;
    public RectTransform shelf;
    public Button tileTemplate;          // children Icon, Name, Info
    public ShopkeeperView preview;

    readonly List<GameObject> tiles = new List<GameObject>();
    string message = "";

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
        if (id != ScreenId.Boutique || GameManager.Instance.Save == null) return;
        message = "";
        Show();
    }

    public void Chat() => day.ChatWith(OwnerId, ScreenId.Boutique);

    public void Tap(string cosmeticId)
    {
        var c = System.Array.Find(day.content.cosmetics, x => x != null && x.cosmeticId == cosmeticId);
        if (c == null) return;
        if (!P.owned.Contains(c.cosmeticId))
        {
            if (!GameManager.Instance.TrySpend(c.price)) { message = $"The {c.displayName.ToLower()} is {c.price} coins. Not today."; Show(); return; }
            P.owned.Add(c.cosmeticId);
            GameManager.Instance.AddRelationshipPoints(OwnerId, 3);   // a good customer is a friend
            message = $"Bought the {c.displayName.ToLower()}!";
            Wear(c, true);
        }
        else
        {
            bool wearing = Wearing(c);
            Wear(c, !wearing);
            message = wearing ? $"Took off the {c.displayName.ToLower()}." : $"Wearing the {c.displayName.ToLower()}.";
        }
        GameManager.Instance.Persist();
        Show();
    }

    bool Wearing(CosmeticDef c) => c.slot switch
    {
        CosmeticSlot.Hair => P.hairStyle == c.styleIndex,
        CosmeticSlot.Top => P.top == c.styleIndex,
        _ => P.wornAccessory == c.cosmeticId,
    };

    void Wear(CosmeticDef c, bool on)
    {
        switch (c.slot)
        {
            case CosmeticSlot.Hair: P.hairStyle = on ? c.styleIndex : 0; break;
            case CosmeticSlot.Top: P.top = on ? c.styleIndex : 0; break;
            default: P.wornAccessory = on ? c.cosmeticId : ""; break;
        }
    }

    void Show()
    {
        var owner = day.content.Customer(OwnerId);
        var rel = GameManager.Instance.GetRelationship(OwnerId);
        statusText.text = $"{GameManager.Instance.Save.coins} coins.  {(owner != null ? owner.displayName : "The owner")} · friendship level {rel.level}" +
                          (rel.romance ? " ♥" : "") + (string.IsNullOrEmpty(message) ? "" : $"\n{message}");
        preview.Apply(P);
        foreach (var t in tiles) Destroy(t);
        tiles.Clear();
        tileTemplate.gameObject.SetActive(false);
        foreach (var c in day.content.cosmetics)
        {
            if (c == null) continue;
            string id = c.cosmeticId;
            bool owned = P.owned.Contains(id);
            var tile = Instantiate(tileTemplate, shelf);
            tile.gameObject.SetActive(true);
            tile.name = $"Cosmetic_{id}";
            tile.onClick.AddListener(() => Tap(id));
            tile.GetComponent<Image>().color = owned && Wearing(c) ? new Color(0.75f, 0.92f, 0.75f) : new Color(0.98f, 0.95f, 0.88f);
            var icon = tile.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = c.icon;
            icon.color = c.icon != null ? Color.white : c.color;
            tile.transform.Find("Name").GetComponent<Text>().text = c.displayName;
            tile.transform.Find("Info").GetComponent<Text>().text = !owned ? $"{c.price} coins" : Wearing(c) ? "Wearing it" : "Yours: tap to wear";
            tiles.Add(tile.gameObject);
        }
    }
}
