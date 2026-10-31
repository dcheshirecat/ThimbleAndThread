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
    public List<string> threads = new List<string>();   // supplyId of the thread for each palette colour
    public List<StitchCell> cells = new List<StitchCell>();
    public string fabricId;   // the cloth it's designed on (owner, 2026-10-31); empty = whichever Aida you have
}

/// Something ordered from a shop, on its way (ShopDayOrders): kind "supply" (packs) or "pattern"; it arrives on that morning.
[Serializable]
public class Delivery
{
    public string kind, id;
    public int amount = 1, arrives;
}

/// A piece made for the Sunday market stall, waiting to be sold.
[Serializable]
public class StallItem
{
    public string id, itemId, name;
    public float quality;
}

// How much of a supply the player has, in that supply's units (squares of floss, squares of Aida...).
[Serializable]
public class SupplyStock
{
    public string supplyId;
    public int amount;
}

// How much of a supply a piece of work needs.
[Serializable]
public class MaterialNeed
{
    public string supplyId;
    public int amount;
}

// A tool a piece of work needs: any owned tool of this type that is at least this size.
[Serializable]
public class ToolNeed
{
    public string toolType;
    public int minSize;
    public string label;        // e.g. "Embroidery hoop for 16 x 16 or bigger"
    public bool exactSize;      // the size must match exactly (needles and hooks for a yarn weight)
    public bool matchYarn;      // recipes: the size comes from the chosen yarn's weight
}

// Work done on a grid cell. mask: 1 = "/" diagonal, 2 = "\" diagonal, 3 = full X (or a painted pattern cell).
// color: the thread used, an index into the pattern's palette.
[Serializable]
public class StitchMark
{
    public int x, y, mask;
    public int color;
    public bool crossedWrong;   // the "\" leg went under and "/" on top (fine if the whole piece crosses that way)
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
    public int sold;                // copies of the pattern sold off the shelf
    public int ordered;             // orders taken for the finished item
    public Pattern design = new Pattern();
}

// Experience in one craft; every so much is a new level.
[Serializable]
public class CraftXp
{
    public CraftType craft;
    public int xp;
}

[Serializable]
public class CraftCount
{
    public CraftType craft;
    public int count;
}

// How much of a supply has been bought off the market's shelf, and when it's back if it sold out.
[Serializable]
public class MarketPurchase
{
    public string supplyId;
    public int bought;
    public int soldOutUntil;        // the last day it's sold out (0 = not sold out)
}

// Player-owned state, one per save slot. Saved as JSON to Application.persistentDataPath.
// When adding fields: give them safe defaults, bump CurrentVersion and upgrade old saves in SaveSystem.Upgrade.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 22;   // 2: added night. 3: added patterns. 4: counted stock. 5: recipe steps per order. 6: commissions. 7: display spots, market sell-outs. 8: chapters, skills, upgrades. 9: dialogue flags, romance. 10: trade codes. 11: unannounced pickups (Order.pickupDay), recentLines. 12: player profile, known preferences. 13: knitted garments (sweaters and scarves in progress restart their knitting). 14: things made for yourself. 15: stamps (achievements). 16: things learned about people (notes), who you've met, old knitting kept. 17: cross stitch fabric in pieces. 18: achievement quests, things made per craft, commissions counted. 19: shops open over time (flags shop:market, shop:boutique). 20: the day clock (clock, nextArrival). 21: the Sunday market stall (stall). 22: special orders on their way (deliveries)

    public int version = CurrentVersion;
    public string lastPlayed;                 // ISO 8601, for the save select screen
    public int coins = 50;
    public int day = 1;
    public bool night;                        // false = shop open (day), true = workroom (night)
    public int clock;                         // the time of day in minutes (ShopDay.OpensAt..ClosesAt; 0 = opening time)
    public int nextArrival;                   // when the next visitor reaches the door, in minutes (0 = not on their way yet)
    public List<StallItem> stall = new List<StallItem>();   // pieces made for the Sunday market stall
    public List<Delivery> deliveries = new List<Delivery>();   // special orders on their way
    public List<SupplyStock> stock = new List<SupplyStock>();   // the supply cupboard
    public int marketSeed;                    // picks this save's rotating market stock (0 = not set yet)
    public List<string> ownedSupplies = new List<string>();     // version 3 and older; converted into stock on load
    public List<string> unlockedItems = new List<string>();
    public List<CustomerRelationship> relationships = new List<CustomerRelationship>();

    public List<Order> orders = new List<Order>();             // the order book: accepted, not yet delivered
    public List<string> visitorsToday = new List<string>();    // customerIds still to arrive today
    public List<OwnedPattern> patterns = new List<OwnedPattern>();  // the book and the shelf
    public List<string> commissionsDone = new List<string>();       // delivered commissions, never offered again
    public int displaySpots = 4;                                     // patterns that fit on the shop's display
    public List<MarketPurchase> marketBought = new List<MarketPurchase>();   // packs bought from the market's shelf

    // Progress through the game
    public int chapter = 1;                                          // the story chapter (ChapterDef.number)
    public List<string> skills = new List<string>();                 // learned SkillDef ids
    public List<CraftXp> craftXp = new List<CraftXp>();              // experience per craft
    public List<string> upgrades = new List<string>();               // bought UpgradeDef ids
    public int totalDelivered;                                       // orders handed over, ever
    public int totalPatternsSold;                                    // patterns sold, ever
    public int totalCommissions;                                     // commissions handed over, ever
    public List<CraftCount> itemsMade = new List<CraftCount>();      // things finished, per craft
    public List<string> questsClaimed = new List<string>();          // QuestDef ids whose reward was claimed
    public List<string> questsReady = new List<string>();            // reached and announced, not claimed yet
    public List<string> journalNews = new List<string>();            // recent things learned or unlocked, newest first
    public List<string> flags = new List<string>();                  // set by dialogue (~ flag(x)); "seen:<knot>" for scenes played
    public List<string> pendingScenes = new List<string>();          // story scenes to play next time a hub is on screen
    public PlayerProfile player = new PlayerProfile();               // name, pronouns, look (customization scene)
    public List<string> knownPrefs = new List<string>();
    public List<Stamp> stamps = new List<Stamp>();                   // achievements, stamped in the Journal
    public List<string> notes = new List<string>();                  // "customerId|what you learned" from their dialogue (~ note)
    public List<string> met = new List<string>();                    // customers who've come in (or whose shop you've visited)             // "customerId:preferenceId" the player has been told
    public List<string> recentLines = new List<string>();            // dialogue knots heard lately, newest first (to avoid repeats)
    public List<SentCode> sentCodes = new List<SentCode>();         // share codes this save made, newest first
    public List<string> receivedCodes = new List<string>();          // ids of friends' codes already opened
    [NonSerialized] public bool learnEverything;                     // set when upgrading a save from before skills
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
    public bool romance;            // romanceable characters: you're seeing each other

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

/// A share code this save made (kept so it can be copied again, and so the save never opens its own).
[Serializable]
public class SentCode
{
    public string id;
    public string code;
    public int day;
    public string summary;      // e.g. "Gift: 8 skeins of Pink Floss"
    public string shortCode;    // the six-character online code for it, if one was made (TradeServer)
}
