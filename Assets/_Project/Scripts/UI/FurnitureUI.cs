using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Marsh's Workshop (owner, 2026-10-31: a store for furniture and decorations for your shop). Benji Marsh, the carpenter,
/// makes and sells pieces for each spot in your shop; tap one to buy it. Pieces need a level (DecorDef.minLevel).
public class FurnitureUI : MonoBehaviour
{
    public ShopDay day;
    public Text statusText;
    public RectTransform shelf;
    public Button tileTemplate;          // children Icon, Name, Info

    readonly List<GameObject> tiles = new List<GameObject>();

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
        if (id != ScreenId.Furniture || GameManager.Instance.Save == null) return;
        day.Met(ShopDay.FurnitureOwnerId);
        Show();
    }

    public void Chat() => day.ChatWithFurnitureOwner();
    public void Leave() => day.LeaveFurniture();

    static string SpotName(DecorSlot s) => s switch
    {
        DecorSlot.Wall => "on the wall",
        DecorSlot.Counter => "on the counter",
        DecorSlot.Floor => "on the floor",
        _ => "by the door",
    };

    void Show()
    {
        var save = GameManager.Instance.Save;
        statusText.text = $"Your shop's charm: {day.Charm} (customers pay {day.Charm}% more)" +
                          (string.IsNullOrEmpty(day.LastMessage) ? "\nTap a piece to buy it." : $"\n{day.LastMessage}");
        foreach (var t in tiles) Destroy(t);
        tiles.Clear();
        tileTemplate.gameObject.SetActive(false);
        var list = new List<DecorDef>(day.content.decor);
        list.RemoveAll(d => d == null);
        list.Sort((a, b) => a.minLevel != b.minLevel ? a.minLevel.CompareTo(b.minLevel) : a.price.CompareTo(b.price));
        foreach (var d in list)
        {
            string id = d.decorId;
            bool owned = save.decor.Contains(id), level = day.Progress.PlayerLevel >= d.minLevel;
            var tile = Instantiate(tileTemplate, shelf);
            tile.gameObject.SetActive(true);
            tile.name = $"Decor_{id}";
            tile.onClick.AddListener(() => day.BuyDecor(id));
            tile.GetComponent<Image>().color = owned ? new Color(0.75f, 0.92f, 0.75f) : level ? new Color(0.98f, 0.95f, 0.88f) : new Color(0.7f, 0.68f, 0.66f);
            var icon = tile.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = d.icon;
            icon.color = d.icon != null ? Color.white : d.color;
            if (!level) icon.color *= new Color(0.6f, 0.6f, 0.6f, 0.6f);
            tile.transform.Find("Name").GetComponent<Text>().text = d.displayName;
            tile.transform.Find("Info").GetComponent<Text>().text = owned ? "Yours" : !level ? $"From level {d.minLevel}"
                : $"{d.price} coins  ·  charm +{d.charm}\nGoes {SpotName(d.slot)}";
            tiles.Add(tile.gameObject);
        }
    }
}
