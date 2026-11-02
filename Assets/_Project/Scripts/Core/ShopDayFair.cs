using System.Collections.Generic;
using UnityEngine;

/// Sunday market goals (owner, 2026-11-02): the stall on the square has goals; reach them all and you're offered a pitch
/// at the Harbour Fair, a bigger Sunday market along the quay: more shoppers, better prices, more patterns on show, and its
/// own regulars who come back week after week. Counts are kept in SaveData.stallStats.
public partial class ShopDay
{
    public const string FairFlag = "stall:fair";
    public bool AtFair => Save != null && Save.flags.Contains(FairFlag);
    public string MarketName => AtFair ? "Harbour Fair" : "Square Market";

    /// The square's goals: name, how far along, target.
    public List<(string name, int have, int need)> MarketGoals()
    {
        var s = Save.stallStats;
        return new List<(string, int, int)>
        {
            ("Handmade pieces sold", s.piecesSold, 10),
            ("Pattern copies sold", s.patternsSold, 5),
            ("Coins in one Sunday", s.bestSunday, 150),
            ("Sales to friends", s.friendSales, 3),
            ("Bargains struck (Ask for More)", s.bargains, 3),
        };
    }

    public bool MarketGoalsMet => MarketGoals().TrueForAll(g => g.have >= g.need);

    int sundayTakings;

    /// After each sale: counts, and the fair once every goal is met.
    void CountStallSale(Shopper s)
    {
        var st = Save.stallStats;
        if (s.piece != null) st.piecesSold++; else st.patternsSold++;
        if (s.friend != null) st.friendSales++;
        if (s.haggled) st.bargains++;
        sundayTakings += s.offer;
        st.bestSunday = Mathf.Max(st.bestSunday, sundayTakings);
        if (!AtFair && MarketGoalsMet)
        {
            Save.flags.Add(FairFlag);
            Stamps.Earn(Save, FairFlag, "The Harbour Fair", "Every goal on the square met: a pitch at the big Sunday fair.", StampTheme.Shop);
            Explainers.Explain(Save, FairFlag, true,
                "The Harbour Fair\nWord's got round about your stall. From next Sunday you've a pitch at the Harbour Fair along the quay: " +
                "bigger crowds, people from out of town who pay more, room for more patterns, and regulars who'll come back to see you.");
        }
    }

    /// The fair's regulars: the same faces every week, each with their own way of talking (and of buying).
    public struct Regular
    {
        public string id, name;
        public Color face;
        public string[] hello;    // first words, chosen fresh each visit
        public string[] chat;     // what they say when you chat
        public float pays;        // how much more than the usual price they offer
    }

    public static readonly Regular[] FairRegulars =
    {
        new Regular { id = "marguerite", name = "Marguerite, an antiques dealer", face = new Color(0.75f, 0.6f, 0.8f), pays = 1.4f,
            hello = new[] { "Darling! What have you got for me this week?", "I've a client in Bath who'd adore something like this.", "Don't tell the others, but yours is the only stall I come for." },
            chat = new[] { "I sold one of your pieces to a woman from London. She cried.", "Old samplers are my weakness. Yours will be old one day too, you know.", "Forty years at fairs, and I still get butterflies on a Sunday." } },
        new Regular { id = "tam", name = "Tam, from the honey stall next door", face = new Color(0.95f, 0.8f, 0.35f), pays = 1.1f,
            hello = new[] { "Morning, neighbour! Swap you a jar of honey for a look?", "Bees were grumpy this week. I need cheering up.", "You're busy today! Good for you." },
            chat = new[] { "Heather honey this week. Tastes like the cliffs.", "My stall's been here since my dad's day. Yours'll be a fixture soon.", "If it rains, we share my awning. Deal?" } },
        new Regular { id = "achterberg", name = "Mr Achterberg, a collector", face = new Color(0.55f, 0.65f, 0.75f), pays = 1.6f,
            hello = new[] { "Good morning. I am looking for something... exceptional.", "Ah. The stitching here is very even. Very even.", "I came on the early ferry for this stall." },
            chat = new[] { "I collect needlework from every harbour town in Europe. Yours is the youngest in my collection.", "A good piece is a letter to the future.", "My mother made lace in Bruges. Her hands were never still." } },
        new Regular { id = "bea", name = "Bea, a student with a sketchbook", face = new Color(0.6f, 0.85f, 0.7f), pays = 0.9f,
            hello = new[] { "Can I draw your stall? It's so pretty.", "I saved up all week for this!", "Hiya! Is that new? It's new, isn't it." },
            chat = new[] { "I'm doing textiles at college. You're basically my hero.", "I tried cross stitch. I sewed it to my jeans by mistake.", "One day I want a stall like this." } },
    };

    static readonly string[] FairStrangers =
    {
        "A day-tripper off the ferry", "A family in matching anoraks", "A man with a camera", "A woman with three shopping bags",
        "A couple from the yacht club", "A coach-party grandmother", "A busker on his break", "A girl with candyfloss",
    };

    /// The fair's version of a new shopper: more often a regular, more often paying over the odds.
    void FairShopper(Shopper picked)
    {
        if (picked.friend != null) return;
        if (Random.value < 0.35f)
        {
            var r = FairRegulars[Random.Range(0, FairRegulars.Length)];
            picked.regular = r.id;
            picked.name = r.name;
            picked.face = r.face;
            if (picked.piece != null || picked.pattern != null)
            {
                picked.offer = Mathf.Max(1, Mathf.RoundToInt(picked.offer * r.pays));
                picked.line = r.hello[Random.Range(0, r.hello.Length)] + " " +
                    (picked.piece != null ? $"I'll give you {picked.offer} for the {picked.piece.name}." : $"{picked.offer} for a copy of {picked.pattern.name}?");
            }
            else picked.line = r.hello[Random.Range(0, r.hello.Length)];
            return;
        }
        picked.name = FairStrangers[Random.Range(0, FairStrangers.Length)];
        picked.offer = Mathf.Max(1, Mathf.RoundToInt(picked.offer * 1.25f));   // out-of-towners pay more
        if (picked.piece != null) picked.line = Pick($"Is the {picked.piece.name} handmade? I'll give you {picked.offer}.", $"We'll take the {picked.piece.name}! {picked.offer} coins?");
        else if (picked.pattern != null) picked.line = Pick($"A copy of {picked.pattern.name}, please. {picked.offer}?", $"Ooh, a pattern! {picked.offer} for {picked.pattern.name}?");
    }

    /// A regular's chat line.
    string RegularChat(string id)
    {
        foreach (var r in FairRegulars) if (r.id == id) return r.chat[Random.Range(0, r.chat.Length)];
        return null;
    }
}
