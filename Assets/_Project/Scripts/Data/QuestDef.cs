using UnityEngine;

/// What an achievement quest counts.
public enum QuestGoal { OrdersDelivered, PatternsSold, ItemsMade, BestFriendship, PeopleMet, PatternsDesigned, Commissions }

// An achievement quest in the Journal's Goals page, e.g. "Sell 5 patterns": reach it, then claim its coins
// and experience (owner, 2026-10-31). Quests are open goals; they never hint at how the story moves on or
// what's still to be unlocked. Create via Assets > Create > Craft > Quest.
[CreateAssetMenu(menuName = "Craft/Quest", fileName = "quest_")]
public class QuestDef : ScriptableObject
{
    public string questId;                 // permanent once saves exist
    public string title;                   // "Sell 5 patterns"
    public QuestGoal goal;
    public int target = 1;
    [Tooltip("ItemsMade only counts things made in this craft, and the quest stays hidden until the craft is learned. " +
             "The experience goes to this craft (otherwise to the craft practised most).")]
    public bool oneCraft;
    public CraftType craft;
    [Tooltip("Shown only once this quest has been claimed (the next tier).")]
    public QuestDef after;
    public int coins;
    public int xp;
}
