using System.Collections.Generic;
using UnityEngine;

/// The shop window (owner, 2026-11-02 evening): dress it with up to three things, finished pieces waiting for the
/// stall or patterns from your book. A window dressed within the last week draws passers-by in (more visitors), and
/// people who'd want what's on show, or who make that kind of thing themselves, are likelier to be among them.
/// A tired window (not changed for a week) draws no one extra; a Monday reminder says so.
public partial class ShopDay
{
    public const int WindowMax = 3, WindowFreshDays = 7;

    /// What's in the window: "p:<patternId>" or "s:<stall piece id>" (gone pieces drop out).
    public List<string> WindowShown()
    {
        Save.window.RemoveAll(e => ItemInWindow(e) == null);
        return Save.window;
    }

    public CraftItemDef ItemInWindow(string entry)
    {
        if (entry.StartsWith("p:")) { var p = Save.patterns.Find(x => x.id == entry.Substring(2)); return p != null ? ItemFor(p.itemId) : null; }
        if (entry.StartsWith("s:")) { var s = Save.stall.Find(x => x.id == entry.Substring(2)); return s != null ? content.Item(s.itemId) : null; }
        return null;
    }

    public string WindowLabel(string entry)
    {
        if (entry.StartsWith("p:")) { var p = Save.patterns.Find(x => x.id == entry.Substring(2)); return p != null ? $"The {p.name} pattern" : "?"; }
        var s = Save.stall.Find(x => x.id == entry.Substring(2));
        return s != null ? $"Your {s.name}" : "?";
    }

    public bool WindowFresh => Save.window.Count > 0 && Save.day - Save.windowDay < WindowFreshDays;

    /// Put something in the window or take it out; any change counts as dressing it.
    public void ToggleWindow(string entry)
    {
        if (Save.night) return;
        if (!Save.window.Remove(entry))
        {
            if (Save.window.Count >= WindowMax) { LastMessage = $"The window holds {WindowMax} things. Take one out first."; return; }
            Save.window.Add(entry);
        }
        Save.windowDay = Save.day;
        if (Save.window.Count > 0) Stamps.Earn(Save, "window:first", "Window dressing", "Your first shop window.", StampTheme.Shop);
        LastMessage = "";
        GM.Persist();
    }

    /// Extra visitors from a fresh window: one, and one more for a full window.
    public int WindowVisits => WindowFresh ? 1 + (Save.window.Count >= WindowMax ? 1 : 0) : 0;

    /// Whether a customer is drawn in by what's in the window.
    public bool DrawnByWindow(string customerId)
    {
        if (!WindowFresh) return false;
        var c = CustomerFor(customerId);
        if (c == null) return false;
        var wants = OrderGenerator.Wants(c, GM.GetRelationship(customerId).level);
        foreach (var e in WindowShown())
        {
            var item = ItemInWindow(e);
            if (item == null) continue;
            if (wants.Exists(i => i.itemId == item.itemId)) return true;
            if (c.makes != null && System.Array.IndexOf(c.makes, item.craft) >= 0) return true;
        }
        return false;
    }

    /// Monday morning: a nudge if the window's tired.
    void WindowReminder()
    {
        if (Week.Index(Save.day) != 0 || Save.night) return;
        if (Save.window.Count == 0 && Save.day < 8) return;   // not before the second week
        if (!WindowFresh)
            Stamps.Announce("A new week\n<size=68>Your shop window could do with a change. Tap the window by the door to dress it: a fresh window draws people in.</size>");
    }

    /// Options for the window panel: stall pieces first, then patterns you could make.
    public List<string> WindowChoices()
    {
        var list = new List<string>();
        foreach (var s in Save.stall) list.Add("s:" + s.id);
        foreach (var p in Save.patterns)
            if (PatternShown(p) && Progress.CanMake(ItemFor(p.itemId)) && !IsMystery(p)) list.Add("p:" + p.id);
        return list;
    }
}
