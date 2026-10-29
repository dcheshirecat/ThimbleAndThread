using System;
using UnityEngine;

public enum Expression { Neutral, Happy, Sad, Surprised, Thoughtful }

[Serializable]
public class ExpressionSprite
{
    public Expression expression;
    public Sprite sprite;
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

    public Sprite GetPortrait(Expression e)
    {
        if (portraits == null || portraits.Length == 0) return null;
        foreach (var p in portraits)
            if (p.expression == e && p.sprite != null) return p.sprite;
        return portraits[0].sprite;
    }
}
