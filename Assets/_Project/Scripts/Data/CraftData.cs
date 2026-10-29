using System;
using System.Collections.Generic;
using UnityEngine;

public enum CraftType { CrossStitch, Knitting, Embroidery, Sewing, Crochet }

// A pattern is just data: what a customer buys on its own.
[Serializable]
public class StitchCell
{
    public int x, y;
    public int colorIndex;      // index into Pattern.palette
    public bool halfStitch;     // true = only one diagonal done
}

[Serializable]
public class Pattern
{
    public string id;
    public string displayName;
    public CraftType craft;
    public int width, height;
    public List<Color> palette = new List<Color>();
    public List<StitchCell> cells = new List<StitchCell>();
}

// Defines something that can be made (sweater, plushie, tablecloth...).
// Create via Assets > Create > Craft > Item Definition.
[CreateAssetMenu(menuName = "Craft/Item Definition")]
public class CraftItemDef : ScriptableObject
{
    public string itemName;
    public CraftType craft;
    public Sprite icon;                 // your vector art, exported as sprite
    public Pattern requiredPattern;     // optional: pattern the item is built from
    public int basePrice = 20;
    public int patternPrice = 8;        // price when selling just the pattern
    public string[] requiredSupplyIds;  // e.g. "aida_14ct", "wool_yarn_red"
}

// Player-owned state. Save/load this as JSON to Application.persistentDataPath.
[Serializable]
public class SaveData
{
    public int coins = 50;
    public int day = 1;
    public List<string> ownedSupplies = new List<string>();
    public List<string> unlockedItems = new List<string>();
    public List<CustomerRelationship> relationships = new List<CustomerRelationship>();
}

[Serializable]
public class CustomerRelationship
{
    public string customerId;
    public int level;
    public int points;

    // Simple curve: each level needs more points than the last.
    public int PointsForNextLevel() => 50 + level * 25;

    public bool AddPoints(int amount)
    {
        points += amount;
        if (points >= PointsForNextLevel())
        {
            points -= PointsForNextLevel();
            level++;
            return true; // leveled up: trigger dialogue/unlocks
        }
        return false;
    }
}
