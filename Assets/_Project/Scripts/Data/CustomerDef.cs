using System;
using UnityEngine;

public enum Expression { Neutral, Happy, Sad, Surprised, Thoughtful }

[Serializable]
public class ExpressionSprite
{
    public Expression expression;
    public Sprite sprite;
}

/// Something a customer likes or dislikes (e.g. Tobias likes wooden buttons). Never said in dialogue: the
/// player finds out by adding that supply to their order as a special touch; the reaction notes it in the
/// Journal's Friends page. It also decides that reaction (instead of chance).
[Serializable]
public class Preference
{
    public string id;                      // permanent (saves store "customerId:id")
    public string text;                    // as the Journal shows it, e.g. "wooden buttons"
    public bool likes = true;
    public SupplyDef supply;               // optional: the special touch this is about
}

[Serializable]
public class OrderOption
{
    public CraftItemDef item;
    public int minRelationshipLevel;   // customer only asks for this once friendship reaches this level
}

// One per customer. Create via Assets > Create > Craft > Customer.
[CreateAssetMenu(menuName = "Craft/Customer")]
public class CustomerDef : ScriptableObject
{
    public string customerId;
    public string displayName;
    [TextArea] public string bio;
    public Sprite fullBody;
    public ExpressionSprite[] portraits;
    public OrderOption[] requestable;
    public int firstChapter = 1;           // starts visiting the shop in this story chapter
    [Range(0f, 1f)] public float likesSurprises = 0.65f;
    public Preference[] preferences = new Preference[0];
    public bool shopOwner;                 // runs a shop the player visits (market, boutique): never comes in as a customer   // chance they're pleased by a special touch they didn't ask for
    public bool romanceable;               // adult characters only: friendship can grow into romance
    public string inkName;                 // the name of this character's knots in the dialogue (e.g. "mara")

    public Sprite GetPortrait(Expression e)
    {
        if (portraits == null || portraits.Length == 0) return null;
        foreach (var p in portraits)
            if (p.expression == e && p.sprite != null) return p.sprite;
        return portraits[0].sprite;
    }
}
