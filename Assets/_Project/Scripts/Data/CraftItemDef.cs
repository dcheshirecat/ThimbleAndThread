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

    [Tooltip("How it's made, in order. Leave empty for a single cross stitch step.")]
    public RecipeStep[] steps = new RecipeStep[0];
    public Garment garment;             // knitting: the shape it's knitted in (Square, Scarf, Sweater pieces)

    static readonly RecipeStep[] StitchOnly = { new RecipeStep { kind = StepKind.CrossStitch, label = "Stitch the pattern" } };

    /// The recipe: its steps, or a single cross stitch step when none are set.
    public RecipeStep[] Steps => steps != null && steps.Length > 0 ? steps : StitchOnly;
}
