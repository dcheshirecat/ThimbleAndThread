using UnityEngine;

// Append new values at the end: assets store these as numbers.
public enum SupplyKind { Other, Fabric, Thread, Yarn, Stuffing, Eyes, Tool, Notion, Book }

// When the Supply Market sells it: always, in the 4-day rotation, only now and then, or never.
public enum MarketSlot { Rotating, Always, Rare, NotSold }

// Cloth, floss, yarn, tools, stuffing, toy eyes... Create via Assets > Create > Craft > Supply.
// Most supplies are bought in packs and used up: a pack adds "perPack" units (e.g. a skein of floss
// covers 30 squares, a piece of Aida 400 squares, a bag of stuffing 3 plushies).
// Tools (kind Tool) are bought once and kept; recipes check you own the right type and size.
// Books (kind Book) are bought once and kept too: each teaches a skill (see SkillDef.book).
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
    public MarketSlot market = MarketSlot.Rotating;

    [Header("Tools and sizes")]
    public string toolType;                // tools: "hoop", "tapestry_needle", "crochet_hook", "knitting_needles", "scissors"...
    public int size;                       // hoops: largest pattern side that fits (squares); hooks, needles, yarn: yarn weight

    [Header("Special touches (optional extras, e.g. lace, buttons, ribbon)")]
    public CraftItemDef[] decorates;       // items this can be added to as a surprise extra (empty = not an extra)
    public int decorateAmount = 1;         // units one extra uses
    [Range(0f, 1f)] public float decorateBonus = 0.25f;   // extra share of the price when the customer likes it

    [HideInInspector] public int generatorVersion;   // placeholder data only: lets the generator update its own supplies
}
