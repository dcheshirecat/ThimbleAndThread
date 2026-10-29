using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

/// Save select screen: one row per slot with a summary, Play (New Game / Continue) and Delete.
/// Delete asks for a second tap instead of a pop-up.
public class SaveSlotsUI : MonoBehaviour
{
    [Serializable]
    public class Row
    {
        public Text summary;
        public Text playLabel;
        public Button deleteButton;
        public Text deleteLabel;
    }

    public Row[] rows = new Row[SaveSystem.SlotCount];

    int confirmDelete = -1;

    void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScreenChanged += OnScreenChanged;
        Refresh();
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreenChanged;
    }

    void OnScreenChanged(ScreenId id)
    {
        if (id != ScreenId.SaveSelect) return;
        confirmDelete = -1;
        Refresh();
    }

    public void Play(int slot)
    {
        confirmDelete = -1;
        if (!GameManager.Instance.PlaySlot(slot))
        {
            rows[slot].summary.text = $"Slot {slot + 1}: this save can't be read. You can delete it to start over.";
        }
    }

    public void Delete(int slot)
    {
        if (confirmDelete != slot)
        {
            confirmDelete = slot;
            Refresh();
            return;
        }
        SaveSystem.Delete(slot);
        confirmDelete = -1;
        Refresh();
    }

    void Refresh()
    {
        for (int slot = 0; slot < rows.Length; slot++)
        {
            var row = rows[slot];
            bool exists = SaveSystem.Exists(slot);
            var data = exists ? SaveSystem.Load(slot) : null;

            if (!exists) row.summary.text = $"Slot {slot + 1}\nEmpty";
            else if (data == null) row.summary.text = $"Slot {slot + 1}\nCan't be read";
            else row.summary.text = $"Slot {slot + 1}\nDay {data.day}  ·  {data.coins} coins  ·  {data.orders.Count} open orders\n" +
                                    $"Last played {LastPlayed(data.lastPlayed)}";

            row.playLabel.text = exists ? "Continue" : "New Game";
            row.deleteButton.interactable = exists;
            row.deleteLabel.text = confirmDelete == slot ? "Tap again to delete" : "Delete";
        }
    }

    static string LastPlayed(string iso) =>
        DateTime.TryParse(iso, null, DateTimeStyles.RoundtripKind, out var t) ? t.ToString("d MMM, HH:mm") : "unknown";
}
