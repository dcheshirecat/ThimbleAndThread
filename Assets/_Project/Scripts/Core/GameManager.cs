using System;
using UnityEngine;

public enum ScreenId
{
    Title, Shop, Dialogue, Workbench, Minigame, Result,
    Market, PatternStudio, Journal, Settings, DaySummary
}

/// Central game state: current screen, save data, coins, relationships.
/// Put one in your first scene. It persists across scene loads.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SaveData Save { get; private set; }
    public ScreenId Current { get; private set; } = ScreenId.Title;

    public event Action<ScreenId> OnScreenChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<string, int> OnRelationshipLevelUp;   // customerId, new level

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Save = SaveSystem.Load();
    }

    public void StartNewGame() { Save = SaveSystem.NewGame(); Persist(); ChangeScreen(ScreenId.Shop); }
    public void Continue() { Save = SaveSystem.Load(); ChangeScreen(ScreenId.Shop); }

    public void ChangeScreen(ScreenId next)
    {
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

    public void Persist() => SaveSystem.Save(Save);

    void OnApplicationPause(bool paused) { if (paused && Save != null) Persist(); }   // iPad/Android backgrounding
    void OnApplicationQuit() { if (Save != null) Persist(); }
}
