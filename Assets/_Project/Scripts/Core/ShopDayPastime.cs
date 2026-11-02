using System.Collections.Generic;
using UnityEngine;

/// Things to do in the shop between customers (owner, 2026-10-31: the pace between visits was slow). "Pass the Time"
/// on the shop page: put the kettle on (time moves on, toward the next visitor), quick chores (each makes the shop a
/// little more charming for the day), and looking out of the window at the town going by.
public partial class ShopDay
{
    public static readonly (string id, string name, string done)[] Chores =
    {
        ("dust", "Dust the Shelves", "The shelves gleam."),
        ("sweep", "Sweep the Floor", "Not a crumb left."),
        ("tidy", "Tidy the Thread Drawer", "Every skein back in its place."),
    };
    public const int ChoreCharm = 2, ChoreMinutes = 10, KettleMinutes = 30;

    public bool ChoreDone(string id) => Save.flags.Contains($"chore:{id}:{Save.day}");

    /// Charm from today's chores (it fades overnight).
    int ChoreCharmToday()
    {
        int n = 0;
        foreach (var c in Chores) if (ChoreDone(c.id)) n += ChoreCharm;
        return n;
    }

    /// Whether there's time in the day for a pastime: open, before closing, nobody at the door.
    public bool CanPassTime => Save != null && !Save.night && !IsSaturday && !IsSunday && Save.clock < ClosesAt && !BellRinging;

    public void FinishChore(string id)
    {
        if (!CanPassTime || ChoreDone(id)) return;
        var c = System.Array.Find(Chores, x => x.id == id);
        Save.flags.Add($"chore:{id}:{Save.day}");
        PassMinutes(ChoreMinutes);
        Stamps.Notice($"{c.done}\n<size=52>Charm +{ChoreCharm} today.</size>");
        GM.Persist();
    }

    static readonly string[] KettleLines =
    {
        "You put the kettle on and watch the harbour while it boils.",
        "A cup of tea, a biscuit, and a quiet half hour.",
        "You make tea and sort the button tin while it steeps.",
        "The kettle sings. You drink your tea by the window.",
        "Tea, and a few rows of your own knitting under the counter.",
    };

    /// Time moves on: half an hour, or until the next visitor if they're due sooner.
    public void PutKettleOn()
    {
        if (!CanPassTime) return;
        int minutes = KettleMinutes;
        if (Save.nextArrival > Save.clock) minutes = Mathf.Min(minutes, Save.nextArrival - Save.clock);
        PassMinutes(Mathf.Max(5, minutes));
        Stamps.Notice(KettleLines[Random.Range(0, KettleLines.Length)]);
        GM.Persist();
    }

    void PassMinutes(int minutes)
    {
        int before = Save.clock;
        Save.clock = Mathf.Min(ClosesAt, Mathf.Max(Save.clock, OpensAt) + minutes);
        if (before < ClosesAt && Save.clock >= ClosesAt) Stamps.Notice("Six o'clock: closing time.\n<size=52>Flip the sign when you're ready.</size>");
    }

    // ---- The window: the town going by, once an hour ----

    static readonly string[] TownLines =
    {
        "Gulls squabble over a chip on the harbour wall.",
        "A fishing boat chugs in, low in the water. Good catch today.",
        "Two children press their noses to your window, then run off giggling.",
        "The bakery van rattles past. The whole street smells of bread.",
        "A dog in a raincoat drags its owner towards the beach.",
        "Washing flaps on a line between two cottages, every colour of the rainbow.",
        "The tide's going out. The boats lean over on the sand like sleepy cows.",
        "Someone's whistling a sea shanty, badly, somewhere up the lane.",
        "A cat sits in the sunny patch outside your door and refuses to move.",
        "The church clock chimes, a little late as always.",
        "Fog rolls in off the sea, and the lighthouse blinks on early.",
        "A tourist photographs your shop window. You straighten the display.",
    };

    static readonly (string who, string line)[] PeopleLines =
    {
        ("tobias", "Tobias walks past with a bucket of mackerel and lifts his cap to you."),
        ("mara", "Mara pushes the pram past, very slowly, hoping the baby stays asleep."),
        ("juno", "Juno skates by, headphones on, and waves without stopping."),
        ("priya", "Priya leads a line of schoolchildren to the beach, counting heads."),
        ("hal", "Hal stops outside to tell a pigeon something important."),
        ("dot", "Dot taps on your window with a teaspoon and mouths: \"Tea later?\""),
        ("theo", "Theo carries a coil of rope twice his size towards the harbour."),
        ("benji", "Benji wobbles past on a bicycle with a chair strapped to his back."),
        ("gwen", "Gwen marches along the quay with her clipboard, frowning at a boat."),
        ("eli", "Eli props the café door open and waves a cinnamon bun at you."),
    };

    public bool CanLookOutside => CanPassTime && !Save.flags.Contains($"window:{Save.day}:{Save.clock / 60}");

    public void LookOutside()
    {
        if (!CanLookOutside) return;
        Save.flags.Add($"window:{Save.day}:{Save.clock / 60}");
        var lines = new List<string>(TownLines);
        foreach (var (who, line) in PeopleLines) if (Save.met.Contains(who)) { lines.Add(line); lines.Add(line); }   // people you know catch your eye
        Stamps.Notice(lines[Random.Range(0, lines.Count)]);
        PassMinutes(5);
        GM.Persist();
    }
}
