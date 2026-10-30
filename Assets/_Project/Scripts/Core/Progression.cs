using System.Collections.Generic;
using UnityEngine;

/// The player's progress: which crafts and skills they've learned, experience and levels in each
/// craft, the story chapter and its goals, and shop upgrades. ShopDay calls CheckUnlocks and
/// AdvanceChapter after anything that might unlock something (crafting, hand-overs, buying books...).
public class Progression
{
    public const int XpPerLevel = 60;
    const int MaxNews = 30;

    readonly ContentLibrary content;
    readonly ShopManager shop;

    public Progression(ContentLibrary content, ShopManager shop)
    {
        this.content = content;
        this.shop = shop;
    }

    GameManager GM => GameManager.Instance;
    SaveData Save => GM.Save;

    // ---- Skills ----

    public bool Knows(string skillId) => string.IsNullOrEmpty(skillId) || Save.skills.Contains(skillId);

    /// The skill that is a whole craft, or null if there isn't one (then the craft is always open).
    public SkillDef CraftSkill(CraftType craft) => System.Array.Find(content.skills, s => s != null && s.isCraft && s.craft == craft);

    public bool CraftOpen(CraftType craft) => CraftSkill(craft) is SkillDef s ? Knows(s.skillId) : true;

    /// What the player still has to learn to make an item: its craft, and any technique its steps need.
    public List<SkillDef> MissingFor(CraftItemDef item)
    {
        var missing = new List<SkillDef>();
        var craft = CraftSkill(item.craft);
        if (craft != null && !Knows(craft.skillId)) missing.Add(craft);
        foreach (var step in item.Steps)
        {
            var s = content.Skill(step.requiredSkill);
            if (s != null && !Knows(s.skillId) && !missing.Contains(s)) missing.Add(s);
        }
        return missing;
    }

    public bool CanMake(CraftItemDef item) => item != null && MissingFor(item).Count == 0;

    /// How a skill is learned, for the Journal: "Story chapter 2, or a book: ...".
    public string HowToLearn(SkillDef s)
    {
        var ways = new List<string>();
        if (s.chapter > 0) ways.Add($"story chapter {s.chapter}");
        if (s.craftLevel > 0) ways.Add($"{CraftName(s.levelCraft)} level {s.craftLevel}");
        if (s.book != null) ways.Add($"the book \"{s.book.displayName}\" from the Supply Market");
        if (s.friend != null && s.friendLevel > 0) ways.Add($"{s.friend.displayName} at friendship level {s.friendLevel}");
        return ways.Count > 0 ? string.Join(", or ", ways) : "not yet known";
    }

    /// Learns anything whose route the player has now reached. Returns what was learned.
    public List<SkillDef> CheckUnlocks()
    {
        var learned = new List<SkillDef>();
        foreach (var s in content.skills)
        {
            if (s == null || Knows(s.skillId)) continue;
            string how = null;
            if (s.chapter > 0 && Save.chapter >= s.chapter) how = $"chapter {Save.chapter}";
            else if (s.craftLevel > 0 && Level(s.levelCraft) >= s.craftLevel) how = $"reaching {CraftName(s.levelCraft)} level {s.craftLevel}";
            else if (s.book != null && shop.Owns(s.book.supplyId)) how = $"reading \"{s.book.displayName}\"";
            else if (s.friend != null && s.friendLevel > 0 && GM.GetRelationship(s.friend.customerId).level >= s.friendLevel) how = $"{s.friend.displayName} showing you how";
            if (how == null) continue;
            Learn(s, how);
            learned.Add(s);
        }
        return learned;
    }

    void Learn(SkillDef s, string how)
    {
        Save.skills.Add(s.skillId);
        foreach (var p in s.grantsPatterns)
            if (p != null && p.item != null && !Save.patterns.Exists(x => x.id == p.patternId))
                Save.patterns.Add(new OwnedPattern { id = p.patternId, name = p.displayName, itemId = p.item.itemId, price = p.price });
        AddNews(s.isCraft ? $"New craft: {s.displayName}! (from {how})" : $"Learned {s.displayName}, from {how}.");
    }

