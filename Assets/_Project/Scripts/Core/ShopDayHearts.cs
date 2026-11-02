using System.Collections.Generic;
using UnityEngine;

/// Romance and birthdays (owner, 2026-11-01).
///  - Romance opens once you're close friends (level RomanceLevel) with someone romanceable. From then on a heart scale
///    sits next to the names of romanceable people you've met, and time with them fills it: ten hearts, each needing a
///    little more than the last. Two hearts: you can ask them on a Saturday date. Five: you're dating. Ten: you move in
///    together, and they help around the shop (a chore done each morning).
///  - Everyone has a birthday on the calendar (tap the weekday on the top bar). If they come in on their birthday, wish
///    them a happy one: extra friendship, and hearts for someone you're seeing. They might not come by, though; on a
///    Saturday you can go and find them.
public partial class ShopDay
{
    public const int RomanceLevel = 3, HeartsToDate = 2, HeartsDating = 5, HeartsMoveIn = 10, MaxHearts = 10;
    public const string RomanceOpenFlag = "romance:open";

    /// Heart points for heart n (1..10): 20, 25, 30... more each time.
    static int HeartCost(int n) => 15 + 5 * n;

    public static int Hearts(CustomerRelationship r)
    {
        int left = r.heartPoints, n = 0;
        while (n < MaxHearts && left >= HeartCost(n + 1)) { left -= HeartCost(n + 1); n++; }
        return n;
    }

    static int PointsForHearts(int hearts)
    {
        int total = 0;
        for (int n = 1; n <= hearts; n++) total += HeartCost(n);
        return total;
    }

    public bool RomanceOpen => Save != null && Save.flags.Contains(RomanceOpenFlag);

    /// Whether this person's heart scale shows: romance has opened, they're romanceable, and you've met.
    public bool ShowsHearts(CustomerDef c) => c != null && c.romanceable && RomanceOpen && Save.met.Contains(c.customerId);

    /// Hearts only fill once you're close friends.
    bool HeartsCanGrow(CustomerDef c) => ShowsHearts(c) && GM.GetRelationship(c.customerId).level >= RomanceLevel;

    public bool CanDate(CustomerDef c) => ShowsHearts(c) && Hearts(GM.GetRelationship(c.customerId)) >= HeartsToDate;

    /// e.g. "♥♥♥" in rose and the rest pale, for a name.
    public string HeartScale(CustomerDef c)
    {
        if (!ShowsHearts(c)) return "";
        int h = Hearts(GM.GetRelationship(c.customerId));
        return $"<color=#D0506A>{new string('♥', h)}</color><color=#C8B8B8>{new string('♥', MaxHearts - h)}</color>";
    }

    /// Every friendship point with someone romanceable you're close to fills the hearts a little too.
    void OnFriendshipPoints(string customerId, int points)
    {
        FriendshipBoost(customerId, points);
        var c = content.Customer(customerId);
        if (c == null || !c.romanceable || points <= 0) return;
        var r = GM.GetRelationship(customerId);
        if (!RomanceOpen && r.level >= RomanceLevel && Save.met.Contains(customerId))
        {
            Save.flags.Add(RomanceOpenFlag);
            Stamps.Earn(Save, RomanceOpenFlag, "Romance", $"Something about {c.displayName} makes your heart skip.", StampTheme.Romance);
            Explainers.Explain(Save, RomanceOpenFlag, false,
                "Romance\nSome of the people you've grown close to have a heart scale by their name now. Time together fills it: " +
                $"{HeartsToDate} hearts and you can ask them on a Saturday date, {HeartsDating} and you're dating, " +
                $"{HeartsMoveIn} and you might just move in together.");
        }
        if (HeartsCanGrow(c)) AddHearts(c, Mathf.Max(1, points / 2));
    }

    /// Adds heart points, with a stamp at each milestone.
    public void AddHearts(CustomerDef c, int points)
    {
        if (!HeartsCanGrow(c)) return;
        var r = GM.GetRelationship(c.customerId);
        int before = Hearts(r);
        r.heartPoints = Mathf.Min(r.heartPoints + points, PointsForHearts(MaxHearts));
        int now = Hearts(r);
        if (now == before) return;
        if (before < HeartsToDate && now >= HeartsToDate)
            Stamps.Earn(Save, $"hearts:{c.customerId}:date", $"{c.displayName}: {HeartsToDate} hearts", "Maybe ask them out on Saturday?", StampTheme.Romance);
        if (before < HeartsDating && now >= HeartsDating)
        {
            r.romance = true;   // you're seeing each other
            Stamps.Earn(Save, $"romance:{c.customerId}", $"You and {c.displayName}", "You're dating.", StampTheme.Romance);
        }
        if (before < HeartsMoveIn && now >= HeartsMoveIn && Partner == null)
        {
            Save.flags.Add($"together:{c.customerId}");
            Stamps.Earn(Save, $"together:{c.customerId}", $"Moving in with {c.displayName}", $"{c.displayName} moves in, and helps around the shop.", StampTheme.Romance);
        }
    }

    /// Who you live with, if anyone.
    public CustomerDef Partner
    {
        get
        {
            var f = Save.flags.Find(x => x.StartsWith("together:"));
            return f != null ? content.Customer(f.Substring("together:".Length)) : null;
        }
    }

    /// Living together: your partner's done a chore before you're up (it counts towards the day's charm).
    void PartnerHelps()
    {
        var p = Partner;
        if (p == null || IsSaturday || IsSunday) return;
        var chore = Chores[Random.Range(0, Chores.Length)];
        Save.flags.Add($"chore:{chore.id}:{Save.day}");
        Stamps.Notice($"{p.displayName} was up early: {chore.name.ToLower()}, done.\n<size=68>{chore.done}</size>");
    }

    // ---- Birthdays ----

    public bool IsBirthday(CustomerDef c, int day) => c != null && c.birthday > 0 && Week.DayOfYear(day) == c.birthday;

    /// People whose birthday it is today and who you've met.
    public List<CustomerDef> BirthdaysOn(int day)
    {
        var list = new List<CustomerDef>();
        foreach (var c in content.customers) if (c != null && Save.met.Contains(c.customerId) && IsBirthday(c, day)) list.Add(c);
        return list;
    }

    string BirthdayFlag(CustomerDef c) => $"birthday:{c.customerId}:{Save.day}";
    bool CanWishBirthday(CustomerDef c) => IsBirthday(c, Save.day) && Save.met.Contains(c.customerId) && !Save.flags.Contains(BirthdayFlag(c));

    /// Happy birthday: extra friendship (and hearts for someone special), then on with the visit.
    void WishBirthday(CustomerDef c, System.Action then)
    {
        Save.flags.Add(BirthdayFlag(c));
        GM.AddRelationshipPoints(c.customerId, 12);
        AddHearts(c, 10);
        Stamps.Earn(Save, $"birthday:{c.customerId}", $"Happy birthday, {c.displayName}", "You remembered.", StampTheme.Friendship);
        string knot = content.Ink.Has($"{c.inkName}_birthday") ? $"{c.inkName}_birthday" : null;
        if (knot != null) Play(c, new List<string> { knot }, then);
        else Say(c, Pick("You remembered! Oh, that's made my whole day.", "How did you know? Nobody ever remembers. Thank you.",
                         "Ha! Another year older, and somehow still in your shop. Thank you, really."), ("Continue", then));
    }
}
