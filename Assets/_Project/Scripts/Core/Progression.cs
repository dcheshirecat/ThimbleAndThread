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

    /// Whether the market shows this supply: only once a craft that uses it is learned, so the shelves don't
    /// give away what's still to come (owner, 2026-10-31). Yarn serves knitting and crochet; stuffing and
    /// eyes serve sewing and crochet; hoops and tapestry needles serve cross stitch as well.
    public bool SupplyShown(SupplyDef s) => s != null && (CraftShown(s) || NeededByMakeable(s));

    /// Tools and materials a recipe you can already make calls for, whatever craft they belong to (e.g. pins for mitring
    /// a tablecloth's corners, a crochet hook for knotting a scarf's fringe).
    bool NeededByMakeable(SupplyDef s)
    {
        foreach (var item in content.items)
        {
            if (item == null || !CanMake(item)) continue;
            foreach (var step in item.Steps)
            {
                if (s.kind == SupplyKind.Tool && step.tools != null && System.Array.Exists(step.tools, t => t != null && t.toolType == s.toolType)) return true;
                if (step.materials != null && System.Array.Exists(step.materials, m => m != null && m.supplyId == s.supplyId)) return true;
            }
        }
        return false;
    }

    bool CraftShown(SupplyDef s) => s.kind switch
    {
        SupplyKind.Yarn => CraftOpen(CraftType.Knitting) || CraftOpen(CraftType.Crochet),
        SupplyKind.Stuffing or SupplyKind.Eyes => CraftOpen(CraftType.Sewing) || CraftOpen(CraftType.Crochet),
        SupplyKind.Tool when s.toolType == "hoop" || s.toolType == "tapestry_needle" => true,   // cross stitch uses these too
        SupplyKind.Tool when s.toolType == "marking_pen" => CraftOpen(CraftType.Embroidery) || CraftOpen(CraftType.Sewing),
        _ => CraftOpen(s.craft),
    };

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
        if (s.craftLevel > 0) ways.Add($"{CraftName(s.levelCraft)} mastery {s.craftLevel}");
        if (s.playerLevel > 0) ways.Add($"level {s.playerLevel}");
        if (s.book != null) ways.Add($"reading the book \"{s.book.displayName}\" from the Supply Market");
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
            else if (s.craftLevel > 0 && Level(s.levelCraft) >= s.craftLevel) how = $"reaching {CraftName(s.levelCraft)} mastery {s.craftLevel}";
            else if (s.playerLevel > 0 && PlayerLevel >= s.playerLevel) how = $"reaching level {s.playerLevel}";
            else if (s.book != null && Save.books.Exists(b => b.id == s.book.supplyId && b.chapters >= ShopDay.BookChapters)) how = $"reading \"{s.book.displayName}\"";
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
        Stamps.Earn(Save, $"skill:{s.skillId}", s.isCraft ? $"New craft: {s.displayName}" : $"Learned {s.displayName}",
                    $"From {how}.", s.isCraft ? StampTheme.Craft : StampTheme.Skill);
        Explain(s);
    }

    /// A short explainer for what's just been learned (the first craft is covered by How to Play).
    void Explain(SkillDef s)
    {
        if (s.isCraft && s.craft != CraftType.CrossStitch)
            Explainers.Explain(Save, $"skill:{s.skillId}", false,
                $"New craft: {s.displayName}\nCustomers can now ask for {s.displayName.ToLower()} pieces, its patterns appear in your book, " +
                $"and the Supply Market stocks what it needs. Each piece is made step by step at night, the way it's really done.");
        else if (s.skillId == ShopDay.PatternDesignSkill)
            Explainers.Explain(Save, "skill:pattern_design", false,
                "The Pattern Studio is open\nAt night, in the workroom, design your own patterns square by square. " +
                "Put them on display to sell, or make them for customers. Customers who know you well may ask for something special.\n" +
                $"You can design for a craft once you've handed over {OrdersToDesign} orders in it.");
    }

    /// Your level, experience, crafts and techniques: the panel the experience bar opens (owner, 2026-11-01: moved out of
    /// the Journal). Crafts and techniques only appear once learned; how the rest are learned stays a surprise.
    public string SkillsReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>Level {PlayerLevel}</b>");
        sb.AppendLine($"Experience: {XpIntoLevel} of {XpForLevel} for this level. <b>{XpToNextLevel} more</b> to reach level {PlayerLevel + 1}.\n");
        sb.AppendLine("<b>Crafts</b>  (mastery grows with every order you hand over)");
        int unknown = 0;
        foreach (CraftType craft in System.Enum.GetValues(typeof(CraftType)))
        {
            if (!CraftOpen(craft)) { unknown++; continue; }
            int l = Level(craft);
            sb.AppendLine($"{CraftName(craft)}: {MasteryName(l)} ({l} of {MaxMastery})" +
                          (l < MaxMastery ? $", {OrdersToNext(craft)} more order{(OrdersToNext(craft) == 1 ? "" : "s")} to the next" : "") +
                          (Knows(ShopDay.PatternDesignSkill) && !CanDesign(craft) ? $". Design for it after {OrdersToDesign} orders." : ""));
        }
        sb.AppendLine("\n<b>Techniques</b>");
        foreach (var s in content.skills)
        {
            if (s == null || s.isCraft) continue;
            if (Knows(s.skillId)) sb.AppendLine($"{s.displayName}: {s.description}");
            else unknown++;
        }
        if (unknown > 0) sb.AppendLine(unknown == 1 ? "There's one more thing to learn out there..." : $"There are {unknown} more things to learn out there...");
        return sb.ToString();
    }

    /// Older saves get stamps for what they'd already done, quietly (no pop-ups).
    public void CatchUpStamps()
    {
        foreach (var s in content.skills)
            if (s != null && Knows(s.skillId))
                Stamps.Earn(Save, $"skill:{s.skillId}", s.isCraft ? $"New craft: {s.displayName}" : $"Learned {s.displayName}", "Before you kept a stamp book.", s.isCraft ? StampTheme.Craft : StampTheme.Skill, true);
        for (int n = 2; n <= Save.chapter; n++)
            if (content.Chapter(n) is ChapterDef c) Stamps.Earn(Save, $"chapter:{n}", $"Chapter {n}", c.title, StampTheme.Story, true);
        foreach (var r in Save.relationships)
            for (int l = 1; l <= r.level; l++)
                if (content.Customer(r.customerId) is CustomerDef who) Stamps.Earn(Save, $"friend:{r.customerId}:{l}", who.displayName, $"Friendship level {l}", StampTheme.Friendship, true);
    }

    /// Older saves (from before skills existed) keep everything they could already do.
    public void LearnEverything()
    {
        foreach (var s in content.skills) if (s != null && !Knows(s.skillId)) Save.skills.Add(s.skillId);
    }

    // ---- Experience ----

    /// Extra experience from the shop's decorations (ShopDay.XpBoost), in percent.
    public System.Func<int> ExtraXpPercent;

    public int Xp(CraftType craft) => Save.craftXp.Find(x => x.craft == craft)?.xp ?? 0;

    // ---- Mastery (owner, 2026-11-01): a craft's level comes from orders handed over in it, ten levels, each further
    //      apart than the last ----
    public const int MaxMastery = 10;
    static readonly int[] MasteryAt = { 1, 3, 6, 10, 15, 21, 28, 36, 45, 55 };   // orders to reach level 1, 2... 10
    public int OrdersIn(CraftType craft) => Save.craftOrders.Find(x => x.craft == craft)?.count ?? 0;
    public int Level(CraftType craft)
    {
        int n = OrdersIn(craft), l = 0;
        while (l < MaxMastery && n >= MasteryAt[l]) l++;
        return l;
    }
    /// Orders still to hand over for the next level (0 at the top).
    public int OrdersToNext(CraftType craft) => Level(craft) >= MaxMastery ? 0 : MasteryAt[Level(craft)] - OrdersIn(craft);
    public static string MasteryName(int level) => level switch
    {
        0 => "Beginner", 1 => "Novice", 2 => "Apprentice", 3 => "Hobbyist", 4 => "Practised", 5 => "Skilled",
        6 => "Accomplished", 7 => "Expert", 8 => "Master", 9 => "Grand Master", _ => "Legend",
    };

    /// An order in this craft has been handed over. Returns a message if it was a new mastery level, else null.
    public string CountOrder(CraftType craft)
    {
        var entry = Save.craftOrders.Find(x => x.craft == craft);
        if (entry == null) Save.craftOrders.Add(entry = new CraftCount { craft = craft });
        int before = Level(craft);
        entry.count++;
        if (Level(craft) == before) return null;
        int l = Level(craft);
        string msg = $"{CraftName(craft)} mastery {l}: {MasteryName(l)}!";
        AddNews(msg);
        Stamps.Earn(Save, $"mastery:{craft}:{l}", $"{CraftName(craft)}: {MasteryName(l)}", $"Mastery level {l}, after {OrdersIn(craft)} orders.", StampTheme.Craft, craft: craft.ToString());
        CheckUnlocks();
        return msg;
    }

    /// Whether you're good enough at a craft to design patterns for it (owner, 2026-11-01): Pattern Design learned, and
    /// at least three orders in that craft handed over.
    public const int OrdersToDesign = 3;
    public bool CanDesign(CraftType craft) => Knows(ShopDay.PatternDesignSkill) && OrdersIn(craft) >= OrdersToDesign;
    public bool CanDesign(CraftItemDef item) => item != null && CanMake(item) && CanDesign(item.craft);

    // Your own level (owner, 2026-10-31: an XP bar and a level, top left): all your experience together. Each level
    // takes a little longer than the last: level L starts at 30 * L * (L - 1) (60, 180, 360, 600...).
    public int TotalXp { get { int n = Save.bonusXp; foreach (var x in Save.craftXp) n += x.xp; return n; } }
    public int XpToNextLevel => LevelStart(PlayerLevel + 1) - TotalXp;
    public int XpIntoLevel => TotalXp - LevelStart(PlayerLevel);
    public int XpForLevel => LevelStart(PlayerLevel + 1) - LevelStart(PlayerLevel);

    /// Experience for the shop itself (good days at the stall...), not for a craft.
    public void AddBonusXp(int amount)
    {
        int before = PlayerLevel;
        Save.bonusXp += amount;
        LevelCheck(before);
    }

    void LevelCheck(int before)
    {
        if (PlayerLevel <= before) return;
        Stamps.Earn(Save, $"player_level:{PlayerLevel}", $"Level {PlayerLevel}", "You're getting the hang of this shop.", StampTheme.Shop);
        CheckUnlocks();
    }
    static int LevelStart(int level) => 30 * level * (level - 1);
    public int PlayerLevel { get { int l = 1; while (TotalXp >= LevelStart(l + 1)) l++; return l; } }
    public float LevelProgress
    {
        get
        {
            int l = PlayerLevel, from = LevelStart(l);
            return Mathf.Clamp01((TotalXp - from) / (float)(LevelStart(l + 1) - from));
        }
    }

    /// Adds experience (more with a better lamp). Returns a message if it was a level up, else null.
    public string AddXp(CraftType craft, int amount)
    {
        amount = Mathf.RoundToInt(amount * (1f + (UpgradeAmount(UpgradeEffect.CraftLamp) + (ExtraXpPercent?.Invoke() ?? 0)) / 100f));
        var entry = Save.craftXp.Find(x => x.craft == craft);
        if (entry == null) Save.craftXp.Add(entry = new CraftXp { craft = craft });
        int playerBefore = PlayerLevel;
        entry.xp += amount;
        LevelCheck(playerBefore);
        return PlayerLevel > playerBefore ? $"Level {PlayerLevel}!" : null;   // a craft's own level comes from orders (CountOrder)
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
        Stamps.Earn(Save, $"chapter:{next.number}", $"Chapter {next.number}", next.title, StampTheme.Story);
        CheckUnlocks();
        return next;
    }

    // ---- Achievement quests (owner, 2026-10-31: e.g. "Sell 5 patterns", claimed for coins and experience) ----

    public int Made(CraftType craft) => Save.itemsMade.Find(x => x.craft == craft)?.count ?? 0;

    public int QuestCount(QuestDef q)
    {
        switch (q.goal)
        {
            case QuestGoal.OrdersDelivered: return Save.totalDelivered;
            case QuestGoal.PatternsSold: return Save.totalPatternsSold;
            case QuestGoal.BestFriendship: return BestFriendLevel();
            case QuestGoal.PeopleMet: return Save.met.Count;
            case QuestGoal.PatternsDesigned: return Save.patterns.FindAll(p => p.designed).Count;
            case QuestGoal.Commissions: return Save.totalCommissions;
            default:
                if (q.oneCraft) return Made(q.craft);
                int all = 0;
                foreach (var m in Save.itemsMade) all += m.count;
                return all;
        }
    }

    public bool QuestClaimed(QuestDef q) => Save.questsClaimed.Contains(q.questId);
    public bool QuestDone(QuestDef q) => QuestCount(q) >= q.target;

    /// Shown in the Journal: not claimed, its earlier tier claimed, and its craft (if any) already learned,
    /// so quests never give away a craft (or designing) that's still to come.
    public bool QuestShown(QuestDef q) =>
        q != null && !QuestClaimed(q) && (q.after == null || QuestClaimed(q.after)) && (!q.oneCraft || CraftOpen(q.craft))
        && (q.goal != QuestGoal.PatternsDesigned && q.goal != QuestGoal.Commissions || Knows(ShopDay.PatternDesignSkill));

    /// The quests to show, the ones ready to claim first.
    public List<QuestDef> OpenQuests()
    {
        var list = new List<QuestDef>();
        foreach (var q in content.quests) if (QuestShown(q)) list.Add(q);
        list.Sort((a, b) => QuestDone(b).CompareTo(QuestDone(a)));
        return list;
    }

    /// Announces quests that have just been reached (once each). Returns how many.
    public int CheckQuests()
    {
        int n = 0;
        foreach (var q in content.quests)
        {
            if (!QuestShown(q) || !QuestDone(q) || Save.questsReady.Contains(q.questId)) continue;
            Save.questsReady.Add(q.questId);
            Stamps.Announce($"Goal reached: {q.title}!\n<size=68>Claim your reward in the Journal.</size>");
            n++;
        }
        return n;
    }

    /// The craft practised most (of those learned): where a general quest's experience goes.
    public CraftType MostPractised()
    {
        var best = CraftType.CrossStitch;
        foreach (CraftType c in System.Enum.GetValues(typeof(CraftType)))
            if (CraftOpen(c) && Xp(c) > Xp(best)) best = c;
        return best;
    }

    /// Pays out a reached quest. Returns what it gave, or null if it isn't ready.
    public string Claim(QuestDef q)
    {
        if (q == null || QuestClaimed(q) || !QuestDone(q)) return null;
        Save.questsClaimed.Add(q.questId);
        Save.questsReady.Remove(q.questId);
        var craft = q.oneCraft ? q.craft : MostPractised();
        if (q.coins > 0) GM.AddCoins(q.coins);
        string levelUp = q.xp > 0 ? AddXp(craft, q.xp) : null;
        var parts = new List<string>();
        if (q.coins > 0) parts.Add($"{q.coins} coins");
        if (q.xp > 0) parts.Add($"{q.xp} {CraftName(craft).ToLower()} experience");
        var gift = RewardSupply(q);
        if (gift != null)
        {
            shop.Add(gift.supplyId, Mathf.Max(1, gift.perPack));
            parts.Add(gift.market == MarketSlot.Rare ? $"a rare find: {gift.displayName}" : gift.displayName);
        }
        string msg = $"{q.title}: {string.Join(" and ", parts)}.";
        AddNews(msg);
        CheckUnlocks();
        return levelUp != null ? $"{msg}\n{levelUp}" : msg;
    }

    /// How hard a goal is: 1 for the first of a chain, 2 for the one shown after it (QuestDef.after), and so on.
    public int Tier(QuestDef q)
    {
        int tier = 1;
        for (var at = q.after; at != null && tier < 20; at = at.after) tier++;
        return tier;
    }

    /// A supply to go with a claimed goal (owner, 2026-11-01): the harder the goal, the rarer. Something for a craft
    /// you've learned; never a book or a tool you already have.
    SupplyDef RewardSupply(QuestDef q)
    {
        int tier = Tier(q);
        var slot = tier >= 3 ? MarketSlot.Rare : tier == 2 ? MarketSlot.Rotating : MarketSlot.Always;
        var pool = new List<SupplyDef>();
        foreach (var s in content.supplies)
            if (s != null && s.market == slot && s.kind != SupplyKind.Book && SupplyShown(s) && !(s.kind == SupplyKind.Tool && shop.Owns(s.supplyId)))
                pool.Add(s);
        return pool.Count == 0 ? null : pool[Random.Range(0, pool.Count)];
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
        CraftType.Needlepoint => "Needlepoint",
        CraftType.Quilting => "Quilting",
        CraftType.Weaving => "Weaving",
        _ => c.ToString(),
    };
}
