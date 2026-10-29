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

// Work done on a grid cell. mask: 1 = "/" diagonal, 2 = "\" diagonal, 3 = full X (or a painted pattern cell).
[Serializable]
public class StitchMark
{
    public int x, y, mask;
}

// A pattern the player owns. Book patterns point at a PatternDef (patternId); patterns the player
// designs carry their own grid in "design". Owned patterns can sit on the shop shelf for sale.
[Serializable]
public class OwnedPattern
{
    public string id;               // PatternDef.patternId for book patterns, a new GUID for designed ones
    public string name;
    public string itemId;           // what it makes
    public int price;
    public bool onShelf;
    public bool designed;           // true = made in the Pattern Studio, grid stored in "design"
    public Pattern design = new Pattern();
}

// Player-owned state, one per save slot. Saved as JSON to Application.persistentDataPath.
// When adding fields: give them safe defaults, bump CurrentVersion and upgrade old saves in SaveSystem.Upgrade.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 3;   // 2: added night. 3: added patterns

    public int version = CurrentVersion;
    public string lastPlayed;                 // ISO 8601, for the save select screen
    public int coins = 50;
    public int day = 1;
    public bool night;                        // false = shop open (day), true = workroom (night)
    public List<string> ownedSupplies = new List<string>();
    public List<string> unlockedItems = new List<string>();
    public List<CustomerRelationship> relationships = new List<CustomerRelationship>();

    public List<Order> orders = new List<Order>();             // the order book: accepted, not yet delivered
    public List<string> visitorsToday = new List<string>();    // customerIds still to arrive today
    public List<OwnedPattern> patterns = new List<OwnedPattern>();  // the book and the shelf
    public int deliveredToday;
    public int coinsToday;
    public int patternsSoldToday;
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
