using UnityEngine;

// Every customer, item, supply, pattern and commission in the game, in one asset the scene points at.
// It fills itself: whenever an asset under Assets/_Project/Data is added, changed or removed,
// Editor/ContentLibraryRefresher.cs updates these lists. You never need to edit it by hand.
public class ContentLibrary : ScriptableObject
{
    public CustomerDef[] customers = new CustomerDef[0];
    public CraftItemDef[] items = new CraftItemDef[0];
    public SupplyDef[] supplies = new SupplyDef[0];
    public PatternDef[] patterns = new PatternDef[0];
    public CommissionDef[] commissions = new CommissionDef[0];
    public TutorialDef tutorial;
    public SkillDef[] skills = new SkillDef[0];
    public ChapterDef[] chapters = new ChapterDef[0];
    public UpgradeDef[] upgrades = new UpgradeDef[0];
    public CosmeticDef[] cosmetics = new CosmeticDef[0];
    public QuestDef[] quests = new QuestDef[0];
    public TextAsset[] dialogue = new TextAsset[0];   // the .ink files in Assets/_Project/Dialogue

    InkLite ink;
    /// All the dialogue, read once.
    public InkLite Ink
    {
        get
        {
            if (ink != null) return ink;
            ink = new InkLite();
            foreach (var t in dialogue) if (t != null) ink.Load(t.text);
            return ink;
        }
    }

    public SkillDef Skill(string id) => System.Array.Find(skills, s => s != null && s.skillId == id);
    public ChapterDef Chapter(int number) => System.Array.Find(chapters, c => c != null && c.number == number);
    public QuestDef Quest(string id) => System.Array.Find(quests, q => q != null && q.questId == id);
    public UpgradeDef Upgrade(string id) => System.Array.Find(upgrades, u => u != null && u.upgradeId == id);

    public CustomerDef Customer(string id) => System.Array.Find(customers, c => c != null && c.customerId == id);
    public CraftItemDef Item(string id) => System.Array.Find(items, i => i != null && i.itemId == id);
    public SupplyDef Supply(string id) => System.Array.Find(supplies, s => s != null && s.supplyId == id);
    public CommissionDef Commission(string id) => System.Array.Find(commissions, c => c != null && c.commissionId == id);
    public PatternDef Pattern(string id) => System.Array.Find(patterns, p => p != null && p.patternId == id);
}
