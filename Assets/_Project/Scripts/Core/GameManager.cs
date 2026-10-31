using System;
using UnityEngine;

public enum ScreenId
{
    Title, Shop, Dialogue, Workbench, Minigame, Result,
    Market, PatternStudio, Journal, Settings, DaySummary,
    SaveSelect, OrderBook, Workroom, PatternEdit, PatternDesigner, Tutorial, Trade, Gather, Customize, Boutique, PatternShop, Stall   // new values go at the end so saved scene references keep their numbers
}

/// Central game state: current screen, save data, coins, relationships.
/// Put one in your first scene. It persists across scene loads.
/// Runs before other scripts so listeners like ScreenRouter find Instance in OnEnable.
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SaveData Save { get; private set; }
    public ScreenId Current { get; private set; } = ScreenId.Title;

    public event Action<ScreenId> OnScreenChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<string, int> OnRelationshipLevelUp;   // customerId, new level

    // Fast Enter Play Mode keeps statics between Play sessions; clear the stale instance.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    /// Save slot in play, or -1 before one is chosen.
    public int Slot { get; private set; } = -1;

    /// The hub for the current time of day: the shop by day, the workroom by night.
    public ScreenId HubScreen => Save == null ? ScreenId.Shop : Save.night ? ScreenId.Workroom
                                 : Week.IsSunday(Save.day) && Save.flags.Contains(ShopDay.StallFlag) ? ScreenId.Stall : ScreenId.Shop;

    /// Raised after a slot is started or loaded, so other systems can rebuild from Save.
    public event Action<bool> OnGameLoaded;   // true for a brand-new game

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGame(int slot)
    {
        Slot = slot;
        Save = SaveSystem.NewGame();
        OnGameLoaded?.Invoke(true);
        Persist();
        ChangeScreen(ScreenId.Customize);   // a new game starts by making the shopkeeper
    }

    /// Loads the slot, or starts a new game there if it is empty.
    /// Returns false (and changes nothing) if the slot has a file that can't be read, so it is never overwritten.
    public bool PlaySlot(int slot)
    {
        if (!SaveSystem.Exists(slot)) { StartNewGame(slot); return true; }
        var data = SaveSystem.Load(slot);
        if (data == null) return false;
        Slot = slot;
        Save = data;
        OnGameLoaded?.Invoke(false);
        Persist();   // refreshes "last played"
        ChangeScreen(Save.player.created ? HubScreen : ScreenId.Customize);   // saves from before customization make theirs now
        return true;
    }

    /// Saves and returns to the title screen.
    public void QuitToTitle()
    {
        Persist();
        Slot = -1;
        Save = null;
        ChangeScreen(ScreenId.Title);
    }

    /// The screen shown before the current one (e.g. so the tutorial can go back to Settings).
    public ScreenId Previous { get; private set; } = ScreenId.Title;

    public void ChangeScreen(ScreenId next)
    {
        if (next != Current) Previous = Current;
        Current = next;
        OnScreenChanged?.Invoke(next);
    }

    // ---- Coins ----
    public void AddCoins(int amount)
    {
        Save.coins += amount;
        OnCoinsChanged?.Invoke(Save.coins);
    }

    public bool TrySpend(int amount)
    {
        if (Save.coins < amount) return false;
        Save.coins -= amount;
        OnCoinsChanged?.Invoke(Save.coins);
        return true;
    }

    // ---- Relationships ----
    public CustomerRelationship GetRelationship(string customerId)
    {
        var r = Save.relationships.Find(x => x.customerId == customerId);
        if (r == null)
        {
            r = new CustomerRelationship { customerId = customerId, level = 0, points = 0 };
            Save.relationships.Add(r);
        }
        return r;
    }

    public void AddRelationshipPoints(string customerId, int points)
    {
        var r = GetRelationship(customerId);
        if (r.AddPoints(points)) OnRelationshipLevelUp?.Invoke(customerId, r.level);
    }

    // ---- Days ----
    public void NextDay() { Save.day++; Persist(); }

    /// Autosave: call after every meaningful change. Does nothing before a slot is chosen.
    public void Persist()
    {
        if (Save != null && Slot >= 0) SaveSystem.Save(Save, Slot);
    }

    void OnApplicationPause(bool paused) { if (paused) Persist(); }   // iPad/Android backgrounding
    void OnApplicationQuit() => Persist();
}