    /// Older saves (from before skills existed) keep everything they could already do.
    public void LearnEverything()
    {
        foreach (var s in content.skills) if (s != null && !Knows(s.skillId)) Save.skills.Add(s.skillId);
    }

    // ---- Experience ----

    public int Xp(CraftType craft) => Save.craftXp.Find(x => x.craft == craft)?.xp ?? 0;
    public int Level(CraftType craft) => 1 + Xp(craft) / XpPerLevel;
    public int XpToNext(CraftType craft) => XpPerLevel - Xp(craft) % XpPerLevel;

    /// Adds experience (more with a better lamp). Returns a message if it was a level up, else null.
    public string AddXp(CraftType craft, int amount)
    {
        amount = Mathf.RoundToInt(amount * (1f + UpgradeAmount(UpgradeEffect.CraftLamp) / 100f));
        var entry = Save.craftXp.Find(x => x.craft == craft);
        if (entry == null) Save.craftXp.Add(entry = new CraftXp { craft = craft });
        int before = Level(craft);
        entry.xp += amount;
        if (Level(craft) == before) return null;
        string msg = $"{CraftName(craft)} level {Level(craft)}!";
        AddNews(msg);
        return msg;
    }

    // ---- Story chapters ----

    public ChapterDef Chapter => content.Chapter(Save.chapter);

    public bool GoalsMet(ChapterDef c) =>
        Save.totalDelivered >= c.deliveries && Save.totalPatternsSold >= c.patternsSold && BestFriendLevel() >= c.friendLevel;

    public int BestFriendLevel()
    {
        int best = 0;
        foreach (var r in Save.relationships) best = Mathf.Max(best, r.level);
        return best;
    }

    /// e.g. "Orders handed over: 2 of 3. Patterns sold: 1 of 2."
    public string GoalProgress(ChapterDef c)
    {
        var parts = new List<string>();
        if (c.deliveries > 0) parts.Add($"Orders handed over: {Mathf.Min(Save.totalDelivered, c.deliveries)} of {c.deliveries}");
        if (c.patternsSold > 0) parts.Add($"Patterns sold: {Mathf.Min(Save.totalPatternsSold, c.patternsSold)} of {c.patternsSold}");
        if (c.friendLevel > 0) parts.Add($"A friendship at level {c.friendLevel}: best so far {BestFriendLevel()}");
        return string.Join("\n", parts);
    }

    /// Moves on to the next chapter if this one's goals are met. Returns the new chapter, or null.
    public ChapterDef AdvanceChapter()
    {
        var current = Chapter;
        var next = content.Chapter(Save.chapter + 1);
        if (current == null || next == null || !GoalsMet(current)) return null;
        Save.chapter = next.number;
        AddNews($"Chapter {next.number}: {next.title}");
        CheckUnlocks();
        return next;
    }

    // ---- Shop upgrades ----

    public bool Owns(UpgradeDef u) => Save.upgrades.Contains(u.upgradeId);

    public int UpgradeAmount(UpgradeEffect effect)
    {
        int total = 0;
        foreach (var id in Save.upgrades)
        {
            var u = content.Upgrade(id);
            if (u != null && u.effect == effect) total += u.amount;
        }
        return total;
    }

    /// Upgrades the player can buy now: offered in this chapter and not owned yet.
    public List<UpgradeDef> UpgradesOnOffer()
    {
        var list = new List<UpgradeDef>();
        foreach (var u in content.upgrades) if (u != null && u.chapter <= Save.chapter && !Owns(u)) list.Add(u);
        list.Sort((a, b) => a.price.CompareTo(b.price));
        return list;
    }

    // ---- Journal news ----

    public void AddNews(string text)
    {
        Save.journalNews.Insert(0, $"Day {Save.day}: {text}");
        if (Save.journalNews.Count > MaxNews) Save.journalNews.RemoveRange(MaxNews, Save.journalNews.Count - MaxNews);
    }

    public static string CraftName(CraftType c) => c switch
    {
        CraftType.CrossStitch => "Cross stitch",
        CraftType.Knitting => "Knitting",
        CraftType.Embroidery => "Embroidery",
        CraftType.Sewing => "Sewing",
        CraftType.Crochet => "Crochet",
        _ => c.ToString(),
    };
}
