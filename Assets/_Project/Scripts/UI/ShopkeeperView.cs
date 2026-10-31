using UnityEngine;
using UnityEngine.UI;

/// Draws the player's shopkeeper from their profile: skin, hair (style and colour), top (style and
/// colour) and an accessory. Placeholder shapes until the owner's layered art exists (the plan is one
/// drawing per option and colour, swapped in here).
public class ShopkeeperView : MonoBehaviour
{
    public ContentLibrary content;
    public Image body, head, hair, hairBack, apron, accessory;

    void OnEnable() => Refresh();

    public void Refresh() => Apply(GameManager.Instance != null && GameManager.Instance.Save != null ? GameManager.Instance.Save.player : null);

    public void Apply(PlayerProfile p)
    {
        if (p == null || body == null) return;
        head.color = Appearance.Skin[Mathf.Clamp(p.skin, 0, Appearance.Skin.Length - 1)];
        var hairColor = Appearance.Hair[Mathf.Clamp(p.hairColor, 0, Appearance.Hair.Length - 1)];
        hair.color = hairBack.color = hairColor;
        body.color = Appearance.Cloth[Mathf.Clamp(p.topColor, 0, Appearance.Cloth.Length - 1)];

        // Hair: the cap on top, plus hair falling behind the head for longer styles.
        var hairRt = hair.rectTransform;
        var backRt = hairBack.rectTransform;
        hairBack.enabled = true;
        switch (p.hairStyle)
        {
            case 1:  Size(hairRt, 0.5f, 1.43f, 340, 170); Size(backRt, 0.5f, 1.2f, 360, 300); break;   // bob
            case 2:  Size(hairRt, 0.5f, 1.43f, 330, 160); Size(backRt, 0.5f, 1.05f, 380, 560); break;  // long
            case 3:  Size(hairRt, 0.5f, 1.43f, 320, 150); Size(backRt, 0.5f, 1.62f, 170, 170); break;  // bun
            case 4:  Size(hairRt, 0.5f, 1.45f, 400, 230); Size(backRt, 0.5f, 1.25f, 430, 330); break;  // curly
            default: Size(hairRt, 0.5f, 1.42f, 320, 150); hairBack.enabled = false; break;           // short
        }

        // Tops: a jumper is plain; an apron adds a pale bib; a cardigan a darker opening.
        apron.enabled = p.top != 0;
        apron.color = p.top == 1 ? new Color(0.97f, 0.95f, 0.90f) : Color.Lerp(body.color, Color.black, 0.25f);
        Size(apron.rectTransform, 0.5f, 0.55f, p.top == 1 ? 260 : 90, p.top == 1 ? 420 : 600);

        // Accessory
        var acc = content != null && !string.IsNullOrEmpty(p.wornAccessory) ? System.Array.Find(content.cosmetics, c => c != null && c.cosmeticId == p.wornAccessory) : null;
        accessory.enabled = acc != null;
        if (acc != null)
        {
            accessory.color = acc.color;
            var art = accessory.rectTransform;
            switch (acc.styleIndex)
            {
                case 1: Size(art, 0.5f, 1.55f, 380, 110); break;    // hat
                case 2: Size(art, 0.72f, 0.78f, 80, 80); break;     // brooch
                case 3: Size(art, 0.5f, 0.98f, 300, 70); break;     // neckerchief
                default: Size(art, 0.5f, 1.27f, 230, 50); break;    // glasses
            }
        }
    }

    static void Size(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
}
