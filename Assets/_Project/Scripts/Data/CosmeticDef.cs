using UnityEngine;

public enum CosmeticSlot { Hair, Top, Accessory }   // append only: assets store these as numbers

// Something from the boutique for the shopkeeper: a hairstyle, a top, or an accessory.
// Hair and tops unlock a style (styleIndex into Appearance.HairStyles / Appearance.Tops);
// accessories are worn on their own (style picks the placeholder shape: 0 glasses, 1 hat, 2 brooch, 3 neckerchief).
// Create via Assets > Create > Craft > Cosmetic.
[CreateAssetMenu(menuName = "Craft/Cosmetic")]
public class CosmeticDef : ScriptableObject
{
    public string cosmeticId;            // permanent once saves exist
    public string displayName;
    public CosmeticSlot slot;
    public int price = 20;
    public int styleIndex;
    public Color color = Color.white;    // accessories: their colour
    public Sprite icon;                  // shown in the boutique (512 x 512); a colour swatch until there's art
}
