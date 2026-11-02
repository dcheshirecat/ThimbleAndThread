using System.Collections.Generic;
using UnityEngine;

/// Letters in the post (owner, 2026-11-02 evening): friends write after milestones (a closer friendship, the first
/// thing you made them, a festival stall). A letter is posted the day it's earned and arrives the next morning; the
/// Journal's Letters page keeps them all. A friend's own words come from their ink file (knot name_letter_<kind>,
/// with variants) when there is one; otherwise a warm letter in general words.
public partial class ShopDay
{
    /// Posts a letter from someone, arriving tomorrow morning. Each kind comes once per person.
    public void PostLetter(string customerId, string kind, string text = null)
    {
        string key = $"letter:{customerId}:{kind}";
        if (Save.flags.Contains(key)) return;
        var c = CustomerFor(customerId);
        if (c == null) return;
        Save.flags.Add(key);
        Save.letters.Add(new Letter { from = customerId, kind = kind, posted = Save.day, arrives = Save.day + 1, text = text ?? LetterText(c, kind) });
        GM.Persist();
    }

    string LetterText(CustomerDef c, string kind)
    {
        if (!string.IsNullOrEmpty(c.inkName))   // their own words, if written
        {
            var knot = Fresh(Variants($"{c.inkName}_letter_{kind}"));
            if (knot != null)
            {
                var k = content.Ink.Get(knot);
                var said = new List<string>();
                if (k != null) foreach (var ins in k.code) if (ins.op == InkLite.Op.Say && !string.IsNullOrEmpty(ins.text)) said.Add(ins.text);
                if (said.Count > 0) return PlayerText.Fill(string.Join("\n", said));
            }
        }
        string me = Save.player.name, first = c.displayName.Split(' ')[0];
        string body = kind switch
        {
            "first" => Pick($"I wanted to write and say thank you properly. I've used it every day since, and every time I do I think of your little shop.",
                            $"Thank you again. People keep asking where I got it, and I keep sending them to you. I hope you don't mind!"),
            "level2" => Pick("I've so enjoyed our chats lately. It's nice to have somewhere to go where someone's pleased to see you.",
                             "I don't write many letters these days, but I wanted you to know how glad I am your shop is open again."),
            "level4" => Pick("You've become a real friend, you know. I tell people about you as if I've known you for years.",
                             "I was thinking about you the other day, and about your great-aunt. She'd be so proud of what you've done with the place."),
            "level6" => Pick("Some people make a town feel like home. You're one of them. Thank you for everything.",
                             "I keep the things you've made where I can see them. I hope you know how much they mean."),
            "fair" => Pick("I saw your stall at the fair! It was the busiest on the square. Well done, you.",
                           "What a day the fair was. I walked past your stall three times just to see all the lovely things."),
            _ => "Just a note to say hello, and that I'm thinking of you.",
        };
        return $"Dear {me},\n\n{body}\n\nWith love,\n{first}";
    }

    /// Letters that have come: the post arrives each morning.
    public List<Letter> Delivered => Save.letters.FindAll(l => l.arrives <= Save.day);
    public int UnreadLetters => Save.letters.FindAll(l => l.arrives <= Save.day && !l.read).Count;

    /// Called in the morning: news of any letters that came.
    void MorningPost()
    {
        var come = Save.letters.FindAll(l => l.arrives == Save.day);
        if (come.Count == 0) return;
        var names = new List<string>();
        foreach (var l in come) names.Add(CustomerFor(l.from)?.displayName ?? "a friend");
        Stamps.Announce(come.Count == 1 ? $"The post came!\n<size=68>A letter from {names[0]}. Read it in your journal.</size>"
                                        : $"The post came!\n<size=68>Letters from {string.Join(" and ", names)}. Read them in your journal.</size>");
    }

    /// The Journal's Letters page: newest first; reading them marks them read.
    public string LettersPage()
    {
        var list = Delivered;
        if (list.Count == 0) return "<b>Letters</b>\n\nNo letters yet. Friends write now and then, when something special happens.";
        var sb = new System.Text.StringBuilder("<b>Letters</b>\n");
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var l = list[i];
            sb.Append($"\n<b>From {NameFor(l.from)}</b>  ·  day {l.arrives}{(l.read ? "" : "  (new)")}\n{l.text}\n");
            l.read = true;
        }
        GM.Persist();
        return sb.ToString();
    }

    /// Milestones that bring a letter.
    void LetterForLevel(string customerId, int level)
    {
        var c = CustomerFor(customerId);
        if (c == null || !Save.met.Contains(customerId)) return;
        if (level == 2 || level == 4 || level == 6) PostLetter(customerId, $"level{level}");
    }
}
