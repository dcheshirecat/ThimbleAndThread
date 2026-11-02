using UnityEngine;

/// Reading between customers (owner, 2026-11-02: real things to do in the shop, not minigames). Books from the Supply
/// Market are read a chapter at a time behind the counter (half an hour each, or until the bell goes). Each chapter is a
/// little experience; finishing a book teaches what it's about (SkillDef.book) and is worth more.
public partial class ShopDay
{
    public const int BookChapters = 5, ChapterMinutes = 30, ChapterXp = 6, BookXp = 20;

    public int ChaptersRead(string bookId) => Save.books.Find(b => b.id == bookId)?.chapters ?? 0;

    /// The book you're reading: one you've started, else the first unread one on your shelf.
    public SupplyDef CurrentBook
    {
        get
        {
            SupplyDef first = null;
            foreach (var s in content.supplies)
            {
                if (s == null || s.kind != SupplyKind.Book || !shop.Owns(s.supplyId)) continue;
                int read = ChaptersRead(s.supplyId);
                if (read >= BookChapters) continue;
                if (read > 0) return s;
                first ??= s;
            }
            return first;
        }
    }

    public bool CanRead => CanPassTime && CurrentBook != null;

    /// "Read: Cables and Arans (2/5)", or what to do about having nothing to read.
    public string ReadLabel => CurrentBook is SupplyDef b ? $"Read ({ChaptersRead(b.supplyId)}/{BookChapters})" : "Nothing to Read";

    public void ReadChapter()
    {
        var book = CurrentBook;
        if (book == null || !CanPassTime) return;
        var entry = Save.books.Find(b => b.id == book.supplyId);
        if (entry == null) Save.books.Add(entry = new BookProgress { id = book.supplyId });
        PassMinutes(ChapterMinutes);
        entry.chapters++;
        Progress.AddBonusXp(ChapterXp);
        if (entry.chapters < BookChapters)
            Stamps.Notice($"You read chapter {entry.chapters} of \"{book.displayName}\".\n<size=68>+{ChapterXp} experience. {BookChapters - entry.chapters} chapters to go.</size>");
        else
        {
            Progress.AddBonusXp(BookXp);
            Stamps.Earn(Save, $"book:{book.supplyId}", $"Read \"{book.displayName}\"", "Cover to cover.", StampTheme.Skill);
            var learned = Progress.CheckUnlocks();
            Stamps.Notice($"You finished \"{book.displayName}\"!\n<size=68>+{BookXp + ChapterXp} experience" +
                          (learned.Count > 0 ? $", and you've learned {string.Join(", ", learned.ConvertAll(s => s.displayName))}." : ".") + "</size>");
        }
        GM.Persist();
    }
}
