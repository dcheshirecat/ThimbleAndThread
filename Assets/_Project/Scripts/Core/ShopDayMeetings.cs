using System.Collections.Generic;
using UnityEngine;

/// Friends who visit each other in your shop (owner, 2026-11-02 evening): now and then, as someone comes in, another
/// friend you've met happens to be in too, and they chat while you watch. At most once a day. Both get a little
/// friendship, and the Journal notes that they know each other. People with history have their own conversations
/// (Pairs); anyone else, a friendly general one. A pair's own ink knot (a_meets_b, with variants) is used when written.
public partial class ShopDay
{
    const float MeetChance = 0.22f;

    // Who knows whom, with a conversation each: lines alternate, starting with the first named.
    static readonly (string a, string b, string[] lines)[] Pairs =
    {
        ("mara", "priya", new[] { "Priya! Fancy seeing you here.", "Mara! How's the little one? Still not sleeping?",
                                  "Three hours last night. A record.", "Then you deserve something lovely from this shop." }),
        ("tobias", "theo", new[] { "Theo, lad. Your gran would've loved this place.", "I know, Grandad. That's why I keep coming in.",
                                   "Don't you go spending all your wages on wool.", "Says the man who's here twice a week." }),
        ("benji", "sam", new[] { "There you are! I thought you were at the hall.", "I was. I came to see if our order's ready.",
                                 "Great minds.", "Don't let me buy anything else for the wedding. Please." }),
        ("dot", "wenna", new[] { "Wenna Trelawney, as I live and breathe.", "Dot! I came for thread, I'll leave with half the shop.",
                                 "Win used to say exactly that.", "She did, didn't she. Every single week." }),
        ("hal", "gwen", new[] { "Morning, harbourmaster. Boats all in?", "Every one, Hal. Your pigeons home?",
                                "Two of them. The third's taking the scenic route.", "Like someone else I know." }),
        ("juno", "sol", new[] { "Sol! You're not on the beach?", "Day off. I'm looking for something that isn't a whistle on a string.",
                                "Get a scarf. Not a grandma scarf.", "Noted. No grandma scarves." }),
        ("kit", "felix", new[] { "Felix, those colours on your shirt. Stand still, I need to remember them.", "Kit, it's just a shirt.",
                                 "Nothing is just a shirt.", "Fine. But you're buying me a coffee after." }),
        ("midge", "owen", new[] { "Vicar! I've put that book on bees aside for you.", "Midge, you are a treasure. How much do I owe you?",
                                  "A jar of honey and you can stop asking.", "Done. The bees will be delighted." }),
        ("isla", "gwen", new[] { "Gwen, the lamp's running like a dream.", "Glad to hear it. Storm coming Thursday, mind.",
                                 "I'll be up all night, then.", "Bring a flask. And something warm from this place." }),
        ("nadia", "mara", new[] { "Mara! I delivered your little girl, you know.", "I know! I still can't believe how calm you were.",
                                  "Practice. And night shifts. And knitting in the quiet hours.", "Is that where all your cardigans come from?" }),
        ("eli", "isla", new[] { "Isla! Your usual's on the house next time.", "Eli, you'll go out of business.",
                                "Not if you keep bringing the whole lighthouse crew.", "Fair point. Two cinnamon buns, then." }),
        ("owen", "wenna", new[] { "Wenna, the quilting bee is the talk of the parish.", "Is it? Good. We need more hands.",
                                  "I'll put it in the newsletter.", "Put it in the sermon. They actually listen to those." }),
    };

    static readonly string[][] GeneralChats =
    {
        new[] { "Oh, hello! I didn't know you shopped here too.", "Every chance I get. Isn't it lovely?", "It really is. See you at the market?", "Wouldn't miss it." },
        new[] { "Well, look who it is!", "Ha! Small town. How are you keeping?", "Busy, busy. You?", "The same. This place is my little treat." },
        new[] { "Are you buying for yourself or someone else?", "Myself, for once.", "Good for you.", "Don't tell anyone." },
    };

    /// Another friend who happens to be in when this one comes: someone they know, or anyone you've met.
    CustomerDef PalFor(CustomerDef c)
    {
        string flag = $"meeting:{Save.day}";
        if (c == null || c.shopOwner || Save.flags.Contains(flag) || Random.value > MeetChance) return null;
        var known = new List<CustomerDef>();
        var anyone = new List<CustomerDef>();
        foreach (var other in content.customers)
        {
            if (other == null || other == c || other.shopOwner || !Save.met.Contains(other.customerId) || other.firstChapter > Save.chapter) continue;
            if (PairLines(c.customerId, other.customerId) != null) known.Add(other);
            else anyone.Add(other);
        }
        var pick = known.Count > 0 && Random.value < 0.8f ? known[Random.Range(0, known.Count)]
                 : anyone.Count > 0 ? anyone[Random.Range(0, anyone.Count)] : null;
        if (pick != null) Save.flags.Add(flag);
        return pick;
    }

    static string[] PairLines(string a, string b)
    {
        foreach (var p in Pairs)
            if (p.a == a && p.b == b || p.a == b && p.b == a) return p.lines;
        return null;
    }

    /// The two of them chat while you watch, then the visit carries on.
    void FriendsMeet(CustomerDef c, CustomerDef pal, System.Action then)
    {
        // Their own written meeting, if there is one.
        foreach (var (x, y) in new[] { (c, pal), (pal, c) })
        {
            if (string.IsNullOrEmpty(x.inkName) || string.IsNullOrEmpty(y.inkName)) continue;
            var knot = Fresh(Variants($"{x.inkName}_meets_{y.inkName}"));
            if (knot != null) { AfterMeeting(c, pal); Play(c, new List<string> { knot }, then); return; }
        }
        var lines = PairLines(c.customerId, pal.customerId) ?? GeneralChats[Random.Range(0, GeneralChats.Length)];
        // Pair lines start with the first-named person.
        bool cFirst = true;
        foreach (var p in Pairs) if (p.b == c.customerId && p.a == pal.customerId) cFirst = false;
        AfterMeeting(c, pal);
        Narrate(null, $"{NameFor(pal.customerId)} is browsing the shelves when {NameFor(c.customerId)} comes in.",
            ("Continue", () => SayLines(cFirst ? c : pal, cFirst ? pal : c, lines, 0, then)));
    }

    void SayLines(CustomerDef first, CustomerDef second, string[] lines, int i, System.Action then)
    {
        if (i >= lines.Length) { then(); return; }
        Say(i % 2 == 0 ? first : second, lines[i], ("Continue", () => SayLines(first, second, lines, i + 1, then)));
    }

    void AfterMeeting(CustomerDef c, CustomerDef pal)
    {
        GM.AddRelationshipPoints(c.customerId, 2);
        GM.AddRelationshipPoints(pal.customerId, 2);
        AddNote(c.customerId, $"Friends with {pal.displayName}.");
        AddNote(pal.customerId, $"Friends with {c.displayName}.");
        GM.Persist();
    }

    void AddNote(string customerId, string text)
    {
        string entry = $"{customerId}|{text}";
        if (!Save.notes.Contains(entry)) Save.notes.Add(entry);
    }
}
