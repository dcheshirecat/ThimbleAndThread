using UnityEngine;

public enum SupplyKind { Other, Fabric, Thread, Yarn, Stuffing, Eyes }

// Cloth, floss, yarn, stuffing, toy eyes... Create via Assets > Create > Craft > Supply.
// Supplies are bought in packs and used up: a pack adds "perPack" units (e.g. a skein of floss
// covers 30 squares, a piece of Aida 400 squares, a bag of stuffing 3 plushies).
[CreateAssetMenu(menuName = "Craft/Supply")]
public class SupplyDef : ScriptableObject
{
    public string supplyId;
    public string displayName;
    public SupplyKind kind;
    public CraftType craft;
    public Sprite icon;
    public Color color = Color.white;      // threads, yarns, fabrics: the colour used in minigames and the designer
    public int price = 10;                 // per pack
    [Range(1, 3)] public int tier = 1;     // higher tier = nicer materials = higher sale price
    public int perPack = 1;                // units one purchase gives
    public string unit = "uses";           // what a unit is: "squares", "stitches", "plushies", "pairs"...
    public int starterPacks;               // packs a new game starts with (the starter kit)
}
