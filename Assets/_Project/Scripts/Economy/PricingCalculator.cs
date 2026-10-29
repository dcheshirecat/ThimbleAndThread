using UnityEngine;

public static class PricingCalculator
{
    /// accuracy: 0..1 from the minigame. supplyTier: 1..3. relationshipLevel: 0+.
    public static int Compute(CraftItemDef item, float accuracy, int supplyTier,
                              int relationshipLevel, bool patternOnly)
    {
        if (patternOnly)
        {
            // Patterns sell on design, not stitching quality, but loyal customers pay a bit more.
            return Mathf.RoundToInt(item.patternPrice * (1f + 0.05f * relationshipLevel));
        }

        float quality = 0.5f + 0.5f * Mathf.Clamp01(accuracy);   // sloppy work still sells at half price
        float materials = 1f + 0.15f * (Mathf.Clamp(supplyTier, 1, 3) - 1);
        float loyalty = 1f + 0.05f * relationshipLevel;
        return Mathf.Max(1, Mathf.RoundToInt(item.basePrice * quality * materials * loyalty));
    }

    /// Relationship points awarded for a delivered order.
    /// No deadlines, so there is no lateness penalty.
    public static int RelationshipReward(float accuracy) => Mathf.RoundToInt(10 + 20 * Mathf.Clamp01(accuracy));

    /// 1-3 stars for the result screen.
    public static int Stars(float accuracy) => accuracy >= 0.95f ? 3 : accuracy >= 0.75f ? 2 : 1;
}
