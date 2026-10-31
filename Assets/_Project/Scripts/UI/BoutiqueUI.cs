using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Moreau's, the boutique across the square (owner, 2026-10-30): hairstyles, tops and accessories for the
/// shopkeeper, run by Celeste Moreau, someone the player can befriend (and romance). Tap a piece on the
/// rail to try it on (the mirror shows you in it), then buy it or put it back; tap an owned piece to wear
/// it or take it off. The rail changes every 4 days, like the market (owner, 2026-10-31): a few pieces you
/// don't own yet are in each time (StockPerPeriod), picked per save.
public class BoutiqueUI : MonoBehaviour
{
    public const string OwnerId = "celeste";

    public ShopDay day;
    public Text statusText;
    public RectTransform shelf;
    public Button tileTemplate;          // children Icon, Name, Info
    public ShopkeeperView preview;

    public const int DaysPerRotation = 4, StockPerPeriod = 4;

    public GameObject tryPanel;          // shown while trying something on: Buy it / Put it back
    public Text tryText;

    readonly List<GameObject> tiles = new List<GameObject>();
    string message = "";
    CosmeticDef trying;                  // on in the mirror, not bought yet

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
        trying = null;
        day.Met(OwnerId);   // you've met Celeste once you've been in her shop
        Show();
    }

    public void Chat() => day.ChatWith(OwnerId, ScreenId.Boutique);
    public void Leave() { trying = null; day.LeaveShop(OwnerId); }

    public int DaysUntilRestock => DaysPerRotation - (GameManager.Instance.Save.day - 1) % DaysPerRotation;

    /// What's on the rail this period: everything you own, plus a few pieces you don't (the same all period).
    public List<CosmeticDef> OnRail()
    {
        var save = GameManager.Instance.Save;
        var rail = new List<CosmeticDef>();
        var notOwned = new List<CosmeticDef>();
        foreach (var c in day.content.cosmetics)
            if (c != null) (P.owned.Contains(c.cosmeticId) ? rail : notOwned).Add(c);
        if (save.marketSeed == 0) save.marketSeed = Random.Range(1, int.MaxValue);
        var rng = new System.Random(unchecked(save.marketSeed * 17 + 7 + (save.day - 1) / DaysPerRotation));
        notOwned.Sort((a, b) => string.CompareOrdinal(a.cosmeticId, b.cosmeticId));   // the same order every time
        for (int i = notOwned.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (notOwned[i], notOwned[j]) = (notOwned[j], notOwned[i]); }
        rail.AddRange(notOwned.GetRange(0, Mathf.Min(StockPerPeriod, notOwned.Count)));
        return rail;
    }

    /// Buys what's being tried on.
    public void BuyTried()
    {
        var c = trying;
        if (c == null) return;
        if (!GameManager.Instance.TrySpend(c.price)) { message = $"The {c.displayName.ToLower()} is {c.price} coins. Not today."; Show(); return; }
        P.owned.Add(c.cosmeticId);
        GameManager.Instance.AddRelationshipPoints(OwnerId, 3);   // a good customer is a friend
        day.BoughtInShop();
        message = $"Bought the {c.displayName.ToLower()}! You're wearing it.";
        Wear(c, true);
        trying = null;
        GameManager.Instance.Persist();
        Show();
    }

    /// Takes off what's being tried on (back on the rail).
    public void PutBack()
    {
        if (trying != null) message = $"You put the {trying.displayName.ToLower()} back.";
        trying = null;
        Show();
    }

    public void Tap(string cosmeticId)
    {
        var c = System.Array.Find(day.content.cosmetics, x => x != null && x.cosmeticId == cosmeticId);
        if (c == null) return;
        if (!P.owned.Contains(c.cosmeticId))   // try it on first
        {
            trying = trying == c ? null : c;
            message = trying != null ? "" : $"You put the {c.displayName.ToLower()} back.";
            Show();
            return;
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

    void Wear(CosmeticDef c, bool on) => Wear(P, c, on);

    static void Wear(PlayerProfile p, CosmeticDef c, bool on)
    {
        switch (c.slot)
        {
            case CosmeticSlot.Hair: p.hairStyle = on ? c.styleIndex : 0; break;
            case CosmeticSlot.Top: p.top = on ? c.styleIndex : 0; break;
            default: p.wornAccessory = on ? c.cosmeticId : ""; break;
        }
    }

    /// You as you'd look in the piece being tried on (your own look is untouched until you buy it).
    PlayerProfile Mirror()
    {
        if (trying == null) return P;
        var copy = JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(P));
        Wear(copy, trying, true);
        return copy;
    }

    void Show()
    {
        var owner = day.content.Customer(OwnerId);
        var rel = GameManager.Instance.GetRelationship(OwnerId);
        int restock = DaysUntilRestock;
        statusText.text = $"{(owner != null ? owner.displayName : "The owner")} · friendship level {rel.level}" + (rel.romance ? " ♥" : "") +
                          $"  ·  {(restock == 1 ? "New pieces tomorrow" : $"New pieces in {restock} days")}" +
                          (string.IsNullOrEmpty(message) ? "\nTap something to try it on." : $"\n{message}");
        preview.Apply(Mirror());
        if (tryPanel != null)
        {
            tryPanel.SetActive(trying != null);
            if (trying != null) tryText.text = $"Trying on the {trying.displayName.ToLower()}: {trying.price} coins";
        }
        foreach (var t in tiles) Destroy(t);
        tiles.Clear();
        tileTemplate.gameObject.SetActive(false);
        foreach (var c in OnRail())
        {
            string id = c.cosmeticId;
            bool owned = P.owned.Contains(id);
            var tile = Instantiate(tileTemplate, shelf);
            tile.gameObject.SetActive(true);
            tile.name = $"Cosmetic_{id}";
            tile.onClick.AddListener(() => Tap(id));
            tile.GetComponent<Image>().color = c == trying ? new Color(0.95f, 0.85f, 0.95f)
                : owned && Wearing(c) ? new Color(0.75f, 0.92f, 0.75f) : new Color(0.98f, 0.95f, 0.88f);
            var icon = tile.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = c.icon;
            icon.color = c.icon != null ? Color.white : c.color;
            tile.transform.Find("Name").GetComponent<Text>().text = c.displayName;
            tile.transform.Find("Info").GetComponent<Text>().text = c == trying ? "Trying it on" : !owned ? $"{c.price} coins" : Wearing(c) ? "Wearing it" : "Yours: tap to wear";
            tiles.Add(tile.gameObject);
        }
    }
}
