using UnityEngine;

// Something the player learns: a whole craft (knitting...), or a technique or ability within one
// (French knots, designing patterns...). Crafts let the player take orders for that craft's items;
// a recipe step can need a technique (RecipeStep.requiredSkill).
// A skill is learned by ANY of its routes: a story chapter, a craft level, a book from the market,
// or a friend who teaches it. Create via Assets > Create > Craft > Skill.
// Lives in its own file: Unity only links a ScriptableObject asset to its class when the file name matches.
[CreateAssetMenu(menuName = "Craft/Skill")]
public class SkillDef : ScriptableObject
{
    public string skillId;                 // stable id used in save files, e.g. "french_knots"
    public string displayName;
    [TextArea(2, 5)] public string description;
    public bool isCraft;                   // a whole craft: its items can be ordered and made
    public CraftType craft;                // the craft it belongs to (or is)

    [Header("Learned by any of these")]
    public int chapter;                    // reaching this story chapter (0 = not this way)
    public CraftType levelCraft;           // reaching a level in a craft...
    public int craftLevel;                 // ...this level (0 = not this way)
    public SupplyDef book;                 // buying this book at the Supply Market
    public CustomerDef friend;             // this friend teaches it...
    public int friendLevel;                // ...once your friendship reaches this level

    [Header("When learned")]
    public PatternDef[] grantsPatterns = new PatternDef[0];   // new patterns for the book
}
