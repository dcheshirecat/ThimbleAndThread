using UnityEngine;

// Defines something that can be made (sweater, plushie, tablecloth...).
// Create via Assets > Create > Craft > Item Definition.
// Lives in its own file: Unity only links a ScriptableObject asset to its class when the file name matches.
[CreateAssetMenu(menuName = "Craft/Item Definition")]
public class CraftItemDef : ScriptableObject
{
    public string itemId;               // stable id used in save files, e.g. "scarf"
    public string itemName;
    public CraftType craft;
    public Sprite icon;                 // your vector art, exported as sprite
    public Pattern requiredPattern;     // optional: pattern the item is built from
    public int basePrice = 20;
    public int patternPrice = 8;        // price when selling just the pattern
    public string[] requiredSupplyIds;  // e.g. "aida_14ct", "wool_yarn_red"
}
